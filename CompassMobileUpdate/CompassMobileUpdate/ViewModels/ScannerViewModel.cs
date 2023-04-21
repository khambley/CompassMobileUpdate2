using System;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Services;

namespace CompassMobileUpdate.ViewModels
{
	public class ScannerViewModel : ViewModelBase
	{
        private readonly MeterService _meterService;

        public ScannerViewModel(MeterService meterService)
		{
            _meterService = meterService;
        }

        public bool IsValidSerialNumber(string serialNumber, out SerialNumberFormatException ex)
        {
            return _meterService.IsValidSerialNumber(serialNumber, out ex);
        }

        public async Task<Meter> GetMeterByDeviceUtilityIDAsync(string deviceUtilityID)
        {
            return await _meterService.GetMeterByDeviceUtilityIDAsync(deviceUtilityID);
        }
    }
}

