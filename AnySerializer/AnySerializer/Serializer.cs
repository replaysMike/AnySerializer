using System;
using System.IO;

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
        public static byte[] Serialize<T>(T obj) => Extensions.SerializerExtensions.Serialize<T>(obj);

        /// <summary>
        /// Serialize an object of type <typeparamref name="T"/> to a byte array
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="obj"></param>
        /// <param name="embedTypes">True to embed concrete types in serialization data (increases size)</param>
        /// <returns>Byte array</returns>
        public static byte[] Serialize<T>(T obj, bool embedTypes) => Extensions.SerializerExtensions.Serialize<T>(obj, embedTypes);

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serializer options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns>Deserialized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(byte[] bytes, SerializerOptions options, params SerializationTypeMap[] typeMaps) => Extensions.SerializerExtensions.Deserialize<T>(bytes, options, typeMaps);

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serializer options</param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns>Deserialized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(byte[] bytes, SerializerOptions options, SerializationTypeRegistry typeRegistry) => Extensions.SerializerExtensions.Deserialize<T>(bytes, options, typeRegistry);

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="stream">A stream of bytes</param>
        /// <param name="options">The serializer options</param>
        /// <returns>Deserialized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(Stream stream, SerializerOptions options) => Extensions.SerializerExtensions.Deserialize<T>(stream, options);

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <param name="type">The type to deserialize</param>
        /// <param name="bytes"></param>
        /// <param name="options">The serializer options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns>Deserialized <typeparamref name="T"/></returns>
        public static object Deserialize(Type type, byte[] bytes, SerializerOptions options, params SerializationTypeMap[] typeMaps) => Extensions.SerializerExtensions.Deserialize(bytes, type, options, typeMaps);

        /// <summary>
        /// Validate a byte array for valid serialization data
        /// </summary>
        /// <param name="bytes">The byte array result from Serialize operation</param>
        /// <returns></returns>
        public static bool Validate(byte[] bytes) => Extensions.SerializerExtensions.Validate(bytes);
    }
}
