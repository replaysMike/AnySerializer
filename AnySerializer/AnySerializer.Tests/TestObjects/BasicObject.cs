using System;

namespace AnySerializer.Tests.TestObjects
{
    public class BasicObject : IEquatable<BasicObject>
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }

        public override bool Equals(object obj)
        {
            var basicObject = (BasicObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(BasicObject other)
        {
            return Id == other.Id
                && (Description == null && other.Description == null) || Description.Equals(other.Description)
                && IsEnabled == other.IsEnabled
                ;
        }
    }
}
