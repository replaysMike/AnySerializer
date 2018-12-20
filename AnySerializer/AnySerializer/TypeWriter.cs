using AnySerializer.CustomSerializers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TypeSupport;
using TypeSupport.Extensions;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    internal class TypeWriter : TypeBase
    {
        private ObjectReferenceTracker _referenceTracker;

        /// <summary>
        /// Write the parent object, and recursively process it's children
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="obj"></param>
        /// <param name="typeSupport"></param>
        /// <param name="maxDepth"></param>
        /// <param name="options">The serialization options</param>
        /// <param name="objectTree"></param>
        /// <param name="ignoreAttributes"></param>
        internal static TypeDescriptors Write(BinaryWriter writer, object obj, ExtendedType typeSupport, int maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, ICollection<string> ignorePropertiesOrPaths = null)
        {
            var currentDepth = 0;

            TypeDescriptors typeDescriptors = null;
            if (options.BitwiseHasFlag(SerializerOptions.EmbedTypes))
                typeDescriptors = new TypeDescriptors();

            var dataSettings = SerializerDataSettings.None;
            if (typeDescriptors != null)
                dataSettings |= SerializerDataSettings.TypeMap;
            if (options.BitwiseHasFlag(SerializerOptions.Compact))
                dataSettings |= SerializerDataSettings.Compact;
            if (options.BitwiseHasFlag(SerializerOptions.Compress))
                dataSettings |= SerializerDataSettings.Compress;
            // write the serializer byte 0, data settings
            writer.Write((byte)dataSettings);

            var typeWriter = new TypeWriter(maxDepth, dataSettings, options, ignoreAttributes, typeDescriptors, ignorePropertiesOrPaths);
            var length = typeWriter.WriteObject(writer, obj, typeSupport, currentDepth, string.Empty);

            return typeDescriptors;
        }

        public TypeWriter(int maxDepth, SerializerDataSettings dataSettings, SerializerOptions options, ICollection<object> ignoreAttributes, TypeDescriptors typeDescriptors, ICollection<string> ignorePropertiesOrPaths = null)
        {
            _maxDepth = maxDepth;
            _dataSettings = dataSettings;
            _options = options;
            _ignoreAttributes = ignoreAttributes;
            _ignorePropertiesOrPaths = ignorePropertiesOrPaths;
            _typeDescriptors = typeDescriptors;
            _referenceTracker = new ObjectReferenceTracker();
            _customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>()
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) }
            };
        }

        internal long WriteObject(BinaryWriter writer, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            TypeId objectTypeId = TypeId.None;
            try
            {
                if (typeSupport.IsStruct)
                    objectTypeId = TypeId.Struct;
                else
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
            var isTypeMapped = _typeDescriptors != null && (typeSupport.IsInterface || typeSupport.IsAnonymous || typeSupport.Type == typeof(object));
            if (isTypeMapped)
                objectTypeIdByte |= (byte)TypeId.TypeMapped;
            var isValueType = TypeUtil.IsValueType(objectTypeId);

            // write the object type being serialized in position 0x00
            writer.Write(objectTypeIdByte);

            // make a note of where this object starts, so we can populate the length header later
            var lengthStartPosition = writer.BaseStream.Position;

            // make room for the length prefix and object reference id
            if (_dataSettings.BitwiseHasFlag(SerializerDataSettings.Compact))
                writer.Seek(Constants.CompactLengthHeaderSize + Constants.ObjectReferenceIdSize + (int)writer.BaseStream.Position, SeekOrigin.Begin);
            else
                writer.Seek(Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + (int)writer.BaseStream.Position, SeekOrigin.Begin);

            // write the optional type descriptor id - only interfaces can store type descriptors
            var containsTypeDescriptorId = false;
            if (isTypeMapped)
            {
                var typeId = _typeDescriptors.AddKnownType(typeSupport);
                writer.Write(typeId);
                containsTypeDescriptorId = true;
            }

            // construct a hashtable of objects we have already inspected (simple recursion loop preventer)
            // we use this hashcode method as it does not use any custom hashcode handlers the object might implement
            ushort objectReferenceId = 0;
            var hashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            var alreadyMapped = _referenceTracker.ContainsHashcode(hashCode);
            // if we already wrote this object, we want to write a reference to it in the data
            if (alreadyMapped)
                objectReferenceId = _referenceTracker.GetObjectReferenceId(hashCode);
            if (hashCode != 0 && !alreadyMapped)
            {
                // ensure we can refer back to the reference for this object
                objectReferenceId = _referenceTracker.AddObject(obj, hashCode);

                switch (objectTypeId)
                {
                    case TypeId.Object:
                    case TypeId.Struct:
                        WriteObjectType(writer, lengthStartPosition, obj, typeSupport, currentDepth, path);
                        break;
                    case TypeId.Array:
                        WriteArrayType(writer, lengthStartPosition, obj, typeSupport, currentDepth, path);
                        break;
                    case TypeId.IDictionary:
                        WriteDictionaryType(writer, lengthStartPosition, obj, typeSupport, currentDepth, path);
                        break;
                    case TypeId.IEnumerable:
                        WriteEnumerableType(writer, lengthStartPosition, obj, typeSupport, currentDepth, path);
                        break;
                    case TypeId.KeyValuePair:
                        WriteKeyValueType(writer, lengthStartPosition, obj, typeSupport, currentDepth, path);
                        break;
                    case TypeId.Tuple:
                        WriteTupleType(writer, lengthStartPosition, obj, typeSupport, currentDepth, path);
                        break;
                    case TypeId.Enum:
                        WriteValueType(writer, lengthStartPosition, obj, new ExtendedType(typeof(Enum)));
                        break;
                    default:
                        WriteValueType(writer, lengthStartPosition, obj, typeSupport);
                        break;
                }
            }

            var currentPosition = writer.BaseStream.Position;
            // write the length header at the start of this object, excluding the objectReferenceId at the end
            var dataLength = (int)(writer.BaseStream.Position - lengthStartPosition - sizeof(ushort));
            // if we wrote a typeDescriptorId, that doesn't apply to the dataLength
            if (containsTypeDescriptorId)
            {
                dataLength -= Constants.ObjectTypeDescriptorId;
            }
            writer.Seek((int)lengthStartPosition, SeekOrigin.Begin);
            if (_dataSettings.BitwiseHasFlag(SerializerDataSettings.Compact))
            {
                if (dataLength > ushort.MaxValue)
                    throw new ExceedsMaxSizeException($"The object type '{typeSupport.Type}' serializes to a data size '{dataLength}' which is greater than supported for Compact mode (max: '{ushort.MaxValue}')");
                writer.Write((ushort)dataLength);
            }
            else
                writer.Write((uint)dataLength);
            // write the object reference Id from the object tree.
            // this is used so we don't have to serialize objects already in the data, we can just reference it's id
            writer.Write(objectReferenceId);
            // reset the position to current
            writer.Seek((int)currentPosition, SeekOrigin.Begin);

            return dataLength;
        }

        internal void WriteValueType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport)
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
                            var bytes = _customSerializers[typeof(Enum)].Value.Serialize((Enum)obj);
                            writer.Write(bytes);
                            }
                        },
                        { typeof(string), () =>
                            {
                                writer.Write((string)obj);
                            }
                        },
                        { typeof(char), () => writer.Write((char)obj) },
                        { typeof(IntPtr), () => 
                            {
                                writer.Write(((IntPtr)obj).ToInt64());
                            }
                        },
                        { typeof(Guid), () => writer.Write(((Guid)obj).ToByteArray()) },
                        { typeof(DateTime), () => writer.Write(((DateTime)obj).ToBinary()) },
                        { typeof(TimeSpan), () => writer.Write(((TimeSpan)obj).Ticks) },
                        { typeof(Point), () =>
                            {
                            var bytes = _customSerializers[typeof(Point)].Value.Serialize((Point)obj);
                            writer.Write(bytes);
                            }
                        },
                    };

            // write the type bytes. If the value is null, don't write it and it will receive a zero length instruction
            if (obj != null)
                @switch[typeSupport.NullableBaseType]();
        }

        internal void WriteArrayType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            // write each element
            var array = (IEnumerable)obj;

            var elementExtendedType = new ExtendedType(typeSupport.ElementType);
            ExtendedType elementConcreteExtendedType = null;
            foreach (var item in array)
            {
                if (item != null && elementConcreteExtendedType == null)
                    elementConcreteExtendedType = item.GetType().GetExtendedType();
                WriteObject(writer, item, elementConcreteExtendedType ?? elementExtendedType, currentDepth, path);
            }
        }

        internal void WriteEnumerableType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            // write each element
            var enumerable = (IEnumerable)obj;

            // special case for stack types, write the data in reverse order
            if (typeSupport.IsGeneric)
            {
                if (typeSupport.Type.GetGenericTypeDefinition() == typeof(ConcurrentBag<>)
                    || typeSupport.Type.GetGenericTypeDefinition() == typeof(ConcurrentStack<>)
                    || typeSupport.Type.GetGenericTypeDefinition() == typeof(Stack<>)
                )
                {
                    enumerable = Enumerable.Reverse((IEnumerable<object>)obj);
                }
            }

            var elementExtendedType = new ExtendedType(typeSupport.ElementType);
            ExtendedType elementConcreteExtendedType = null;
            foreach (var item in enumerable)
            {
                if (item != null && elementConcreteExtendedType == null)
                    elementConcreteExtendedType = item.GetType().GetExtendedType();
                WriteObject(writer, item, elementConcreteExtendedType ?? elementExtendedType, currentDepth, path);
            }
        }

        internal void WriteTupleType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
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
                WriteObject(writer, item, valueExtendedTypes[index], currentDepth, path);
                index++;
            }
        }

        internal void WriteDictionaryType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            // write each element
            var dictionary = (IDictionary)obj;

            var keyExtendedType = typeSupport.GenericArgumentTypes.First().GetExtendedType();
            var valueExtendedType = typeSupport.GenericArgumentTypes.Skip(1).First().GetExtendedType();
            ExtendedType valueConcreteExtendedType = null;
            foreach (DictionaryEntry item in dictionary)
            {
                // write the key
                WriteObject(writer, item.Key, keyExtendedType, currentDepth, path);
                // write the value
                if (item.Value != null && valueConcreteExtendedType == null)
                    valueConcreteExtendedType = item.Value.GetType().GetExtendedType();
                WriteObject(writer, item.Value, valueConcreteExtendedType ?? valueExtendedType, currentDepth, path);
            }
        }

        internal void WriteKeyValueType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            var keyExtendedType = typeSupport.GenericArgumentTypes.First().GetExtendedType();
            var valueExtendedType = typeSupport.GenericArgumentTypes.Skip(1).First().GetExtendedType();
            ExtendedType valueConcreteExtendedType = null;

            var key = obj.GetPropertyValue("Key");
            var value = obj.GetPropertyValue("Value");
            // write the key
            WriteObject(writer, key, keyExtendedType, currentDepth, path);
            // write the value
            if (value != null && valueConcreteExtendedType == null)
                valueConcreteExtendedType = value.GetType().GetExtendedType();
            WriteObject(writer, value, valueConcreteExtendedType ?? valueExtendedType, currentDepth, path);
        }

        internal void WriteObjectType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            // write each element
            var fields = obj.GetFields(FieldOptions.AllWritable).OrderBy(x => x.Name);

            var rootPath = path;
            foreach (var field in fields)
            {
                path = $"{rootPath}.{field.ReflectedType.Name}.{field.Name}";
                if (path == ".XObject.parent")
                    System.Diagnostics.Debugger.Break();
                var fieldExtendedType = new ExtendedType(field.Type);
                var fieldValue = obj.GetFieldValue(field);
                fieldExtendedType.SetConcreteTypeFromInstance(fieldValue);
                ExtendedType concreteFieldExtendedType = null;
                if (fieldExtendedType.ConcreteType != null && fieldExtendedType.Type != fieldExtendedType.ConcreteType)
                    concreteFieldExtendedType = new ExtendedType(fieldExtendedType.ConcreteType);

                // a special condition for writing anonymous types
                if (concreteFieldExtendedType?.IsAnonymous == true)
                    fieldExtendedType = concreteFieldExtendedType;

                if (fieldExtendedType.IsDelegate)
                    continue;

                // check for ignore attributes
                if (IgnoreObjectName(field.Name, path, field.CustomAttributes))
                    continue;
                // also check the property for ignore, if this is a auto-backing property
                if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.BackedPropertyName}", field.BackedProperty.CustomAttributes))
                    continue;

                WriteObject(writer, fieldValue, fieldExtendedType, currentDepth, path);
            }
        }
    }
}