namespace AnySerializer.CustomSerializers
{
    public interface ICustomSerializer<T> : ICustomSerializer
    {
        /// <summary>
        /// Serialize a custom type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        byte[] Serialize(T type);

        /// <summary>
        /// Deserialize a custom type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        new T Deserialize(byte[] bytes, uint length);
    }

    public interface ICustomSerializer
    {
        /// <summary>
        /// The data size of the custom object
        /// </summary>
        short DataSize { get; }

        /// <summary>
        /// Serialize a custom type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        byte[] Serialize(object type);

        /// <summary>
        /// Deserialize a custom type
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        object Deserialize(byte[] bytes, uint length);
    }
}
