using NUnit.Framework;
using System;
using System.Drawing;

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
        public void ShouldDeserialize_Byte()
        {
            var test = byte.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<byte>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_SByte()
        {
            var test = sbyte.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<sbyte>(bytes);

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
        public void ShouldDeserialize_UShort()
        {
            var test = ushort.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<ushort>(bytes);

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
        public void ShouldDeserialize_UInt()
        {
            var test = uint.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<uint>(bytes);

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
        public void ShouldDeserialize_ULong()
        {
            var test = ulong.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<ulong>(bytes);

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

        [Test]
        public void ShouldDeserialize_NullableInt()
        {
            int? test = int.MaxValue;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<int?>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Enum()
        {
            var test = DayOfWeek.Wednesday;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<DayOfWeek>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Guid()
        {
            var test = Guid.NewGuid();
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Guid>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_DateTime()
        {
            var test = new DateTime();
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<DateTime>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_TimeSpan()
        {
            var test = TimeSpan.FromMinutes(10);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<TimeSpan>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Point()
        {
            var test = new Point(20, 50);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Point>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Tuple1()
        {
            var test = new Tuple<int>(10);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Tuple<int>>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Tuple2()
        {
            var test = new Tuple<int, string>(10, "test");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Tuple<int, string>>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Tuple3()
        {
            var test = new Tuple<int, string, bool>(10, "test", true);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Tuple<int, string, bool>>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Tuple4()
        {
            var test = new Tuple<int, string, bool, double>(10, "test", true, 3.14);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Tuple<int, string, bool, double>>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

#if FEATURE_CUSTOM_VALUETUPLE
        [Test]
        public void ShouldDeserialize_ValueTuple()
        {
            var test = (10, "test");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<(int, string)>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ValueTuple2()
        {
            var test = (10, "test", true);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<(int, string, bool)>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ValueTuple3()
        {
            var test = (10, "test", true, 3.14);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<(int, string, bool, double)>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }
#endif
    }
}
