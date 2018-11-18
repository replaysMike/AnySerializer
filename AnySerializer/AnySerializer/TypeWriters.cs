using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
            var currentDepth = 0;
            WriteObject(writer, obj, typeSupport, currentDepth, maxDepth, objectTree, string.Empty, ignoreAttributes);
        }

        internal static void WriteObject(BinaryWriter writer, object obj, TypeSupport typeSupport, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
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
                case TypeUtil.TypeId.Object:
                    TypeWriters.WriteObjectType(writer, lengthStartPosition, obj, typeSupport, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeUtil.TypeId.Array:
                    TypeWriters.WriteArrayType(writer, lengthStartPosition, obj, typeSupport, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeUtil.TypeId.IDictionary:
                    TypeWriters.WriteDictionaryType(writer, lengthStartPosition, obj, typeSupport, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                case TypeUtil.TypeId.IEnumerable:
                    TypeWriters.WriteEnumerableType(writer, lengthStartPosition, obj, typeSupport, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                    break;
                default:
                    TypeWriters.WriteValueType(writer, lengthStartPosition, obj, typeSupport);
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

        internal static void WriteValueType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport)
        {
            var @switch = new Dictionary<Type, Action> {
                        { typeof(bool), () => writer.Write((bool)obj) },
                        { typeof(byte), () => writer.Write((byte)obj) },
                        { typeof(short), () => writer.Write((short)obj) },
                        { typeof(int), () => writer.Write((int)obj) },
                        { typeof(long), () => writer.Write((long)obj) },
                        { typeof(float), () => writer.Write((float)obj) },
                        { typeof(double), () => writer.Write((double)obj) },
                        { typeof(decimal), () => writer.Write((decimal)obj) },
                        { typeof(string), () => 
                            {
                                if(obj != null)
                                    writer.Write((string)obj);
                            }
                        },
                        { typeof(char), () => writer.Write((char)obj) },
                    };

            // write the type bytes
            @switch[typeSupport.Type]();
        }

        internal static void WriteArrayType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var array = (IEnumerable)obj;

            foreach (var item in array)
            {
                WriteObject(writer, item, new TypeSupport(item.GetType()), currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }

        internal static void WriteEnumerableType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var enumerable = (IEnumerable)obj;

            foreach (var item in enumerable)
            {
                WriteObject(writer, item, new TypeSupport(item.GetType()), currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }

        internal static void WriteDictionaryType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var dictionary = (IDictionary)obj;

            foreach (DictionaryEntry item in dictionary)
            {
                // write the key
                WriteObject(writer, item.Key, new TypeSupport(item.Key.GetType()), currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                // write the value
                WriteObject(writer, item.Value, new TypeSupport(item.Value.GetType()), currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }

        internal static void WriteObjectType(BinaryWriter writer, long lengthStartPosition, object obj, TypeSupport typeSupport, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // write each element
            var properties = TypeUtil.GetProperties(obj).OrderBy(x => x.Name);
            var fields = TypeUtil.GetFields(obj).OrderBy(x => x.Name);

            var rootPath = path;
            foreach (var property in properties)
            {
                path = $"{rootPath}.{property.Name}";
                var propertyTypeSupport = new TypeSupport(property.PropertyType);
                if (property.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (propertyTypeSupport.IsDelegate)
                    continue;
                var propertyValue = property.GetValue(obj);
                WriteObject(writer, propertyValue, propertyTypeSupport, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }

            foreach (var field in fields)
            {
                path = $"{rootPath}.{field.Name}";
                var fieldTypeSupport = new TypeSupport(field.FieldType);
                if (field.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (fieldTypeSupport.IsDelegate)
                    continue;
                var fieldValue = field.GetValue(obj);
                WriteObject(writer, fieldValue, fieldTypeSupport, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }
        }
    }
}