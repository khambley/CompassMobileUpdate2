using System;
namespace CompassMobileUpdate.Models
{
	public class Enums
	{
        public enum AppEnvironment
        {
            Integration,
            Stage,
            Production
        }
        public enum CustomerClassType
        {
            Unknown,
            Residential,
            Commercial
        }
        public enum Manufacturers
        {
            Elster,
            GE,
            LandG,
            Unknown
        }
        public enum MeterAvailabilityEventsEventType
        {
            Restorations = 100007,
            Outages = 12007
        }

    }
}

