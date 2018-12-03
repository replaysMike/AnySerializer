using System;
using System.Runtime.Serialization;

namespace AnySerializer.Tests.TestObjects
{
    public class ConcreteInterfaceObject : IEquatable<ConcreteInterfaceObject>
    {
        public int Id { get; set; }

        public ITestInterface TestInterface { get; set; }

        // todo: this shouldn't be needed
        public ConcreteInterfaceObject() { }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var basicObject = (ConcreteInterfaceObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ConcreteInterfaceObject other)
        {
            return Id == other.Id
                && (TestInterface.Name == null && other.TestInterface.Name == null) || TestInterface.Name.Equals(other.TestInterface.Name)
                && (TestInterface.SecondaryIgnoredProperty == null && other.TestInterface.SecondaryIgnoredProperty == null) || TestInterface.SecondaryIgnoredProperty.Equals(other.TestInterface.SecondaryIgnoredProperty)
                ;
        }
    }

    public interface ITestInterface
    {
        string Name { get; set; }
        [IgnoreDataMember]
        string SecondaryIgnoredProperty { get; set; }
    }

    public class TestConcreteInterface : ITestInterface
    {
        public string Name { get; set; }

        [IgnoreDataMember]
        public string SecondaryIgnoredProperty { get; set; }
    }
}
