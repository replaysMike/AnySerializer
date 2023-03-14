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

        [Test]
        public void ShouldDeserialize_ModifiedExtraPropertyObject()
        {
            var test = new ExtraPropertyBase {
                Property1 = 1,
                Property2 = 2,
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var restoredTest = provider.Deserialize<ExtraPropertyOne>(bytes);

            Assert.NotNull(bytes);
            Assert.NotNull(restoredTest);
            Assert.That(restoredTest.Property1, Is.EqualTo(test.Property1));
            Assert.That(restoredTest.Property2, Is.EqualTo(test.Property2));
        }

        [Test]
        public void ShouldDeserialize_ChangedPropertyOrderObject()
        {
            var test = new ChangedPropertyOrderBase {
                Property1 = 1,
                Property2 = 2,
                Property3 = "Test"
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var restoredTest = provider.Deserialize<ChangedPropertyOrderOne>(bytes);

            Assert.NotNull(bytes);
            Assert.NotNull(restoredTest);
            Assert.That(restoredTest.Property1, Is.EqualTo(test.Property1));
            Assert.That(restoredTest.Property2, Is.EqualTo(test.Property2));
            Assert.That(restoredTest.Property3, Is.EqualTo(test.Property3));
        }

        // todo: currently can't support missing properties without a more significant refactoring. Saved for a later date.
        /*[Test]
        public void ShouldDeserialize_MissingPropertyObject()
        {
            var test = new MissingPropertyBase {
                Property1 = 1,
                Property2 = 2,
                Property3 = "Test"
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var restoredTest = provider.Deserialize<MissingPropertyOne>(bytes);

            Assert.NotNull(bytes);
            Assert.NotNull(restoredTest);
            Assert.That(restoredTest.Property1, Is.EqualTo(test.Property1));
            Assert.That(restoredTest.Property3, Is.EqualTo(test.Property3));
        }*/

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
