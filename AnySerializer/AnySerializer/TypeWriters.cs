using AnySerializer.CustomSerializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    internal static class TypeWriters
    {
        /// <summary>
        /// Write the parent object, and recursively process it's children
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="obj"></param>
        /// <param name="typeSupport"></param>
        /// <param name="maxDepth"></param>
        /// <param name="objectTree"></param>
        /// <param name="ignoreAttributes"></param>
        internal static void Write(BinaryWriter writer, object obj, TypeSupport typeSupport, int maxDepth, IDictionary<int, object> objectTree, ICollection<Type> ignoreAttributes)
        {
            var customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>()
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) }
            };
            var currentDepth = 0;
            WriteObject(writer, obj, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, string.Empty, ignoreAttributes);
        }

        internal static void WriteObject(BinaryWriter writer, object obj, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // construct a hashtable of objects we have already inspected (simple recursion loop preventer)
            // we use this hashcode method as it does not use any custom hashcode handlers the object might implement
            var hashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            if (objectTree.ContainsKey(hashCode))
                return;

            // ensure we can refer back to the reference for this object
            objectTree.Add(hashCode, obj);

            var objectTypeId = TypeUtil.GetTypeId(typeSupport);
            // write the object type being serialized in position 0x00
            writer.Write((byte)objectTypeId);

            // make a note of where this object starts, so we can populate the length header later
            var lengthStartPosition = writer.BaseStream.Position;

            // make room for the length prefix
            writer.Seek(Constants.LengthHeaderSize + (int)writer.BaseStream.Position, SeekOrigin.Begin);

            switch (objectTypeId)
            {
                case TypeId.Object:
                    TypeWriters.WriteObjectType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeId.Array:
                    TypeWriters.WriteArrayType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeId.IDictionary:
                    TypeWriters.WriteDictionaryType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeId.IEnumerable:
                    TypeWriters.WriteEnumerableType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeId.Tuple:
                    TypeWriters.WriteTupleType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeId.Enum:
                    TypeWriters.WriteValueType(writer, lengthStartPosition, obj, new TypeSupport(typeof(Enum)), customSerializers);
                    break;
                default:
                    TypeWriters.WriteValueType(writer, lengthStartPosition, obj, typeSupport, customSerializers);
                    break;
            }

            var currentPosition = writer.BaseStream.Position;
            // write the length header at the start of this object
            var length = writer.BaseStream.Length - lengthStartPosition;
            writer.Seek((int)lengthStartPosition, SeekOrigin.Begin);
            writer.Write((int)length);
            // reset the position to current
            writer.Seek((int)currentPosition, SeekOrigin.Begin);
        }

        internal static void WriteValueType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers)
        {
            var @switch = new Dictionary<Type, Action> {
                        { typeof(bool), () => writer.Write((bool)obj) },
                        { typeof(byte), () => writer.Write((byte)obj) },
                        { typeof(sbyte), () => writer.Write((sbyte)obj) },
                        { typeof(short), () => writer.Write((short)obj) },
                        { typeof(ushort), () => writer.Write((ushort)obj) },
                        { typeof(int), () => writer.Write((int)obj) },
                        { typeof(uint), () => writer.Write((uint)obj) },
                        { typeof(long), () => writer.Write((long)obj) },
                        { typeof(ulong), () => writer.Write((ulong)obj) },
                        { typeof(float), () => writer.Write((float)obj) },
                        { typeof(double), () => writer.Write((double)obj) },
                        { typeof(decimal), () => writer.Write((decimal)obj) },
                        { typeof(Enum), () =>
                            {
                            var bytes = customSerializers[typeof(Enum)].Value.Serialize((Enum)obj);
                            writer.Write(bytes);
                            }
                        },
                        { typeof(string), () =>
                            {
                                if(obj != null)
                                    writer.Write((string)obj);
                            }
                        },
                        { typeof(char), () => writer.Write((char)obj) },
                        { typeof(Guid), () => writer.Write(((Guid)obj).ToByteArray()) },
                        { typeof(DateTime), () => writer.Write(((DateTime)obj).ToBinary()) },
                        { typeof(TimeSpan), () => writer.Write(((TimeSpan)obj).Ticks) },
                        { typeof(Point), () => 
                            {
                            var bytes = customSerializers[typeof(Point)].Value.Serialize((Point)obj);
                            writer.Write(bytes);
                            }
                        },
                    };

            // write the type bytes. If the value is null, don't write it and it will receive a zero length instruction
            if(obj != null)
                @switch[typeSupport.NullableBaseType]();
        }

        internal static void WriteArrayType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var array = (IEnumerable)obj;

            var elementTypeSupport = new TypeSupport(typeSupport.ElementType);
            TypeSupport elementConcreteTypeSupport = null;
            foreach (var item in array)
            {
                if (item != null && elementConcreteTypeSupport == null)
                    elementConcreteTypeSupport = item.GetType().TypeSupport();
                WriteObject(writer, item, elementConcreteTypeSupport ?? elementTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }

        internal static void WriteEnumerableType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var enumerable = (IEnumerable)obj;

            var elementTypeSupport = new TypeSupport(typeSupport.ElementType);
            TypeSupport elementConcreteTypeSupport = null;
            foreach (var item in enumerable)
            {
                if (item != null && elementConcreteTypeSupport == null)
                    elementConcreteTypeSupport = item.GetType().TypeSupport();
                WriteObject(writer, item, elementConcreteTypeSupport ?? elementTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }

        internal static void WriteTupleType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element, treat a tuple as a list of objects
            List<object> enumerable = new List<object>();
            if (typeSupport.IsValueTuple)
                enumerable = obj.GetValueTupleItemObjects();
            else if (typeSupport.IsTuple)
                enumerable = obj.GetTupleItemObjects();

            var valueTypeSupports = typeSupport.GenericArgumentTypes.Select(x => x.TypeSupport()).ToArray();
            var index = 0;
            foreach (var item in enumerable)
            {
                WriteObject(writer, item, valueTypeSupports[index], customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                index++;
            }
        }

        internal static void WriteDictionaryType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var dictionary = (IDictionary)obj;

            var keyTypeSupport = typeSupport.GenericArgumentTypes.First().TypeSupport();
            var valueTypeSupport = typeSupport.GenericArgumentTypes.Skip(1).First().TypeSupport();
            TypeSupport valueConcreteTypeSupport = null;
            foreach (DictionaryEntry item in dictionary)
            {
                // write the key
                WriteObject(writer, item.Key, keyTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                // write the value
                if (item.Value != null && valueConcreteTypeSupport == null)
                    valueConcreteTypeSupport = item.Value.GetType().TypeSupport();
                WriteObject(writer, item.Value, valueConcreteTypeSupport ?? valueTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }

        internal static void WriteObjectType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var properties = TypeUtil.GetProperties(obj).OrderBy(x => x.Name);
            var fields = TypeUtil.GetFields(obj).OrderBy(x => x.Name);

            var rootPath = path;
            foreach (var property in properties)
            {
                path = $"{rootPath}.{property.Name}";
                var propertyTypeSupport = new TypeSupport(property.PropertyType);
                var concretePropertyTypeSupport = TypeUtil.GetProperty(obj, property.Name);
                var propertyType = new TypeSupport(concretePropertyTypeSupport.PropertyType);

                var propertyValue = property.GetValue(obj);
                TypeSupport propertyConcreteTypeSupport = null;
                if(propertyValue != null)
                    propertyConcreteTypeSupport = propertyValue.GetType().TypeSupport();
                if (property.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (propertyTypeSupport.IsDelegate)
                    continue;
                WriteObject(writer, propertyValue, propertyConcreteTypeSupport ?? propertyTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }

            foreach (var field in fields)
            {
                path = $"{rootPath}.{field.Name}";
                var fieldTypeSupport = new TypeSupport(field.FieldType);
                var concreteFieldTypeSupport = TypeUtil.GetField(obj, field.Name);
                if (field.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (fieldTypeSupport.IsDelegate)
                    continue;
                var fieldValue = field.GetValue(obj);
                TypeSupport fieldConcreteTypeSupport = null;
                if (fieldValue != null)
                    fieldConcreteTypeSupport = fieldValue.GetType().TypeSupport();
                WriteObject(writer, fieldValue, fieldConcreteTypeSupport ?? fieldTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }
    }
}