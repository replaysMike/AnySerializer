#if FEATURE_COMPRESSION
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

        /// <summary>
        /// Read the parent object, and recursively process it's children
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeSupport">The type of the root object</param>
        /// <param name="maxDepth">The max depth tree to process</param>
        /// <param name="options">The serialization options</param>
        /// <param name="objectTree">Tracks the tree that has been traversed</param>
        /// <param name="ignoreAttributes">Properties/Fields with these attributes will be ignored from processing</param>
        /// <param name="typeRegistry">A registry that contains custom type mappings</param>
        /// <returns></returns>
        internal static object Read(BinaryReader reader, ExtendedType typeSupport, uint maxDepth, SerializerOptions options, ICollection<object> ignoreAttributes, TypeRegistry typeRegistry = null, ICollection<string> ignorePropertiesOrPaths = null)
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

            var typeReader = new TypeReader(dataSettings, options, maxDepth, ignoreAttributes, typeRegistry, ignorePropertiesOrPaths);

            return typeReader.ReadObject(dataReader, typeSupport, currentDepth, string.Empty, ref dataLength, ref headerLength);
        }

        public TypeReader(SerializerDataSettings dataSettings, SerializerOptions options, uint maxDepth, ICollection<object> ignoreAttributes, TypeRegistry typeRegistry, ICollection<string> ignorePropertiesOrPaths = null)
        {
            _dataSettings = dataSettings;
            _options = options;
            _maxDepth = maxDepth;
            _ignoreAttributes = ignoreAttributes;
            _ignorePropertiesOrPaths = ignorePropertiesOrPaths;
            _typeRegistry = typeRegistry;
            _objectReferences = new Dictionary<ushort, object>();
            _customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) },
                { typeof(XDocument), new Lazy<ICustomSerializer>(() => new XDocumentSerializer()) },
            };

        }

        /// <summary>
        /// Read an object recursively
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeSupport">Type type of object being read</param>
        /// <param name="customSerializers"></param>
        /// <param name="currentDepth"></param>
        /// <param name="maxDepth"></param>
        /// <param name="objectReferences"></param>
        /// <param name="path"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="typeRegistry"></param>
        /// <param name="typeDescriptors"></param>
        /// <param name="typeDescriptor"></param>
        /// <param name="dataLength"></param>
        /// <param name="headerLength"></param>
        /// <returns></returns>
        internal object ReadObject(BinaryReader reader, ExtendedType typeSupport, int currentDepth, string path, ref uint dataLength, ref uint headerLength)
        {
            var objectFactory = new ObjectFactory();
            dataLength = 0;
            headerLength = 0;

            // increment the current recursion depth
            currentDepth++;

            // ensure we don't go too deep if specified
            if (_maxDepth > 0 && currentDepth >= _maxDepth)
                return default(object);

            // drop any objects we are ignoring by attribute
            if (typeSupport.Attributes.Any(x => _ignoreAttributes.Contains(x)))
                return default(object);

            // for delegate types, return null
            if (typeSupport.IsDelegate)
                return default(object);

            // read the object type
            var objectTypeByte = reader.ReadByte();
            headerLength += Constants.TypeHeaderSize;
            var objectTypeId = TypeUtil.GetTypeId(objectTypeByte);
            var isNullValue = TypeUtil.IsNullValue((TypeId)objectTypeByte);
            var isTypeMapped = TypeUtil.IsTypeMapped((TypeId)objectTypeByte);
            var isTypeDescriptorMap = TypeUtil.IsTypeDescriptorMap((TypeId)objectTypeByte);

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
            if (_objectReferences.ContainsKey(objectReferenceId))
                return _objectReferences[objectReferenceId];

            try
            {
                if (dataLength == 0)
                {
                    // an empty initialized object was written
                    if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                        return objectFactory.CreateEmptyObject(typeDescriptor.FullName, _typeRegistry);
                    return objectFactory.CreateEmptyObject(typeSupport.Type, _typeRegistry);
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
                objectExtendedType = TypeUtil.GetType(objectTypeId);

                // does this object map to something expected?
                if (!TypeUtil.GetTypeId(objectExtendedType).Equals(objectTypeId))
                    throw new DataFormatException($"Serialized data wants to map {objectTypeId} to {typeSupport.Type.Name}, invalid data.");
            }

            object newObj = null;
            var destinationTypeSupport = typeSupport; 
            try
            {
                if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                {
                    newObj = objectFactory.CreateEmptyObject(typeDescriptor.FullName, _typeRegistry);
                    destinationTypeSupport = Type.GetType(typeDescriptor.FullName).GetExtendedType();
                }
                else
                    newObj = objectFactory.CreateEmptyObject(destinationTypeSupport.Type, _typeRegistry);
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
                        newObj = ReadArrayType(newObj, reader, dataLength, destinationTypeSupport, currentDepth, path, typeDescriptor);
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
                        newObj = ReadValueType(reader, dataLength, new ExtendedType(typeof(Enum)), currentDepth, path);
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

        internal Array ReadArrayType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element
            uint i = 0;
            uint dataLength = 0;
            uint headerLength = 0;
            var genericType = typeSupport.ElementType;
            var listType = typeof(List<>).MakeGenericType(genericType);
            var newList = (IList)Activator.CreateInstance(listType);
            var elementExtendedType = new ExtendedType(typeSupport.ElementType);
            while (i < length)
            {
                var element = ReadObject(reader, elementExtendedType, currentDepth, path, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                newList.Add(element);
            }

            var returnObj = newObj;

            // return the value
            if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                returnObj = new ObjectFactory().CreateEmptyObject(typeDescriptor.FullName, _typeRegistry, length: newList.Count);
            else
                returnObj = new ObjectFactory().CreateEmptyObject(typeSupport.Type, _typeRegistry, length: newList.Count);

            newList.CopyTo((Array)returnObj, 0);
            return (Array)returnObj;
        }

        internal object ReadEnumerableType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element
            uint i = 0;
            uint dataLength = 0;
            uint headerLength = 0;

            var genericType = typeSupport.Type.GetGenericArguments().First();           
            var genericExtendedType = new ExtendedType(genericType);
            var addMethod = typeSupport.Type.GetMethod("Add");
            if(addMethod == null)
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
                addMethod.Invoke(newObj, new [] { element });
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
            var typeSupports = genericTypes.Select(x => new ExtendedType(x)).ToList();
            Type tupleType = null;
            if (typeSupport.IsValueTuple)
                tupleType = TypeSupport.Extensions.TupleExtensions.CreateValueTuple(typeSupports.Select(x => x.Type).ToList());
            else
                tupleType = TypeSupport.Extensions.TupleExtensions.CreateTuple(typeSupports.Select(x => x.Type).ToList());
            object newTuple = null;
            if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                newTuple = new ObjectFactory().CreateEmptyObject(typeDescriptor.FullName, _typeRegistry);
            else
                newTuple = new ObjectFactory().CreateEmptyObject(tupleType, _typeRegistry);
            var index = 0;
            while (i < length)
            {
                var element = ReadObject(reader, typeSupports[index], currentDepth, path, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                if (typeSupport.IsValueTuple)
                    TypeUtil.SetFieldValue($"Item{index + 1}", newTuple, element);
                else
                    TypeUtil.SetFieldValue($"m_Item{index + 1}", newTuple, element);
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
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => new ExtendedType(x)).ToList();
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
            if(typeSupport.Type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
            {
                dictionaryType = typeof(ConcurrentDictionary<,>).MakeGenericType(typeArgs);
                var newConcurrentDictionary = Activator.CreateInstance(dictionaryType, new object[] { newDictionary }) as IDictionary;
                newDictionary = newConcurrentDictionary;
            }

            // return the value
            return newDictionary;
        }

        internal object ReadKeyValueType(object newObj, BinaryReader reader, uint length, ExtendedType typeSupport, int currentDepth, string path, TypeDescriptor typeDescriptor)
        {
            uint dataLength = 0;
            uint headerLength = 0;
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => new ExtendedType(x)).ToList();
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
            var localPath = path;
            foreach (var field in fields)
            {
                localPath = $"{rootPath}.{field.ReflectedType.Name}.{field.Name}";
                uint dataLength = 0;
                uint headerLength = 0;
                var fieldExtendedType = new ExtendedType(field.Type);

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
            var localPath = path;
            foreach (var field in fields)
            {
                localPath = $"{rootPath}.{field.ReflectedType.Name}.{field.Name}";
                uint dataLength = 0;
                uint headerLength = 0;
                var fieldExtendedType = new ExtendedType(field.Type);

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
