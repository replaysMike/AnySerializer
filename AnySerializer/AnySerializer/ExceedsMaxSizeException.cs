using System;

namespace AnySerializer
{
    public class ExceedsMaxSizeException : Exception
    {
        public ExceedsMaxSizeException(string message) : base(message)
        {

        }

        public ExceedsMaxSizeException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
