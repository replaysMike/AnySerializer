using System;
using System.Runtime.Serialization;

namespace AnySerializer.Tests.TestObjects
{
    public class ConcreteGenericInterfaceObject : IEquatable<ConcreteGenericInterfaceObject>
    {
        public int Id { get; set; }

        public IGenericTestInterface<bool> TestInterface { get; set; }

        // todo: this shouldn't be needed
        public ConcreteGenericInterfaceObject() { }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var basicObject = (ConcreteGenericInterfaceObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ConcreteGenericInterfaceObject other)
        {
            return Id == other.Id
                && (TestInterface.Name == null && other.TestInterface.Name == null) || TestInterface.Name.Equals(other.TestInterface.Name)
                && TestInterface.TestObject.Equals(other.TestInterface.TestObject)
                && (TestInterface.SecondaryIgnoredProperty == null && other.TestInterface.SecondaryIgnoredProperty == null) || TestInterface.SecondaryIgnoredProperty.Equals(other.TestInterface.SecondaryIgnoredProperty)
                ;
        }
    }

    public interface IGenericTestInterface<T>
    {
        string Name { get; set; }

        T TestObject { get; set; }

        [IgnoreDataMember]
        string SecondaryIgnoredProperty { get; set; }
    }

    public class TestGenericConcreteInterface : IGenericTestInterface<bool>
    {
        public string Name { get; set; }

        public bool TestObject { get; set; }

        [IgnoreDataMember]
        public string SecondaryIgnoredProperty { get; set; }
    }
}
