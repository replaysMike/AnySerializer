using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class ObjectSerializationTests
    {
        [Test]
        public void ShouldSerialize_BasicObject()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + serialized object size
            var size = ManagedObjectSize(test);
            Assert.Greater(bytes.Length, Constants.LengthHeaderSize);
        }

        [Test]
        public void ShouldDeserialize_BasicObject()
        {
            var test = new BasicObject();
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var restoredTest = provider.Deserialize<BasicObject>(bytes);

            Assert.NotNull(bytes);
            Assert.NotNull(restoredTest);
        }

        private int ManagedObjectSize(object obj)
        {
            unsafe
            {
                RuntimeTypeHandle th = obj.GetType().TypeHandle;
                int size = *(*(int**)&th + 1);
                return size;
            }
        }
    }
}
