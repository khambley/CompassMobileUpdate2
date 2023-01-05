using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
	public class MeterSearchViewModel : ViewModelBase
	{
		public bool CustomerResultsIsVisible { get; set; }

        public ObservableCollection<LocalMeter> LocalMeters { get; set; }

		public MeterSearchViewModel()
		{
            CustomerResultsIsVisible = false;
            LocalMeters = new ObservableCollection<LocalMeter>();
			Task.Run(async () => await LoadData());
		}
        public ICommand PerformSearch => new Command(() =>
        {
            PerformCustomerSearch();
        });

        private async void PerformCustomerSearch()
        {
            //popup for testing purposes
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    App.Current.MainPage.DisplayAlert("PerformCustomerSearch method test", "The PerformCustomerSearch method was called successfully", "OK");
            //});
            await Navigation.PushAsync(Resolver.Resolve<MeterDetailPage>());
        }

        private async Task LoadData()
		{
            LocalMeters.Add(new LocalMeter
            {
                DeviceUtilityID = "G230296247",
                DeviceUtilityIDWithLocation = "G230296247 ON",
                CustomerName = "Baxter Credit Union",
                CustomerAddress = "300 N Milwaukee Ave Vernon Hills IL 60061"
            });
            LocalMeters.Add(new LocalMeter
            {
                DeviceUtilityID = "G270280650",
                DeviceUtilityIDWithLocation = "G270280650 ON",
                CustomerName = "ENRIQUE LOPEZ",
                CustomerAddress = "5931 S MOODY AVE CHICAGO IL 60638"
            });
        }


	}
}

