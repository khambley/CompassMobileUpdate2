using System;
using System.Collections.Generic;
using System.Linq;
using CompassMobileUpdate.Helpers;
using CompassMobileUpdate.Models;
using Xamarin.Forms;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.Pages.Views
{
    class AvailabilityEventsView
    {
        private List<AvailabilityEventView> _availabilityEvents;
        public List<AvailabilityEventView> AvailabilityEvents { get { return _availabilityEvents; } }
        public AvailabilityEventsView(List<AvailabilityEvent> events)
        {
            _availabilityEvents = new List<AvailabilityEventView>();
            if (events != null)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    _availabilityEvents.Add(new AvailabilityEventView(events[i].EventTime, events[i].EventType));
                }

                //Sort them desc
                //Comparing via b.compareto(a) sorts desc, ascending would be a.compareto(b)
                _availabilityEvents.Sort((a, b) => b.EventTime.CompareTo(a.EventTime));
            }
        }

        public List<String> GetDistinctEventTypes()
        {
            return _availabilityEvents.Select(x => x.EventType.ToString()).Distinct().ToList();
        }
        public int EventTypeCount
        {
            get { return GetDistinctEventTypes().Count; }
        }
    }

    public class AvailabilityEventView
    {
        public const string EventTypeField = "EventType";
        public const string EventTimeField = "EventTime";
        public const string DataColorField = "EventTypeColor";
        public const string EventTypeCapitalFirstLetterField = "EventTypeCapitalFirstLetter";
        public AvailabilityEventView(DateTimeOffset eventTime, MeterAvailabilityEventsEventType eventType)
        {
            this.EventTime = AppHelper.GetConfiguredTimeZone(eventTime).ToLocalTime().DateTime;
            this.EventType = eventType;
        }
        public DateTime EventTime { get; set; }
        public MeterAvailabilityEventsEventType EventType { get; set; }

        public Xamarin.Forms.Color EventTypeColor
        {
            get
            {
                if (EventType == MeterAvailabilityEventsEventType.Outages)
                    return Xamarin.Forms.Color.Red;
                else
                    return Xamarin.Forms.Color.Green;
            }
        }

        public string EventTypeCapitalFirstLetter
        {
            get { return EventType.ToString().Substring(0, 1).ToUpper(); }
        }
    }
}


