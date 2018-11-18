namespace AnySerializer
{
    public static class Constants
    {
        /// <summary>
        /// The size of the object type header
        /// </summary>
        public const int TypeHeaderSize = 1;
        /// <summary>
        /// The size of the length header
        /// </summary>
        public const int LengthHeaderSize = sizeof(int);
        /// <summary>
        /// The maximum recursion depth to use, by default.
        /// </summary>
        public const int DefaultMaxDepth = 1000;
    }
}
