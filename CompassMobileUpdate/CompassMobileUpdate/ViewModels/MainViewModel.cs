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
    }
}

