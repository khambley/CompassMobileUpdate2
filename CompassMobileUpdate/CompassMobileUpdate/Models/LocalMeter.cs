using System;
using SQLite;
namespace CompassMobileUpdate.Models
{
    public class LocalMeter
    {
        public const string PrimaryKeyPropertyName = "DeviceUtilityID";
        [PrimaryKey, Collation("NOCASE")]
        public string DeviceUtilityID { get; set; }
        public string DeviceUtilityIDWithLocation { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerContactNumber { get; set; }
        public double? Distance { get; set; }
        [Indexed]
        public bool IsFavorite { get; set; }

        public DateTime CreatedTime { get; set; }
        [Indexed]
        public DateTime LastAccessedTime { get; set; }
        [Indexed]
        public DateTime LastUpdatedTime { get; set; }

        public string CustomerNameAndDeviceUtilityId
        {
            get
            {
                return CustomerName + " - " + DeviceUtilityID;
            }
        }

        public void ConvertCompassMeterToLocalMeter(Meter meter)
        {
            this.DeviceUtilityID = meter.DeviceUtilityID;
            this.CustomerName = meter.CustomerName;
            this.CustomerAddress = meter.CustomerAddress;
            this.CustomerContactNumber = meter.CustomerContactNumber;
            this.DeviceUtilityIDWithLocation = meter.DeviceUtilityIDWithLocation;
            this.Distance = meter.Distance;
        }

        public static LocalMeter GetLocalMeterFromMeter(Meter meter)
        {
            LocalMeter local = new LocalMeter();
            local.ConvertCompassMeterToLocalMeter(meter);

            return local;
        }
    }
}

