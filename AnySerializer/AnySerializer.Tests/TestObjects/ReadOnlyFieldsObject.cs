using System;

namespace AnySerializer.Tests.TestObjects
{
    public class ReadOnlyFieldsObject : IEquatable<ReadOnlyFieldsObject>
    {
        private readonly int _id;

        public ReadOnlyFieldsObject(int id)
        {
            _id = id;
        }

        public override bool Equals(object obj)
        {
            var basicObject = (ReadOnlyFieldsObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ReadOnlyFieldsObject other)
        {
            return _id == other._id
                ;
        }
    }
}
