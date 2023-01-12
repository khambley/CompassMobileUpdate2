using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
	public class MapSearchViewModel : ViewModelBase
	{
		public ICommand PerformSearch => new Command(async () =>
		{
            //popup for testing purposes
            Device.BeginInvokeOnMainThread(() =>
            {
                App.Current.MainPage.DisplayAlert("PerformMapSearch method test", "The PerformCustomerSearch method was called successfully", "OK");
            });
        });
	}
}

