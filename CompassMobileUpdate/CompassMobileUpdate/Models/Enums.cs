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

        public enum Company
        {
            ComEd = 1,
            PECO = 2,
            BGE = 3,
            PHI = 4
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

