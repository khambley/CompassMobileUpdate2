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
                return CustomerName + " " + DeviceUtilityID;
            }
        }
    }
}

