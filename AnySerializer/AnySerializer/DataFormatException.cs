using System;

namespace AnySerializer
{
    public class DataFormatException : Exception
    {
        public DataFormatException(string message) : base(message)
        {

        }

        public DataFormatException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
