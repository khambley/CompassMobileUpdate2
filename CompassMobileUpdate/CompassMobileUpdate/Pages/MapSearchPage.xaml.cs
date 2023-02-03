using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace CompassMobileUpdate.Pages
{	
	public partial class MapSearchPage : ContentPage
	{
        private double width;
        private double height;

        public MapSearchPage (MapSearchViewModel viewModel)
		{
			InitializeComponent ();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            // TODO: Add custom background effect for searchbar on ios.
            // https://stackoverflow.com/questions/67581008/how-to-change-the-color-of-searchbar-search-icon-and-cancel-button-color-in-xama

            meterMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(41.8500, -87.6500), Distance.FromMiles(5)));

        }

    }
}

