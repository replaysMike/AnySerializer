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
                using (var stream = new MemoryStream(bytes))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        isValid = ReadChunk(reader);
                    }
                }
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }

        private bool ReadChunk(BinaryReader reader)
        {
            var isChunkValid = true;
            var objectTypeId = (TypeId)reader.ReadByte();
            var objectTypeSupport = TypeUtil.GetType(objectTypeId);
            var length = reader.ReadInt32();
            var startPosition = reader.BaseStream.Position;

            // value type
            if (length > 0)
            {
                switch (objectTypeId)
                {
                    case TypeId.Object:
                    case TypeId.Array:
                    case TypeId.IDictionary:
                    case TypeId.IEnumerable:
                    case TypeId.Tuple:
                        isChunkValid = ReadChunk(reader);
                        break;
                }
                if (!isChunkValid)
                    return false;

                if(reader.BaseStream.Position - startPosition == 0)
                {
                    // it's a value type
                    var data = reader.ReadBytes(length - Constants.LengthHeaderSize);
                }
                else
                {
                    // read another chunk
                    isChunkValid = ReadChunk(reader);
                    if (!isChunkValid)
                        return false;
                }
            }
            return isChunkValid;
        }

    }
}
