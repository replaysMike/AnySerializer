using System;

namespace AnySerializer.Tests.TestObjects
{
    public class NoConstructorObject : IEquatable<NoConstructorObject>
    {
        public int Id { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var basicObject = (NoConstructorObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(NoConstructorObject other)
        {
            return Id == other.Id
                ;
        }
    }
}
