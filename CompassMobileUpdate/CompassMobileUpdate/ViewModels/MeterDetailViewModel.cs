using System;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Services;

namespace CompassMobileUpdate.ViewModels
{
	public class MeterDetailViewModel : ViewModelBase
	{
        private readonly MeterService _meterService;

        public Meter MeterItem { get; set; }

        public string MeterTypeNumber { get; set; }
		
		public MeterDetailViewModel(MeterService meterService)
		{
            _meterService = meterService;
        }
    }
}

