using System;
using System.Collections.Generic;

namespace CompassMobileUpdate
{
	public static class AppVariables
	{
		static bool isInitialized = false;
        static int _defaultFadeMs;
        static DateTimeOffset _startTime;
        //static List<VoltageRule> _voltageRules = new List<VoltageRule>();

        public static String MilitaryFormatStringShort
        {
            get { return "MMM dd HH:mm"; }
        }
    }
}

