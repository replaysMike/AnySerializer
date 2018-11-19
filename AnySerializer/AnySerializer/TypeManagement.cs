using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace AnySerializer
{
    public static class TypeManagement
    {
        public static readonly IDictionary<Type, TypeId> TypeMapping = new Dictionary<Type, TypeId> {
                        { typeof(bool), TypeId.Bool },
                        { typeof(byte), TypeId.Byte },
                        { typeof(sbyte), TypeId.Byte },
                        { typeof(short), TypeId.Short },
                        { typeof(ushort), TypeId.Short },
                        { typeof(int), TypeId.Int },
                        { typeof(uint), TypeId.Int },
                        { typeof(long), TypeId.Long },
                        { typeof(ulong), TypeId.Long },
                        { typeof(float), TypeId.Float },
                        { typeof(double), TypeId.Double },
                        { typeof(decimal), TypeId.Decimal },
                        { typeof(string), TypeId.String },
                        { typeof(char), TypeId.Char },
                        { typeof(Enum), TypeId.Enum },
                        { typeof(object), TypeId.Object },
                        { typeof(Guid), TypeId.Guid },
                        { typeof(DateTime), TypeId.DateTime },
                        { typeof(TimeSpan), TypeId.TimeSpan },
                        { typeof(Point), TypeId.Point },
                        { typeof(Array), TypeId.Array },
                        { typeof(IEnumerable), TypeId.IEnumerable },
                        { typeof(IDictionary<,>), TypeId.IDictionary },
                        { typeof(Tuple<,>), TypeId.Tuple },
                    };

        /// <summary>
        /// The supported serialization data type
        /// </summary>
        public enum TypeId : byte
        {
            Bool = 1,
            Byte,
            Short,
            Int,
            Long,
            Float,
            Double,
            Decimal,
            String,
            Char,
            Enum,
            Object,
            Array,
            IEnumerable,
            IDictionary,
            Guid,
            DateTime,
            TimeSpan,
            Point,
            Tuple
        }
    }
}
