using System;
using System.Collections.Generic;
using System.Linq;

namespace AnySerializer
{
    /// <summary>
    /// A type registry to map one type to another
    /// </summary>
    public class SerializationTypeRegistry
    {
        /// <summary>
        /// Gets the registered type mappings
        /// </summary>
        public ICollection<SerializationTypeMap> Mappings { get; private set; } = new List<SerializationTypeMap>();
        /// <summary>
        /// Gets the registered type factories
        /// </summary>
        public ICollection<SerializationTypeFactory> Factories { get; private set; } = new List<SerializationTypeFactory>();

        private SerializationTypeRegistry() { }

        internal SerializationTypeRegistry(SerializationTypeMap[] typeMaps)
        {
            Mappings = new List<SerializationTypeMap>();
            foreach (var typeMap in typeMaps)
                Mappings.Add(typeMap);
        }

        /// <summary>
        /// Add a type mapping from source type to destination type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        public void AddMapping<TSource, TDestination>()
            => Mappings.Add(new SerializationTypeMap<TSource, TDestination>());

        /// <summary>
        /// True if a mapping exists for the source type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool ContainsType(Type type)
            => Mappings.Any(x => x.Source.Equals(type));

        /// <summary>
        /// True if a factory exists for the source type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool ContainsFactoryType(Type type)
            => Factories.Any(x => x.Source.Equals(type));

        /// <summary>
        /// Get the destination mapping for a source type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal Type GetMapping(Type type)
        {
            var mapping = Mappings
                .Where(x => x.Source.Equals(type))
                .Select(x => x.Destination)
                .FirstOrDefault();
            return mapping;
        }

        /// <summary>
        /// Configure a new type registry
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static SerializationTypeRegistry Configure(Action<SerializationTypeRegistry> config)
        {
            var registry = new SerializationTypeRegistry();
            config(registry);
            return registry;
        }

        /// <summary>
        /// For the source type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SerializationTypeConfiguration<T> For<T>()
            => new SerializationTypeConfiguration<T>();
    }
}
