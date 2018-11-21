using System;
using System.IO;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    /// <summary>
    /// Validates serialization result data
    /// </summary>
    public class ValidatorInternal
    {
        /// <summary>
        /// Validate a byte array for valid serialization data
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Validate(byte[] bytes)
        {
            var isValid = false;
            try
            {
                TypeDescriptors typeDescriptors = null;
                using (var stream = new MemoryStream(bytes))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        /**
                         * Chunk Format 
                         * [ChunkType]      1 byte (byte)
                         * [ChunkLength]    4 bytes (Int32)
                         * [OptionalTypeDescriptor] 2 bytes (UInt16)
                         * [Data]           [ChunkLength-Int32] bytes
                         * 
                         * Chunks may contain value types or Chunks, it's a recursive structure.
                         * By reading if you've read all of the data bytes, you know you've read 
                         * the whole structure.
                         */
                        isValid = ReadChunk(reader, typeDescriptors);
                    }
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Read a single chunk of data, recursively.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private bool ReadChunk(BinaryReader reader, TypeDescriptors typeDescriptors)
        {
            var isChunkValid = true;
            var objectTypeIdByte = reader.ReadByte();
            var objectTypeId = (TypeId)objectTypeIdByte;
            var isNullValue = TypeUtil.IsNullValue(objectTypeId);
            var isAbstractInterface = TypeUtil.IsAbstractInterface(objectTypeId);
            var isTypeDescriptorMap = TypeUtil.IsTypeDescriptorMap(objectTypeId);

            // strip the flags and get only the type
            var objectTypeIdOnly = TypeUtil.GetTypeId(objectTypeIdByte);
            var isValueType = TypeUtil.IsValueType(objectTypeIdOnly);
            var length = reader.ReadInt32();
            var lengthStartPosition = reader.BaseStream.Position;
            ushort typeDescriptorId = 0;

            // only interfaces can store type descriptors
            if (typeDescriptors != null && isAbstractInterface)
            {
                typeDescriptorId = reader.ReadUInt16();
                if (!typeDescriptors.Contains(typeDescriptorId))
                    return false;
            }

            if (isTypeDescriptorMap)
            {
                var dataLength = length - sizeof(int);
                // read in the type descriptor map
                typeDescriptors = TypeReaders.GetTypeDescriptorMap(reader, dataLength);
                // continue reading the data
                return ReadChunk(reader, typeDescriptors);
            }

            var objectTypeSupport = TypeUtil.GetType(objectTypeIdOnly);

            // value type
            if (length > 0)
            {
                switch (objectTypeId)
                {
                    // these types may contain additional chunks, only value types may not.
                    case TypeId.Array:
                    case TypeId.Tuple:
                    case TypeId.IDictionary:
                    case TypeId.IEnumerable:
                    case TypeId.Object:
                        isChunkValid = ReadChunk(reader, typeDescriptors);
                        break;
                }
                if (!isChunkValid)
                    return false;

                if(reader.BaseStream.Position - lengthStartPosition == 0)
                {
                    // it's a value type, read the full data
                    var data = reader.ReadBytes(length - Constants.LengthHeaderSize);
                }
                else if(reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // read another chunk
                    isChunkValid = ReadChunk(reader, typeDescriptors);
                    if (!isChunkValid)
                        return false;
                }
            }
            return isChunkValid;
        }

    }
}
