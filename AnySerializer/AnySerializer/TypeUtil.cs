using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    public static class TypeUtil
    {
        /// <summary>
        /// Create a new, empty object of a given type
        /// </summary>
        /// <param name="type">The type of object to construct</param>
        /// <param name="typeRegistry">A type registry for constructing unknown types</param>
        /// <param name="typeDescriptor">A type descriptor that indicates the embedded concrete type for an interface type</param>
        /// <param name="initializer">An optional initializer to use to create the object</param>
        /// <param name="length">For array types, the length of the array to create</param>
        /// <returns></returns>
        public static object CreateEmptyObject(Type type, TypeRegistry typeRegistry, TypeDescriptor typeDescriptor, Func<object> initializer = null, int length = 0)
        {
            if (initializer != null)
                return initializer();

            // check the type registry for a custom type mapping
            if (typeRegistry?.ContainsType(type) == true)
                type = typeRegistry.GetMapping(type);
            // check the type registry for a custom type factory
            if (typeRegistry?.ContainsFactoryType(type) == true)
                return typeRegistry.GetFactory(type).Invoke();

            var typeSupport = new TypeSupport(type);
            // if we are asked to create an instance of an interface, try to initialize using a valid concrete type
            if (typeSupport.IsInterface && !typeSupport.IsEnumerable)
            {
                if (typeDescriptor != null)
                {
                    // override the type passed with the one embedded in the typeDescriptors
                    type = Type.GetType(typeDescriptor.FullName);
                    typeSupport = new TypeSupport(type);
                }
                else
                {
                    // try a known concrete type from typeSupport
                    var concreteType = typeSupport.KnownConcreteTypes.FirstOrDefault();
                    if (concreteType == null)
                        throw new InvalidOperationException($"Unable to locate a concrete type for '{typeSupport.Type.FullName}'! Cannot create instance.");

                    typeSupport = new TypeSupport(concreteType);
                }
            }

            if (typeSupport.IsArray)
                return Activator.CreateInstance(typeSupport.Type, new object[] { length });
            else if (typeSupport.IsDictionary)
            {
                var genericType = typeSupport.Type.GetGenericArguments().ToList();
                if (genericType.Count != 2)
                    throw new InvalidOperationException("IDictionary should contain 2 element types.");
                Type[] typeArgs = { genericType[0], genericType[1] };

                var listType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
                var newDictionary = Activator.CreateInstance(listType) as IDictionary;
                return newDictionary;
            }
            else if (typeSupport.IsEnumerable && !typeSupport.IsImmutable)
            {
                var genericType = typeSupport.Type.GetGenericArguments().FirstOrDefault();
                if (genericType != null)
                {
                    var listType = typeof(List<>).MakeGenericType(genericType);
                    var newList = (IList)Activator.CreateInstance(listType);
                    return newList;
                }
                return Enumerable.Empty<object>();
            }
            else if (typeSupport.Type.ContainsGenericParameters)
            {
                // create a generic type and create an instance
                // to accomplish this, we need to create a new generic type using the type arguments from the interface
                // and the concrete class definition. voodoo!
                var originalTypeSupport = new TypeSupport(type);
                var genericArguments = originalTypeSupport.Type.GetGenericArguments();
                var newType = typeSupport.Type.MakeGenericType(genericArguments);
                object newObject = null;
                if (typeSupport.HasEmptyConstructor)
                    newObject = Activator.CreateInstance(newType);
                else
                    newObject = FormatterServices.GetUninitializedObject(newType);
                return newObject;
            }
            else if (typeSupport.HasEmptyConstructor && !typeSupport.Type.ContainsGenericParameters)
                return Activator.CreateInstance(typeSupport.Type);
            else if (typeSupport.IsImmutable)
                return null;

            return FormatterServices.GetUninitializedObject(typeSupport.Type);
        }

        /// <summary>
        /// Create a new, empty object of a given type
        /// </summary>
        /// <param name="initializer">An optional initializer to use to create the object</param>
        /// <param name="length">For array types, the length of the array to create</param>
        /// <returns></returns>
        public static T CreateEmptyObject<T>(Func<T> initializer = null, int length = 0)
        {
            var type = typeof(T);
            if (initializer != null)
                return initializer();

            var typeSupport = new TypeSupport(type);

            if (typeSupport.IsArray)
                return (T)Activator.CreateInstance(typeSupport.Type, new object[] { length });
            else if (typeSupport.IsDictionary)
            {
                var genericType = typeSupport.Type.GetGenericArguments().ToList();
                if (genericType.Count != 2)
                    throw new InvalidOperationException("IDictionary should contain 2 element types.");
                Type[] typeArgs = { genericType[0], genericType[1] };

                var listType = typeof(Dictionary<,>).MakeGenericType(typeArgs);
                var newDictionary = Activator.CreateInstance(listType) as IDictionary;
                return (T)newDictionary;
            }
            else if (typeSupport.IsEnumerable && !typeSupport.IsImmutable)
            {
                var genericType = typeSupport.Type.GetGenericArguments().FirstOrDefault();
                if (genericType != null)
                {
                    var listType = typeof(List<>).MakeGenericType(genericType);
                    var newList = (IList)Activator.CreateInstance(listType);
                    return (T)newList;
                }
                return (T)Enumerable.Empty<object>();
            }
            else if (typeSupport.HasEmptyConstructor)
                return (T)Activator.CreateInstance(typeSupport.Type);
            else if (typeSupport.IsImmutable)
                return default(T);
            return (T)FormatterServices.GetUninitializedObject(typeSupport.Type);
        }

        /// <summary>
        /// Get all of the properties of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ICollection<PropertyInfo> GetProperties(object obj)
        {
            if (obj != null)
            {
                var t = obj.GetType();
                return t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return null;
        }

        /// <summary>
        /// Get all of the fields of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeAutoPropertyBackingFields">True to include the compiler generated backing fields for auto-property getters/setters</param>
        /// <returns></returns>
        public static ICollection<FieldInfo> GetFields(object obj, bool includeAutoPropertyBackingFields = false)
        {
            if (obj != null)
            {
                var t = obj.GetType();
                var allFields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (!includeAutoPropertyBackingFields)
                {
                    var allFieldsExcludingAutoPropertyFields = allFields.Where(x => !x.Name.Contains("k__BackingField")).ToList();
                    return allFieldsExcludingAutoPropertyFields;
                }
                return allFields;
            }
            return null;
        }

        /// <summary>
        /// Get a property from an object instance
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(object obj, string name)
        {
            if (obj != null)
            {
                var t = obj.GetType();
                return t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return null;
        }

        /// <summary>
        /// Get a field from an object instance
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FieldInfo GetField(object obj, string name)
        {
            if (obj != null)
            {
                var t = obj.GetType();
                return t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return null;
        }

        public static void SetPropertyValue(string propertyName, object obj, object valueToSet)
        {
            var type = obj.GetType();
            // var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                if(property.SetMethod != null)
                    property.SetValue(obj, valueToSet);
                else
                {
                    // if this is an auto-property with a backing field, set it
                    var field = obj.GetType().GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                        field.SetValue(obj, valueToSet);
                    else
                        throw new ArgumentException($"Property '{propertyName}' does not exist.");
                }
            }               
            else
                throw new ArgumentException($"Property '{propertyName}' does not exist.");
        }

        public static void SetFieldValue(string fieldName, object obj, object valueToSet)
        {
            var type = obj.GetType();
            // var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(obj, valueToSet);
            else
                throw new ArgumentException($"Field '{fieldName}' does not exist.");
        }

        public static void SetPropertyValue(PropertyInfo property, object obj, object valueToSet, string path)
        {
            try
            {
                if (property.SetMethod != null)
                    property.SetValue(obj, valueToSet);
                else
                {
                    // if this is an auto-property with a backing field, set it
                    var field = obj.GetType().GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                        field.SetValue(obj, valueToSet);
                }
            }
            catch (Exception ex)
            {
                //OnError?.Invoke(ex, path, property, obj);
                //if (OnError == null)
                throw ex;
            }
        }

        public static void SetFieldValue(FieldInfo field, object obj, object valueToSet, string path)
        {
            try
            {
                field.SetValue(obj, valueToSet);
            }
            catch (Exception ex)
            {
                //OnError?.Invoke(ex, path, field, obj);
                //if (OnError == null)
                throw ex;
            }
        }


        /// <summary>
        /// Get the byte indicator for a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeId GetTypeId(TypeSupport typeSupport)
        {
            if (typeSupport.IsArray)
                return TypeId.Array;

            if (typeSupport.IsEnum)
                return TypeId.Enum;

            if (TypeManagement.TypeMapping.ContainsKey(typeSupport.NullableBaseType))
                return TypeManagement.TypeMapping[typeSupport.NullableBaseType];

            // some other special circumstances

            if (typeSupport.IsTuple)
            {
                return TypeManagement.TypeMapping[typeof(Tuple<,>)];
            }

            if (typeof(IDictionary<,>).IsAssignableFrom(typeSupport.NullableBaseType)
                || typeSupport.NullableBaseType.IsGenericType
                && (
                    typeSupport.NullableBaseType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                    || typeSupport.NullableBaseType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                ))
                return TypeManagement.TypeMapping[typeof(IDictionary<,>)];

            if (typeof(IDictionary).IsAssignableFrom(typeSupport.NullableBaseType))
                return TypeManagement.TypeMapping[typeof(IDictionary)];

            if (typeof(IEnumerable).IsAssignableFrom(typeSupport.NullableBaseType))
                return TypeManagement.TypeMapping[typeof(IEnumerable)];

            if (typeSupport.IsValueType || typeSupport.IsPrimitive)
                throw new InvalidOperationException($"Unsupported type: {typeSupport.NullableBaseType.FullName}");

            return TypeManagement.TypeMapping[typeof(object)];
        }

        /// <summary>
        /// Get type support for a given byte indicator
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeSupport GetType(TypeId type)
        {
            if (!TypeManagement.TypeMapping.Values.Contains(type))
                throw new InvalidOperationException($"Invalid type specified: {(int)type}");
            var typeSupport = new TypeSupport(TypeManagement.TypeMapping.Where(x => x.Value == type).Select(x => x.Key).FirstOrDefault());
            return typeSupport;
        }

        /// <summary>
        /// Get the type id without special flag bits
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeId GetTypeId(byte typeId)
        {
            return GetTypeId((TypeId)typeId);
        }

        /// <summary>
        /// Get the type id without special flag bits
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeId GetTypeId(TypeId typeId)
        {
            return (typeId & ~TypeId.NullValue & ~TypeId.TypeDescriptorMap & ~TypeId.AbstractInterface);
        }

        /// <summary>
        /// True if the type is a value type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueType(TypeId type)
        {
            return type == TypeId.Bool
                || type == TypeId.Byte
                || type == TypeId.Char
                || type == TypeId.DateTime
                || type == TypeId.Decimal
                || type == TypeId.Double
                || type == TypeId.Enum
                || type == TypeId.Float
                || type == TypeId.Guid
                || type == TypeId.Int
                || type == TypeId.Long
                || type == TypeId.Point
                || type == TypeId.Short
                || type == TypeId.String
                || type == TypeId.TimeSpan;
        }

        /// <summary>
        /// True if the value of the type contains a null value
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullValue(TypeId type)
        {
            return ((int)type & (int)TypeId.NullValue) == (int)TypeId.NullValue;
        }

        /// <summary>
        /// True if the value indicates a type descriptor map is stored
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTypeDescriptorMap(TypeId type)
        {
            return ((int)type & (int)TypeId.TypeDescriptorMap) == (int)TypeId.TypeDescriptorMap;
        }

        /// <summary>
        /// True if the type is an abstract interface
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAbstractInterface(TypeId type)
        {
            return ((int)type & (int)TypeId.AbstractInterface) == (int)TypeId.AbstractInterface;
        }
    }
}
