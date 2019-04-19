using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using TypeSupport;

namespace AnySerializer
{
    /// <summary>
    /// Provides serialization/deserialization
    /// </summary>
    public class SerializerProvider
    {
        private readonly ICollection<object> _ignoreAttributes = new List<object> {
            typeof(IgnoreDataMemberAttribute),
            typeof(NonSerializedAttribute),
            "JsonIgnoreAttribute",
        };
        private readonly SerializerInternal _serializer;
        private readonly DeserializerInternal _deserializer;
        private readonly ValidatorInternal _validator;

        /// <summary>
        /// Get the diagnostic log
        /// </summary>
        public string DiagnosticLog => _serializer?.DiagnosticLog;

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
        public byte[] Serialize<T>(T obj, bool embedTypes)
        {
            return Serialize<T>(obj, embedTypes ? SerializerOptions.EmbedTypes : SerializerOptions.None);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj)
        {
            return Serialize<T>(obj, SerializerOptions.None);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="ignorePropertiesOrPaths">List of property names or property paths to ignore</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, params string[] ignorePropertiesOrPaths)
        {
            return Serialize<T>(obj, SerializerOptions.None, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="ignoreProperties">A list expressions that define properties to ignore</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, params Expression<Func<T, object>>[] ignoreProperties)
        {
            return Serialize<T>(obj, SerializerOptions.None, ConvertToPropertyNameList(ignoreProperties));
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="options">Serialization options</param>
        /// <param name="ignoreProperties">A list expressions that define properties to ignore</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, SerializerOptions options, params Expression<Func<T, object>>[] ignoreProperties)
        {
            return Serialize<T>(obj, options, ConvertToPropertyNameList(ignoreProperties));
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="options">Serialization options</param>
        /// <param name="ignorePropertiesOrPaths">List of property names or property paths to ignore</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, SerializerOptions options, params string[] ignorePropertiesOrPaths)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return _serializer.InspectAndSerialize(obj, Constants.DefaultMaxDepth, options, _ignoreAttributes, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="options">Serialization options</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, SerializerOptions options)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return _serializer.InspectAndSerialize(obj, Constants.DefaultMaxDepth, options, _ignoreAttributes);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignorePropertiesOrPaths">A list of property names or property paths to ignore</param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj, SerializerOptions options, ICollection<string> ignorePropertiesOrPaths)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            return _serializer.InspectAndSerialize(obj, Constants.DefaultMaxDepth, options, _ignoreAttributes, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="ignorePropertiesOrPaths">List of property names or property paths to ignore</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, params string[] ignorePropertiesOrPaths)
        {
            return Deserialize<T>(bytes, SerializerOptions.None, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="ignoreProperties">A list expressions that define properties to ignore</param>
        /// <returns></returns>
        public object Deserialize<T>(byte[] bytes, params Expression<Func<T, object>>[] ignoreProperties)
        {
            return Deserialize<T>(bytes, SerializerOptions.None, ignoreProperties);
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
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] bytes, params TypeMap[] typeMaps)
        {
            return Deserialize(type, bytes, SerializerOptions.None, typeMaps);
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

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] bytes, TypeRegistry typeRegistry)
        {
            return Deserialize(type, bytes, SerializerOptions.None, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public T Deserialize<T>(Stream stream)
        {
            return Deserialize<T>(stream, SerializerOptions.None);
        }

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <param name="type"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public object Deserialize(Type type, Stream stream)
        {
            return Deserialize(type, stream, SerializerOptions.None);
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
        /// <param name="ignorePropertiesOrPaths">List of property names or property paths to ignore</param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return null;

            return _deserializer.InspectAndDeserialize(type, bytes, Constants.DefaultMaxDepth, SerializerOptions.None, _ignoreAttributes);
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

            return _deserializer.InspectAndDeserialize<T>(bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, SerializerOptions options, params string[] ignorePropertiesOrPaths)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return default(T);

            return _deserializer.InspectAndDeserialize<T>(bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes, null, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes, SerializerOptions options, params Expression<Func<T, object>>[] ignoreProperties)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return default(T);

            return _deserializer.InspectAndDeserialize<T>(bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes, null, ConvertToPropertyNameList(ignoreProperties));
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] bytes, SerializerOptions options, params string[] ignorePropertiesOrPaths)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return null;

            return _deserializer.InspectAndDeserialize(type, bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes, null, ignorePropertiesOrPaths);
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
        /// <param name="options">The serialization options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] bytes, SerializerOptions options, params TypeMap[] typeMaps)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return null;

            TypeRegistry typeRegistry = null;
            if (typeMaps != null && typeMaps.Length > 0)
            {
                TypeRegistry.Configure((config) =>
                {
                    foreach (var typeMap in typeMaps)
                        config.Mappings.Add(typeMap);
                });
            }

            return Deserialize(type, bytes, options, typeRegistry);
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

            return _deserializer.InspectAndDeserialize<T>(bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object from a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public object Deserialize(Type type, byte[] bytes, SerializerOptions options, TypeRegistry typeRegistry)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                return null;

            return _deserializer.InspectAndDeserialize(type, bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes, typeRegistry);
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
            var bytesRead = stream.Read(bytes, 0, bytes.Length);

            if (bytesRead != bytes.Length)
                throw new InvalidOperationException($"The data read ({bytesRead}) was different than the expected bytes to read ({bytes.Length})! This indicates corrupt data.");

            return _deserializer.InspectAndDeserialize<T>(bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes);
        }

        public object Deserialize(Type type, Stream stream, SerializerOptions options)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new InvalidOperationException($"Stream is not readable.");
            if (stream.Length == 0)
                return null;
            var bytes = new byte[stream.Length];
            var bytesRead = stream.Read(bytes, 0, bytes.Length);
            if (bytesRead != bytes.Length)
                throw new InvalidOperationException($"The data read ({bytesRead}) was different than the expected bytes to read ({bytes.Length})! This indicates corrupt data.");

            return _deserializer.InspectAndDeserialize(type, bytes, Constants.DefaultMaxDepth, options, _ignoreAttributes);
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

        /// <summary>
        /// Convert an expression of properties to a list of property names
        /// </summary>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        private ICollection<string> ConvertToPropertyNameList<T>(Expression<Func<T, object>>[] ignoreProperties)
        {
            var ignorePropertiesList = new List<string>();
            foreach (var expression in ignoreProperties)
            {
                var name = "";
                switch (expression.Body)
                {
                    case MemberExpression m:
                        name = m.Member.Name;
                        break;
                    case UnaryExpression u when u.Operand is MemberExpression m:
                        name = m.Member.Name;
                        break;
                    default:
                        throw new NotImplementedException(expression.GetType().ToString());
                }
                ignorePropertiesList.Add(name);
            }
            return ignorePropertiesList;
        }
    }
}
