using AnySerializer.Tests.TestObjects;
using NUnit.Framework;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class CompressDeserializationTests
    {
        [Test]
        public void ShouldDeserialize_Compress_Bool()
        {
            var test = true;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compress);
            var deserializedTest = provider.Deserialize<bool>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Compress_String()
        {
            var test = "Test string";
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compress);
            var deserializedTest = provider.Deserialize<string>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Compress_BasicObject()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compress);
            var testDeserialized = provider.Deserialize<BasicObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }
    }
}
