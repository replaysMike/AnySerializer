using AnySerializer.CustomSerializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    internal static class TypeReaders
    {
        /// <summary>
        /// Read the parent object, and recursively process it's children
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeSupport"></param>
        /// <param name="maxDepth"></param>
        /// <param name="objectTree"></param>
        /// <param name="ignoreAttributes"></param>
        /// <returns></returns>
        internal static object Read(BinaryReader reader, TypeSupport typeSupport, int maxDepth, IDictionary<int, object> objectTree, ICollection<Type> ignoreAttributes)
        {
            var customSerializers = new Dictionary<Type, Lazy<ICustomSerializer>>()
            {
                { typeof(Point), new Lazy<ICustomSerializer>(() => new PointSerializer()) },
                { typeof(Enum), new Lazy<ICustomSerializer>(() => new EnumSerializer()) }
            };
            var currentDepth = 0;
            var dataLength = 0;
            return ReadObject(reader, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, string.Empty, ignoreAttributes, ref dataLength);
        }

        internal static object ReadObject(BinaryReader reader, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes, ref int dataLength)
        {
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
            var objectTypeId = (TypeId)reader.ReadByte();
            var objectTypeSupport = TypeUtil.GetType(objectTypeId);
            // read the length prefix (minus the length field itself)
            dataLength = reader.ReadInt32();
            if (dataLength > 0) dataLength -= sizeof(int);

            // an empty object was written
            if (dataLength == 0)
                return null;

            // does this object map to something expected?
            if (TypeUtil.GetTypeId(objectTypeSupport) != objectTypeId)
                throw new InvalidOperationException($"Serialized data wants to map {typeSupport.Type.Name} to {objectTypeId}, invalid data.");

            var newObj = TypeUtil.CreateEmptyObject(typeSupport.Type);

            // construct a hashtable of objects we have already inspected (simple recursion loop preventer)
            // we use this hashcode method as it does not use any custom hashcode handlers the object might implement
            if (newObj != null)
            {
                var hashCode = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(newObj);
                if (objectTree.ContainsKey(hashCode))
                    return objectTree[hashCode];
                // ensure we can refer back to the reference for this object
                objectTree.Add(hashCode, newObj);
            }

            switch (objectTypeId)
            {
                case TypeId.Object:
                    return TypeReaders.ReadObjectType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                case TypeId.Array:
                    return TypeReaders.ReadArrayType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                case TypeId.IDictionary:
                    return TypeReaders.ReadDictionaryType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                case TypeId.IEnumerable:
                    return TypeReaders.ReadEnumerableType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
                case TypeId.Enum:
                    return TypeReaders.ReadValueType(reader, dataLength, new TypeSupport(typeof(Enum)), customSerializers, currentDepth, maxDepth, path, ignoreAttributes);
                case TypeId.Tuple:
                    return TypeReaders.ReadTupleType(newObj, reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes);
            }

            return TypeReaders.ReadValueType(reader, dataLength, typeSupport, customSerializers, currentDepth, maxDepth, path, ignoreAttributes);
        }

        internal static object ReadValueType(BinaryReader reader, int dataLength, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, string path, ICollection<Type> ignoreAttributes)
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

        internal static Array ReadArrayType(object newObj, BinaryReader reader, int length, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // length = entire collection
            // read each element
            var i = 0;
            var dataLength = 0;
            var genericType = typeSupport.ElementType;
            var listType = typeof(List<>).MakeGenericType(genericType);
            var newList = (IList)Activator.CreateInstance(listType);
            var enumerator = (IEnumerable)newObj;
            var elementTypeSupport = new TypeSupport(typeSupport.ElementType);
            while (i < length)
            {
                var element = ReadObject(reader, elementTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, ref dataLength);
                // increment by typeSize + length of data read + length header
                i += 1 + dataLength + sizeof(int);
                newList.Add(element);
            }

            // return the value
            newObj = TypeUtil.CreateEmptyObject(typeSupport.Type, length: newList.Count);
            newList.CopyTo((Array)newObj, 0);
            return (Array)newObj;
        }

        internal static object ReadEnumerableType(object newObj, BinaryReader reader, int length, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // length = entire collection
            // read each element
            var i = 0;
            var dataLength = 0;
            var genericType = typeSupport.Type.GetGenericArguments().First();
            var genericTypeSupport = new TypeSupport(genericType);
            var listType = typeof(List<>).MakeGenericType(genericType);
            var newList = (IList)Activator.CreateInstance(listType);
            newObj = newList;
            var enumerator = (IEnumerable)newObj;
            while (i < length)
            {
                var element = ReadObject(reader, genericTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, ref dataLength);
                // increment by typeSize + length of data read + length header
                i += 1 + dataLength + sizeof(int);
                newList.Add(element);
            }

            // return the value
            return newObj;
        }

        internal static object ReadTupleType(object newObj, BinaryReader reader, int length, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // length = entire collection
            // read each element, treat a tuple as a list of objects
            var i = 0;
            var dataLength = 0;
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => new TypeSupport(x)).ToList();
            Type tupleType = null;
            if (typeSupport.IsValueTuple)
                tupleType = TupleExtensions.CreateValueTuple(typeSupports.Select(x => x.Type).ToList());
            else
                tupleType = TupleExtensions.CreateTuple(typeSupports.Select(x => x.Type).ToList());
            var newTuple = TypeUtil.CreateEmptyObject(tupleType);
            newObj = newTuple;
            var index = 0;
            while (i < length)
            {
                var element = ReadObject(reader, typeSupports[index], customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, ref dataLength);
                // increment by typeSize + length of data read + length header
                i += 1 + dataLength + sizeof(int);
                if(typeSupport.IsValueTuple)
                    TypeUtil.SetFieldValue($"Item{index + 1}", newObj, element);
                else
                    TypeUtil.SetFieldValue($"m_Item{index + 1}", newObj, element);
                index++;
            }

            // return the value
            return newTuple;
        }

        internal static object ReadDictionaryType(object newObj, BinaryReader reader, int length, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // length = entire collection
            // read each element
            var i = 0;
            var dataLength = 0;
            var genericTypes = typeSupport.Type.GetGenericArguments().ToList();
            var typeSupports = genericTypes.Select(x => new TypeSupport(x)).ToList();
            var keyTypeSupport = typeSupports.First();
            var valueTypeSupport = typeSupports.Skip(1).First();
            Type[] typeArgs = { genericTypes[0], genericTypes[1] };

            var listType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
            var newDictionary = Activator.CreateInstance(listType) as IDictionary;
            newObj = newDictionary;
            var enumerator = (IDictionary)newObj;

            while (i < length)
            {
                var key = ReadObject(reader, keyTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, ref dataLength);
                // increment by typeSize + length of data read + length header
                i += 1 + dataLength + sizeof(int);
                var value = ReadObject(reader, valueTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, ref dataLength);
                // increment by typeSize + length of data read + length header
                i += 1 + dataLength + sizeof(int);
                newDictionary.Add(key, value);
            }

            // return the value
            return newObj;
        }

        internal static object ReadObjectType(object newObj, BinaryReader reader, int length, TypeSupport typeSupport, IDictionary<Type, Lazy<ICustomSerializer>> customSerializers, int currentDepth, int maxDepth, IDictionary<int, object> objectTree, string path, ICollection<Type> ignoreAttributes)
        {
            // read each property into the object
            var properties = TypeUtil.GetProperties(newObj).OrderBy(x => x.Name);
            var fields = TypeUtil.GetFields(newObj).OrderBy(x => x.Name);
            var rootPath = path;
            foreach (var property in properties)
            {
                path = $"{rootPath}.{property.Name}";
                var dataLength = 0;
                var propertyTypeSupport = new TypeSupport(property.PropertyType);
                if (property.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (propertyTypeSupport.IsDelegate)
                    continue;
                var propertyValue = ReadObject(reader, propertyTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, ref dataLength);
                TypeUtil.SetPropertyValue(property, newObj, propertyValue, path);
            }

            foreach (var field in fields)
            {
                path = $"{rootPath}.{field.Name}";
                var dataLength = 0;
                var fieldTypeSupport = new TypeSupport(field.FieldType);
                if (field.CustomAttributes.Any(x => ignoreAttributes.Contains(x.AttributeType)))
                    continue;
                if (fieldTypeSupport.IsDelegate)
                    continue;
                var fieldValue = ReadObject(reader, fieldTypeSupport, customSerializers, currentDepth, maxDepth, objectTree, path, ignoreAttributes, ref dataLength);
                TypeUtil.SetFieldValue(field, newObj, fieldValue, path);
            }

            return newObj;
        }
    }
}
