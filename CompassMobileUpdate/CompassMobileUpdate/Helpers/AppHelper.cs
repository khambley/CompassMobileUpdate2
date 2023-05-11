using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
using CompassMobileUpdate.Models;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using static CompassMobileUpdate.Models.Enums;

namespace CompassMobileUpdate.Helpers
{
	class AppHelper
	{
        public static bool ContainsAuthenticationRequiredException(Exception ex)
        {
            Exception e = ex;

            while (e != null)
            {
                if (e.GetType() == typeof(AuthenticationRequiredException))
                {
                    return true;
                }

                e = e.InnerException;
            }

            return false;

        }

        public static bool ContainsNullResponseException(Exception ex)
        {
            Exception e = ex;
            while (e != null)
            {
                if (e.GetType() == typeof(NullResponseException))
                {
                    return true;
                }
                e = e.InnerException;
            }
            return false;
        }

        public static async Task FadeOutLabelByEmptyString(Label lbl, int delayInMs)
        {

            if (delayInMs <= 0)
            {
                lbl.Text = string.Empty;
            }
            else
            {
                int fadeLength = 750;

                if (delayInMs > fadeLength)
                {
                    int diff = delayInMs - fadeLength;

                    await Task.Delay(diff);
                    await lbl.FadeTo(0, Convert.ToUInt32(fadeLength), null);
                }
                else
                {
                    await lbl.FadeTo(0, Convert.ToUInt32(delayInMs), null);
                }
                lbl.Text = string.Empty;
                lbl.Opacity = 1;
            }
        }

        public static async Task FadeOutLabelByEmptyString(Label lbl)
        {
            int delayInMs = AppVariables.DefaultFadeMs;

            if (delayInMs <= 0)
            {
                lbl.Text = string.Empty;
            }
            else
            {
                int fadeLength = 750;

                if (delayInMs > fadeLength)
                {
                    int diff = delayInMs - fadeLength;

                    await Task.Delay(diff);
                    await lbl.FadeTo(0, Convert.ToUInt32(fadeLength), null);
                }
                else
                {
                    await lbl.FadeTo(0, Convert.ToUInt32(delayInMs), null);
                }
                lbl.Text = string.Empty;
                lbl.Opacity = 1;
            }
        }

        public static DateTimeOffset GetConfiguredTimeZone(DateTimeOffset dto)
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dto, AppVariables.TimeZoneID);
        }

        public static bool? HasOverlappingVoltage(Meter meter)
        {
            List<LocalVoltageRule> possibleRanges = (from voltageRule
                                                    in AppVariables.VoltageRules
                                                     where (
                                                            voltageRule.MeterForm.Equals(meter.Form, StringComparison.InvariantCultureIgnoreCase)
                                                            && voltageRule.MeterType.Equals(meter.Type, StringComparison.InvariantCultureIgnoreCase)
                                                            && voltageRule.OverlappingVoltages == true
                                                            )
                                                     select voltageRule).ToList();

            if (possibleRanges.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool? IsVoltageInRangeAsync(Meter meter, decimal? voltage)
        {
            if (!voltage.HasValue)
                return null;

            // we currently don't have targetVoltage or customerClass within the app at the right time,
            // so we're ignoring it for now, but leaving it in the function signature
            bool result = false;

            List<LocalVoltageRule> possibleRanges = (from voltageRule
                                                    in AppVariables.VoltageRules
                                                     where (
                                                            voltageRule.MeterForm.Equals(meter.Form, StringComparison.InvariantCultureIgnoreCase)
                                                            && voltageRule.MeterType.Equals(meter.Type, StringComparison.InvariantCultureIgnoreCase)
                                                            )
                                                     select voltageRule).ToList();

            if (possibleRanges.Count > 0)
            {
                foreach (LocalVoltageRule currentRange in possibleRanges)
                {
                    decimal residentialLowRange = currentRange.ResidentialVoltageLow;
                    decimal residentialHighRange = currentRange.ResidentialVoltageHigh;
                    decimal commercialLowRange = currentRange.CommercialVoltageLow;
                    decimal commercialHighRange = currentRange.CommercialVoltageHigh;

                    if (meter.CustomerClassType == CustomerClassType.Unknown)
                    {
                        // check both ranges
                        if (((residentialLowRange < voltage) && (voltage < residentialHighRange)) || ((commercialLowRange < voltage) && (voltage < commercialHighRange)))
                        {
                            result = true;
                            break;
                        }
                    }
                    else if (meter.CustomerClassType == CustomerClassType.Residential)
                    {
                        // uh....check residential only
                        if ((residentialLowRange < voltage) && (voltage < residentialHighRange))
                        {
                            result = true;
                            break;
                        }
                    }
                    else if (meter.CustomerClassType == CustomerClassType.Commercial)
                    {
                        // uh....you guessed it, check commercial only
                        if ((commercialLowRange < voltage) && (voltage < commercialHighRange))
                        {
                            result = true;
                            break;
                        }
                    }
                    else
                    {
                        // if we get here, a programmer added a value to the CustomerClass enum and didn't handle it here
                        throw new Exception("Unexpected CustomerClass enum value '" + meter.CustomerClassType.ToString() + "'.  Programmer Error.");
                    }
                }
            }
            else
            {
                var localSql = new LocalSql();
                var list = localSql.GetVoltageRules().Result;
                int count = list.Count;
                string message = string.Format("No Voltage Rules found for Meter_Type = {0} And Meter_Form = {1}. There are currently {2} rows of Voltage Rules in this mobile device's memory", meter.Type, meter.Form, count);
                //TODO: Add app logging
                //AppVariables.AppService.LogApplicationError("MeterHelper.IsVoltageInRange", new Exception(message));
                return null;
            }
            return result;
        }

        public static string GetAmalgamatedExceptionMessage(Exception ex)
        {
            string message = string.Empty;
            bool firstMessage = true;
            while (ex != null)
            {
                if (firstMessage)
                {
                    message = ex.Message;
                    firstMessage = false;
                }
                else
                {
                    message += Environment.NewLine + ex.Message;
                }
                ex = ex.InnerException;
            }
            return message;
        }

        public static BoundingCoords GetBoundingCoords(MapSpan visibleRegion)
        {

            var center = visibleRegion.Center;
            var halfHeightDegrees = visibleRegion.LatitudeDegrees / 2;
            var halfWidthDegrees = visibleRegion.LongitudeDegrees / 2;

            var left = center.Longitude - halfWidthDegrees;
            var right = center.Longitude + halfWidthDegrees;
            var top = center.Latitude + halfHeightDegrees;
            var bottom = center.Latitude - halfHeightDegrees;

            // Adjust for Internation Date Line (+/- 180 degrees longitude)
            if (left < -180) left = 180 + (180 + left);
            if (right > 180) right = (right - 180) - 180;

            return new BoundingCoords(left, top, right, bottom);
        }

        /// <summary>
        /// Expects a list of digits only.
        /// It will then Format the number like xxx-xxxx or x-xxx-xxxx
        /// where the delimeter is by default a hyphen ("-") or you can pass your own
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryFormatPhoneNumberFromDigits(string phoneNumber, out string result, char delimiter = '-')
        {
            long number;

            //if the phone number is all numbers
            if (long.TryParse(phoneNumber, out number))
            {
                if (phoneNumber.Length == 10 || phoneNumber.Length == 11)
                {
                    //lets add some "-" for readability
                    if (phoneNumber.Length == 10)
                    {
                        result = phoneNumber.Insert(3, delimiter.ToString());
                        result = result.Insert(7, delimiter.ToString());
                    }
                    else if (phoneNumber.Length == 11)
                    {
                        result = phoneNumber.Insert(1, delimiter.ToString());
                        result = result.Insert(5, delimiter.ToString());
                        result = result.Insert(9, delimiter.ToString());
                    }
                    else
                        result = null;
                }
                else
                    result = null;

                return true;
            }

            result = null;
            return false;
        }
    }
}

