using System;
using System.Runtime.Serialization;

namespace AnySerializer.Tests.TestObjects
{
    public class IgnorePropertiesObject : IEquatable<IgnorePropertiesObject>
    {
        public int Id { get; set; }
        [IgnoreDataMember]
        public string Name { get; set; }


        public override bool Equals(object obj)
        {
            var basicObject = (IgnorePropertiesObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(IgnorePropertiesObject other)
        {
            return Id == other.Id
                && (Name == null && other.Name == null) || Name.Equals(other.Name)
                ;
        }
    }
}
