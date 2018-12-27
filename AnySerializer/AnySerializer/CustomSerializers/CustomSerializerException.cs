using System;

namespace AnySerializer.CustomSerializers
{
    /// <summary>
    /// Custom serializer exception
    /// </summary>
    public class CustomSerializerException : Exception
    {
        public Type Serializer { get; }
        public CustomSerializerException(Type serializer, string message) : base(message)
        {
            Serializer = serializer;
        }
    }
}
