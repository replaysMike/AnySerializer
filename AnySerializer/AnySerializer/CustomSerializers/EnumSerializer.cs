using System;
using System.Reflection;

namespace AnySerializer.CustomSerializers
{
    public class EnumSerializer : ICustomSerializer
    {
        public short DataSize => sizeof(long);

        public object Deserialize(byte[] bytes, uint length)
        {
            switch (length)
            {
                case 1:
                    return bytes[0];
                case 2:
                    return BitConverter.ToInt16(bytes, 0);
                case 4:
                    return BitConverter.ToInt32(bytes, 0);
                case 8:
                    return BitConverter.ToInt64(bytes, 0);
                default:
                    throw new CustomSerializerException(typeof(EnumSerializer), $"Unknown Enum size - {length} bytes");
            }
        }

        public byte[] Serialize(object type)
        {
            var integralType = type.GetType().GetEnumUnderlyingType();
            var value = Convert.ChangeType(type, integralType);
            if (integralType == typeof(byte))
                return new [] { (byte)value };
            else if (integralType == typeof(sbyte))
                return BitConverter.GetBytes((sbyte)value);
            else if (integralType == typeof(short))
                return BitConverter.GetBytes((short)value);
            else if (integralType == typeof(ushort))
                return BitConverter.GetBytes((ushort)value);
            else if (integralType == typeof(int))
                return BitConverter.GetBytes((int)value);
            else if (integralType == typeof(uint))
                return BitConverter.GetBytes((uint)value);
            else if (integralType == typeof(long))
                return BitConverter.GetBytes((long)value);

            return BitConverter.GetBytes((ulong)value);
        }

        object ICustomSerializer.Deserialize(byte[] bytes, uint length)
        {
            return Deserialize(bytes, length);
        }
    }
}
