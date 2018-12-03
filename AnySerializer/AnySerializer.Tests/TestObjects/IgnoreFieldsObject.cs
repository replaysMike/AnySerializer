using System;
using System.Runtime.Serialization;

namespace AnySerializer.Tests.TestObjects
{
    public class IgnoreFieldsObject : IEquatable<IgnoreFieldsObject>
    {
        [IgnoreDataMember]
        private int _anInternalField;

        public int Id { get; set; }
        public string Name { get; set; }

        public IgnoreFieldsObject(int internalFieldValue)
        {
            _anInternalField = internalFieldValue;
        }

        public int GetInternalField()
        {
            return _anInternalField;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var basicObject = (IgnoreFieldsObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(IgnoreFieldsObject other)
        {
            return Id == other.Id
                && (Name == null && other.Name == null) || Name.Equals(other.Name)
                && _anInternalField == other._anInternalField
                ;
        }
    }
}
