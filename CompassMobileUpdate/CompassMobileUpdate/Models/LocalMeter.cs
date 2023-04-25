using System;
using System.Collections.Generic;
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
        
        public string IsFavoriteImage
        {
            get
            {
                if (IsFavorite)
                {
                    return "star_yellow.png";
                }
                else
                {
                    return "star_gray.png";
                }
            }
        }
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
        public static List<LocalMeter> GetListOfLocalMetersFromMeters(List<Meter> meters)
        {
            List<LocalMeter> localMeters = new List<LocalMeter>();
            for (int i = 0; i < meters.Count; i++)
            {
                localMeters.Add(LocalMeter.GetLocalMeterFromMeter(meters[i]));
            }

            return localMeters;
        }
    }
}

