using System;
using System.IO;
using TypeSupport;

namespace AnySerializer
{
    /// <summary>
    /// AnySerializer serialization
    /// </summary>
    public static class Serializer
    {
        /// <summary>
        /// Serialize an object of type <typeparamref name="T"/> to a byte array
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj"></param>
        /// <returns>Byte array</returns>
        public static byte[] Serialize<T>(T obj)
        {
            return Extensions.SerializerExtensions.Serialize<T>(obj);
        }

        /// <summary>
        /// Serialize an object of type <typeparamref name="T"/> to a byte array
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj"></param>
        /// <param name="embedTypes">True to embed concrete types in serialization data (increases size)</param>
        /// <returns>Byte array</returns>
        public static byte[] Serialize<T>(T obj, bool embedTypes)
        {
            return Extensions.SerializerExtensions.Serialize<T>(obj, embedTypes);
        }

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serializer options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(byte[] bytes, SerializerOptions options, params TypeMap[] typeMaps)
        {
            return Extensions.SerializerExtensions.Deserialize<T>(bytes, options, typeMaps);
        }

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serializer options</param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(byte[] bytes, SerializerOptions options, TypeRegistry typeRegistry)
        {
            return Extensions.SerializerExtensions.Deserialize<T>(bytes, options, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="stream">A stream of bytes</param>
        /// <param name="options">The serializer options</param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(Stream stream, SerializerOptions options)
        {
            return Extensions.SerializerExtensions.Deserialize<T>(stream, options);
        }

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <param name="type">The type to deserialize</param>
        /// <param name="bytes"></param>
        /// <param name="options">The serializer options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static object Deserialize(Type type, byte[] bytes, SerializerOptions options, params TypeMap[] typeMaps)
        {
            return Extensions.SerializerExtensions.Deserialize(bytes, type, options, typeMaps);
        }

        /// <summary>
        /// Validate a byte array for valid serialization data
        /// </summary>
        /// <param name="bytes">The byte array result from Serialize operation</param>
        /// <returns></returns>
        public static bool Validate(byte[] bytes)
        {
            return Extensions.SerializerExtensions.Validate(bytes);
        }
    }
}
