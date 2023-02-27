using System;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.Models
{
	public class Meter
	{
        public string MacID { get; set; }
        public string DeviceSSNID { get; set; }
        public string DeviceUtilityID { get; set; }
        public string DeviceUtilityIDWithLocation { get; set; }
        public string ManufacturerName { get; set; }
        public Decimal? Latitude { get; set; }
        public Decimal? Longitude { get; set; }
        public double? Distance { get; set; }
        public CustomerClassType CustomerClassType { get; set; }
        public string CustomerContactNumber { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerClass { get; set; }
        public string Form { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string Version { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public DateTimeOffset StatusDate { get; set; }
        public string NICSoftwareVersion { get; set; }
        public string NICSoftwareRevision { get; set; }
        public string NICSoftwarePatch { get; set; }
        public string CustomerNameAndDeviceUtilityId
        {
            get
            {
                return CustomerName + " - " + DeviceUtilityID;
            }
        }
    }
}

