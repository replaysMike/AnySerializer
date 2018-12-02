using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TypeSupport;

namespace AnySerializer
{
    /// <summary>
    /// Provides serialization/deserialization
    /// </summary>
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
        /// <param name="obj">The object to serialize</param>
        /// <param name="embedTypes">True to embed concrete types in serialization data (increases size)</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, bool embedTypes = false)
        {
            return Serialize<T>(obj, embedTypes ? SerializerOptions.EmbedTypes : SerializerOptions.None);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, SerializerOptions options)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return _serializer.InspectAndSerialize(obj, Constants.DefaultMaxDepth, options, _ignoreAttributes);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes)
        {
            return Deserialize<T>(bytes, SerializerOptions.None);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, SerializerOptions options)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return default(T);

            return _deserializer.InspectAndDeserialize<T>(new ExtendedType<T>(), bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, SerializerOptions options, params TypeMap[] typeMaps)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return default(T);

            TypeRegistry typeRegistry = null;
            if (typeMaps != null && typeMaps.Length > 0)
            {
                TypeRegistry.Configure((config) =>
                {
                    foreach (var typeMap in typeMaps)
                        config.Mappings.Add(typeMap);
                });
            }

            return Deserialize<T>(bytes, options, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, params TypeMap[] typeMaps)
        {
            return Deserialize<T>(bytes, SerializerOptions.None, typeMaps);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, SerializerOptions options, TypeRegistry typeRegistry)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return default(T);

            return _deserializer.InspectAndDeserialize<T>(new ExtendedType<T>(), bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, TypeRegistry typeRegistry)
        {
            return Deserialize<T>(bytes, SerializerOptions.None, typeRegistry);
        }

        public T Deserialize<T>(Stream stream, SerializerOptions options)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new InvalidOperationException($"Stream is not readable.");
            if (stream.Length == 0)
                return default(T);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            return _deserializer.InspectAndDeserialize<T>(new ExtendedType<T>(), bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes);
        }

        public T Deserialize<T>(Stream stream)
        {
            return Deserialize<T>(stream, SerializerOptions.None);
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
