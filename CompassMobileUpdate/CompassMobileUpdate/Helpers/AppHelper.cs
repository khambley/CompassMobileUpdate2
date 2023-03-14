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
    }
}

