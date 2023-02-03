using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
	public class MapSearchViewModel : ViewModelBase
	{
       public bool IsVisibleMessage { get; set; }

        public bool IsVisibleSearchResults { get; set; }

        public bool IsVisibleListViewMeters { get; set; }

        public MapSearchViewModel()
        {
            IsVisibleMessage = false;
            IsVisibleSearchResults = false;
            IsVisibleListViewMeters = false;
        }

		public ICommand PerformSearch => new Command(async () =>
		{
            IsBusy = true;

            // TODO: Remove this when API service is implemented.
            await Task.Delay(3000);

            IsBusy = false;
            IsVisibleMessage = true;

            //popup for testing purposes
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    App.Current.MainPage.DisplayAlert("PerformMapSearch method test", "The PerformCustomerSearch method was called successfully", "OK");
            //});
        });
	}
}

