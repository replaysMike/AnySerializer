using System.IO;
using TypeSupport;

namespace AnySerializer.Extensions
{
    /// <summary>
    /// AnySerializer Extensions
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="embedTypes">True to embed concrete types in serialization data (increases size)</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this T obj, bool embedTypes = false)
        {
            var provider = new SerializerProvider();
            return provider.Serialize<T>(obj, embedTypes);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static bool Validate(this byte[] bytes)
        {
            var provider = new SerializerProvider();
            return provider.Validate(bytes);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] bytes)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(bytes);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] bytes, params TypeMap[] typeMaps)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(bytes, typeMaps);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] bytes, TypeRegistry typeRegistry)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(bytes, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this Stream stream)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(stream);
        }
    }
}
