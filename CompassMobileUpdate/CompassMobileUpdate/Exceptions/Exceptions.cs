using System;
namespace CompassMobileUpdate.Exceptions
{
	public class Exceptions
	{
		
	}

    public class ApplicationMaintenanceException : Exception
    {
        public ApplicationMaintenanceException() : base()
        {

        }
        public ApplicationMaintenanceException(string message) : base(message)
        {

        }
    }

    public class AuthenticationRequiredException : Exception
    {
        public AuthenticationRequiredException(string message) : base(message)
        {

        }
    }

    public class MeterNotFoundException : Exception
    {
        public MeterNotFoundException(string message)
            : base(message)
        {

        }
    }

    public class NullResponseException : Exception
    {
        public NullResponseException()
        {

        }

        public NullResponseException(string message)
            : base(message)
        {

        }

        public NullResponseException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }

    
}

