using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace AnySerializer
{
    /// <summary>
    /// Helper class for getting information about a <see cref="Type"/>
    /// </summary>
    public class TypeSupport : IEquatable<TypeSupport>, IEquatable<Type>
    {
        public Type Type { get; }
        public bool HasEmptyConstructor { get; private set; }
        public bool IsAbstract { get; private set; }
        public Type UnderlyingType { get; private set; }
        public bool IsImmutable { get; private set; }
        public bool IsEnumerable { get; private set; }
        public bool IsCollection { get; private set; }
        public bool IsArray { get; private set; }
        public bool IsDictionary { get; private set; }
        public bool IsGeneric { get; private set; }
        public bool IsDelegate { get; private set; }
        public bool IsValueType { get; private set; }
        public bool IsPrimitive { get; private set; }
        public bool IsEnum { get; private set; }
        public bool IsTuple { get; private set; }
        public bool IsValueTuple { get; private set; }
        public bool IsNullable { get; private set; }
        public bool IsInterface { get; private set; }
        public Type ConcreteType { get; private set; }
        public ICollection<Type> KnownConcreteTypes { get; private set; }
        public ICollection<Type> Attributes { get; private set; }
        public Type ElementType { get; private set; }
        public ICollection<Type> GenericArgumentTypes { get; private set; }
        public Type ElementNullableBaseType { get; private set; }
        public Type EnumType { get; private set; }
        public Type NullableBaseType { get; private set; }

        public TypeSupport(Type type)
        {
            if (type == null)
                throw new ArgumentNullException();
            Type = type;
            InspectType();
        }

        public void SetConcreteTypeFromInstance(object concreteObject)
        {
            if(concreteObject != null)
                ConcreteType = concreteObject.GetType();
        }

        private void InspectType()
        {
            var emptyConstructorDefined = Type.GetConstructor(Type.EmptyTypes);
            Attributes = new List<Type>();
            GenericArgumentTypes = new List<Type>();
            ConcreteType = Type;
            HasEmptyConstructor = Type.IsValueType || emptyConstructorDefined != null;
            IsAbstract = Type.IsAbstract;
            UnderlyingType = Type.UnderlyingSystemType;
            if (
                Type == typeof(string)
            )
                IsImmutable = true;

            // collections support
            IsArray = Type.IsArray;
            IsGeneric = Type.IsGenericType;
            if (IsArray)
            {
                ElementType = GetElementType(Type);
                if (ElementType != null)
                    ElementNullableBaseType = GetNullableBaseType(ElementType);
            }
            IsTuple = Type.IsValueTupleType() || Type.IsTupleType();
            IsValueTuple = Type.IsValueTupleType();
            IsValueType = Type.IsValueType;
            IsPrimitive = Type.IsPrimitive;
            IsInterface = Type.IsInterface;
            if (IsInterface)
                KnownConcreteTypes = GetConcreteTypes(Type);
            IsEnum = Type.IsEnum;
            if (IsEnum)
                EnumType = Type.GetEnumUnderlyingType();

            if (IsGeneric)
            {
                var args = Type.GetGenericArguments();
                if(args?.Any() == true)
                {
                    foreach(var arg in args)
                        GenericArgumentTypes.Add(arg);
                }
            }

            if (typeof(IEnumerable).IsAssignableFrom(Type))
            {
                IsEnumerable = true;
                if (IsGeneric)
                {
                    var args = Type.GetGenericArguments();
                    ElementType = args.FirstOrDefault();
                    if (ElementType != null)
                        ElementNullableBaseType = GetNullableBaseType(ElementType);
                }
            }
            if (Type.IsGenericType && typeof(Collection<>).IsAssignableFrom(Type.GetGenericTypeDefinition()))
            {
                IsCollection = true;
                var args = Type.GetGenericArguments();
                ElementType = args.FirstOrDefault();
                if (ElementType != null)
                    ElementNullableBaseType = GetNullableBaseType(ElementType);
            }
            if (Type.IsGenericType && (Type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || Type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                IsDictionary = true;
            if (typeof(Delegate).IsAssignableFrom(Type))
                IsDelegate = true;

            // nullable
            var nullableBaseType = GetNullableBaseType(Type);
            NullableBaseType = nullableBaseType ?? Type;
            IsNullable = nullableBaseType != null;

            // attributes
            if (Type.CustomAttributes.Any())
                Attributes = Type.CustomAttributes.Select(x => x.AttributeType).ToList();
        }

        public ICollection<Type> GetConcreteTypes(Type type)
        {
            var typeAssembly = Assembly.GetAssembly(type);

            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                return typeAssembly.GetTypes()
                    .Select(p => new KeyValuePair<Type, Type[]>(p, p.GetInterfaces()))
                    .Where(x => x.Value.Any(y => y.IsGenericType && y.GetGenericTypeDefinition().Equals(genericType)))
                    .Select(x => x.Key)
                    .ToList();
            }
            else
            {
                var allTypes = typeAssembly.GetTypes()
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                    .ToList();

                return allTypes;
            }
        }

        public bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public Type GetConcreteType(object obj)
        {
            var objectType = obj.GetType();
            if(KnownConcreteTypes != null)
                return KnownConcreteTypes.Where(x => objectType.IsAssignableFrom(x)).FirstOrDefault();
            return objectType;
        }

        public Type GetNullableBaseType(Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        public Type GetElementType(Type type)
        {
            return type.GetElementType();
        }

        public override bool Equals(object obj)
        {
            var objTyped = (TypeSupport)obj;
            return objTyped.Type.Equals(Type);
        }

        public override string ToString()
        {
            return $"{Type.Name} ({UnderlyingType.Name})";
        }

        public bool Equals(TypeSupport other)
        {
            return other.Type.Equals(Type);
        }

        public bool Equals(Type other)
        {
            return other.Equals(Type);
        }
    }

    /// <summary>
    /// Helper class for getting information about a <see cref="Type"/>
    /// </summary>
    public class TypeSupport<T> : TypeSupport
    {
        public TypeSupport() : base(typeof(T))
        {
        }
    }

    public static class TypeSupportExtensions
    {
        /// <summary>
        /// Get the Type Support for a Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeSupport TypeSupport(this Type type)
        {
            return new TypeSupport(type);
        }
    }
}
