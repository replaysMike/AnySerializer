using System;

namespace AnySerializer
{
    /// <summary>
    /// Type factory
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    public class SerializationTypeFactory<TSource, TDestination> : SerializationTypeFactory
    {
        /// <summary>
        /// Generic factory method
        /// </summary>
        public new Func<TDestination> Factory { get; }

        public SerializationTypeFactory(Func<TDestination> factory) : base(typeof(TSource), factory as Func<object>)
        {
            Factory = factory;
        }
    }

    /// <summary>
    /// Type factory
    /// </summary>
    public class SerializationTypeFactory
    {
        /// <summary>
        /// The source type
        /// </summary>
        public Type Source { get; }

        /// <summary>
        /// The factory method
        /// </summary>
        public Func<object> Factory { get; }

        public SerializationTypeFactory(Type source)
        {
            Source = source;
        }

        public SerializationTypeFactory(Type source, Func<object> factory)
        {
            Source = source;
            Factory = factory;
        }
    }
}
