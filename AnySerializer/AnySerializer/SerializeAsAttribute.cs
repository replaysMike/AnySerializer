using System;

namespace AnySerializer
{
    public class SerializeAsAttribute : Attribute
    {
        /// <summary>
        /// Alternate name to serialize data as
        /// </summary>
        public string Name { get; set; }

        public SerializeAsAttribute(string name)
        {
            Name = name;
        }
    }
}
