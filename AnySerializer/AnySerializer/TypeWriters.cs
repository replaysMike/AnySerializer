using AnySerializer.CustomSerializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TypeSupport;
using TypeSupport.Extensions;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    internal class TypeWriters
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
        /// <param name="useTypeDescriptors">True to embed a type descriptor map in the serialized data</param>
        internal static TypeDescriptors Write(BinaryWriter writer, object obj, ExtendedType typeSupport, int maxDepth, ICollection<Type> ignoreAttributes, bool useTypeDescriptors = false)
        {
            var customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>()
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) }
            };
            var currentDepth = 0;

            TypeDescriptors typeDescriptors = null;
            if(useTypeDescriptors) typeDescriptors = new TypeDescriptors();

            var referenceTracker = new ObjectReferenceTracker();

            var typeWriter = new TypeWriters();
            var length = typeWriter.WriteObject(writer, obj, typeSupport, customSerializers, currentDepth, maxDepth, referenceTracker, string.Empty, ignoreAttributes, typeDescriptors);

            return typeDescriptors;
        }

        internal long WriteObject(BinaryWriter writer, object obj, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, ObjectReferenceTracker referenceTracker, string path, ICollection<Type> ignoreAttributes, TypeDescriptors typeDescriptors)
        {
            TypeId objectTypeId = TypeId.None;
            try
            {
                objectTypeId = TypeUtil.GetTypeId(typeSupport);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"[{path}] {ex.Message}", ex);
            }

            byte objectTypeIdByte = (byte)objectTypeId;
            // if the object is null, indicate so in the type mask
            if (obj == null)
                objectTypeIdByte |= (byte)TypeId.NullValue;
            // if the object type is an interface, indicate so in the type mask
            var isTypeMapped = typeDescriptors != null && (typeSupport.IsInterface || typeSupport.IsAnonymous);
            if (isTypeMapped)
                objectTypeIdByte |= (byte)TypeId.TypeMapped;
            var isValueType = TypeUtil.IsValueType(objectTypeId);

            // write the object type being serialized in position 0x00
            writer.Write(objectTypeIdByte);

            // make a note of where this object starts, so we can populate the length header later
            var lengthStartPosition = writer.BaseStream.Position;

            // make room for the length prefix and object reference id
            writer.Seek(Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + (int)writer.BaseStream.Position, SeekOrigin.Begin);

            // write the optional type descriptor id - only interfaces can store type descriptors
            var containsTypeDescriptorId = false;
            if (isTypeMapped)
            {
                var typeId = typeDescriptors.AddKnownType(typeSupport);
                writer.Write((ushort)typeId);
                containsTypeDescriptorId = true;
            }

            // construct a hashtable of objects we have already inspected (simple recursion loop preventer)
            // we use this hashcode method as it does not use any custom hashcode handlers the object might implement
            ushort objectReferenceId = 0;
            var hashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            var alreadyMapped = referenceTracker.ContainsHashcode(hashCode);
            // if we already wrote this object, we want to write a reference to it in the data
            if (alreadyMapped)
                objectReferenceId = referenceTracker.GetObjectReferenceId(hashCode);
            if (hashCode != 0 && !alreadyMapped)
            {
                // ensure we can refer back to the reference for this object
                objectReferenceId = referenceTracker.AddObject(obj, hashCode);

                switch (objectTypeId)
                {
                    case TypeId.Object:
                        WriteObjectType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
                        break;
                    case TypeId.Array:
                        WriteArrayType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
                        break;
                    case TypeId.IDictionary:
                        WriteDictionaryType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
                        break;
                    case TypeId.IEnumerable:
                        WriteEnumerableType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
                        break;
                    case TypeId.Tuple:
                        WriteTupleType(writer, lengthStartPosition, obj, typeSupport, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
                        break;
                    case TypeId.Enum:
                        WriteValueType(writer, lengthStartPosition, obj, new ExtendedType(typeof(Enum)), customSerializers);
                        break;
                    default:
                        WriteValueType(writer, lengthStartPosition, obj, typeSupport, customSerializers);
                        break;
                }
            }

            var currentPosition = writer.BaseStream.Position;
            // write the length header at the start of this object, excluding the objectReferenceId at the end
            var dataLength = writer.BaseStream.Position - lengthStartPosition - sizeof(ushort);
            // if we wrote a typeDescriptorId, that doesn't apply to the dataLength
            if (containsTypeDescriptorId)
                dataLength -= sizeof(ushort);
            writer.Seek((int)lengthStartPosition, SeekOrigin.Begin);
            writer.Write((int)dataLength);
            // write the object reference Id from the object tree.
            // this is used so we don't have to serialize objects already in the data, we can just reference it's id
            writer.Write((ushort)objectReferenceId);
            // reset the position to current
            writer.Seek((int)currentPosition, SeekOrigin.Begin);

            return dataLength;
        }

        internal void WriteValueType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers)
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
            if (obj != null)
                @switch[typeSupport.NullableBaseType]();
        }

        internal void WriteArrayType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, ObjectReferenceTracker referenceTracker, string path, ICollection<Type> ignoreAttributes, TypeDescriptors typeDescriptors)
        {
            // write each element
            var array = (IEnumerable)obj;

            var elementExtendedType = new ExtendedType(typeSupport.ElementType);
            ExtendedType elementConcreteExtendedType = null;
            foreach (var item in array)
            {
                if (item != null && elementConcreteExtendedType == null)
                    elementConcreteExtendedType = item.GetType().GetExtendedType();
                WriteObject(writer, item, elementConcreteExtendedType ?? elementExtendedType, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
            }
        }

        internal void WriteEnumerableType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, ObjectReferenceTracker referenceTracker, string path, ICollection<Type> ignoreAttributes, TypeDescriptors typeDescriptors)
        {
            // write each element
            var enumerable = (IEnumerable)obj;

            var elementExtendedType = new ExtendedType(typeSupport.ElementType);
            ExtendedType elementConcreteExtendedType = null;
            foreach (var item in enumerable)
            {
                if (item != null && elementConcreteExtendedType == null)
                    elementConcreteExtendedType = item.GetType().GetExtendedType();
                WriteObject(writer, item, elementConcreteExtendedType ?? elementExtendedType, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
            }
        }

        internal void WriteTupleType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, ObjectReferenceTracker referenceTracker, string path, ICollection<Type> ignoreAttributes, TypeDescriptors typeDescriptors)
        {
            // write each element, treat a tuple as a list of objects
            var enumerable = new List<object>();
            if (typeSupport.IsValueTuple)
                enumerable = obj.GetValueTupleItemObjects();
            else if (typeSupport.IsTuple)
                enumerable = obj.GetTupleItemObjects();

            var valueExtendedTypes = typeSupport.GenericArgumentTypes.Select(x => x.GetExtendedType()).ToArray();
            var index = 0;
            foreach (var item in enumerable)
            {
                WriteObject(writer, item, valueExtendedTypes[index], customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
                index++;
            }
        }

        internal void WriteDictionaryType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, ObjectReferenceTracker referenceTracker, string path, ICollection<Type> ignoreAttributes, TypeDescriptors typeDescriptors)
        {
            // write each element
            var dictionary = (IDictionary)obj;

            var keyExtendedType = typeSupport.GenericArgumentTypes.First().GetExtendedType();
            var valueExtendedType = typeSupport.GenericArgumentTypes.Skip(1).First().GetExtendedType();
            ExtendedType valueConcreteExtendedType = null;
            foreach (DictionaryEntry item in dictionary)
            {
                // write the key
                WriteObject(writer, item.Key, keyExtendedType, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
                // write the value
                if (item.Value != null && valueConcreteExtendedType == null)
                    valueConcreteExtendedType = item.Value.GetType().GetExtendedType();
                WriteObject(writer, item.Value, valueConcreteExtendedType ?? valueExtendedType, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
            }
        }

        internal void WriteObjectType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, ObjectReferenceTracker referenceTracker, string path, ICollection<Type> ignoreAttributes, TypeDescriptors typeDescriptors)
        {
            // write each element
            var fields = obj.GetFields(FieldOptions.AllWritable).OrderBy(x => x.Name);

            var rootPath = path;
            foreach (var field in fields)
            {
                path = $"{rootPath}.{field.Name}";
                if (field.Name == "_tablePlayers")
                    System.Diagnostics.Debugger.Break();
                var fieldExtendedType = new ExtendedType(field.Type);
                var fieldValue = obj.GetFieldValue(field);
                fieldExtendedType.SetConcreteTypeFromInstance(fieldValue);
                if (field.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (fieldExtendedType.IsDelegate)
                    continue;
                if (field.BackedProperty != null && field.BackedProperty.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                WriteObject(writer, fieldValue, fieldExtendedType, customSerializers, currentDepth, maxDepth, referenceTracker, path, ignoreAttributes, typeDescriptors);
            }
        }

        internal class ObjectReferenceTracker
        {
            private ushort _referenceId = 0;
            private Dictionary<int, (object, ushort)> _objectTree = new Dictionary<int, (object, ushort)>();

            internal ushort GetNextReferenceId()
            {
                return _referenceId;
            }

            internal ushort AddObject(object obj, int hashCode)
            {
                _objectTree.Add(hashCode, (obj, _referenceId));
                return _referenceId++;
            }

            internal bool ContainsHashcode(int hashCode)
            {
                return _objectTree.ContainsKey(hashCode);
            }

            internal ushort GetObjectReferenceId(int hashCode)
            {
                return _objectTree[hashCode].Item2;
            }

            internal object GetObject(int hashCode)
            {
                return _objectTree[hashCode].Item1;
            }
        }
    }
}