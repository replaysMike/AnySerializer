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
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(byte[] bytes, params TypeMap[] typeMaps)
        {
            return Extensions.SerializerExtensions.Deserialize<T>(bytes, typeMaps);
        }

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="bytes"></param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(byte[] bytes, TypeRegistry typeRegistry)
        {
            return Extensions.SerializerExtensions.Deserialize<T>(bytes, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="stream">A stream of bytes</param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(Stream stream)
        {
            return Extensions.SerializerExtensions.Deserialize<T>(stream);
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
