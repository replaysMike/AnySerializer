using System;

namespace AnySerializer
{
    /// <summary>
    /// Custom type mapping
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class SerializationTypeMap<TSource, TDestination> : SerializationTypeMap
    {
        public SerializationTypeMap() : base(typeof(TSource), typeof(TDestination))
        {
        }
    }

    /// <summary>
    /// Custom type mapping
    /// </summary>
    public class SerializationTypeMap
    {
        /// <summary>
        /// The source type
        /// </summary>
        public Type Source { get; set; }

        /// <summary>
        /// The destination type
        /// </summary>
        public Type Destination { get; set; }

        internal SerializationTypeMap(Type source, Type destination)
        {
            Source = source;
            Destination = destination;
        }

        public override string ToString() => $"{Source.Name} => {Destination.Name}";
    }
}
