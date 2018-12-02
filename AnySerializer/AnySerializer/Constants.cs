namespace AnySerializer
{
    public static class Constants
    {
        /// <summary>
        /// The size of the data settings header value (byte-0)
        /// </summary>
        public const int DataSettingsSize = 1;
        /// <summary>
        /// The size of the object type header
        /// </summary>
        public const int TypeHeaderSize = 1;
        /// <summary>
        /// The size of the length header
        /// </summary>
        public const int LengthHeaderSize = sizeof(uint);
        /// <summary>
        /// The size of the length header in compact mode
        /// </summary>
        public const int CompactLengthHeaderSize = sizeof(ushort);
        /// <summary>
        /// The size of the object reference id
        /// </summary>
        public const int ObjectReferenceIdSize = sizeof(ushort);
        /// <summary>
        /// The size of the object type id (Type Descriptor)
        /// </summary>
        public const int ObjectTypeDescriptorId = sizeof(ushort);
        /// <summary>
        /// The maximum recursion depth to use, by default.
        /// </summary>
        public const int DefaultMaxDepth = 32;
    }
}
