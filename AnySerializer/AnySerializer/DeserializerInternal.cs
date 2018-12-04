using System;
using System.Collections.Generic;
using System.IO;
using TypeSupport;
using TypeSupport.Extensions;

namespace AnySerializer
{
    /// <summary>
    /// Create a new deserializer
    /// </summary>
    internal sealed class DeserializerInternal
    {
        /// <summary>
        /// Create a new deserializer
        /// </summary>
        internal DeserializerInternal()
        {

        }

        /// <summary>
        /// Inspect a type and deserialize its contents
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceBytes"></param>
        /// <param name="maxDepth"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="typeRegistry">Custom type registry</param>
        /// <returns></returns>
        internal T InspectAndDeserialize<T>(byte[] sourceBytes, int maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, TypeRegistry typeRegistry = null, ICollection<string> ignorePropertiesOrPaths = null)
        {
            return (T)InspectAndDeserialize(typeof(T), sourceBytes, maxDepth, options, ignoreAttributes, typeRegistry, ignorePropertiesOrPaths);
        }

        /// <summary>
        /// Inspect a type and deserialize its contents
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceExtendedType"></param>
        /// <param name="sourceBytes"></param>
        /// <param name="maxDepth"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="typeRegistry">Custom type registry</param>
        /// <returns></returns>
        internal object InspectAndDeserialize(Type type, byte[] sourceBytes, int maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, TypeRegistry typeRegistry = null, ICollection<string> ignorePropertiesOrPaths = null)
        {
            if (sourceBytes == null)
                return null;

            using (var stream = new MemoryStream(sourceBytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var obj = TypeReader.Read(reader, type.GetExtendedType(), maxDepth, options, ignoreAttributes, typeRegistry, ignorePropertiesOrPaths);
                    return obj;
                }
            }
        }
    }
}
