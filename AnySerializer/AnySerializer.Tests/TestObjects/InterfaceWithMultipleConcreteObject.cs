namespace AnySerializer.Tests.TestObjects
{
    public class InterfaceWithMultipleConcreteObject
    {
        public IMultipleConcrete UnknownClass { get; set; }


    }

    public interface IMultipleConcrete
    {
        int Id { get; }
    }

    public class TestInterfaceObject1 : IMultipleConcrete
    {
        public int Id { get; set; } 
    }

    public class TestInterfaceObject2 : IMultipleConcrete
    {
        public int Id { get; set; }
    }

    public class TestInterfaceObject3 : IMultipleConcrete
    {
        public int Id { get; set; }
    }

    public class TestInterfaceObject4 : IMultipleConcrete
    {
        public int Id { get; set; }
    }
}
