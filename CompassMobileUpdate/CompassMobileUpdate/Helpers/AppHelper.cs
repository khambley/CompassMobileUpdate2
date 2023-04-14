using System;
using System.Threading.Tasks;
using CompassMobileUpdate.Exceptions;
using Xamarin.Forms;

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
    }
}

