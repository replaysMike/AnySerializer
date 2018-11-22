using AnySerializer.CustomSerializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TypeSupport;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    internal static class TypeReaders
    {
        /// <summary>
        /// Read the parent object, and recursively process it's children
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeSupport">The type of the root object</param>
        /// <param name="maxDepth">The max depth tree to process</param>
        /// <param name="objectTree">Tracks the tree that has been traversed</param>
        /// <param name="ignoreAttributes">Properties/Fields with these attributes will be ignored from processing</param>
        /// <param name="typeRegistry">A registry that contains custom type mappings</param>
        /// <returns></returns>
        internal static object Read(BinaryReader reader, ExtendedType typeSupport, int maxDepth, IDictionary<int, object> objectTree, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry = null)
        {
            var customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>()
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) }
            };
            var currentDepth = 0;
            var dataLength = 0;
            var headerLength = 0;
            return ReadObject(reader, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, string.Empty, ignoreAttributes, typeRegistry, TypeDescriptors.None, ref dataLength, ref headerLength);
        }

        /// <summary>
        /// Read an object recursively
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeSupport">Type type of object being read</param>
        /// <param name="customSerializers"></param>
        /// <param name="currentDepth"></param>
        /// <param name="maxDepth"></param>
        /// <param name="objectTree"></param>
        /// <param name="path"></param>
        /// <param name="ignoreAttributes"></param>
        /// <param name="typeRegistry"></param>
        /// <param name="typeDescriptors"></param>
        /// <param name="typeDescriptor"></param>
        /// <param name="dataLength"></param>
        /// <param name="headerLength"></param>
        /// <returns></returns>
        internal static object ReadObject(BinaryReader reader, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry, TypeDescriptors typeDescriptors, ref int dataLength, ref int headerLength)
        {
            var objectFactory = new ObjectFactory();
            dataLength = 0;
            headerLength = 0;

            // increment the current recursion depth
            currentDepth++;

            // ensure we don't go too deep if specified
            if (maxDepth > 0 && currentDepth >= maxDepth)
                return default(object);

            // drop any objects we are ignoring by attribute
            if (typeSupport.Attributes.Any(x => ignoreAttributes.Contains(x)))
                return default(object);

            // for delegate types, return null
            if (typeSupport.IsDelegate)
                return default(object);

            // read the object type
            var objectTypeByte = reader.ReadByte();
            headerLength += sizeof(byte);
            var objectTypeId = TypeUtil.GetTypeId(objectTypeByte);
            var isNullValue = TypeUtil.IsNullValue((TypeId)objectTypeByte);
            var isAbstractInterface = TypeUtil.IsAbstractInterface((TypeId)objectTypeByte);
            var isTypeDescriptorMap = TypeUtil.IsTypeDescriptorMap((TypeId)objectTypeByte);
            var isValueType = TypeUtil.IsValueType(objectTypeId);

            // read the length prefix (minus the length field itself)
            dataLength = reader.ReadInt32();
            var dataLengthOriginal = dataLength;
            if (dataLength > 0)
            {
                dataLength -= sizeof(int);
                headerLength += sizeof(int);
            }

            // an null value was written
            if (dataLength == 0 && isNullValue)
                return null;

            if (isTypeDescriptorMap)
            {
                // process a type descriptor map, then continue
                typeDescriptors = GetTypeDescriptorMap(reader, dataLength);
                return ReadObject(reader, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
            }

            // only interfaces can store type descriptors
            TypeDescriptor typeDescriptor = null;
            if(typeDescriptors?.Types.Any() == true && isAbstractInterface)
            {
                // type descriptors are embedded, read in the type
                var typeId = reader.ReadUInt16();
                headerLength += sizeof(ushort);
                typeDescriptor = typeDescriptors.GetTypeDescriptor(typeId);
            }

            // an empty initialized object was written
            try
            {
                if (dataLength == 0)
                {
                    if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                        return objectFactory.CreateEmptyObject(typeDescriptor.FullName, typeRegistry);
                    return objectFactory.CreateEmptyObject(typeSupport.Type, typeRegistry);
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"[{path}] {ex.Message}", ex);
            }

            // get the type support object for this object type
            var objectExtendedType = TypeUtil.GetType(objectTypeId);

            // does this object map to something expected?
            if (!TypeUtil.GetTypeId(objectExtendedType).Equals(objectTypeId))
                throw new InvalidOperationException($"Serialized data wants to map {objectTypeId} to {typeSupport.Type.Name}, invalid data.");

            object newObj = null;
            try
            {
                if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                    newObj = objectFactory.CreateEmptyObject(typeDescriptor.FullName, typeRegistry);
                else
                    newObj = objectFactory.CreateEmptyObject(typeSupport.Type, typeRegistry);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"[{path}] {ex.Message}", ex);
            }

            // construct a hashtable of objects we have already inspected (simple recursion loop preventer)
            // we use this hashcode method as it does not use any custom hashcode handlers the object might implement
            if (newObj != null)
            {
                var hashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(newObj);
                if (objectTree.ContainsKey(hashCode))
                    return objectTree[hashCode];
                // ensure we can refer back to the reference for this object
                if (hashCode > 0)
                    objectTree.Add(hashCode, newObj);
            }

            switch (objectTypeId)
            {
                case TypeId.Object:
                    return TypeReaders.ReadObjectType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, typeDescriptor);
                case TypeId.Array:
                    return TypeReaders.ReadArrayType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, typeDescriptor);
                case TypeId.IDictionary:
                    return TypeReaders.ReadDictionaryType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, typeDescriptor);
                case TypeId.IEnumerable:
                    return TypeReaders.ReadEnumerableType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, typeDescriptor);
                case TypeId.Enum:
                    return TypeReaders.ReadValueType(reader, dataLength, new ExtendedType(typeof(Enum)), customSerializers, currentDepth, maxDepth, path, ignoreAttributes, typeRegistry);
                case TypeId.Tuple:
                    return TypeReaders.ReadTupleType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, typeDescriptor);
            }

            return TypeReaders.ReadValueType(reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, path, ignoreAttributes, typeRegistry);
        }

        internal static object ReadValueType(BinaryReader reader, int dataLength, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, string path, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry)
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
                    var result = customSerializers[typeof(Enum)].Value.Deserialize(reader.ReadBytes(dataLength), dataLength);
                    return result;
                }},
                { typeof(char), () => reader.ReadChar() },
                { typeof(Guid), () => new Guid(reader.ReadBytes(16)) },
                { typeof(DateTime), () => DateTime.FromBinary(reader.ReadInt64()) },
                { typeof(TimeSpan), () => TimeSpan.FromTicks(reader.ReadInt64()) },
                { typeof(Point), () => {
                    var result = customSerializers[typeof(Point)].Value.Deserialize(reader.ReadBytes(dataLength), dataLength);
                    return result;
                }},
            };

            // return the value
            return @switch[typeSupport.NullableBaseType]();
        }

        internal static Array ReadArrayType(object newObj, BinaryReader reader, int length, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry, TypeDescriptors typeDescriptors, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element
            var i = 0;
            var dataLength = 0;
            var headerLength = 0;
            var genericType = typeSupport.ElementType;
            var listType = typeof(List<>).MakeGenericType(genericType);
            var newList = (IList)Activator.CreateInstance(listType);
            var enumerator = (IEnumerable)newObj;
            var elementExtendedType = new ExtendedType(typeSupport.ElementType);
            while (i < length)
            {
                var element = ReadObject(reader, elementExtendedType, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                newList.Add(element);
            }

            // return the value
            if (!string.IsNullOrEmpty(typeDescriptor?.FullName))
                newObj = new ObjectFactory().CreateEmptyObject(typeDescriptor.FullName, typeRegistry, length: newList.Count);
            else
                newObj = new ObjectFactory().CreateEmptyObject(typeSupport.Type, typeRegistry, length: newList.Count);

            newList.CopyTo((Array)newObj, 0);
            return (Array)newObj;
        }

        internal static object ReadEnumerableType(object newObj, BinaryReader reader, int length, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry, TypeDescriptors typeDescriptors, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element
            var i = 0;
            var dataLength = 0;
            var headerLength = 0;
            var genericType = typeSupport.Type.GetGenericArguments().First();
            var genericExtendedType = new ExtendedType(genericType);
            var listType = typeof(List<>).MakeGenericType(genericType);
            var newList = (IList)Activator.CreateInstance(listType);
            newObj = newList;
            var enumerator = (IEnumerable)newObj;
            while (i < length)
            {
                var element = ReadObject(reader, genericExtendedType, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                newList.Add(element);
            }

            // return the value
            return newObj;
        }

        internal static object ReadTupleType(object newObj, BinaryReader reader, int length, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry, TypeDescriptors typeDescriptors, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element, treat a tuple as a list of objects
            var i = 0;
            var dataLength = 0;
            var headerLength = 0;
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => new ExtendedType(x)).ToList();
            Type tupleType = null;
            if (typeSupport.IsValueTuple)
                tupleType = TypeSupport.Extensions.TupleExtensions.CreateValueTuple(typeSupports.Select(x => x.Type).ToList());
            else
                tupleType = TypeSupport.Extensions.TupleExtensions.CreateTuple(typeSupports.Select(x => x.Type).ToList());
            object newTuple = null;
            if(!string.IsNullOrEmpty(typeDescriptor?.FullName))
                newTuple = new ObjectFactory().CreateEmptyObject(typeDescriptor.FullName, typeRegistry);
            else
                newTuple = new ObjectFactory().CreateEmptyObject(tupleType, typeRegistry);
            newObj = newTuple;
            var index = 0;
            while (i < length)
            {
                var element = ReadObject(reader, typeSupports[index], customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                if (typeSupport.IsValueTuple)
                    TypeUtil.SetFieldValue($"Item{index + 1}", newObj, element);
                else
                    TypeUtil.SetFieldValue($"m_Item{index + 1}", newObj, element);
                index++;
            }

            // return the value
            return newTuple;
        }

        internal static object ReadDictionaryType(object newObj, BinaryReader reader, int length, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry, TypeDescriptors typeDescriptors, TypeDescriptor typeDescriptor)
        {
            // length = entire collection
            // read each element
            var i = 0;
            var dataLength = 0;
            var headerLength = 0;
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => new ExtendedType(x)).ToList();
            var keyExtendedType = typeSupports.First();
            var valueExtendedType = typeSupports.Skip(1).First();
            Type[] typeArgs = { genericTypes[0], genericTypes[1] };

            var listType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
            var newDictionary = Activator.CreateInstance(listType) as IDictionary;
            newObj = newDictionary;
            var enumerator = (IDictionary)newObj;

            while (i < length)
            {
                var key = ReadObject(reader, keyExtendedType, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                var value = ReadObject(reader, valueExtendedType, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
                // increment the size of the data read
                i += dataLength + headerLength;
                newDictionary.Add(key, value);
            }

            // return the value
            return newObj;
        }

        internal static object ReadObjectType(object newObj, BinaryReader reader, int length, ExtendedType typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes, TypeRegistry typeRegistry, TypeDescriptors typeDescriptors, TypeDescriptor typeDescriptor)
        {
            // read each property into the object
            var properties = TypeUtil.GetProperties(newObj).OrderBy(x => x.Name);
            var fields = TypeUtil.GetFields(newObj).OrderBy(x => x.Name);
            var rootPath = path;
            foreach (var property in properties)
            {
                path = $"{rootPath}.{property.Name}";
                var dataLength = 0;
                var headerLength = 0;
                var indexParameters = property.GetIndexParameters();
                if (indexParameters.Any())
                    continue;
                // if the property has no set method, don't set any value it will be handled in the field
                if (property.SetMethod == null)
                    continue;
                var propertyExtendedType = new ExtendedType(property.PropertyType);
                if (property.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (propertyExtendedType.IsDelegate)
                    continue;
                var propertyValue = ReadObject(reader, propertyExtendedType, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
                TypeUtil.SetPropertyValue(property, newObj, propertyValue, path);
            }

            foreach (var field in fields)
            {
                path = $"{rootPath}.{field.Name}";
                var dataLength = 0;
                var headerLength = 0;
                var fieldExtendedType = new ExtendedType(field.FieldType);
                if (field.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (fieldExtendedType.IsDelegate)
                    continue;
                var fieldValue = ReadObject(reader, fieldExtendedType, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, typeRegistry, typeDescriptors, ref dataLength, ref headerLength);
                TypeUtil.SetFieldValue(field, newObj, fieldValue, path);
            }

            return newObj;
        }

        /// <summary>
        /// Read in a map of all the types used in the serialized data
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        internal static TypeDescriptors GetTypeDescriptorMap(BinaryReader reader, int dataLength)
        {
            // read in the data
            var typeDescriptors = new TypeDescriptors();
            if (dataLength > 0)
            {
                var bytes = reader.ReadBytes(dataLength);
                typeDescriptors.Deserialize(bytes);
            }
            return typeDescriptors;
        }
    }
}
