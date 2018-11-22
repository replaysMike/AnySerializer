using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypeSupport;
using static AnySerializer.TypeManagement;

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
        /// <param name="useTypeDescriptors">True to embed a type descriptor map in the serialized data</param>
        /// <returns></returns>
        internal byte[] InspectAndSerialize(object sourceObject, int maxDepth, ICollection<Type> ignoreAttributes, bool useTypeDescriptors)
        {
            if (sourceObject == null)
                return null;

            var objectTree = new Dictionary<int, object>();

            var typeSupport = new ExtendedType(sourceObject.GetType());

            // drop any objects we are ignoring by attribute
            if (typeSupport.Attributes.Any(x => ignoreAttributes.Contains(x)))
                return null;

            // for delegate types, return null
            if (typeSupport.IsDelegate)
                return null;

            byte[] dataBytes = null;
            TypeDescriptors typeDescriptors = null;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    typeDescriptors = TypeWriters.Write(writer, sourceObject, typeSupport, maxDepth, objectTree, ignoreAttributes, useTypeDescriptors);
                }
                dataBytes = stream.ToArray();
            }

            if(typeDescriptors != null)
                dataBytes = BuildTypeDescriptorMap(dataBytes, typeDescriptors);

            return dataBytes;
        }

        /// <summary>
        /// Build a map of all of the types used in serialization
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <param name="typeDescriptors"></param>
        /// <returns></returns>
        private byte[] BuildTypeDescriptorMap(byte[] dataBytes, TypeDescriptors typeDescriptors)
        {
            // write the type descriptors header
            byte[] typeDescriptorBytes = null;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // write the object type header
                    writer.Write((byte)TypeId.TypeDescriptorMap);

                    // make a note of where this object starts, so we can populate the length header later
                    var lengthStartPosition = writer.BaseStream.Position;

                    // make room for the length prefix
                    writer.Seek(Constants.LengthHeaderSize + (int)writer.BaseStream.Position, SeekOrigin.Begin);

                    var descriptorBytes = typeDescriptors.Serialize();
                    writer.Write(descriptorBytes, 0, descriptorBytes.Length);

                    var currentPosition = writer.BaseStream.Position;
                    // write the length header at the start of this object
                    var length = writer.BaseStream.Length - lengthStartPosition;
                    writer.Seek((int)lengthStartPosition, SeekOrigin.Begin);
                    writer.Write((int)length);
                }
                typeDescriptorBytes = stream.ToArray();
            }

            // prepend the type descriptors
            byte[] newDataBytes = new byte[dataBytes.Length + typeDescriptorBytes.Length];
            // copy the type descriptors to the front of dataBytes
            Array.Copy(typeDescriptorBytes, 0, newDataBytes, 0, typeDescriptorBytes.Length);
            // append the dataBytes
            Array.Copy(dataBytes, 0, newDataBytes, typeDescriptorBytes.Length, dataBytes.Length);
            return newDataBytes;
        }
    }
}
