using System;
using System.Collections.Generic;
using System.Linq;

namespace AnySerializer
{
    /// <summary>
    /// A type registry to map one type to another
    /// </summary>
    public class TypeRegistry
    {
        /// <summary>
        /// Gets the registered type mappings
        /// </summary>
        public ICollection<TypeMap> Mappings { get; private set; }
        /// <summary>
        /// Gets the registered type factories
        /// </summary>
        public ICollection<TypeFactory> Factories { get; private set; }

        private TypeRegistry()
        {
            Mappings = new List<TypeMap>();
            Factories = new List<TypeFactory>();
        }

        internal TypeRegistry(TypeMap[] typeMaps)
        {
            Mappings = new List<TypeMap>();
            foreach (var typeMap in typeMaps)
                Mappings.Add(typeMap);
        }

        /// <summary>
        /// Add a type mapping from source type to destination type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        public void AddMapping<TSource, TDestination>()
        {
            Mappings.Add(new TypeMap<TSource, TDestination>());
        }

        /// <summary>
        /// Add a type factory for creating types
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="factory"></param>
        public void AddFactory<TSource, TDestination>(Func<TDestination> factory)
        {
            Factories.Add(new TypeFactory<TSource, TDestination>(factory));
        }

        /// <summary>
        /// True if a mapping exists for the source type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool ContainsType(Type type)
        {
            return Mappings.Any(x => x.Source.Equals(type));
        }

        /// <summary>
        /// True if a factory exists for the source type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool ContainsFactoryType(Type type)
        {
            return Factories.Any(x => x.Source.Equals(type));
        }

        /// <summary>
        /// Get the destination mapping for a source type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Type GetMapping(Type type)
        {
            return Mappings.Where(x => x.Source.Equals(type)).Select(x => x.Destination).FirstOrDefault();
        }

        /// <summary>
        /// Get the factory for creating a source type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Func<object> GetFactory(Type type)
        {
            return Factories.Where(x => x.Source.Equals(type)).Select(x => x.Factory).FirstOrDefault();
        }

        /// <summary>
        /// Configure a new type registry
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static TypeRegistry Configure(Action<TypeRegistry> config)
        {
            var registry = new TypeRegistry();
            config(registry);
            return registry;
        }

        /// <summary>
        /// For the source type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TypeConfiguration<T> For<T>()
        {
            return new TypeConfiguration<T>();
        }
    }

    /// <summary>
    /// Type factory
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class TypeFactory<TSource, TDestination> : TypeFactory
    {
        public new Func<TDestination> Factory { get; }

        public TypeFactory(Func<TDestination> factory) : base(typeof(TSource))
        {
            Factory = factory;
        }
    }

    /// <summary>
    /// Type factory
    /// </summary>
    public class TypeFactory
    {
        public Type Source { get; }
        public Func<object> Factory { get; }
        internal TypeFactory(Type source)
        {
            Source = source;
        }
        internal TypeFactory(Type source, Func<object> factory)
        {
            Source = source;
            Factory = factory;
        }
    }

    public class TypeConfiguration<TSource>
    {
        /// <summary>
        /// Map to a destination type
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        public TypeMap Create<TDestination>()
        {
            return new TypeMap<TSource, TDestination>();
        }

        /// <summary>
        /// Use a factory to create instance of type
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public TypeFactory<TSource, TDestination> CreateUsing<TDestination>(Func<TDestination> factory)
        {
            return new TypeFactory<TSource, TDestination>(factory);
        }
    }

    /// <summary>
    /// Custom type mapping
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class TypeMap<TSource, TDestination> : TypeMap
    {
        public TypeMap() : base(typeof(TSource), typeof(TDestination))
        {
        }
    }

    /// <summary>
    /// Custom type mapping
    /// </summary>
    public class TypeMap
    {
        public Type Source { get; set; }
        public Type Destination { get; set; }

        internal TypeMap(Type source, Type destination)
        {
            Source = source;
            Destination = destination;
        }

        public override string ToString()
        {
            return $"{Source.Name} => {Destination.Name}";
        }
    }
}
