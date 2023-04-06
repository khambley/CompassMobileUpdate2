using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompassMobileUpdate.Models;
using CompassMobileUpdate.Services;

namespace CompassMobileUpdate
{
    using static CompassMobileUpdate.Models.Enums;
    using VoltageRule = LocalVoltageRule;

    public static class AppVariables
	{
		static bool isInitialized = false;
        static int _defaultFadeMs;
        //static LocalSql _localSql;

        public static App Application { get; set; }

        public static AppEnvironment AppEnvironment { get; set; }

        public static MeterService MeterService { get; set; }

        public static string JWTSymmetricKey = "790A5F23BAF44D8851034DF88D1597FBDB6BB716BA029733077C55EB44C6EA58E399943BACEB0D3DA4EF2FCE1D7769B0DD2C5FF8AB4054567616DFBBB4DB792F229473F6FAD716C2296606FC93064D8F875BF08C2979052DF3B282A0EC34B731ECFB88F2290FD2F34702B919DFA241024C034166D33A31878AACD8A9D7604E69";

        public static byte[] JWTSecretKey { get { return ASCIIEncoding.ASCII.GetBytes(JWTSymmetricKey); }}

        public static LocalSql LocalAppSql { get; set; }

        public static string MeterNotFound = "Meter not found";
        public static string MeterCustomerNotFound = "Customer not found";
        public static string MeterAttributesNotFound = "Meter attributes not found";

        public static String MilitaryFormatStringShort
        {
            get { return "MMM dd HH:mm"; }
        }

        public static String MilitaryFormatStringNoSeconds
        {
            get { return "MM/dd/yyyy HH:mm"; }
        }

        public static DateTimeOffset StartTime { get; set; }

        public static string TimeZoneID { get; private set; }

        public static int TokenLifeInMinutes { get; private set; }

        public static int TokenMaxMinutesSinceCreation { get; private set; }

        public static int TokenRefreshThresholdInMinutes { get; private set; }

        public static AppUser User { get; set; }

        public static List<VoltageRule> VoltageRules { get; set; }

        #region Constants
        public const string JWTIssuer = "COMPASSMobile";
        public const string JWTAudience = "http://compassmobile.exeloncorp.com";
        #endregion

        //static List<VoltageRule> _voltageRules = new List<VoltageRule>();

        public static DateTimeOffset GetConfiguredTimeZone(DateTimeOffset dto)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dto, AppVariables.TimeZoneID);
        }

        #region GetHardCodedVoltageRules()
        public static List<VoltageRule> GetHardCodedVoltageRules()
        {
            var voltageRules = new List<VoltageRule>();
            LocalVoltageRule rule;

            rule = new VoltageRule
            {
                ID = 1,
                MeterForm = "1",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 2,
                MeterForm = "1",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 3,
                MeterForm = "1",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 5,
                MeterForm = "2",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 6,
                MeterForm = "2",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 8,
                MeterForm = "2",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 9,
                MeterForm = "2",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 10,
                MeterForm = "2",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 11,
                MeterForm = "2",
                MeterType = "kV2c",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 12,
                MeterForm = "2",
                MeterType = "kV2c",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 13,
                MeterForm = "3",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 14,
                MeterForm = "3",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 15,
                MeterForm = "3",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 16,
                MeterForm = "3",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 17,
                MeterForm = "3",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 18,
                MeterForm = "3",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 19,
                MeterForm = "3",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 20,
                MeterForm = "3",
                MeterType = "kV2c",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 21,
                MeterForm = "3",
                MeterType = "kV2c",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 22,
                MeterForm = "4",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 23,
                MeterForm = "4",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 24,
                MeterForm = "4",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 25,
                MeterForm = "4",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 26,
                MeterForm = "4",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 27,
                MeterForm = "4",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 28,
                MeterForm = "4",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 29,
                MeterForm = "4",
                MeterType = "kV2c",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 30,
                MeterForm = "4",
                MeterType = "kV2c",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 31,
                MeterForm = "9",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 32,
                MeterForm = "9",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 33,
                MeterForm = "9",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 34,
                MeterForm = "9",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 35,
                MeterForm = "9",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 36,
                MeterForm = "9",
                MeterType = "kV2c",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 37,
                MeterForm = "12",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 38,
                MeterForm = "12",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 39,
                MeterForm = "12",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 40,
                MeterForm = "12",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 41,
                MeterForm = "12",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 42,
                MeterForm = "12",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 43,
                MeterForm = "12",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 44,
                MeterForm = "12",
                MeterType = "kV2c",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 45,
                MeterForm = "12",
                MeterType = "kV2c",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 46,
                MeterForm = "12",
                MeterType = "kV2c",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 47,
                MeterForm = "16",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 48,
                MeterForm = "16",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 49,
                MeterForm = "16",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 277,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 249,
                CommercialVoltageHigh = 305
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 50,
                MeterForm = "16",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 51,
                MeterForm = "16",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 52,
                MeterForm = "16",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 277,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 249,
                CommercialVoltageHigh = 305
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 53,
                MeterForm = "16",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 54,
                MeterForm = "16",
                MeterType = "kV2c",
                TargetVoltage = 208,
                ResidentialVoltageLow = 196,
                ResidentialVoltageHigh = 220,
                CommercialVoltageLow = 187,
                CommercialVoltageHigh = 229
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 55,
                MeterForm = "16",
                MeterType = "kV2c",
                TargetVoltage = 277,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 249,
                CommercialVoltageHigh = 305
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 56,
                MeterForm = "36",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 57,
                MeterForm = "36",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 58,
                MeterForm = "36",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 59,
                MeterForm = "36",
                MeterType = "kV2c",
                TargetVoltage = 277,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 249,
                CommercialVoltageHigh = 305
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 60,
                MeterForm = "45",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 61,
                MeterForm = "45",
                MeterType = "I-210+C-RD HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 62,
                MeterForm = "45",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 63,
                MeterForm = "45",
                MeterType = "L+G AX-SD C12 HAN",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 64,
                MeterForm = "45",
                MeterType = "kV2c",
                TargetVoltage = 120,
                ResidentialVoltageLow = 113,
                ResidentialVoltageHigh = 127,
                CommercialVoltageLow = 108,
                CommercialVoltageHigh = 132
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 65,
                MeterForm = "45",
                MeterType = "kV2c",
                TargetVoltage = 240,
                ResidentialVoltageLow = 226,
                ResidentialVoltageHigh = 254,
                CommercialVoltageLow = 216,
                CommercialVoltageHigh = 264
            };
            voltageRules.Add(rule);
            rule = new VoltageRule
            {
                ID = 66,
                MeterForm = "45",
                MeterType = "kV2c",
                TargetVoltage = 480,
                ResidentialVoltageLow = 0,
                ResidentialVoltageHigh = 0,
                CommercialVoltageLow = 432,
                CommercialVoltageHigh = 528
            };
            voltageRules.Add(rule);
            return voltageRules;
        }
        #endregion

        public async static Task ResetVoltageRules(bool forceResync)
        {
            DateTime lastSyncTime = await LocalAppSql.GetLastVoltageSyncTime();
            bool usedHardCodedValues = false;
            bool alreadyGotLocalValues = false;

            //Default to cached or hardcoded values in case service takes a bit to complete
            if (lastSyncTime <= DateTime.MinValue)
            {
                VoltageRules = GetHardCodedVoltageRules();
            }
            else
            {
                VoltageRules = await LocalAppSql.GetVoltageRules();
                alreadyGotLocalValues = true;
            }

            //Now if we need to resync go get new values
            if (forceResync || lastSyncTime < DateTime.Now.AddHours(-168))
            {
                //TODO: Add logging
                //AppLogger.Debug(" - GetVoltageRule Start");

                try
                {

                    VoltageRules = await MeterService.GetVoltageRulesAsync();
                    await LocalAppSql.ResetVoltageRules(VoltageRules, false);
                    //TODO: Add logging
                    //AppLogger.Debug(" - GetVoltageRule - Updated from Services");
                }
                catch (Exception ex)
                {
                    //Only update to our hardcoded values if we haven't ever added them from the webservice
                    if (lastSyncTime <= DateTime.MinValue)
                    {
                        usedHardCodedValues = true;
                        VoltageRules = GetHardCodedVoltageRules();
                        await LocalAppSql.ResetVoltageRules(VoltageRules, usedHardCodedValues);
                        //TODO: Add logging
                        //AppLogger.Debug(" - GetVoltageRule - Using Hardcoded Values because of Service Error");
                    }
                    else
                    {
                        //TODO: Add logging
                        //AppLogger.Debug(string.Format(" - GetVoltageRule - Using Previously synced values from: {0} because of Service Error", lastSyncTime.ToShortDateString()));

                    }

                    //TODO: Add logging
                    //AppVariables.AppService.LogApplicationError("AppVariables.ResetVoltageRules", ex);

                    if (forceResync)
                    {
                        throw;
                    }
                }
                finally
                {
                    //TODO: Add logging
                    //AppLogger.Debug(" - Get VoltageRule End");
                }

            }
            else
            {
                //TODO: Add logging
                //AppLogger.Debug(" - GetVoltageRule - Using Synced Values from: " + lastSyncTime.ToLocalTime().ToShortDateString());
                if (!alreadyGotLocalValues)
                {
                    VoltageRules = await LocalAppSql.GetVoltageRules();
                }
                //TODO: Add logging
                //AppLogger.Debug(" - Get VoltageRule End");

            }
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

