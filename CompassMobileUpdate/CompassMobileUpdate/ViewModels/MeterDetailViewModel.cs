using System;
using System.Windows.Input;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Pages;
using CompassMobileUpdate.Services;
using Xamarin.Forms;

namespace CompassMobileUpdate.ViewModels
{
	public class MeterDetailViewModel : ViewModelBase
	{
        private readonly MeterService _meterService;

        public bool IsVisibleMeterStateIndicator { get; set; }

        public bool IsRunningMeterStateIndicator { get; set; }

        public Meter MeterItem { get; set; }

        public string MeterTypeNumber { get; set; }

        public MeterAttributesResponse MeterAttributes { get; set; }

        public string StatusDate { get; set; }

        public string CustomerContactNumber { get; set; }

        public ICommand TapOutageRestoreCommand => new Command(async () =>
        {
            var availabilityEventsPage = Resolver.Resolve<AvailabilityEventsPage>();
            await Navigation.PushAsync(availabilityEventsPage);
        });
		
		public MeterDetailViewModel(MeterService meterService)
		{
            _meterService = meterService;
        }
        protected void StartAllIndicators()
        {
            IsVisibleMeterStateIndicator = true;
            IsRunningMeterStateIndicator = true;
        }
        protected void StopAllIndicators()
        {
            IsVisibleMeterStateIndicator = false;
            IsRunningMeterStateIndicator = false;
        }
    }
}

