using System.IO;

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
        /// <returns></returns>
        public static byte[] Serialize<T>(this T obj)
        {
            var provider = new SerializerProvider();
            return provider.Serialize<T>(obj);
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
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this Stream stream)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(stream);
        }
    }
}
