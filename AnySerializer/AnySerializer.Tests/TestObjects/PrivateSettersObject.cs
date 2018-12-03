using System;

namespace AnySerializer.Tests.TestObjects
{
    public class PrivateSettersObject : IEquatable<PrivateSettersObject>
    {
        public int Id { get; }
        public PrivateSettersObject(int id)
        {
            Id = id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var basicObject = (PrivateSettersObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(PrivateSettersObject other)
        {
            return Id == other.Id
                ;
        }
    }
}
