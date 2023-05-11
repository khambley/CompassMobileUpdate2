using System;
using System.Collections.Generic;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.Models
{
	public class MeterAvailabilityEventsResponse
	{
        public MeterAvailabilityEventsEventType EventType { get; set; }
        public List<AvailabilityEvent> Events { get; set; }

        public MeterAvailabilityEventsResponse()
        {
            Events = new List<AvailabilityEvent>();
        }
    }
}

