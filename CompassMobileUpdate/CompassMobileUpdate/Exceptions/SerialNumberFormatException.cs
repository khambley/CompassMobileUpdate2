using System;
namespace CompassMobileUpdate.Exceptions
{
	public class SerialNumberFormatException : Exception
	{
		public SerialNumberFormatException() { }
        public SerialNumberFormatException(string message) : base(message) { }
        public SerialNumberFormatException(string message, Exception inner) : base(message, inner) { }
    }
}

