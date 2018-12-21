using AnySerializer.Tests.TestObjects;
using NUnit.Framework;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class StructTests
    {
        [Test]
        public void Should_Deserialize_Struct()
        {
            var test = new StructObject(1, 50);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<StructObject>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void Should_Deserialize_RecursiveStruct()
        {
            var test = new RecursiveStructObject(1);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<RecursiveStructObject>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }
    }
}
