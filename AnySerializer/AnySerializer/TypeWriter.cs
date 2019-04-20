using AnySerializer.CustomSerializers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TypeSupport;
using TypeSupport.Extensions;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    internal class TypeWriter : TypeBase
    {
        private readonly ObjectReferenceTracker _referenceTracker;

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
        internal static TypeDescriptors Write(BinaryWriter writer, object obj, ExtendedType typeSupport, uint maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, out string diagnosticLog, ICollection<string> ignorePropertiesOrPaths = null)
        {
            var currentDepth = 0;
            diagnosticLog = string.Empty;
            TypeDescriptors typeDescriptors = null;
            if (options.BitwiseHasFlag(SerializerOptions.EmbedTypes))
                typeDescriptors = new TypeDescriptors();

            var dataSettings = SerializerDataSettings.None;
            if (typeDescriptors != null)
            {
                dataSettings |= SerializerDataSettings.TypeMap;
            }
            if (options.BitwiseHasFlag(SerializerOptions.Compact))
            {
                dataSettings |= SerializerDataSettings.Compact;
            }
            if (options.BitwiseHasFlag(SerializerOptions.Compress))
            {
                dataSettings |= SerializerDataSettings.Compress;
            }
            // write the serializer byte 0, data settings
            writer.Write((byte)dataSettings);

            var typeWriter = new TypeWriter(maxDepth, dataSettings, options, ignoreAttributes, typeDescriptors, ignorePropertiesOrPaths);
            typeWriter.WriteObject(writer, obj, typeSupport, currentDepth, string.Empty, 0);
            if (options.BitwiseHasFlag(SerializerOptions.WriteDiagnosticLog))
            {
                diagnosticLog = typeWriter.GetDiagnosticLog();
            }
            return typeDescriptors;
        }

        public TypeWriter(uint maxDepth, SerializerDataSettings dataSettings, SerializerOptions options, ICollection<object> ignoreAttributes, TypeDescriptors typeDescriptors, ICollection<string> ignorePropertiesOrPaths = null)
        {
            _debugWriter = new DebugReportWriter();
            _maxDepth = maxDepth;
            _dataSettings = dataSettings;
            _options = options;
            _ignoreAttributes = ignoreAttributes;
            _ignorePropertiesOrPaths = ignorePropertiesOrPaths;
            _typeDescriptors = typeDescriptors;
            _referenceTracker = new ObjectReferenceTracker();
            _customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) },
                { typeof(XDocument), new Lazy<ICustomSerializer>(() => new XDocumentSerializer()) },
            };
        }

        internal long WriteObject(BinaryWriter writer, object obj, ExtendedType typeSupport, int currentDepth, string path, int index)
        {
            // if (typeSupport.Name == "StandardCard" || path == ".Table`1.<GameRound>k__BackingField.TexasHoldemPokerGame.<GamePots>k__BackingField.StandardCard._rank")
            //     System.Diagnostics.Debugger.Break();
            // increment the current recursion depth
            currentDepth++;

            var isTypeMapped = false;
            TypeId objectTypeId = TypeId.None;
            var newTypeSupport = typeSupport;
            try
            {
                objectTypeId = TypeUtil.GetTypeId(newTypeSupport);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"[{path}] {ex.Message}", ex);
            }

            // if the object type is not a concrete type, indicate so in the type mask
            isTypeMapped = _typeDescriptors != null && !newTypeSupport.IsConcreteType;
            // also resolve the concrete type as it may require being typemapped
            if (_typeDescriptors != null && newTypeSupport.ConcreteType != null && newTypeSupport.Type != newTypeSupport.ConcreteType && !newTypeSupport.IsConcreteType)
            {
                // a special condition for writing anonymous types and types without implementation or concrete type
                newTypeSupport = new ExtendedType(newTypeSupport.ConcreteType);
                isTypeMapped = true;
                objectTypeId = TypeUtil.GetTypeId(newTypeSupport);
            }

            // if we couldn't resolve a concrete type, don't map it
            if (isTypeMapped && newTypeSupport.Type == typeof(object))
                isTypeMapped = false;

            byte objectTypeIdByte = (byte)objectTypeId;
            // if the object is null, indicate so in the type mask
            if (obj == null)
                objectTypeIdByte |= (byte)TypeId.NullValue;
            if (isTypeMapped)
                objectTypeIdByte |= (byte)TypeId.TypeMapped;

            // write the object type being serialized in position 0x00
            writer.Write(objectTypeIdByte);

            // make a note of where this object starts, so we can populate the length header later
            var lengthStartPosition = writer.BaseStream.Position;

            // make room for the length prefix and object reference id
            if (_dataSettings.BitwiseHasFlag(SerializerDataSettings.Compact))
                writer.Seek((int)(Constants.CompactLengthHeaderSize + Constants.ObjectReferenceIdSize + (int)writer.BaseStream.Position), SeekOrigin.Begin);
            else
                writer.Seek((int)(Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + (int)writer.BaseStream.Position), SeekOrigin.Begin);

            // write the optional type descriptor id - only interfaces can store type descriptors
            var containsTypeDescriptorId = false;
            ushort typeDescriptorId = 0;
            if (isTypeMapped)
            {
                typeDescriptorId = _typeDescriptors.AddKnownType(newTypeSupport);
                writer.Write(typeDescriptorId);
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

                // custom types support
                var @switch = new Dictionary<Type, Action>
                {
                    { typeof(XDocument), () => WriteValueType(writer, lengthStartPosition, obj, newTypeSupport) },
                };

                if (@switch.ContainsKey(newTypeSupport.Type))
                    @switch[newTypeSupport.Type]();
                else
                {
                    switch (objectTypeId)
                    {
                        case TypeId.Object:
                            WriteObjectType(writer, lengthStartPosition, obj, newTypeSupport, currentDepth, path);
                            break;
                        case TypeId.Struct:
                            WriteStructType(writer, lengthStartPosition, obj, newTypeSupport, currentDepth, path);
                            break;
                        case TypeId.Array:
                            WriteArrayType(writer, lengthStartPosition, obj, newTypeSupport, currentDepth, path);
                            break;
                        case TypeId.IDictionary:
                            WriteDictionaryType(writer, lengthStartPosition, obj, newTypeSupport, currentDepth, path);
                            break;
                        case TypeId.IEnumerable:
                            WriteEnumerableType(writer, lengthStartPosition, obj, newTypeSupport, currentDepth, path);
                            break;
                        case TypeId.KeyValuePair:
                            WriteKeyValueType(writer, lengthStartPosition, obj, newTypeSupport, currentDepth, path);
                            break;
                        case TypeId.Tuple:
                            WriteTupleType(writer, lengthStartPosition, obj, newTypeSupport, currentDepth, path);
                            break;
                        case TypeId.Enum:
                            WriteValueType(writer, lengthStartPosition, obj, new ExtendedType(typeof(Enum)));
                            break;
                        default:
                            WriteValueType(writer, lengthStartPosition, obj, newTypeSupport);
                            break;
                    }
                }
            }

            var currentPosition = writer.BaseStream.Position;
            // write the length header at the start of this object, excluding the objectReferenceId at the end
            var dataLength = (int)(writer.BaseStream.Position - lengthStartPosition - sizeof(ushort));
            // if we wrote a typeDescriptorId, that doesn't apply to the dataLength
            if (containsTypeDescriptorId)
            {
                dataLength -= (int)Constants.ObjectTypeDescriptorId;
            }
            WriteDebugBuilder(writer.BaseStream.Position, typeSupport, objectTypeId, currentDepth, path, index, dataLength, objectReferenceId, typeDescriptorId, hashCode);
            writer.Seek((int)lengthStartPosition, SeekOrigin.Begin);
            if (_dataSettings.BitwiseHasFlag(SerializerDataSettings.Compact))
            {
                if (dataLength > ushort.MaxValue)
                    throw new ExceedsMaxSizeException($"The object type '{newTypeSupport.Type}' serializes to a data size '{dataLength}' which is greater than supported for Compact mode (max: '{ushort.MaxValue}')");
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
                        { typeof(XDocument), () =>
                            {
                            var bytes = _customSerializers[typeof(XDocument)].Value.Serialize((XDocument)obj);
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
            var arrayEnumerable = (IEnumerable)obj;
            var array = (Array)obj;

            var elementExtendedType = new ExtendedType(typeSupport.ElementType);
            ExtendedType elementConcreteExtendedType = null;
            var index = 0;
            // calculate the dimensions of the array
            var rank = (uint)array.Rank;
            // write out the total number of dimensions
            writer.Write(rank);
            // write the length of each dimension
            for (var i = 0; i < rank; i++)
            {
                var dimensionSize = (uint)array.GetLength(i);
                writer.Write(dimensionSize);
            }
            // this will flatten a multidimensional array into a single list of values
            // we will need to know the dimensions (above) in order to restore it
            foreach (var item in arrayEnumerable)
            {
                if (item != null && elementConcreteExtendedType == null)
                    elementConcreteExtendedType = item.GetType().GetExtendedType();
                WriteObject(writer, item, elementConcreteExtendedType ?? elementExtendedType, currentDepth, path, index);
                index++;
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
            var index = 0;
            foreach (var item in enumerable)
            {
                if (item != null && elementConcreteExtendedType == null)
                    elementConcreteExtendedType = item.GetType().GetExtendedType();
                WriteObject(writer, item, elementConcreteExtendedType ?? elementExtendedType, currentDepth, path, index);
                index++;
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
                WriteObject(writer, item, valueExtendedTypes[index], currentDepth, path, index);
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
            var index = 0;
            foreach (DictionaryEntry item in dictionary)
            {
                // write the key
                WriteObject(writer, item.Key, keyExtendedType, currentDepth, path, index);
                // write the value
                if (item.Value != null && valueConcreteExtendedType == null)
                    valueConcreteExtendedType = item.Value.GetType().GetExtendedType();
                WriteObject(writer, item.Value, valueConcreteExtendedType ?? valueExtendedType, currentDepth, path, index);
                index++;
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
            WriteObject(writer, key, keyExtendedType, currentDepth, path, 0);
            // write the value
            valueConcreteExtendedType = value?.GetType().GetExtendedType();
            WriteObject(writer, value, valueConcreteExtendedType ?? valueExtendedType, currentDepth, path, 0);
        }

        internal void WriteStructType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            // write each element
            var fields = obj.GetFields(FieldOptions.AllWritable).Where(x => !x.FieldInfo.IsStatic).OrderBy(x => x.Name);

            var rootPath = path;
            var localPath = string.Empty;
            var index = 0;
            foreach (var field in fields)
            {
                localPath = $"{rootPath}.{field.ReflectedType.Name}.{field.Name}";
                var fieldExtendedType = new ExtendedType(field.Type);
                var fieldValue = obj.GetFieldValue(field);
                fieldExtendedType.SetConcreteTypeFromInstance(fieldValue);

                if (fieldExtendedType.IsDelegate)
                    continue;

                // check for ignore attributes
                if (IgnoreObjectName(field.Name, localPath, field.CustomAttributes))
                    continue;
                // also check the property for ignore, if this is a auto-backing property
                if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.ReflectedType.Name}.{field.BackedPropertyName}", field.BackedProperty.CustomAttributes))
                    continue;

                WriteObject(writer, fieldValue, fieldExtendedType, currentDepth, localPath, index);
                index++;
            }
        }

        internal void WriteObjectType(BinaryWriter writer, long lengthStartPosition, object obj, ExtendedType typeSupport, int currentDepth, string path)
        {
            // write each element
            var fields = obj.GetFields(FieldOptions.AllWritable).OrderBy(x => x.Name);

            var rootPath = path;
            var localPath = string.Empty;
            var index = 0;
            foreach (var field in fields)
            {
                localPath = $"{rootPath}.{field.ReflectedType.Name}.{field.Name}";
                var fieldExtendedType = new ExtendedType(field.Type);
                var fieldValue = obj.GetFieldValue(field);
                fieldExtendedType.SetConcreteTypeFromInstance(fieldValue);

                if (fieldExtendedType.IsDelegate)
                    continue;

                // check for ignore attributes
                if (IgnoreObjectName(field.Name, localPath, field.CustomAttributes))
                    continue;
                // also check the property for ignore, if this is a auto-backing property
                if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.ReflectedType.Name}.{field.BackedPropertyName}", field.BackedProperty.CustomAttributes))
                    continue;

                WriteObject(writer, fieldValue, fieldExtendedType, currentDepth, localPath, index);
                index++;
            }
        }

        /// <summary>
        /// Get the diagnostic log
        /// </summary>
        /// <returns></returns>
        public string GetDiagnosticLog()
        {
            return _debugWriter.ToString();
        }

        private void WriteDebugBuilder(long pos, ExtendedType typeSupport, TypeId typeId, int currentDepth, string path, int index, int dataLength, ushort objectReferenceId, ushort typeDescriptorId, int hashCode)
        {
            if (_options.BitwiseHasFlag(SerializerOptions.WriteDiagnosticLog))
                _debugWriter.WriteLine(pos, typeSupport, typeId, currentDepth, path, index, dataLength, objectReferenceId, typeDescriptorId, hashCode);
        }
    }
}