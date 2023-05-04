using System;
using static CompassMobileUpdate.Models.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CompassMobileUpdate.Models
{
    [DataContract]
    [Serializable]
    public class AvailabilityEvent : IEqualityComparer<AvailabilityEvent>
    {

        #region Public Properties
        [DataMember]
        public DateTimeOffset EventTime { get; set; }
        [DataMember]
        public MeterAvailabilityEventsEventType EventType { get; set; }
        #endregion //Public Properties

        #region Constructors
        public AvailabilityEvent()
        {

        }
        public AvailabilityEvent(MeterAvailabilityEventsEventType eventType)
        {
            EventType = eventType;
        }

        public AvailabilityEvent(MeterAvailabilityEventsEventType eventType, DateTimeOffset eventTime)
        {
            EventType = eventType;
            EventTime = eventTime;
        }
        #endregion

        #region EqualityComparerImplementation
        public static bool operator ==(AvailabilityEvent event1, AvailabilityEvent event2)
        {
            if (event1.EventTime == event2.EventTime)
            {
                if (event1.EventType == event2.EventType)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool operator !=(AvailabilityEvent event1, AvailabilityEvent event2)
        {
            return !(event1 == event2);
        }

        public override bool Equals(object obj)
        {
            if (obj is AvailabilityEvent)
            {
                return this == ((AvailabilityEvent)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (object.ReferenceEquals(this, null))
                return 0;

            int eventTimeHashCode = this.EventTime.GetHashCode();
            int eventTypeHashCode = this.EventType.GetHashCode();

            return eventTimeHashCode ^ eventTypeHashCode;
        }

        public int GetHashCode(AvailabilityEvent event1)
        {
            return event1.GetHashCode();
        }

        public bool Equals(AvailabilityEvent event1, AvailabilityEvent event2)
        {
            return event1 == event2;
        }
        #endregion //EqualityComparerImplementation

    }//End AvailabilityEvent
}

