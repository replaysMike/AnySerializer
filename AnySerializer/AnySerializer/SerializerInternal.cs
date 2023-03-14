#if FEATURE_COMPRESSION
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
#endif
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
        /// Get the diagnostic log
        /// </summary>
        public string DiagnosticLog { get; private set; }

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
        /// <param name="maxDepth"></param>
        /// <param name="options"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="ignorePropertiesOrPaths"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal byte[] InspectAndSerialize(object sourceObject, uint maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, ICollection<string> ignorePropertiesOrPaths = null)
        {
            if (sourceObject == null)
                return null;

            var typeSupport = sourceObject.GetType().GetExtendedType();

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
                    typeDescriptors = TypeWriter.Write(writer, sourceObject, typeSupport, maxDepth, options, ignoreAttributes, out var diagnosticLog, ignorePropertiesOrPaths);
                    DiagnosticLog = diagnosticLog;
                }
                dataBytes = stream.ToArray();
                
            }

            if(typeDescriptors != null)
                dataBytes = BuildTypeDescriptorMap(dataBytes, typeDescriptors);

            if (options.BitwiseHasFlag(SerializerOptions.Compress))
            {
#if FEATURE_COMPRESSION
                // enable data compression for strings
                dataBytes = CompressData(dataBytes);
#else
                throw new InvalidOperationException($"Compression is only available in .Net Framework 4.6+ and .Net Standard 1.6+");
#endif
            }

            return dataBytes;
        }

#if FEATURE_COMPRESSION
        private byte[] CompressData(byte[] dataBytes)
        {
            var settingsByte = dataBytes[0];
            var compressedArrayWithSettingsByte = new byte[0];
            var dataBytesWithoutSettingsByte = new byte[dataBytes.Length - 1];
            Array.Copy(dataBytes, 1, dataBytesWithoutSettingsByte, 0, dataBytes.Length - 1);
            using (var compressedStream = new MemoryStream())
            {
                using (var lz4Stream = LZ4Stream.Encode(compressedStream))
                {
                    using (var compressedWriter = new StreamWriter(lz4Stream))
                    {
                        compressedWriter.Write(Convert.ToBase64String(dataBytesWithoutSettingsByte));
                    }
                }
                var compressedArray = compressedStream.ToArray();
                compressedArrayWithSettingsByte = new byte[compressedArray.Length + 1];
                compressedArrayWithSettingsByte[0] = settingsByte;
                Array.Copy(compressedArray, 0, compressedArrayWithSettingsByte, 1, compressedArray.Length);
            }
            return compressedArrayWithSettingsByte;
        }
#endif

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
                    writer.Seek((int)(Constants.LengthHeaderSize + writer.BaseStream.Position), SeekOrigin.Begin);

                    var descriptorBytes = typeDescriptors.Serialize();
                    writer.Write(descriptorBytes, 0, descriptorBytes.Length);

                    // write the length header at the start of this object
                    var length = writer.BaseStream.Length - lengthStartPosition;
                    writer.Seek((int)lengthStartPosition, SeekOrigin.Begin);
                    writer.Write((int)length);
                }
                typeDescriptorBytes = stream.ToArray();
            }

            // prepend the type descriptors
            var newDataBytes = new byte[dataBytes.Length + typeDescriptorBytes.Length];
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
