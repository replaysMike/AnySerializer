using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace AnySerializer
{
    public static class TypeUtil
    {
        private static readonly IDictionary<Type, TypeId> _switch = new Dictionary<Type, TypeId> {
                        { typeof(bool), TypeId.Bool },
                        { typeof(byte), TypeId.Byte },
                        { typeof(short), TypeId.Short },
                        { typeof(int), TypeId.Int },
                        { typeof(long), TypeId.Long },
                        { typeof(float), TypeId.Float },
                        { typeof(double), TypeId.Double },
                        { typeof(decimal), TypeId.Decimal },
                        { typeof(string), TypeId.String },
                        { typeof(char), TypeId.Char },
                        { typeof(object), TypeId.Object },
                        { typeof(Array), TypeId.Array },
                        { typeof(IEnumerable), TypeId.IEnumerable },
                        { typeof(IDictionary<,>), TypeId.IDictionary },
                    };

        /// <summary>
        /// Create a new, empty object of a given type
        /// </summary>
        /// <param name="type">The type of object to construct</param>
        /// <param name="initializer">An optional initializer to use to create the object</param>
        /// <param name="length">For array types, the length of the array to create</param>
        /// <returns></returns>
        public static object CreateEmptyObject(Type type, Func<object> initializer = null, int length = 0)
        {
            if (initializer != null)
                return initializer();

            var typeSupport = new TypeSupport(type);

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
            else if (typeSupport.HasEmptyConstructor)
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
            return new PropertyInfo[0];
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
            return new FieldInfo[0];
        }

        public static void SetPropertyValue(PropertyInfo property, object obj, object valueToSet, string path)
        {
            try
            {
                if (property.SetMethod != null)
                {
                    property.SetValue(obj, valueToSet);
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
        public static TypeId GetTypeId(TypeSupport typeSupport)
        {
            if (typeSupport.IsArray)
                return TypeId.Array;

            if (_switch.ContainsKey(typeSupport.Type))
                return _switch[typeSupport.Type];

            // some other special circumstances

            if (typeof(IDictionary<,>).IsAssignableFrom(typeSupport.Type)
                || typeSupport.Type.IsGenericType
                && (
                    typeSupport.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                    || typeSupport.Type.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                ))
                return _switch[typeof(IDictionary<,>)];

            if (typeof(IDictionary).IsAssignableFrom(typeSupport.Type))
                return _switch[typeof(IDictionary)];

            if (typeof(IEnumerable).IsAssignableFrom(typeSupport.Type))
                return _switch[typeof(IEnumerable)];

            return _switch[typeof(object)];
        }

        /// <summary>
        /// Get type support for a given byte indicator
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeSupport GetType(TypeId type)
        {
            return new TypeSupport(_switch.Where(x => x.Value == type).Select(x => x.Key).First());
        }

        /// <summary>
        /// The internal data type
        /// </summary>
        public enum TypeId : byte
        {
            Bool = 1,
            Byte,
            Short,
            Int,
            Long,
            Float,
            Double,
            Decimal,
            String,
            Char,
            Object,
            Array,
            IEnumerable,
            IDictionary
        }
    }
}
