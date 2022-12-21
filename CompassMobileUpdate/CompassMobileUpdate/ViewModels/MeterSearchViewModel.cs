using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;

namespace CompassMobileUpdate.ViewModels
{
	public class MeterSearchViewModel : ViewModelBase
	{
		public bool LocalMetersIsVisible { get; set; }
        public ObservableCollection<LocalMeter> LocalMeters { get; set; }

		public MeterSearchViewModel()
		{
            LocalMetersIsVisible = true;
            LocalMeters = new ObservableCollection<LocalMeter>();
			Task.Run(async () => await LoadData());
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

