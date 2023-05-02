using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Threading;
using Xamarin.Forms.Maps;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Essentials;
using CompassMobile.Models;
using CompassMobileUpdate;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages;
using CompassMobileUpdate.ViewModels;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Services;

namespace CompassMobileUpdate.Pages
{
    using MapPosition = Xamarin.Forms.Maps.Position;

    public class MapPage : ContentPage
    {
        //Controls
        SearchBar _sBar;
        Label _lblFollowMovementLabel;
        Switch _switchFollowMovement;
        Label _lblMessage;
        ListView _lvSearchResults;
        ListView _lvMeters;
        Xamarin.Forms.Maps.Map _map;
        Geocoder _geoCoder;
        Button _btnToggleView;
        Button _btnHybrid;
        Button _btnSatellite;
        Button _btnStreet;
        StackLayout _slToggleView;
        Grid _gridMapType;
        ActivityIndicator _aiMeterListLoading;
        ActivityIndicator _aiSearchingForMeters;
        bool _isSearchResultSelection;
        bool _isSearchBusy = false;

        Color _textColor = Color.Black;
        Color _backGroundColor = Color.FromHex("ECECEC");
        Color _buttonBacktroundColor = Color.FromHex("CC0033");
        Color _buttonTextColor = Color.White;
        string _textColorString = "#000000";
        string _lblHeightChecker = "25@#5@!0a>^";
        bool _sizeAllocated = false;
        double _toggleButtonHeight;
        bool _cacheMapPins = false;
        HashSet<string> _existingMapPinHash = new HashSet<string>();

        AuthService _authService;
        MeterService _meterService;
        MapSearchViewModel vm => this.BindingContext as MapSearchViewModel;

        //Bindables
        static BindableProperty _bpDeviceUtilityID = BindableProperty.Create("DeviceUtilityID", typeof(string), typeof(string), string.Empty, BindingMode.Default, null, null, null, null, null);
        public MapPage()
        {
            this.Title = "Search by Location";

            _authService = new AuthService();
            _meterService = new MeterService(_authService);

            try
            {
                initControls();
                loadControls();
                SetMapToDefaultPosition();
            }
            catch (Exception e)
            {
                string message = e.Message;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!_sizeAllocated)
            {

                //Find what the height of a label with text is
                double labelHeight = _lblMessage.Height;

                //Apply that height to the labels that won't have heights set yet
                _lblMessage.HeightRequest = labelHeight;
                _lblFollowMovementLabel.HeightRequest = labelHeight;
                _aiSearchingForMeters.HeightRequest = labelHeight;

                if (_lblMessage.Text.Equals(_lblHeightChecker))
                    _lblMessage.Text = string.Empty;

                //Cache the height of the button on first load. then on future size allocation force it to keep the same
                //This forces our Toggle (map/list) button to keep the same height between Map and List view
                _toggleButtonHeight = _btnHybrid.Height;

                //If our buttons are greater than the width the of the screen lets shrink them as much as we can
                if (width < (_btnHybrid.Width + _btnSatellite.Width + _btnStreet.Width + _btnToggleView.Width))
                {
                    _btnHybrid.FontSize =
                        _btnSatellite.FontSize =
                        _btnStreet.FontSize =
                        _btnToggleView.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Button));

                    _btnHybrid.WidthRequest = _btnStreet.WidthRequest = 70;
                    _btnToggleView.WidthRequest = 60;

                }
                else //lets see if we can space them out a bit
                {
                    double diff = width - (_btnHybrid.Width + _btnSatellite.Width + _btnStreet.Width + _btnToggleView.Width);
                    double added;
                    if ((added = ((_btnSatellite.Width - _btnHybrid.Width) + (_btnSatellite.Width - _btnStreet.Width))) < diff)
                    {
                        _btnHybrid.WidthRequest = _btnSatellite.Width;
                        _btnStreet.WidthRequest = _btnSatellite.Width;
                        double leftOver = diff - added;
                        if (leftOver > 0)
                        {
                            if ((_btnSatellite.Width - _btnToggleView.Width) + 10 <= leftOver)
                            {
                                _btnToggleView.WidthRequest = _btnSatellite.Width;
                            }
                        }
                    }
                }
            }

            //Set the toggle button height to keep the same height when the ListView meters is visible
            _btnToggleView.HeightRequest = _toggleButtonHeight;

            _sizeAllocated = true;

        }

        protected async override void OnDisappearing()
        {
            base.OnDisappearing();

            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
            }

            //cache pins and locations
            if (AppVariables.IsCachingMapPins)
            {
                AppVariables.CachedMapPins = _map.Pins.ToList();
                AppVariables.CachedMapBoundingCoords = _submittedBounds.ToList();
            }
        }

        protected override void OnAppearing()
        {          
            if (_lvMeters.IsVisible)
            {
                UpdateMeterListView();
            }
            else if (_map.IsVisible)
            {
                GetPinsForMap();
            }
            
            base.OnAppearing();          
        }

        public void initControls()
        {
            //Message Label
            _lblMessage = new Label
            {
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                Text = _lblHeightChecker,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = _textColor
            };

            //Search Bar
            _sBar = new SearchBar
            {
                WidthRequest = 50,
                HeightRequest = 50,
                Placeholder = "search"
            };
            _sBar.SearchButtonPressed += _sBar_SearchButtonPressed;
            _sBar.TextChanged += _sBar_TextChanged;
            _sBar.TextColor = Color.Black;
            _sBar.PlaceholderColor = Color.Gray;

            _lblFollowMovementLabel = new Label
            {
                Text = "Auto-Follow",
                FontSize = _lblMessage.FontSize,
                VerticalOptions = LayoutOptions.Center,
                TextColor = _textColor
            };

            _switchFollowMovement = new Switch
            {
                IsToggled = true
            };
            _switchFollowMovement.Toggled += _switchFollowMovement_Toggled;

            //listview
            _lvSearchResults = new ListView(ListViewCachingStrategy.RecycleElement);
            _lvSearchResults.ItemSelected += _searchResults_ItemSelected;
            _lvSearchResults.IsVisible = false;
            //_searchResults.Scale = .95;

            #region Location Objects

            //Locator
            var _locator = CrossGeolocator.Current;
            _locator.DesiredAccuracy = 50;
            _locator.PositionChanged += _locator_position_changed;
            _geoCoder = new Geocoder();

            //Map
            _map = new Xamarin.Forms.Maps.Map(MapSpan.FromCenterAndRadius(new MapPosition(41.8500300, -87.6500500), Distance.FromMiles(5)));
            _map.PropertyChanged += _map_PropertyChanged;
            _map.MapType = MapType.Hybrid;

            if (AppVariables.IsCachingMapPins)
            {
                if (AppVariables.CachedMapPins != null && AppVariables.CachedMapPins.Count > 0)
                {
                    //Load Cached Map Pins and BoundingCoords
                    for (int i = 0; i < AppVariables.CachedMapPins.Count; i++)
                    {
                        _map.Pins.Add(AppVariables.CachedMapPins[i]);
                        //Add the cached values to a high performing searchable hashset
                        _existingMapPinHash.Add(AppVariables.CachedMapPins[i].Label);
                    }
                    AppVariables.CachedMapPins.Clear();
                }

                if (AppVariables.CachedMapBoundingCoords != null && AppVariables.CachedMapBoundingCoords.Count > 0)
                {
                    _submittedBounds.AddRange(AppVariables.CachedMapBoundingCoords);
                    AppVariables.CachedMapBoundingCoords.Clear();
                }
            }

            #endregion

            _aiMeterListLoading = new ActivityIndicator
            {
                IsVisible = false
            };

            _aiSearchingForMeters = new ActivityIndicator
            {
                IsVisible = false
            };

            _lvMeters = new ListView(ListViewCachingStrategy.RecycleElement)
            {
                BackgroundColor = _backGroundColor,

                ItemTemplate = new DataTemplate (typeof(CustomCell)),
                IsVisible = false,
                RowHeight = 60
            };

            _lvMeters.ItemTapped += _lvMeters_ItemTapped;

            _slToggleView = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.End,
                Padding = new Thickness(0, 0, 10, 0)
            };


            _gridMapType = new Grid
            {
                Padding = new Thickness(1, 0, 0, 1),
#if __IOS__
                ColumnSpacing = 6,
#else
                ColumnSpacing = 2,
#endif
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = new GridLength(80) },
                    new ColumnDefinition { Width = new GridLength(80) },
                    new ColumnDefinition { Width = new GridLength(80) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                BackgroundColor = _backGroundColor
            };

            _btnToggleView = GetMapButton("List");
            _btnToggleView.Clicked += _btnToggleView_Clicked;

            _btnHybrid = GetMapTypeButton("Hybrid", MapType.Hybrid);
            _btnSatellite = GetMapTypeButton("Satellite", MapType.Satellite);
            _btnStreet = GetMapTypeButton("Street", MapType.Street);

            if (!_locator.IsListening)
            {
                _locator.StartListeningAsync(TimeSpan.FromSeconds(0), 3);  // no minimum time and if the distance changes by 3 meters
            }
        }

        private Button GetMapTypeButton(string btnLabel, MapType mapType)
        {
            var _btn = GetMapButton(btnLabel);
            _btn.CommandParameter = mapType;
            _btn.Clicked += Button_MapType_Clicked;

            return _btn;
        }

        private Button GetMapButton(string btnLabel)
        {
            var _btn = new Button
            {
                Text = btnLabel,
                TextColor = _buttonTextColor,
                BackgroundColor = _buttonBacktroundColor
            };

            return _btn;
        }

        private async void _switchFollowMovement_Toggled(object sender, ToggledEventArgs e)
        {
            // if they're switching from off to on, then recenter the map based on their location
            if (_switchFollowMovement.IsToggled)
            {
                var _locator = CrossGeolocator.Current;
                var p = await _locator.GetPositionAsync(TimeSpan.FromSeconds(10000), null, false);
                //Position p = await _locator.GetPositionAsync(10000);

                if (p != null)
                {
                    MapSpan mapSpanNewCenter = MapSpan.FromCenterAndRadius(new MapPosition(p.Latitude, p.Longitude), _map.VisibleRegion.Radius);

                    _map.MoveToRegion(mapSpanNewCenter);
                }
                else
                    Console.WriteLine("position was null");
            }
        }

        protected void _locator_position_changed(object sender, PositionEventArgs posEventArgs)
        {
            //string sText = "position changed";
            if (_switchFollowMovement.IsToggled)
            {
                // the intention here is to allow the default initial location and zoom to happen before we start tracking location via our new mechanism
                // this ensures that the first zoom happens
                if (AppVariables.LastMapPosition != null)
                {
                    // let's recenter the map withouth changing any zoom...the users specifically asked for this
                    if (posEventArgs.Position != null)
                    {
                        if (_map.VisibleRegion != null)
                        {
                            //sText += string.Format(" {0},{1}, radius: {2} meters", posEventArgs.Position.Latitude, posEventArgs.Position.Longitude, _map.VisibleRegion.Radius.Meters.ToString());
                            MapSpan mapSpanNewCenter = MapSpan.FromCenterAndRadius(new MapPosition(posEventArgs.Position.Latitude, posEventArgs.Position.Longitude), _map.VisibleRegion.Radius);
                            Console.WriteLine("MJ - E");
                            _map.MoveToRegion(mapSpanNewCenter);
                            //sText += " after MoveToRegion";
                        }
                        else
                        {
                            //sText += " _map.VisibleRegion is null";
                        }
                    }
                    else
                    {
                        //sText += " position was null";
                        Console.WriteLine("position was null");
                    }
                }
            }
            else
            {
                //sText += " not tracking location";
            }
            //Console.WriteLine(sText);
        }

        private void loadControls()
        {

            StackLayout slContent = new StackLayout();
            slContent.BackgroundColor = _backGroundColor;

            // left side (message asnd indicator) is spinner and search results
            // right side is toggle button
            StackLayout stackUnderSearch = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                VerticalOptions = LayoutOptions.Center,
                Orientation = StackOrientation.Horizontal
            };

            StackLayout messageAndIndicator = new StackLayout
            {
                Padding = new Thickness(4, 0, 0, 0),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Horizontal
            };
            messageAndIndicator.Children.Add(_lblMessage);
            messageAndIndicator.Children.Add(_aiSearchingForMeters);

            StackLayout stackToggle = new StackLayout()
            {
                Padding = new Thickness(0, 0, 0, 0),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Orientation = StackOrientation.Horizontal
            };
            stackToggle.Children.Add(_lblFollowMovementLabel);
            stackToggle.Children.Add(_switchFollowMovement);

            stackUnderSearch.Children.Add(messageAndIndicator);
            stackUnderSearch.Children.Add(stackToggle);

            slContent.Children.Add(_sBar);
            slContent.Children.Add(stackUnderSearch); //  (messageAndIndicator);
            slContent.Children.Add(_lvSearchResults);

            slContent.Children.Add(_map);
            slContent.Children.Add(_aiMeterListLoading);
            slContent.Children.Add(_lvMeters);

            _slToggleView.Children.Add(_btnToggleView);

            _gridMapType.Children.Add(_btnHybrid, 0, 0);
            _gridMapType.Children.Add(_btnSatellite, 1, 0);
            _gridMapType.Children.Add(_btnStreet, 2, 0);
            _gridMapType.Children.Add(_slToggleView, 3, 0);

            slContent.Children.Add(_gridMapType);
            this.Content = slContent;

        }
        void _lvMeters_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            LocalMeter meter = (LocalMeter)_lvMeters.SelectedItem;
            NavigateToMeterDetailAsync(meter);
            //Navigation.PushAsync(new MeterDetailPage(meter));
        }

        private async void NavigateToMeterDetailAsync(LocalMeter localMeter)
        {
            if (localMeter == null)
            {
                return;
            }

            var meterDetailpage = Resolver.Resolve<MeterDetailPage>();

            var detailViewModel = meterDetailpage.BindingContext as MeterDetailViewModel;

            var meter = new Meter()
            {
                DeviceUtilityID = localMeter.DeviceUtilityID,
                DeviceUtilityIDWithLocation = localMeter.DeviceUtilityIDWithLocation,
                Distance = localMeter.Distance,
                CustomerContactNumber = localMeter.CustomerContactNumber,
                CustomerName = localMeter.CustomerName,
                CustomerAddress = localMeter.CustomerAddress,             
            };

            detailViewModel.MeterItem = meter;

            await detailViewModel.GetAllMeterInfo();

            var localSql = new LocalSql();

            await localSql.AddOrUpdateMeterLastAccessedTime(meter);

            await Navigation.PushAsync(meterDetailpage);
        }

        private void _btnToggleView_Clicked(object sender, EventArgs e)
        {
            if (_lvMeters.IsVisible)
            {
                _lvMeters.IsVisible = false;

                _btnToggleView.Text = "List";

                _btnStreet.IsVisible = _btnSatellite.IsVisible = _btnHybrid.IsVisible = true;

                if (_useLastPositionOnMapLoad)
                {
                    //_map.MoveToRegion(MapSpan.FromCenterAndRadius(_lastPosition, Distance.FromMiles(_lastRadius)));
                    //For whatever reason, moving the map to the region at this point doesn't work. Instead, get the same results
                    //by mimicking a search button pressed using the latest search text
                    _sBar.Text = _lastSearchText; //Set the current text to the last searched text in case they modified the text but didn't submit the search
                    _sBar_SearchButtonPressed(_sBar, null);
                }
                _map.IsVisible = true;
            }
            else
            {
                _map.IsVisible = false;


                //Since we are moving from map to listview reset to false; It will be set to true if a search is performed while the listView is visible.
                //That way when the map is reloaded we can move to the same location as was entered in the list view;
                _useLastPositionOnMapLoad = false;

                UpdateMeterListView();

                _btnToggleView.Text = "Map";

                _btnStreet.IsVisible = _btnSatellite.IsVisible = _btnHybrid.IsVisible = false;

                _lvMeters.IsVisible = true;
            }
        }

        private void Button_MapType_Clicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            MapType type = (MapType)btn.CommandParameter;

            if (type == MapType.Hybrid)
            {
                _map.MapType = MapType.Hybrid;
            }
            else if (type == MapType.Satellite)
            {
                _map.MapType = MapType.Satellite;
            }
            else if (type == MapType.Street)
            {
                _map.MapType = MapType.Street;
            }
        }

        private void UpdateMeterListView()
        {
            UpdateMeterListView(_map.VisibleRegion.Center, _map.VisibleRegion.Radius.Miles);
        }

        private async void UpdateMeterListView(MapPosition position, double radius)
        {
            _lvMeters.ItemsSource = null;

            if (radius <= AppVariables.GetMetersWithinXRadiusMaxValue)
            {
                _aiMeterListLoading.IsRunning = true;
                _aiMeterListLoading.IsVisible = true;
                try
                {
                    _btnToggleView.IsEnabled = false;
                    //We're using LocalMeters because we need access to "Calculated" properties. for the ListView Bindings DeviceUtilityIDAndCustomerName and DistanceAndCustomerAddress
                    //If xamarin ever allows for Complex Property bindings we can use the normal Meter class.
                    List<LocalMeter> meters = LocalMeter.GetListOfLocalMetersFromMeters(await _meterService.GetMetersWithinXRadiusAsync(position.Latitude, position.Longitude, radius));

                    //Remove any meters from the list that don't have the proper Meter Format (bad data or old meters recieved from UIQ)
                    SerialNumberFormatException fe;
                    for (int i = 0; i < meters.Count; i++)
                    {
                        if (!_meterService.IsValidSerialNumber(meters[i].DeviceUtilityID, out fe))
                        {
                            meters.RemoveAt(i);
                            i--;
                        }
                    }
                    _lvMeters.ItemsSource = meters;
                }
                catch (Exception e)
                {
                    if (AppHelper.ContainsAuthenticationRequiredException(e))
                    {
                        await vm.LoginRequired();
                        return;
                    }
                    //else if (AppHelper.ContainsApplicationMaintenance(e))
                    //{
                    //    this.ShowApplicationMaintenance();
                    //}
                    else
                    {
                        //AppLogger.LogError(e);

                        await DisplayAlert("Service Error", "Get Meters Within X Radius service call failed.", "Close");
                    }
                }
                finally
                {
                    _btnToggleView.IsEnabled = true;
                    _aiMeterListLoading.IsRunning = false;
                    _aiMeterListLoading.IsVisible = false;
                }

            }

        }

        private bool _mapIsBusy;
        private Plugin.Geolocator.Abstractions.Position _positionNotedWhileZooming;
        private List<KeyValuePair<Point, double>> _submittedCoords = new List<KeyValuePair<Point, double>>();
        private List<BoundingCoords> _submittedBounds = new List<BoundingCoords>();
        private BoundingCoords _latestBoundingCoords;
        private DateTime _lastMapMove;

        void _map_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            string zoomInMoreMessage = "Zoom in to see meters";

            if (_map.IsVisible)
            {
                if (e.PropertyName.Equals("VisibleRegion", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (_positionNotedWhileZooming != null)
                    {
                        MapSpan mapSpanNewCenter = MapSpan.FromCenterAndRadius(new MapPosition(_positionNotedWhileZooming.Latitude, _positionNotedWhileZooming.Longitude), _map.VisibleRegion.Radius);
                        Console.WriteLine("MJ - E");
                        _map.MoveToRegion(mapSpanNewCenter);

                        _positionNotedWhileZooming = null;
                    }
                    if (_map.VisibleRegion != null && _mapIsBusy == false)
                    {
                        //Cache our location
                        AppVariables.LastMapPosition = _map.VisibleRegion;

                        if (_map.VisibleRegion.Radius.Miles <= AppVariables.GetMetersWithinXRadiusMaxValue)
                        {
                            if (_lblMessage.Text == zoomInMoreMessage)
                                _lblMessage.Text = string.Empty;

                            try
                            {

                                BoundingCoords bounds = AppHelper.GetBoundingCoords(_map.VisibleRegion);
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
                                    GetPinsForMap();
                                }

                            }//End try
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
                            _lblMessage.Text = zoomInMoreMessage;
                        }
                    }//End Visible Region is good
                }//end it's the Property we care about
            }//End Map Is visible

        }//end _map_property_Changed

        bool _getPinsForMapIsRunning = false;

        private async void GetPinsForMap()
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
                    _btnToggleView.IsEnabled = false;
                    ToggleMeterLoadingActivityIndicator(true);
                    _mapIsBusy = true;

                    //List<Meter> meters = await vm.GetMetersWithinBoxBoundsAsync(_latestBoundingCoords);
                    List<Meter> meters = await _meterService.GetMetersWithinBoxBoundsAsync(_latestBoundingCoords);
                    
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
                    //else if (AppHelper.ContainsApplicationMaintenance(e))
                    //{
                    //    this.ShowApplicationMaintenance();
                    //}
                    else
                    {
                        ex = e;
                        //this.LogApplicationError("SearchPage.GetPinsForMap", ex);
                        await DisplayAlert("Service Error", "Get Meters Within Map Bounds service call failed.", "Close");
                    }
                }
                finally
                {
                    _btnToggleView.IsEnabled = true;
                    ToggleMeterLoadingActivityIndicator(false);
                    _mapIsBusy = false;
                    _getPinsForMapIsRunning = false;
                }

                if (ex != null)
                {
                    _lblMessage.TextColor = Color.Red;
                    _lblMessage.Text = ex.Message;
                    await AppHelper.FadeOutLabelByEmptyString(_lblMessage, 2500);
                    _lblMessage.TextColor = _textColor;
                }
                if (meterFoundCount.HasValue)
                {
                    _lblMessage.Text = String.Format("{0} Meters Found", meterFoundCount.Value.ToString());
                    await AppHelper.FadeOutLabelByEmptyString(_lblMessage);
                }

            }
        }

        protected void ToggleMeterLoadingActivityIndicator(bool on)
        {
            if (on)
            {
                _lblMessage.IsVisible = false;
                _aiSearchingForMeters.IsVisible = true;
                _aiSearchingForMeters.IsRunning = true;
            }
            else
            {
                _lblMessage.IsVisible = true;
                _aiSearchingForMeters.IsVisible = false;
                _aiSearchingForMeters.IsRunning = false;
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
                        if (_meterService.IsValidSerialNumber(meter.DeviceUtilityID, out fe))
                        {
                            //Add it only if we haven't already added it.
                            if (!_existingMapPinHash.Contains(meter.DeviceUtilityID)) // _map.Pins.FirstOrDefault(x => x.Label.StartsWith(meter.DeviceUtilityID)) == null)
                            {
                                //Add the ID to our hashset so we can quickly search in the future if it already exists
                                _existingMapPinHash.Add(meter.DeviceUtilityID);

                                Pin pin = new Pin
                                {
                                    Type = PinType.Place,
                                    Position = new MapPosition(Convert.ToDouble(meter.Latitude), Convert.ToDouble(meter.Longitude)),
                                    Label = _meterService.GetCustomerNameAndDeviceUtilityID(meter),
                                    Address = meter.CustomerAddress
                                };
                                pin.SetValue(_bpDeviceUtilityID, meter.DeviceUtilityID);
                                pin.Clicked += PinClicked;
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    _map.Pins.Add(pin);
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //AppLogger.LogError(ex);
                    }
                }

            });
        }

        private void PinClicked(object sender, EventArgs e)
        {
            // Navigation.PushAsync(new MeterDetailPage(((Pin)sender).Label.Substring(0,10)));
            string deviceUtilityID = ((Pin)sender).GetValue(_bpDeviceUtilityID).ToString();
            //Navigation.PushAsync(new MeterDetailPage(deviceUtilityID));
            NavigateToMeterDetailAsync(deviceUtilityID);
        }

        private async void NavigateToMeterDetailAsync(string deviceUtilityID)
        {
            if (deviceUtilityID == null)
            {
                return;
            }

            var meterDetailpage = Resolver.Resolve<MeterDetailPage>();

            var detailViewModel = meterDetailpage.BindingContext as MeterDetailViewModel;

            var meter = await _meterService.GetMeterByDeviceUtilityIDAsync(deviceUtilityID);

            detailViewModel.MeterItem = meter;

            await detailViewModel.GetAllMeterInfo();

            var localSql = new LocalSql();

            await localSql.AddOrUpdateMeterLastAccessedTime(meter);

            await Navigation.PushAsync(meterDetailpage);
        }

        public async void SetMapToDefaultPosition()
        {
            try
            {
                //If we haven't cached our last location, go ahead and set it to the user's current position
                if (AppVariables.LastMapPosition == null)
                {
                    //Xamarin.Geolocation.Position p = await _locator.GetPositionAsync(10000);

                    var _locator = CrossGeolocator.Current;
                    var p= await _locator.GetPositionAsync(TimeSpan.FromSeconds(10000), null, false);

                    if (p != null)
                    {
                        Console.WriteLine("MJ - A");
                        _map.MoveToRegion(MapSpan.FromCenterAndRadius(new MapPosition(p.Latitude, p.Longitude), Distance.FromMiles(.05)));
                    }
                    else
                        Console.WriteLine("position was null");
                }
                else
                {
                    Console.WriteLine("MJ - B");
                    _map.MoveToRegion(AppVariables.LastMapPosition);
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(System.Threading.Tasks.TaskCanceledException) || (e.InnerException != null && e.InnerException.GetType() == typeof(System.Threading.Tasks.TaskCanceledException)))
                {
                    //Set it to Chicago
                    Console.WriteLine("Locator.GetPositionAsync(10000) timed out");
                    Console.WriteLine("MJ - C");
                    _map.MoveToRegion(MapSpan.FromCenterAndRadius(new MapPosition(41.881832, -87.623177), Distance.FromMiles(5)));
                }
                //this.LogApplicationError("SearchPage.SetMapToDefaultPosition", e);
            }

            try
            {
                _map.IsShowingUser = true;
            }
            catch (Exception)
            {
                _map.IsShowingUser = false;
            }

        }

        protected async void _searchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            hideSearchResults();
            _isSearchResultSelection = true;
            string text = _lvSearchResults.SelectedItem.ToString();
            _sBar.Text = text;
            await AppHelper.FadeOutLabelByEmptyString(_lblMessage, AppVariables.DefaultFadeMs);
            _sBar_SearchButtonPressed(_sBar, null);
        }

        protected void _sBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            SearchBar sb = sender as SearchBar;
            StackLayout sl = this.Content as StackLayout;

            if (!string.IsNullOrWhiteSpace(sb.Text))
            {
                string searchText = sb.Text;


                List<string> matchingResults = (from zip
                                               in AppVariables.ILZipCodes.ZipCodes
                                                where zip.StartsWith(searchText)
                                                select zip).ToList();

                if (matchingResults.Count > 0)
                {
                    _searchResults.ItemsSource = matchingResults;
                    _searchResults.IsVisible = true;
                }
                else if (searchText.Length > 4 && !_isSearchBusy)
                {
                    try
                    {
                        _isSearchBusy = true;

                        string searchScope = " ILLINOIS, USA";
                        searchText += searchScope;

                        var approximateLocation = await _geoCoder.GetPositionsForAddressAsync(searchText);

                        List<string> possibleAddresses = new List<string>();
                        MapPosition position = new MapPosition();
                        List<Task<IEnumerable<string>>> locationAddresses = new List<Task<IEnumerable<string>>>();
                        int maxLocationsToCheck = 5;
                        int locationCount = 0;
                        foreach (var p in approximateLocation)
                        {
                            locationCount += 1;
                            if (locationCount > maxLocationsToCheck)
                                break;

                            position = new MapPosition(p.Latitude, p.Longitude);
                            locationAddresses.Add(_geoCoder.GetAddressesForPositionAsync(position));
                        }

                        try
                        {
                            await Task.WhenAll(locationAddresses);

                            if (locationAddresses.Count > 0)
                            {
                                for (int i = 0; i < locationAddresses.Count; i++)
                                {
                                    possibleAddresses.AddRange(locationAddresses[i].Result);
                                }
                            }

                            if (possibleAddresses.Count > 0)
                            {
                                _searchResults.ItemsSource = possibleAddresses;
                                _searchResults.IsVisible = true;
                            }
                        }
                        catch (AggregateException ae)
                        {
                            _lblMessage.Text = ae.InnerExceptions[0].Message;
                            AppHelper.FadeOutLabelByEmptyString(_lblMessage, 5000);
                        }

                    }
                    catch (Exception ex)
                    {
                        _lblMessage.Text = ex.Message;
                        AppHelper.FadeOutLabelByEmptyString(_lblMessage, 5000);
                    }
                    finally
                    {
                        _isSearchBusy = false;
                    }
                }//End if SearchText > 4 and Search is not busy
                else
                {
                    hideSearchResults();
                }
            }
            else
            {
                hideSearchResults();
            }
            */
        }

        /// <summary>
        /// Hides the SearchResultListView
        /// Note: giving it it's own method in case we want to do animation later on.
        /// </summary>
        protected void hideSearchResults()
        {
            //await _searchResults.FadeTo(0, 750, null);
            _lvSearchResults.IsVisible = false;
            //_searchResults.Opacity = 1;
        }

        string _lastSearchText;

        private bool _useLastPositionOnMapLoad;

        protected async void _sBar_SearchButtonPressed(object sender, EventArgs e)
        {
            hideSearchResults();
            try
            {
                if (!string.IsNullOrWhiteSpace(_sBar.Text))
                {

                    string searchText = _sBar.Text.Trim();
                    double radius = .05; //small for exact location or meter
                    int temp;
                    //If we're prety sure it's a zipcode
                    if (searchText.Length == 5 && Int32.TryParse(searchText, out temp))
                    {
                        radius = 3;
                        _lblMessage.Text = "Searching zipcode: " + searchText;
                    }
                    else
                    {
                        _lblMessage.Text = "Searching for: " + searchText;
                    }
                    await AppHelper.FadeOutLabelByEmptyString(_lblMessage, 3000);

                    MapPosition position = new MapPosition();

                    var approximateLocation = (await _geoCoder.GetPositionsForAddressAsync(searchText)).ToList();

                    List<string> possibleAddresses = new List<string>();

                    if (approximateLocation.Count > 0)
                    {

                        position = approximateLocation[0];

                        if (_lvMeters.IsVisible)
                        {
                            UpdateMeterListView(position, radius);

                            _useLastPositionOnMapLoad = true;
                            _lastSearchText = _sBar.Text;
                        }
                        else if (_map.IsVisible)
                        {
                            Console.WriteLine("MJ - D");
                            _map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(radius)));
                        }


                    }
                    else
                    {
                        _lblMessage.Text = "Address not found, please try again";
                        await AppHelper.FadeOutLabelByEmptyString(_lblMessage, 10000);
                        hideSearchResults();
                    }
                }
            }
            catch (Exception ex)
            {
                _lblMessage.Text = ex.Message;
                await AppHelper.FadeOutLabelByEmptyString(_lblMessage, 10000);
            }
        }
    }
}



