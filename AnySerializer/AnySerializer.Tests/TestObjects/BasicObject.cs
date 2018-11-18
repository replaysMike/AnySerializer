using System;

namespace AnySerializer.Tests.TestObjects
{
    public class BasicObject : IEquatable<BasicObject>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        public override bool Equals(object obj)
        {
            var basicObject = (BasicObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(BasicObject other)
        {
            return Id == other.Id
                && (Name == null && other.Name == null) || Name.Equals(other.Name)
                && IsEnabled == other.IsEnabled
                ;
        }
    }
}
