﻿#if FEATURE_COMPRESSION
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
#endif
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
    internal class TypeReader : TypeBase
    {
        private readonly Dictionary<ushort, object> _objectReferences;
        private readonly TypeRegistry _typeRegistry;
        private readonly ObjectFactory _objectFactory = new ObjectFactory();
        private PropertyVersion _skipTag = null;

        /// <summary>
        /// Read the parent object, and recursively process it's children
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeSupport">The type of the root object</param>
        /// <param name="maxDepth">The max depth tree to process</param>
        /// <param name="options">The serialization options</param>
        /// <param name="ignoreAttributes">Properties/Fields with these attributes will be ignored from processing</param>
        /// <param name="typeRegistry">A registry that contains custom type mappings</param>
        /// <param name="ignorePropertiesOrPaths"></param>
        /// <param name="skipTag"></param>
        /// <returns></returns>
        internal static object Read(BinaryReader reader, ExtendedType typeSupport, uint maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, SerializationTypeRegistry typeRegistry = null, ICollection<string> ignorePropertiesOrPaths = null, PropertyVersion skipTag = null)
        {
            var currentDepth = 0;
            uint dataLength = 0;
            uint headerLength = 0;

            var dataReader = reader;
            // read in byte 0, the data settings
            var dataSettings = (SerializerDataSettings)reader.ReadByte();

            if (dataSettings.BitwiseHasFlag(SerializerDataSettings.Compress))
            {
                // decompress the stream
                dataReader = Decompress(reader);
            }

            var typeReader = new TypeReader(dataSettings, options, maxDepth, ignoreAttributes, typeRegistry, ignorePropertiesOrPaths, skipTag);

            return typeReader.ReadObject(dataReader, typeSupport, currentDepth, string.Empty, ref dataLength, ref headerLength);
        }

        public TypeReader(SerializerDataSettings dataSettings, SerializerOptions options, uint maxDepth, ICollection<object> ignoreAttributes, SerializationTypeRegistry typeRegistry, ICollection<string> ignorePropertiesOrPaths = null, PropertyVersion skipTag = null)
        {
            _dataSettings = dataSettings;
            _options = options;
            _maxDepth = maxDepth;
            _ignoreAttributes = ignoreAttributes;
            _ignorePropertiesOrPaths = ignorePropertiesOrPaths;
            _skipTag = skipTag;
            _typeRegistry = ConvertToTypeRegistry(typeRegistry);
            _objectReferences = new Dictionary<ushort, object>();
            _customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) },
                { typeof(XDocument), new Lazy<ICustomSerializer>(() => new XDocumentSerializer()) },
            };
        }

        private TypeRegistry ConvertToTypeRegistry(SerializationTypeRegistry typeRegistry)
        {
            if (typeRegistry == null)
                return null;
            var registry = TypeRegistry.Configure(c => {
                foreach (var factory in typeRegistry.Factories)
                {
                    var newFactory = _objectFactory.CreateEmptyObject<TypeFactory>(factory.Source, factory.Factory);
                    c.Factories.Add(newFactory);
                }
                foreach (var map in typeRegistry.Mappings)
                {
                    var newMap = _objectFactory.CreateEmptyObject<TypeMap>();
                    newMap.Source = map.Source;
                    newMap.Destination = map.Destination;
                    c.Mappings.Add(newMap);
                }
            });
            return registry;
        }

        /// <summary>
        /// Read an object recursively
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeSupport">The type of property being read</param>
        /// <param name="currentDepth"></param>
        /// <param name="path"></param>
        /// <param name="dataLength"></param>
        /// <param name="headerLength"></param>
        /// <returns></returns>
        /// <exception cref="DataFormatException"></exception>
        internal object ReadObject(BinaryReader reader, ExtendedType typeSupport, int currentDepth, string path, ref uint dataLength, ref uint headerLength)
        {
            var arrayDimensions = new List<int>();
            var arrayRank = 0;

            dataLength = 0;
            headerLength = 0;

            // increment the current recursion depth
            currentDepth++;

            // ensure we don't go too deep if specified
            if (_maxDepth > 0 && currentDepth >= _maxDepth)
                return default;

            // drop any objects we are ignoring by attribute
            if (typeSupport.Attributes.Any(x => _ignoreAttributes.Contains(x)))
                return default;

            // for delegate types, return null
            if (typeSupport.IsDelegate)
                return default;

            if (reader.BaseStream.Position >= reader.BaseStream.Length - 1)
            {
                // no more data to be read
                return default;
            }

            // read the object type
            var objectTypeByte = reader.ReadByte();
            headerLength += Constants.TypeHeaderSize;
            var isTypeDescriptorMap = TypeUtil.IsTypeDescriptorMap((TypeId)objectTypeByte);
            var isTypeMapped = TypeUtil.IsTypeMapped((TypeId)objectTypeByte);
            var isNullValue = TypeUtil.IsNullValue((TypeId)objectTypeByte);
            var objectTypeId = TypeUtil.GetTypeId(objectTypeByte);

            // read the length prefix (minus the length field itself)
            if (_dataSettings.BitwiseHasFlag(SerializerDataSettings.Compact))
                dataLength = reader.ReadUInt16();
            else
                dataLength = reader.ReadUInt32();
            var actualDataRemaining = reader.BaseStream.Length - reader.BaseStream.Position;
            uint expectedDataRemaining = 0;

            if (dataLength > 0)
            {
                if (_dataSettings.BitwiseHasFlag(SerializerDataSettings.Compact))
                {
                    dataLength -= Constants.CompactLengthHeaderSize;
                    headerLength += Constants.CompactLengthHeaderSize;
                }
                else
                {
                    dataLength -= Constants.LengthHeaderSize;
                    headerLength += Constants.LengthHeaderSize;
                }
                expectedDataRemaining = dataLength + Constants.ObjectReferenceIdSize;
            }

            if (expectedDataRemaining > actualDataRemaining)
                throw new DataFormatException($"The object length read ({dataLength}) for type {objectTypeId} at path {path} cannot exceed the remaining size ({actualDataRemaining}) of the stream!");

            if (isTypeDescriptorMap)
            {
                // process a type descriptor map, then continue
                _typeDescriptors = GetTypeDescriptorMap(reader, dataLength);
                return ReadObject(reader, typeSupport, currentDepth, path, ref dataLength, ref headerLength);
            }

            // read in the object reference id
            var objectReferenceId = reader.ReadUInt16();
            headerLength += Constants.ObjectReferenceIdSize;

            // only interfaces can store type descriptors
            TypeDescriptor typeDescriptor = null;
            if (_typeDescriptors?.Types.Any() == true && isTypeMapped)
            {
                // type descriptors are embedded, read in the type
                var typeId = reader.ReadUInt16();
                headerLength += Constants.ObjectTypeDescriptorId;
                typeDescriptor = _typeDescriptors.GetTypeDescriptor(typeId);
            }

            // an null value was written
            if (dataLength == 0 && isNullValue)
                return null;

            // do we already have this object as a reference?
            if (!_options.BitwiseHasFlag(SerializerOptions.DisableReferenceTracking))
            {
                if (_objectReferences.ContainsKey(objectReferenceId))
                {
                    var reference = _objectReferences[objectReferenceId];
                    if (reference != null)
                    {
                        // if the types are a match, allow using it as a reference
                        var referenceType = reference.GetType();
                        if (typeDescriptor != null)
                        {
                            var typeDescriptorType = Type.GetType(typeDescriptor.FullName);
                            if (referenceType == typeDescriptorType)
                                return reference;
                        }
                        else
                        {
                            if (referenceType == typeSupport.Type)
                                return reference;
                        }
                    }
                }
            }

            // if it's an array, read it's dimensions before we create a new object for it
            uint arrayStartPosition = 0;
            if (objectTypeId == TypeId.Array)
            {
                // number of dimensions
                arrayRank = (int)reader.ReadUInt32();
                arrayStartPosition += sizeof(uint);
                // length of each dimension
                for (var i = 0; i < arrayRank; i++)
                {
                    arrayDimensions.Add((int)reader.ReadUInt32());
                    arrayStartPosition += sizeof(uint);
                }
            }

            try
            {
                if (dataLength == 0)
                {
                    // an empty initialized object was written
                    if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                        return _objectFactory.CreateEmptyObject(typeDescriptor.FullName, _typeRegistry);
                    return _objectFactory.CreateEmptyObject(typeSupport.Type, _typeRegistry);
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new DataFormatException($"[{path}] {ex.Message}", ex);
            }

            // get the type support object for this object type
            ExtendedType objectExtendedType = null;
            if (objectTypeId != TypeId.Struct)
            {
                objectExtendedType = TypeUtil.GetType(objectTypeId).GetExtendedType();

                // does this object map to something expected?
                if (!TypeUtil.GetTypeId(objectExtendedType).Equals(objectTypeId))
                    throw new DataFormatException($"Serialized data wants to map {objectTypeId} to {typeSupport.Type.Name}, invalid data.");
            }

            object newObj = null;
            // for arrays, we need to pass the dimensions of the desired arrays
            var destinationTypeSupport = typeSupport;
            try
            {
                if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                {
                    newObj = _objectFactory.CreateEmptyObject(typeDescriptor.FullName, _typeRegistry, arrayDimensions);
                    destinationTypeSupport = Type.GetType(typeDescriptor.FullName).GetExtendedType();
                }
                else
                {
                    // if the destination type is a generic object, but we know its a more specific type then swap types
                    if (destinationTypeSupport.Type == typeof(object) && objectExtendedType != typeof(object))
                    {
                        newObj = _objectFactory.CreateEmptyObject(objectExtendedType.Type, _typeRegistry, arrayDimensions);
                        destinationTypeSupport = objectExtendedType;
                    }
                    else
                    {
                        // standard case of create object as intended
                        newObj = _objectFactory.CreateEmptyObject(destinationTypeSupport, _typeRegistry, arrayDimensions);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new DataFormatException($"[{path}] {ex.Message}", ex);
            }

            // custom types support
            var objectDataLength = dataLength;
            var @switch = new Dictionary<Type, Func<object>>
                {
                    { typeof(XDocument), () => { return ReadValueType(reader, objectDataLength, destinationTypeSupport, currentDepth, path); } },
                };

            if (@switch.ContainsKey(destinationTypeSupport.Type))
                newObj = @switch[destinationTypeSupport.Type]();
            else
            {
                switch (objectTypeId)
                {
                    case TypeId.Object:
                        newObj = ReadObjectType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor);
                        break;
                    case TypeId.Struct:
                        newObj = ReadStructType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor);
                        break;
                    case TypeId.Array:
                        newObj = ReadArrayType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor, arrayStartPosition);
                        break;
                    case TypeId.IDictionary:
                        newObj = ReadDictionaryType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor);
                        break;
                    case TypeId.IEnumerable:
                        newObj = ReadEnumerableType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor);
                        break;
                    case TypeId.KeyValuePair:
                        newObj = ReadKeyValueType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor);
                        break;
                    case TypeId.Enum:
                        newObj = ReadValueType(reader, dataLength, typeof(Enum).GetExtendedType(), currentDepth, path);
                        break;
                    case TypeId.Tuple:
                        newObj = ReadTupleType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor);
                        break;
                    default:
                        newObj = ReadValueType(reader, dataLength, destinationTypeSupport, currentDepth, path);
                        break;
                }
            }

            // store the object reference id in the object reference map
            if (!_objectReferences.ContainsKey(objectReferenceId))
            {
                _objectReferences.Add(objectReferenceId, newObj);
            }

            return newObj;
        }

        internal object ReadValueType(BinaryReader reader, uint dataLength, ExtendedType typeSupport, int currentDepth, string path)
        {
            // read a value type
            var @switch = new Dictionary<Type, Func<object>> {
                { typeof(bool), () => reader.ReadBoolean() },
                { typeof(byte), () => reader.ReadByte() },
                { typeof(sbyte), () => reader.ReadSByte() },
                { typeof(short), () => reader.ReadInt16() },
                { typeof(ushort), () => reader.ReadUInt16() },
                { typeof(int), () => reader.ReadInt32() },
                { typeof(uint), () => reader.ReadUInt32() },
                { typeof(long), () => reader.ReadInt64() },
                { typeof(ulong), () => reader.ReadUInt64() },
                { typeof(float), () => reader.ReadSingle() },
                { typeof(double), () => reader.ReadDouble() },
                { typeof(decimal), () => reader.ReadDecimal() },
                { typeof(string), () => reader.ReadString() },
                { typeof(Enum), () => {
                    var result = _customSerializers[typeof(Enum)].Value.Deserialize(reader.ReadBytes((int)dataLength), dataLength);
                    return result;
                }},
                { typeof(XDocument), () => {
                    var result = _customSerializers[typeof(XDocument)].Value.Deserialize(reader.ReadBytes((int)dataLength), dataLength);
                    return result;
                }},
                { typeof(char), () => reader.ReadChar() },
                { typeof(IntPtr), () => new IntPtr(reader.ReadInt64()) },
                { typeof(Guid), () => new Guid(reader.ReadBytes(16)) },
                { typeof(DateTime), () => DateTime.FromBinary(reader.ReadInt64()) },
                { typeof(TimeSpan), () => TimeSpan.FromTicks(reader.ReadInt64()) },
                { typeof(Point), () => {
                    var result = _customSerializers[typeof(Point)].Value.Deserialize(reader.ReadBytes((int)dataLength), dataLength);
                    return result;
                }},
            };

            try
            {
                // return the value
                return @switch[typeSupport.NullableBaseType]();
            }
            catch (Exception ex)
            {
                throw new DataFormatException($"Unknown value data type: {typeSupport.NullableBaseType}", ex);
            }
        }

        internal Array ReadArrayType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor, uint arrayStartPosition)
        {
            // length = entire collection
            // read each element, starting from the position after the rank/dimension information is read
            uint i = arrayStartPosition;
            uint dataLength = 0;
            uint headerLength = 0;
            var elementExtendedType = typeSupport.ElementType.GetExtendedType();
            var array = (Array)newObj;
            var arrayRank = array.Rank;
            var arrayDimensions = new List<int>();
            for (var dimension = 0; dimension < arrayRank; dimension++)
                arrayDimensions.Add(array.GetLength(dimension));
            var flatRowIndex = 0;

            while (i < length)
            {
                var element = ReadObject(reader, elementExtendedType, currentDepth, path, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                // performance optimization, skip dimensional processing if it's a 1d array
                if (arrayRank > 1)
                {
                    // this is an optimized multi-dimensional array reconstruction
                    // based on the formula: indicies.Add((i / (arrayDimensions[arrayRank - 1] * arrayDimensions[arrayRank - 2] * arrayDimensions[arrayRank - 3] * arrayDimensions[arrayRank - 4] * arrayDimensions[arrayRank - 5])) % arrayDimensions[arrayRank - 6]);
                    var indicies = new List<int>();
                    for (var r = 1; r <= arrayRank; r++)
                    {
                        var multi = 1;
                        for (var p = 1; p < r; p++)
                        {
                            multi *= arrayDimensions[arrayRank - p];
                        }
                        var b = (flatRowIndex / multi) % arrayDimensions[arrayRank - r];
                        indicies.Add(b);
                    }
                    indicies.Reverse();
                    // set element of multi-dimensional array
                    array.SetValue(element, indicies.ToArray());
                }
                else
                {
                    // set element of 1d array
                    array.SetValue(element, flatRowIndex);
                }
                flatRowIndex++;
            }
            return array;
        }

        internal object ReadEnumerableType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element
            uint i = 0;
            uint dataLength = 0;
            uint headerLength = 0;

            // determine what this enumerable enumerates (it's not necessarily the generic argument of the class)
            Type genericType;
            ExtendedType genericExtendedType;
            // if it's a custom class that implements IEnumerable generically, get it's type argument
            var enumerableInterface = typeSupport.Interfaces.FirstOrDefault(x => x.IsGenericType && x.Name == "IEnumerable`1");
            if (enumerableInterface != null)
            {
                genericType = enumerableInterface.GetGenericArguments().FirstOrDefault();
                genericExtendedType = genericType.GetExtendedType();
            }
            else
            {
                // use the generic type from the class directly
                genericType = typeSupport.Type.GetGenericArguments().First();
                genericExtendedType = genericType.GetExtendedType();
            }

            var addMethod = typeSupport.Type.GetMethod("Add");
            if (addMethod == null)
                addMethod = typeSupport.Type.GetMethod("Push");
            if (addMethod == null)
                addMethod = typeSupport.Type.GetMethod("Enqueue");
            if (addMethod == null)
                throw new DataFormatException($"TypeReader does not know how to add items to this enumerable: {typeSupport.Type}");

            while (i < length)
            {
                var element = ReadObject(reader, genericExtendedType, currentDepth, path, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                addMethod.Invoke(newObj, new[] { element });
            }
            return newObj;
        }

        internal object ReadTupleType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element, treat a tuple as a list of objects
            uint i = 0;
            uint dataLength = 0;
            uint headerLength = 0;
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => x.GetExtendedType()).ToList();
            Type tupleType = null;
            if (typeSupport.IsValueTuple)
                tupleType = TypeSupport.Extensions.TupleExtensions.CreateValueTuple(typeSupports.Select(x => x.Type).ToList());
            else
                tupleType = TypeSupport.Extensions.TupleExtensions.CreateTuple(typeSupports.Select(x => x.Type).ToList());
            object newTuple = null;
            if (typeDescriptor != null && !string.IsNullOrEmpty(typeDescriptor.FullName))
                newTuple = new ObjectFactory().CreateEmptyObject(typeDescriptor.FullName, _typeRegistry);
            else
                newTuple = new ObjectFactory().CreateEmptyObject(tupleType, _typeRegistry);
            var index = 0;
            while (i < length)
            {
                var element = ReadObject(reader, typeSupports[index], currentDepth, path, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                var fieldName = $"m_Item{index + 1}";
                if (typeSupport.IsValueTuple)
                    fieldName = $"Item{index + 1}";
                TypeUtil.SetFieldValue(fieldName, newTuple, element);
                index++;
            }

            // return the value
            return newTuple;
        }

        internal object ReadDictionaryType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element
            uint i = 0;
            uint dataLength = 0;
            uint headerLength = 0;

            if (typeSupport.IsGeneric && typeSupport.GenericArgumentTypes.Any())
            {
                // generic IDictionary<,>
                var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
                var typeSupports = genericTypes.Select(x => x.GetExtendedType()).ToList();
                var keyExtendedType = typeSupports.First();
                var valueExtendedType = typeSupports.Skip(1).First();
                Type[] typeArgs = { genericTypes[0], genericTypes[1] };

                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
                var newDictionary = Activator.CreateInstance(dictionaryType) as IDictionary;

                while (i < length)
                {
                    var key = ReadObject(reader, keyExtendedType, currentDepth, path, ref dataLength, ref headerLength);
                    // increment the size of the data read
                    i += dataLength + headerLength;
                    var value = ReadObject(reader, valueExtendedType, currentDepth, path, ref dataLength, ref headerLength);
                    // increment the size of the data read
                    i += dataLength + headerLength;
                    newDictionary.Add(key, value);
                }

                // special case for concurrent dictionaries
                if (typeSupport.Type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
                {
                    dictionaryType = typeof(ConcurrentDictionary<,>).MakeGenericType(typeArgs);
                    var newConcurrentDictionary = Activator.CreateInstance(dictionaryType, new object[] { newDictionary }) as IDictionary;
                    newDictionary = newConcurrentDictionary;
                }

                // return the value
                return newDictionary;
            }
            else
            {
                // non-generic IDictionary
                ExtendedType extendedType;
                if (typeSupport.GenericArgumentTypes.Any())
                    extendedType = typeSupport.GenericArgumentTypes.First().GetExtendedType();
                else
                    extendedType = typeof(object).GetExtendedType();

                var factory = new ObjectFactory();
                var newDictionary = (IDictionary)factory.CreateEmptyObject(typeSupport);

                while (i < length)
                {
                    var key = ReadObject(reader, extendedType, currentDepth, path, ref dataLength, ref headerLength);
                    // increment the size of the data read
                    i += dataLength + headerLength;
                    var value = ReadObject(reader, extendedType, currentDepth, path, ref dataLength, ref headerLength);
                    // increment the size of the data read
                    i += dataLength + headerLength;
                    newDictionary.Add(key, value);
                }

                // return the value
                return newDictionary;
            }
        }

        internal object ReadKeyValueType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            uint dataLength = 0;
            uint headerLength = 0;
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => x.GetExtendedType()).ToList();
            var keyExtendedType = typeSupports.First();
            var valueExtendedType = typeSupports.Skip(1).First();

            var key = ReadObject(reader, keyExtendedType, currentDepth, path, ref dataLength, ref headerLength);
            var value = ReadObject(reader, valueExtendedType, currentDepth, path, ref dataLength, ref headerLength);
            newObj.SetFieldValue("key", key);
            newObj.SetFieldValue("value", value);

            // return the kvp
            return newObj;
        }

        internal object ReadStructType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            // read each property into the object
            var fields = newObj.GetFields(FieldOptions.AllWritable).Where(x => !x.FieldInfo.IsStatic).OrderBy(x => x.Name);

            var rootPath = path;
            var localPath = string.Empty;
            foreach (var field in fields)
            {
                localPath = $"{rootPath}.{field.ReflectedType.Name}.{field.Name}";
                uint dataLength = 0;
                uint headerLength = 0;
                var fieldExtendedType = field.Type;

                if (fieldExtendedType.IsDelegate)
                    continue;

                // check for ignore attributes
                if (IgnoreObjectName(field.Name, localPath, field.CustomAttributes))
                    continue;
                // also check the property for ignore, if this is a auto-backing property
                if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.ReflectedType.Name}.{field.BackedPropertyName}", field.BackedProperty.CustomAttributes))
                    continue;

                var fieldValue = ReadObject(reader, fieldExtendedType, currentDepth, localPath, ref dataLength, ref headerLength);
                newObj.SetFieldValue(field, fieldValue);
            }

            return newObj;
        }

        internal object ReadObjectType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            // read each property into the object
            var fields = newObj.GetFields(FieldOptions.AllWritable).OrderBy(x => x.Name);

            var rootPath = path;
            var localPath = string.Empty;
            foreach (var field in fields)
            {
                // if property is marked as skipped then don't process it
                if (_skipTag != null)
                {
                    var skipTagAttribute = field.IsBackingField
                        ? field.BackedProperty.GetAttribute<PropertyVersionAttribute>()
                        : field.GetAttribute<PropertyVersionAttribute>();
                    if (skipTagAttribute != null && _skipTag.Contains(skipTagAttribute.Tag))
                        continue;
                }

                localPath = $"{rootPath}.{field.ReflectedType.Name}.{field.Name}";
                uint dataLength = 0;
                uint headerLength = 0;
                var fieldExtendedType = field.Type;

                if (fieldExtendedType.IsDelegate)
                    continue;

                // check for ignore attributes
                if (IgnoreObjectName(field.Name, localPath, field.CustomAttributes))
                    continue;
                // also check the property for ignore, if this is a auto-backing property
                if (field.BackedProperty != null && IgnoreObjectName(field.BackedProperty.Name, $"{rootPath}.{field.ReflectedType.Name}.{field.BackedPropertyName}", field.BackedProperty.CustomAttributes))
                    continue;

                var fieldValue = ReadObject(reader, fieldExtendedType, currentDepth, localPath, ref dataLength, ref headerLength);
                try
                {
                    newObj.SetFieldValue(field, fieldValue);
                }
                catch (FieldAccessException)
                {
                    // .net core 3.0+ no longer allows you to set values on static initializers
                    // see https://github.com/dotnet/runtime/issues/11571 & https://github.com/dotnet/coreclr/pull/20886
                }
            }

            return newObj;
        }

        /// <summary>
        /// Read in a map of all the types used in the serialized data
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        internal static TypeDescriptors GetTypeDescriptorMap(BinaryReader reader, uint dataLength)
        {
            // read in the data
            var typeDescriptors = new TypeDescriptors();
            if (dataLength > 0)
            {
                var bytes = reader.ReadBytes((int)dataLength);
                typeDescriptors.Deserialize(bytes);
            }
            return typeDescriptors;
        }

        internal static BinaryReader Decompress(BinaryReader reader)
        {
            var dataReader = reader;
            // read in all of the compressed data, minus the SerializerDataSettings byte-0 value
            var compressedBytes = new byte[reader.BaseStream.Length - 1];
            reader.Read(compressedBytes, 0, compressedBytes.Length);

#if FEATURE_COMPRESSION
            // decompress the stream
            using (var compressedStream = new MemoryStream(compressedBytes))
            {
                using (var lz4Stream = LZ4Stream.Decode(compressedStream))
                {
                    using (var compressedReader = new StreamReader(lz4Stream))
                    {
                        var encodedString = compressedReader.ReadToEnd();
                        var decodedBytes = Convert.FromBase64String(encodedString);
                        // provide a new reader which contains the decompressed data
                        dataReader = new BinaryReader(new MemoryStream(decodedBytes));
                    }
                }
            }
#endif
            return dataReader;
        }
    }
}
