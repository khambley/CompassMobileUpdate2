using System;
namespace CompassMobileUpdate.Models
{
    public class Activity
    {
        public int ID { get; set; }
        public ActivityType Type { get; set; }
        public string Tag { get; set; }
        public string UserID { get; set; }
    }

    public enum ActivityType
    {
        FEEDER,
        METER,
        OUTAGE,
        SSO,
        TRANSFORMER
    }
}

