using LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypeSupport;
using TypeSupport.Extensions;
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
        /// <param name="options">The serialization options</param>
        /// <param name="objectTree"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal byte[] InspectAndSerialize(object sourceObject, int maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, ICollection<string> ignorePropertiesOrPaths = null)
        {
            if (sourceObject == null)
                return null;

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
                    typeDescriptors = TypeWriter.Write(writer, sourceObject, typeSupport, maxDepth, options, ignoreAttributes, ignorePropertiesOrPaths);
                }
                dataBytes = stream.ToArray();
                
            }

            if(typeDescriptors != null)
                dataBytes = BuildTypeDescriptorMap(dataBytes, typeDescriptors);

            if (options.BitwiseHasFlag(SerializerOptions.Compress))
            {
                // enable data compression for strings
                dataBytes = CompressData(dataBytes);
            }

            return dataBytes;
        }

        private byte[] CompressData(byte[] dataBytes)
        {
            var settingsByte = dataBytes[0];
            var dataBytesWithoutSettingsByte = new byte[dataBytes.Length - 1];
            Array.Copy(dataBytes, 1, dataBytesWithoutSettingsByte, 0, dataBytes.Length - 1);
            using (var compressedStream = new MemoryStream())
            {
                using (var lz4Stream = new LZ4Stream(compressedStream, LZ4StreamMode.Compress))
                {
                    using (var compressedWriter = new StreamWriter(lz4Stream))
                    {
                        compressedWriter.Write(Convert.ToBase64String(dataBytesWithoutSettingsByte));
                    }
                }
                var compressedArray = compressedStream.ToArray();
                var compressedArrayWithSettingsByte = new byte[compressedArray.Length + 1];
                compressedArrayWithSettingsByte[0] = settingsByte;
                Array.Copy(compressedArray, 0, compressedArrayWithSettingsByte, 1, compressedArray.Length);
                dataBytes = compressedArrayWithSettingsByte;
            }
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
            // copy dataSettings to the front
            newDataBytes[0] = dataBytes[0];
            // copy the type descriptors to the front of dataBytes + 1 (dataSettings is byte-0)
            Array.Copy(typeDescriptorBytes, 0, newDataBytes, 1, typeDescriptorBytes.Length);
            // append the dataBytes without dataSettings at byte-0
            Array.Copy(dataBytes, 1, newDataBytes, typeDescriptorBytes.Length + 1, dataBytes.Length - 1);
            return newDataBytes;
        }
    }
}
