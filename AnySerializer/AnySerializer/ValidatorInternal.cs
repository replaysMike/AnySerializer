﻿using System;
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
            var objectTypeId = (TypeId)reader.ReadByte();
            var objectTypeSupport = TypeUtil.GetType(objectTypeId);
            var length = reader.ReadInt32();
            var lengthStartPosition = reader.BaseStream.Position;
            ushort typeDescriptorId = 0;

            // read in the type descriptor id, if it's not a value type
            if(typeDescriptors != null && !TypeUtil.IsValueType(objectTypeId))
            {
                typeDescriptorId = reader.ReadUInt16();
                if (!typeDescriptors.Contains(typeDescriptorId))
                    return false;
            }

            if (TypeUtil.IsTypeDescriptorMap(objectTypeId))
            {
                // read in the type descriptor map
                typeDescriptors = TypeReaders.GetTypeDescriptorMap(reader, length);
                // continue reading the data
                return ReadChunk(reader, typeDescriptors);
            }

            // value type
            if (length > 0)
            {
                switch (objectTypeId)
                {
                    // these types may contain additional chunks, only value types may not.
                    case TypeId.Object:
                    case TypeId.Array:
                    case TypeId.IDictionary:
                    case TypeId.IEnumerable:
                    case TypeId.Tuple:
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
                else
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
