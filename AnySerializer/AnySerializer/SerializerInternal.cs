using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnySerializer
{
    /// <summary>
    /// Create a new serializer
    /// </summary>
    internal sealed class SerializerInternal
    {
        /// <summary>
        /// Create a new serializer
        /// </summary>
        internal SerializerInternal()
        {

        }

        /// <summary>
        /// Inspect an object and serialize its contents
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="currentDepth"></param>
        /// <param name="maxDepth"></param>
        /// <param name="objectTree"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal byte[] InspectAndSerialize(object sourceObject, int maxDepth, ICollection<Type> ignoreAttributes)
        {
            if (sourceObject == null)
                return null;

            var objectTree = new Dictionary<int, object>();

            var typeSupport = new TypeSupport(sourceObject.GetType());

            // drop any objects we are ignoring by attribute
            if (typeSupport.Attributes.Any(x => ignoreAttributes.Contains(x)))
                return null;

            // for delegate types, return null
            if (typeSupport.IsDelegate)
                return null;

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    TypeWriters.Write(writer, sourceObject, typeSupport, maxDepth, objectTree, ignoreAttributes);
                }
                return stream.ToArray();
            }
        }
    }
}
