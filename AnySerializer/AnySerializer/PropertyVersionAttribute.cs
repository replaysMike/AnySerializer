using System;

namespace AnySerializer
{
    /// <summary>
    /// Tell the deserializer to skip any properties that match this tag
    /// </summary>
    public class PropertyVersionAttribute : Attribute
    {
        public string Tag { get; set; }

        public PropertyVersionAttribute(string tag)
        {
            Tag = tag;
        }
    }
}
