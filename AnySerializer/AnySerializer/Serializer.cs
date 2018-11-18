using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        /// Deserialize an object of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="bytes"></param>
        /// <returns>Deserlized <typeparamref name="T"/></returns>
        public static T Deserialize<T>(byte[] bytes)
        {
            return Extensions.SerializerExtensions.Deserialize<T>(bytes);
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
    }
}
