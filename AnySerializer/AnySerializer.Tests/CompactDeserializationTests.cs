using AnySerializer.Tests.TestObjects;
using NUnit.Framework;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class CompactDeserializationTests
    {
        [Test]
        public void ShouldDeserialize_Compact_Bool()
        {
            var test = true;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compact);
            var deserializedTest = provider.Deserialize<bool>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Compact_String()
        {
            var test = "Test string";
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compact);
            var deserializedTest = provider.Deserialize<string>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Compact_BasicObject()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compact);
            var testDeserialized = provider.Deserialize<BasicObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }
    }
}
