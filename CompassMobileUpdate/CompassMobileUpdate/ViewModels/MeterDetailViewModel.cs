using System;
using System.Threading.Tasks;
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

        public bool IsEnabledCheckStatusButton { get; set; }

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

        public ICommand CheckStatusButtonCommand => new Command(async () =>
        {
            await LoadMeterData();
        });

        public MeterDetailViewModel(MeterService meterService)
		{
            _meterService = meterService;
            IsEnabledCheckStatusButton = true;
        }

        private async Task LoadMeterData()
        {
            IsEnabledCheckStatusButton = false;
            var meter = await _meterService.GetMeterByDeviceUtilityIDAsync(MeterItem.DeviceUtilityID);
            if (meter != null)
            {
                MeterItem = meter;
                // set MeterTypeNumber on MeterDetail
                if (string.IsNullOrWhiteSpace(meter.DeviceUtilityIDWithLocation))
                {
                    MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityID;
                }
                else
                {
                    MeterTypeNumber = meter.ManufacturerName + " Meter #" + meter.DeviceUtilityIDWithLocation;
                }

                // set Meter Attributes on MeterDetail - which includes Meter State (MeterAttributes.Status)
                MeterAttributes = await _meterService.GetMeterAttributesAsync(meter);

                //set Last Comm on MeterDetail
                StatusDate = MeterAttributes.StatusDate.ToLocalTime().ToString(AppVariables.MilitaryFormatStringShort);

                IsEnabledCheckStatusButton = true;
            }
        }       
    }
}

