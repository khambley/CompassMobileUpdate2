using System;
using System.Collections.Generic;
using CompassMobileUpdate.ViewModels;
using Xamarin.Forms;

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
        }

    }
}

