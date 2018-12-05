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
                        { typeof(IDictionary), TypeId.IDictionary },
                        { typeof(IDictionary<,>), TypeId.IDictionary },
                        { typeof(Tuple<,>), TypeId.Tuple },
                        { typeof(KeyValuePair<,>), TypeId.KeyValuePair},
                    };

        /// <summary>
        /// The supported serialization data type
        /// </summary>
        public enum TypeId : byte
        {
            // non-flaggable
            None        = 0,
            Bool        = 1,
            Byte        = 2,
            Short       = 3,
            Int         = 4,
            Long        = 5,
            Float       = 6,
            Double      = 7,
            Decimal     = 8,
            String      = 9,
            Char        = 10,
            Enum        = 11,
            Object      = 12,
            Array       = 13,
            IEnumerable = 14,
            IDictionary = 15,
            Guid        = 16,
            DateTime    = 17,
            TimeSpan    = 18,
            Point       = 19,
            Tuple       = 20,
            KeyValuePair= 21,

            // special bit to indicate a type map is stored for this type (concrete types for interfaces, anonymous types)
            TypeMapped = 32,
            // special bit to indicate type is a typedescriptor map
            TypeDescriptorMap = 64,
            // special bit to indicate type value is null
            NullValue   = 128,
        }
    }
}
