using NUnit.Framework;
using System;
using System.Text;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class BasicSerializationTests
    {
        [Test]
        public void ShouldSerialize_String()
        {
            var test = "Test string";
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + string length + 1 byte (7 bit string header)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + test.Length + 1);
        }

        [Test]
        public void ShouldSerialize_Long()
        {
            var test = long.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + sizeof(data)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(long));
        }

        [Test]
        public void ShouldSerialize_Int()
        {
            var test = int.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + sizeof(data)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(int));
        }

        [Test]
        public void ShouldSerialize_Short()
        {
            var test = short.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + sizeof(data)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(short));
        }

        [Test]
        public void ShouldSerialize_Float()
        {
            var test = float.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + sizeof(data)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(float));
        }

        [Test]
        public void ShouldSerialize_Double()
        {
            var test = double.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + sizeof(data)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(double));
        }

        [Test]
        public void ShouldSerialize_Decimal()
        {
            var test = decimal.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + sizeof(data)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(decimal));
        }

        [Test]
        public void ShouldSerialize_Bool()
        {
            var test = true;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the object type size + size header + sizeof(data)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(bool));
        }

        [Test]
        public void ShouldSerialize_Char()
        {
            var test = '\u00B9'; // force the binaryWriter to use unicode chars
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + sizeof(long)
            Assert.AreEqual(bytes.Length, Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(char));
        }
    }
}
