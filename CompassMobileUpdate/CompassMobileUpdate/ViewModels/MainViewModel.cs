using System;
using System.Windows.Input;
using CompassMobileUpdate.Pages;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand GoToMeterSearchPageCommand => new Command(async () =>
        {
            var meterSearchPage = Resolver.Resolve<MeterSearchPage>();
            await Navigation.PushAsync(meterSearchPage);
        });

        public ICommand GoToMapSearchPageCommand => new Command(async () =>
        {
            var mapSearchPage = Resolver.Resolve<MapSearchPage>();
            await Navigation.PushAsync(Resolver.Resolve<MapPage>());
        });

        public ICommand GoToScannerPageCommand => new Command(async () =>
        {
            var scannerPage = Resolver.Resolve<ScannerPage>();
            await Navigation.PushAsync(scannerPage);
        });
    }
}

