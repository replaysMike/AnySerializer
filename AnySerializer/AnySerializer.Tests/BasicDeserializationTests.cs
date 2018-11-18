using NUnit.Framework;
using System;
using System.Text;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class BasicDeserializationTests
    {
        [Test]
        public void ShouldDeserialize_String()
        {
            var test = "Test string";
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<string>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Long()
        {
            var test = long.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<long>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Int()
        {
            var test = int.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<int>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Short()
        {
            var test = short.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<short>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Float()
        {
            var test = float.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<float>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Double()
        {
            var test = double.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<double>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Decimal()
        {
            var test = decimal.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<decimal>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Bool()
        {
            var test = true;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<bool>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Char()
        {
            var test = '\u00B9'; // force the binaryWriter to use unicode chars
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<char>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }
    }
}
