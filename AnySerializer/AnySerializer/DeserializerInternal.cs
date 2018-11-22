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
        /// <param name="sourceTypeLoader"></param>
        /// <param name="sourceBytes"></param>
        /// <param name="maxDepth"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="typeRegistry">Custom type registry</param>
        /// <returns></returns>
        internal T InspectAndDeserialize<T>(TypeLoader sourceTypeLoader, byte[] sourceBytes, int maxDepth, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry = null)
        {
            if (sourceBytes == null)
                return default(T);

            var objectTree = new Dictionary<int, object>();

            using (var stream = new MemoryStream(sourceBytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    return (T)TypeReaders.Read(reader, typeof(T).TypeSupport(), maxDepth, objectTree, ignoreAttributes, typeRegistry);
                }
            }
        }
    }
}
