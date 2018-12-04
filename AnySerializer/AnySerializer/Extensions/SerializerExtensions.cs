using System;
using System.IO;
using System.Linq.Expressions;
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
        /// <returns></returns>
        public static byte[] Serialize<T>(this T obj)
        {
            var provider = new SerializerProvider();
            return provider.Serialize<T>(obj);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this T obj, SerializerOptions options)
        {
            var provider = new SerializerProvider();
            return provider.Serialize<T>(obj, options);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this T obj, SerializerOptions options, params string[] ignorePropertiesOrPaths)
        {
            var provider = new SerializerProvider();
            return provider.Serialize<T>(obj, options, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this T obj, SerializerOptions options, params Expression<Func<T, object>>[] ignoreProperties)
        {
            var provider = new SerializerProvider();
            return provider.Serialize<T>(obj, options, ignoreProperties);
        }

        /// <summary>
        /// Serialize an object to a byte array
        /// </summary>
        /// <param name="embedTypes">True to embed concrete types in serialization data (increases size)</param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this T obj, bool embedTypes)
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
        /// <returns></returns>
        public static object Deserialize(this byte[] bytes, Type type)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize(type, bytes);
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
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public static object Deserialize(this byte[] bytes, Type type, params TypeMap[] typeMaps)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize(type, bytes, typeMaps);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] bytes, SerializerOptions options, params TypeMap[] typeMaps)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(bytes, options, typeMaps);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignorePropertiesOrPaths">List of property names or property paths to ignore</param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] bytes, SerializerOptions options, params string[] ignorePropertiesOrPaths)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(bytes, options, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignoreProperties">A list expressions that define properties to ignore</param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] bytes, SerializerOptions options, params Expression<Func<T, object>>[] ignoreProperties)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(bytes, options, ignoreProperties);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="typeMaps">A list of type mappings</param>
        /// <returns></returns>
        public static object Deserialize(this byte[] bytes, Type type, SerializerOptions options, params TypeMap[] typeMaps)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize(type, bytes, options, typeMaps);
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
        /// <param name="bytes"></param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public static object Deserialize(this byte[] bytes, Type type, TypeRegistry typeRegistry)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize(type, bytes, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public static T Deserialize<T>(this byte[] bytes, SerializerOptions options, TypeRegistry typeRegistry)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(bytes, options, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="typeRegistry">A list of type mappings</param>
        /// <returns></returns>
        public static object Deserialize(this byte[] bytes, Type type, SerializerOptions options, TypeRegistry typeRegistry)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize(type, bytes, options, typeRegistry);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public static T Deserialize<T>(this Stream stream, SerializerOptions options)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(stream, options);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignorePropertiesOrPaths">List of property names or property paths to ignore</param>
        /// <returns></returns>
        public static T Deserialize<T>(this Stream stream, SerializerOptions options, params string[] ignorePropertiesOrPaths)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(stream, options);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignoreProperties">A list expressions that define properties to ignore</param>
        /// <returns></returns>
        public static T Deserialize<T>(this Stream stream, SerializerOptions options, params Expression<Func<T, object>>[] ignoreProperties)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize<T>(stream, options);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="options">The serialization options</param>
        /// <returns></returns>
        public static object Deserialize(this Stream stream, Type type, SerializerOptions options)
        {
            var provider = new SerializerProvider();
            return provider.Deserialize(type, stream, options);
        }
    }
}
