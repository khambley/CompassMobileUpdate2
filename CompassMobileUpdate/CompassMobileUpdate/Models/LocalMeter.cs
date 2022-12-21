using System;
namespace CompassMobileUpdate.Models
{
	public class LocalMeter
	{
        public string DeviceUtilityID { get; set; }
        public string DeviceUtilityIDWithLocation { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerContactNumber { get; set; }
        public double? Distance { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastAccessedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}

