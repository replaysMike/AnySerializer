using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeSupport;
using static AnySerializer.TypeManagement;

namespace AnySerializer
{
    public static class TypeUtil
    {
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
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                if (property.GetSetMethod(true) != null)
                {
                    var indexParameters = property.GetIndexParameters();
                    if (!indexParameters.Any())
#if FEATURE_SETVALUE
                        property.SetValue(obj, valueToSet);
#else
                        property.SetValue(obj, valueToSet, null);
#endif
                }
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
                if (property.GetSetMethod(true) != null)
                {
                    var indexParameters = property.GetIndexParameters();
                    if(!indexParameters.Any())
#if FEATURE_SETVALUE
                        property.SetValue(obj, valueToSet);
#else
                    property.SetValue(obj, valueToSet, null);
#endif
                }
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
        public static TypeId GetTypeId(ExtendedType typeSupport)
        {
            if (typeSupport.IsArray)
                return TypeId.Array;

            if (typeSupport.IsEnum)
                return TypeId.Enum;

            if (typeSupport.IsGeneric && typeSupport.Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                return TypeManagement.TypeMapping[typeof(KeyValuePair<,>)];

            // some other special circumstances

            if (typeSupport.IsTuple || typeSupport.IsValueTuple)
            {
                return TypeManagement.TypeMapping[typeof(Tuple<,>)];
            }

            if (TypeManagement.TypeMapping.ContainsKey(typeSupport.NullableBaseType))
            {
                return TypeManagement.TypeMapping[typeSupport.NullableBaseType];
            }

            if (typeof(KeyValuePair<,>).IsAssignableFrom(typeSupport.NullableBaseType))
            {
                return TypeManagement.TypeMapping[typeof(KeyValuePair<,>)];
            }

            if (typeof(IDictionary<,>).IsAssignableFrom(typeSupport.NullableBaseType)
                || typeSupport.NullableBaseType.IsGenericType
                && (
                    typeSupport.NullableBaseType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                    || typeSupport.NullableBaseType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>)
                    || typeSupport.NullableBaseType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                ))
                return TypeManagement.TypeMapping[typeof(IDictionary<,>)];

            if (typeof(IDictionary).IsAssignableFrom(typeSupport.NullableBaseType))
                return TypeManagement.TypeMapping[typeof(IDictionary)];

            if (typeof(IEnumerable).IsAssignableFrom(typeSupport.NullableBaseType))
                return TypeManagement.TypeMapping[typeof(IEnumerable)];

            if (typeSupport.Type == typeof(IntPtr))
                return TypeManagement.TypeMapping[typeof(long)];

            if (typeSupport.IsStruct && typeSupport.Type != typeof(decimal))
                return TypeId.Struct;

            if (typeSupport.IsValueType || typeSupport.IsPrimitive)
                throw new InvalidOperationException($"Unsupported type: {typeSupport.NullableBaseType.FullName}");

            return TypeId.Object;
        }

        /// <summary>
        /// Get type support for a given byte indicator
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ExtendedType GetType(TypeId type)
        {
            if (!TypeManagement.TypeMapping.Values.Contains(type))
                throw new InvalidOperationException($"Invalid type specified: {(int)type}");
            var typeSupport = new ExtendedType(TypeManagement.TypeMapping
                .Where(x => x.Value == type)
                .Select(x => x.Key)
                .FirstOrDefault());
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
            return (typeId & ~TypeId.NullValue & ~TypeId.TypeDescriptorMap & ~TypeId.TypeMapped);
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
        public static bool IsTypeMapped(TypeId type)
        {
            return ((int)type & (int)TypeId.TypeMapped) == (int)TypeId.TypeMapped;
        }
    }
}
