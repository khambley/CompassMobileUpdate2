using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.PlatformConfiguration.GTKSpecific;

namespace CompassMobileUpdate.Pages
{	
	public partial class MapSearchPage : ContentPage
	{
        private double width;
        private double height;
        private Geocoder geoCoder;

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

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

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
                        meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(position.Latitude, position.Longitude), Distance.FromMiles(.05)));
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
                    meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(41.881832, -87.623177), Distance.FromMiles(5)));
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

                    Position position = new Position();

                    var approximateLocation = (await geoCoder.GetPositionsForAddressAsync(searchText)).ToList();

                    var possibleAddresses = new List<string>();

                    if (approximateLocation.Count > 0)
                    {
                        position = approximateLocation[0];

                        if (lvMeters.IsVisible)
                        {
                            UpdateMeterListView(position, radius);
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private async void UpdateMeterListView(Position position, double radius)
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
                }
            }
        }
    }
}

