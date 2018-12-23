namespace AnySerializer
{
    public static class Constants
    {
        /// <summary>
        /// The size of the data settings header value (byte-0)
        /// </summary>
        public static readonly uint DataSettingsSize = 1;
        /// <summary>
        /// The size of the object type header
        /// </summary>
        public static readonly uint TypeHeaderSize = 1;
        /// <summary>
        /// The size of the length header
        /// </summary>
        public static readonly uint LengthHeaderSize = sizeof(uint);
        /// <summary>
        /// The size of the length header in compact mode
        /// </summary>
        public static readonly uint CompactLengthHeaderSize = sizeof(ushort);
        /// <summary>
        /// The size of the object reference id
        /// </summary>
        public static readonly uint ObjectReferenceIdSize = sizeof(ushort);
        /// <summary>
        /// The size of the object type id (Type Descriptor)
        /// </summary>
        public static readonly uint ObjectTypeDescriptorId = sizeof(ushort);
        /// <summary>
        /// The maximum recursion depth to use, by default.
        /// </summary>
        public static readonly uint DefaultMaxDepth = 32;
    }
}
