using System;

namespace AnySerializer
{
    public class SerializationTypeConfiguration<TSource>
    {
        /// <summary>
        /// Map to a destination type
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        public SerializationTypeMap Create<TDestination>() => new SerializationTypeMap<TSource, TDestination>();

        /// <summary>
        /// Use a factory to create instance of type
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public SerializationTypeFactory<TSource, TDestination> CreateUsing<TDestination>(Func<TDestination> factory) => new SerializationTypeFactory<TSource, TDestination>(factory);
    }
}
