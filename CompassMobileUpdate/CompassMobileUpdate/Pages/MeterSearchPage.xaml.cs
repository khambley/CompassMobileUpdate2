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
        bool _isFirstPageLoad = true;
 
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

        protected async override void OnAppearing()
        {
            var vm = this.BindingContext as MeterSearchViewModel;
            await vm.BindRecentMetersAsync(); 

            if (vm.RecentMeters.Count > 0)
            {
                // show recent meters
                vm.IsVisibleRecentMetersList = true;
                vm.IsVisibleRecentMetersLabel = true;

                // hide search results
                vm.IsVisibleCustomerResults = false;
                vm.IsVisibleCustomerSearch = false;             
            }
            base.OnAppearing();
        }

        void lvRecentMetersList_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            LocalMeter localMeter = e.SelectedItem as LocalMeter;
            var vm = this.BindingContext as MeterSearchViewModel;
            
            vm.IsFavorite = localMeter.IsFavorite;

            var meter = new Meter
            {
               DeviceUtilityID = localMeter.DeviceUtilityID,
               CustomerName = localMeter.CustomerName,
               CustomerAddress = localMeter.CustomerAddress,
               CustomerContactNumber = localMeter.CustomerContactNumber,
               Distance = localMeter.Distance              
            };

            vm.SelectedRecentMeterItem = meter;
        }
    }
}

