using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="ignorePropertiesOrPaths"></param>
        /// <param name="skipTag">Any properties decorated with the matching skip tags will not be deserialized</param>
        /// <returns></returns>
        internal T InspectAndDeserialize<T>(byte[] sourceBytes, uint maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, PropertyVersion skipTag, SerializationTypeRegistry typeRegistry = null, ICollection<string> ignorePropertiesOrPaths = null)
            => (T)InspectAndDeserialize(typeof(T), sourceBytes, maxDepth, options, ignoreAttributes, typeRegistry, ignorePropertiesOrPaths, skipTag);

        /// <summary>
        /// Inspect a type and deserialize its contents
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceBytes"></param>
        /// <param name="maxDepth"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="typeRegistry">Custom type registry</param>
        /// <param name="ignorePropertiesOrPaths"></param>
        /// <param name="skipTag"></param>
        /// <returns></returns>
        internal T InspectAndDeserialize<T>(byte[] sourceBytes, uint maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, SerializationTypeRegistry typeRegistry = null, ICollection<string> ignorePropertiesOrPaths = null, PropertyVersion skipTag = null)
            => (T)InspectAndDeserialize(typeof(T), sourceBytes, maxDepth, options, ignoreAttributes, typeRegistry, ignorePropertiesOrPaths, skipTag);

        /// <summary>
        /// Inspect a type and deserialize its contents
        /// </summary>
        /// <param name="sourceBytes"></param>
        /// <param name="maxDepth"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="typeRegistry">Custom type registry</param>
        /// <param name="ignorePropertiesOrPaths"></param>
        /// <param name="skipTag"></param>
        /// <returns></returns>
        internal object InspectAndDeserialize(Type type, byte[] sourceBytes, uint maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, SerializationTypeRegistry typeRegistry = null, ICollection<string> ignorePropertiesOrPaths = null, PropertyVersion skipTag = null)
        {
            if (sourceBytes == null)
                return null;

            using (var stream = new MemoryStream(sourceBytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var obj = TypeReader.Read(reader, type.GetExtendedType(), maxDepth, options, ignoreAttributes, typeRegistry, ignorePropertiesOrPaths, skipTag);
                    return obj;
                }
            }
        }
    }
}
