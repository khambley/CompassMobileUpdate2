using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.ViewModels;

using Xamarin.Forms;

namespace CompassMobileUpdate.Pages
{	
	public partial class MeterSearchPage : BasePage
	{
        private double width;
        private double height;
 
        public MeterSearchPage(MeterSearchViewModel viewModel)
		{
			InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            sBar.WidthRequest = width - scanButton.Width - 10;
        }
    }
}

