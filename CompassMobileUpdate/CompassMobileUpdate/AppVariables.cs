using System;
using System.Collections.Generic;
using CompassMobileUpdate.Models;

namespace CompassMobileUpdate
{
	public static class AppVariables
	{
		static bool isInitialized = false;
        static int _defaultFadeMs;
        static DateTimeOffset _startTime;
        public static string TimeZoneID { get; private set; }
        public static AppUser User { get; set; }
        //static List<VoltageRule> _voltageRules = new List<VoltageRule>();

        public static DateTimeOffset GetConfiguredTimeZone(DateTimeOffset dto)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dto, AppVariables.TimeZoneID);
        }

        public static String MilitaryFormatStringShort
        {
            get { return "MMM dd HH:mm"; }
        }

        #region AMISpecificVariables

        public const String SourceForAMICalls = "COMPASS Mobile";
        public const String Compass_AMI_GetMeterAttributes_RequestNoun = "MeterOutages";
        public const String Compass_AMI_GetMeterStatus_RequestNoun = "MeterStatus";
        public const String Compass_AMI_GetMeterReads_RequestNoun = "MeterReads";
        public const String Compass_AMI_GetMeterReads_RequestReadingTypeName = "JOB_OP_LP_READ";
        public const String Compass_AMI_GetMeterAvailabilityEvents_RequestNoun = "MeterAvailabilityEvents";
        public const int COMPASS_AMI_GetMeterAvailabilityEvents_OutageAndRestoreLookBackInMinutes = 44640;
        public const string COMPASS_AMI_GetMeterAvailabilityEvents_OptionName = "NumberRows";
        public const string COMPASS_AMI_COMPASS_AMI_GetMeterAvailabilityEvents_OptionValue = "100";

        public static List<string> Compass_AMI_GetMeterReads_ResponseVoltageTypes
        {
            get
            {
                return new List<string>
                {
                    Compass_AMI_GetMeterReads_MeterPhaseAVoltageName,
                    Compass_AMI_GetMeterReads_MeterPhaseBVoltageName,
                    Compass_AMI_GetMeterReads_MeterPhaseCVoltageName
                };
            }
        }
        public const string Compass_AMI_GetMeterReads_MeterPhaseAVoltageName = "phase a voltage";
        public const string Compass_AMI_GetMeterReads_MeterPhaseBVoltageName = "phase b voltage";
        public const string Compass_AMI_GetMeterReads_MeterPhaseCVoltageName = "phase c voltage";

        #endregion //end AMI Specific Variables
    }
}

