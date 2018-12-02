using System;
using System.Drawing;

namespace AnySerializer.CustomSerializers
{
    public class PointSerializer : ICustomSerializer<Point>
    {
        public short DataSize => sizeof(long);

        public Point Deserialize(byte[] bytes, uint length)
        {
            var res = BitConverter.ToInt64(bytes, 0);
            var y = (int)(res & uint.MaxValue);
            var x = (int)(res >> 32);

            return new Point(x, y);
        }

        public byte[] Serialize(Point type)
        {
            ulong res = (uint)type.X;
            res = (res << 32);
            res = res | (ulong)(uint)type.Y;
            return BitConverter.GetBytes(res);
        }

        public byte[] Serialize(object type)
        {
            return Serialize((Point)type);
        }

        object ICustomSerializer.Deserialize(byte[] bytes, uint length)
        {
            return Deserialize(bytes, length);
        }
    }
}
