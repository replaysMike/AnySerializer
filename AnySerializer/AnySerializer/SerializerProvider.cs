using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace AnySerializer
{
    public class SerializerProvider
    {
        private readonly ICollection<Type> _ignoreAttributes = new List<Type> {
            typeof(IgnoreDataMemberAttribute),
            typeof(NonSerializedAttribute),
            typeof(JsonIgnoreAttribute),
        };
        private SerializerInternal _serializer;
        private DeserializerInternal _deserializer;
        private ValidatorInternal _validator;

        public SerializerProvider()
        {
            _serializer = new SerializerInternal();
            _deserializer = new DeserializerInternal();
            _validator = new ValidatorInternal();
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return _serializer.InspectAndSerialize(obj, Constants.DefaultMaxDepth, _ignoreAttributes);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return default(T);

            return _deserializer.InspectAndDeserialize<T>(new TypeSupport<T>(), bytes, Constants.DefaultMaxDepth, _ignoreAttributes);
        }

        public T Deserialize<T>(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new InvalidOperationException($"Stream is not readable.");
            if (stream.Length == 0)
                return default(T);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            return _deserializer.InspectAndDeserialize<T>(new TypeSupport<T>(), bytes, Constants.DefaultMaxDepth, _ignoreAttributes);
        }

        /// <summary>
        /// Validate a serialized object
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Validate(byte[] bytes)
        {
            return _validator.Validate(bytes);
        }

        
    }
}
