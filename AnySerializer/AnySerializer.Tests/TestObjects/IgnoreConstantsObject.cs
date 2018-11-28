using System;

namespace AnySerializer.Tests.TestObjects
{
    public class IgnoreConstantsObject : IEquatable<IgnoreConstantsObject>
    {
        public const int ConstantValue = 1;
        public int A;
        public int Z;
        public IgnoreConstantsObject() { }

        public override bool Equals(object obj)
        {
            var basicObject = (IgnoreConstantsObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(IgnoreConstantsObject other)
        {
            return A == other.A
                && Z == other.Z
                ;
        }
    }
}
