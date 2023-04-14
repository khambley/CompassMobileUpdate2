﻿using System;
using System.Collections.Generic;
using System.Linq;
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

            meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(41.8500, -87.6500), Distance.FromMiles(5)));

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

                    //If we're prety sure it's a zipcode
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

