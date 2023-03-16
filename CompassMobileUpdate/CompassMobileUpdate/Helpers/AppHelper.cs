using System;
using CompassMobileUpdate.Exceptions;

namespace CompassMobileUpdate.Helpers
{
	class AppHelper
	{
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

