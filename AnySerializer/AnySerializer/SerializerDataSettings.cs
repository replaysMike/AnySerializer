using System;

namespace AnySerializer
{
    [Flags]
    public enum SerializerDataSettings : byte
    {
        /// <summary>
        /// No settings configured
        /// </summary>
        None = 0,
        /// <summary>
        /// Compact mode - uses less data but max 65k object size
        /// </summary>
        Compact = 1,
        /// <summary>
        /// Compress mode - uses LZ4 compression to reduce size of large objects
        /// </summary>
        Compress = 2,
        /// <summary>
        /// Contains an embedded TypeMap
        /// </summary>
        TypeMap = 4,
    }
}
