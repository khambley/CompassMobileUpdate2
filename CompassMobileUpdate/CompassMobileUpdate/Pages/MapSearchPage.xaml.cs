using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.PlatformConfiguration.GTKSpecific;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace CompassMobileUpdate.Pages
{	
	public partial class MapSearchPage : ContentPage
	{
        private double width;
        private double height;
        private Geocoder geoCoder;
        private DateTime _lastMapMove;
        private bool _mapIsBusy;
        private BoundingCoords _latestBoundingCoords;
        private List<BoundingCoords> _submittedBounds = new List<BoundingCoords>();
        HashSet<string> _existingMapPinHash = new HashSet<string>();
        private static BindableProperty _bpDeviceUtilityID = BindableProperty.Create("DeviceUtilityID", typeof(string), typeof(string), string.Empty, BindingMode.Default, null, null, null, null, null);
        private Plugin.Geolocator.Abstractions.Position _positionNotedWhileZooming;
        bool _getPinsForMapIsRunning = false;
        private List<KeyValuePair<Point, double>> _submittedCoords = new List<KeyValuePair<Point, double>>();
        private bool _useLastPositionOnMapLoad;
        string _lastSearchText;

        MapSearchViewModel vm => BindingContext as MapSearchViewModel;

        public MapSearchPage (MapSearchViewModel viewModel)
		{
			InitializeComponent ();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            // TODO: Add custom background effect for searchbar on ios.
            // https://stackoverflow.com/questions/67581008/how-to-change-the-color-of-searchbar-search-icon-and-cancel-button-color-in-xama

            SetMapToDefaultPositionAsync();

            //meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(41.8500, -87.6500), Distance.FromMiles(5)));

            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;
            locator.PositionChanged += Locator_PositionChanged;

            btnToggleView.Clicked += BtnToggleView_Clicked;
        }

        private void BtnToggleView_Clicked(object sender, EventArgs e)
        {
            if (lvMeters.IsVisible)
            {
                lvMeters.IsVisible = false;

                btnToggleView.Text = "List";

                btnStreet.IsVisible = btnSatellite.IsVisible = btnHybrid.IsVisible = true;

                if (_useLastPositionOnMapLoad)
                {
                    //_map.MoveToRegion(MapSpan.FromCenterAndRadius(_lastPosition, Distance.FromMiles(_lastRadius)));
                    //For whatever reason, moving the map to the region at this point doesn't work. Instead, get the same results
                    //by mimicking a search button pressed using the latest search text
                    sBar.Text = _lastSearchText; //Set the current text to the last searched text in case they modified the text but didn't submit the search
                    sBar_SearchButtonPressed(sBar, null);
                }
                meterMap.IsVisible = true;
            }
            else
            {
                meterMap.IsVisible = false;


                //Since we are moving from map to listview reset to false; It will be set to true if a search is performed while the listView is visible.
                //That way when the map is reloaded we can move to the same location as was entered in the list view;
                _useLastPositionOnMapLoad = false;

                UpdateMeterListView();

                btnToggleView.Text = "Map";

                btnStreet.IsVisible = btnSatellite.IsVisible = btnHybrid.IsVisible = false;

                lvMeters.IsVisible = true;
            }
        }

        private void Locator_PositionChanged(object sender, PositionEventArgs positionEvent)
        {
            if (switchFollowMovement.IsToggled)
            {
                if(AppVariables.LastMapPosition != null)
                {
                    if(positionEvent.Position != null)
                    {
                        if(meterMap.VisibleRegion != null)
                        {
                            MapSpan mapSpanNewCenter = MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(positionEvent.Position.Latitude, positionEvent.Position.Longitude), meterMap.VisibleRegion.Radius);

                            meterMap.MoveToRegion(mapSpanNewCenter);
                        }
                        else
                        {
                            Console.WriteLine("VisibleRegion was null");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Position was null");
                    }
                }
            }
            else
            {
                Console.WriteLine("switchFollowMovement was false");
            }
        }

        protected override void OnAppearing()
        {

            if (lvMeters.IsVisible)
            {
                UpdateMeterListView();
            }
            else if (meterMap.IsVisible)
            {
                GetPinsForMapAsync();
            }
            base.OnAppearing();

        }

        private void UpdateMeterListView()
        {
            UpdateMeterListView(meterMap.VisibleRegion.Center, meterMap.VisibleRegion.Radius.Miles);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            var locator = CrossGeolocator.Current;

            if (locator.IsListening)
            {
                locator.StopListeningAsync();
            }

            if (AppVariables.IsCachingMapPins)
            {
                AppVariables.CachedMapPins = meterMap.Pins.ToList();
                AppVariables.CachedMapBoundingCoords = _submittedBounds.ToList();
            }
        }
        private async Task GetPinsForMapAsync()
        {
            Exception ex = null;
            int? meterFoundCount = null;

            //Make sure we only have one of these running at a time.
            if (!_getPinsForMapIsRunning)
            {
                _getPinsForMapIsRunning = true;

                //We want to make sure that the user just isn't scrolling across the map.
                //only start getting the Meters for Map after the map has been stationary for 1000 ms
                while (DateTime.Now.Subtract(_lastMapMove).TotalMilliseconds < 1000)
                {
                    await Task.Delay(100);
                }

                try
                {
                    btnToggleView.IsEnabled = false;

                    ToggleMeterLoadingActivityIndicator(true);

                    _mapIsBusy = true;

                    var meters = await vm.GetMetersWithinBoxBoundsAsync(_latestBoundingCoords);

                    _submittedBounds.Add(_latestBoundingCoords);

                    await AddMetersToMap(meters);

                    meterFoundCount = meters.Count;
                }
                catch (Exception e)
                {
                    if (AppHelper.ContainsAuthenticationRequiredException(e))
                    {
                        await vm.LoginRequired();
                        return;
                    }
                    else
                    {
                        ex = e;

                        //TODO: this.LogApplicationError("SearchPage.GetPinsForMap", ex);

                        await DisplayAlert("Service Error", "Get Meters Within Map Bounds service call failed.", "Close");
                    }
                }
                finally
                {
                    btnToggleView.IsEnabled = true;
                    ToggleMeterLoadingActivityIndicator(false);
                    _mapIsBusy = false;
                    _getPinsForMapIsRunning = false;
                }

                if (ex != null)
                {
                    lblMessage.TextColor = Color.Red;
                    lblMessage.Text = ex.Message;
                    await AppHelper.FadeOutLabelByEmptyString(lblMessage, 2500);
                    lblMessage.TextColor = Color.Black;
                }

                if (meterFoundCount.HasValue)
                {
                    lblMessage.Text = String.Format("{0} Meters Found", meterFoundCount.Value.ToString());
                    await AppHelper.FadeOutLabelByEmptyString(lblMessage);
                }
            }
        }

        protected Task AddMetersToMap(List<Meter> meters)
        {
            return Task.Run(() =>
            {
                Meter meter = null;

                for (int i = 0; i < meters.Count; i++)
                {
                    try
                    {
                        meter = meters[i];
                        SerialNumberFormatException fe;
                        if (vm.IsValidSerialNumber(meter.DeviceUtilityID, out fe))
                        {
                            if (!_existingMapPinHash.Contains(meter.DeviceUtilityID))
                            {
                                _existingMapPinHash.Add(meter.DeviceUtilityID);

                                Pin pin = new Pin
                                {
                                    Type = PinType.Place,
                                    Position = new Xamarin.Forms.Maps.Position(Convert.ToDouble(meter.Latitude), Convert.ToDouble(meter.Longitude)),
                                    Label = vm.GetCustomerNameAndDeviceUtilityID(meter),
                                    Address = meter.CustomerAddress
                                };

                                pin.SetValue(_bpDeviceUtilityID, meter.DeviceUtilityID);

                                pin.MarkerClicked += PinClicked;

                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    meterMap.Pins.Add(pin);
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO: AppLogger.LogError(ex);
                    }
                }
            });
        }

        private void PinClicked(object sender, EventArgs e)
        {
            string deviceUtilityID = ((Pin)sender).GetValue(_bpDeviceUtilityID).ToString();
            Navigation.PushAsync(Resolver.Resolve<MeterDetailPage>());
        }

        protected void ToggleMeterLoadingActivityIndicator(bool on)
        {
            if (on)
            {
                lblMessage.IsVisible = false;
                aiSearchingForMeters.IsVisible = true;
                aiSearchingForMeters.IsRunning = true;
            }
            else
            {
                lblMessage.IsVisible = true;
                aiSearchingForMeters.IsVisible = false;
                aiSearchingForMeters.IsRunning = false;
            }
           
        }

        public async Task SetMapToDefaultPositionAsync()
        {
            try
            {
                //If we haven't cached our last location, go ahead and set it to the user's current position
                if (AppVariables.LastMapPosition == null)
                {
                    var position = await Xamarin.Essentials.Geolocation.GetLocationAsync();
                    if (position != null)
                    {
                        Console.WriteLine("Position: " + position.ToString());
                        meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude), Distance.FromMiles(.05)));
                    }
                    else
                    {
                        Console.WriteLine("Position is null");
                    }
                }
                else
                {
                    Console.WriteLine("AppVariables.LastMapPosition: " + AppVariables.LastMapPosition.ToString());
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(System.Threading.Tasks.TaskCanceledException) || (e.InnerException != null && e.InnerException.GetType() == typeof(System.Threading.Tasks.TaskCanceledException)))
                {
                    //TODO: Add app logging
                    //AppLogger.Debug("Locator.GetPositionAsync(10000) timed out");

                    Console.WriteLine("GetLocation errored out");

                    // Set default position to Chicago
                    meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(41.881832, -87.623177), Distance.FromMiles(5)));
                }
                //TODO: Log app errors
                //this.LogApplicationError("SearchPage.SetMapToDefaultPosition", e);
            }

            try
            {
                meterMap.IsShowingUser = true;
            }
            catch (Exception)
            {
                meterMap.IsShowingUser = false;
            }
        }

        protected async void sBar_SearchButtonPressed(System.Object sender, System.EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(sBar.Text))
                {
                    var searchText = sBar.Text.Trim();

                    double radius = .05; //small for exact location or meter
                    int temp;

                    //If we're pretty sure it's a zipcode
                    if (searchText.Length == 5 && Int32.TryParse(searchText, out temp))
                    {
                        radius = 3;
                        lblMessage.Text = "Searching zipcode: " + searchText;
                    }
                    else
                    {
                        lblMessage.Text = "Searching for: " + searchText;
                    }
                    await AppHelper.FadeOutLabelByEmptyString(lblMessage, 3000);

                    Xamarin.Forms.Maps.Position position = new Xamarin.Forms.Maps.Position();

                    var approximateLocation = (await geoCoder.GetPositionsForAddressAsync(searchText)).ToList();

                    var possibleAddresses = new List<string>();

                    if (approximateLocation.Count > 0)
                    {
                        position = approximateLocation[0];

                        if (lvMeters.IsVisible)
                        {
                            UpdateMeterListView(position, radius);

                            _useLastPositionOnMapLoad = true;
                            _lastSearchText = sBar.Text;
                        }
                        else if (meterMap.IsVisible)
                        {
                            meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(radius)));
                        }
                    }
                    else
                    {
                        lblMessage.Text = "Address not found, please try again";
                        await AppHelper.FadeOutLabelByEmptyString(lblMessage, 10000);
                        HideSearchResults();
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = ex.Message;
                await AppHelper.FadeOutLabelByEmptyString(lblMessage, 10000);
            }
        }

        protected void HideSearchResults()
        {
            lvSearchResults.IsVisible = false;
        }

        private async void UpdateMeterListView(Xamarin.Forms.Maps.Position position, double radius)
        {
            lvMeters.ItemsSource = null;

            if (radius <= AppVariables.GetMetersWithinXRadiusMaxValue)
            {
                aiMeterListLoading.IsRunning = true;
                aiMeterListLoading.IsVisible = true;

                try
                {
                    btnToggleView.IsEnabled = false;

                    var meters = await vm.GetMetersWithinXRadiusAsync(position.Latitude, position.Longitude, radius);

                    SerialNumberFormatException serialNumberFormatEx;
                    for (int i = 0; i < meters.Count; i++)
                    {
                        if (!vm.IsValidSerialNumber(meters[i].DeviceUtilityID, out serialNumberFormatEx))
                        {
                            meters.RemoveAt(i);
                            i--;
                        }
                    }
                    lvMeters.ItemsSource = meters;
                }
                catch (Exception e)
                {
                    if (AppHelper.ContainsAuthenticationRequiredException(e))
                    {
                        await vm.LoginRequired();
                        return;
                    }
                    else
                    {
                        //TODO: Add app logging
                        //AppLogger.LogError(e);

                        await DisplayAlert("Service Error", "Get Meters Within X Radius service call failed.", "Close");
                    }
                }
                finally
                {
                    btnToggleView.IsEnabled = true;
                    aiMeterListLoading.IsRunning = false;
                    aiMeterListLoading.IsVisible = false;
                }
            }
        }

        void meterMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string zoomInMoreMessage = "Zoom in to see meters";

            if (meterMap.IsVisible)
            {
                if (e.PropertyName.Equals("VisibleRegion", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (_positionNotedWhileZooming != null)
                    {
                        MapSpan mapSpanNewCenter = MapSpan.FromCenterAndRadius(new Xamarin.Forms.Maps.Position(_positionNotedWhileZooming.Latitude, _positionNotedWhileZooming.Longitude), meterMap.VisibleRegion.Radius);
                        Console.WriteLine("MJ - E");
                        meterMap.MoveToRegion(mapSpanNewCenter);

                        _positionNotedWhileZooming = null;
                    }
                    if (meterMap.VisibleRegion != null && _mapIsBusy == false)
                    {
                        //Cache our location
                        AppVariables.LastMapPosition = meterMap.VisibleRegion;

                        if (meterMap.VisibleRegion.Radius.Miles <= AppVariables.GetMetersWithinXRadiusMaxValue)
                        {
                            if (lblMessage.Text == zoomInMoreMessage)
                                lblMessage.Text = string.Empty;

                            try
                            {
                                BoundingCoords bounds = AppHelper.GetBoundingCoords(meterMap.VisibleRegion);
                                bool alreadySearched = false;

                                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                                stopWatch.Start();

                                for (int i = 0; i < _submittedBounds.Count; i++)
                                {
                                    if (bounds.Within(_submittedBounds[i], 6))
                                    {
                                        alreadySearched = true;
                                        break;
                                    }
                                }
                                stopWatch.Stop();

                                double time;

                                //Want to see if this ever happens.
                                if ((time = stopWatch.ElapsedMilliseconds) > 500)
                                {

                                    string message = "Bound Search took " + Math.Round(time) + " milliseconds for " + _submittedBounds.Count.ToString() + " BoundingCoords";
                                    //AppVariables.AppService.LogApplicationError("SearchPage.mapPropertyChanged", new Exception(message));

                                }

                                if (alreadySearched)
                                {
                                    return;
                                }
                                else
                                {
                                    _lastMapMove = DateTime.Now;
                                    _latestBoundingCoords = bounds;
                                    GetPinsForMapAsync();
                                }

                            } //End try
                            catch (Exception ex)
                            {
                                //AppLogger.LogError(ex);
                            }
                            finally
                            {
                                _mapIsBusy = false;
                            }

                        }//end we are within the acceptable radius
                        else
                        {
                            lblMessage.Text = zoomInMoreMessage;
                        }
                    } //End VisibleRegion is good
                } //End it's the Property we care about
            } //End Map Is visible

        } //end meterMap_Property_Changed
    }
}

