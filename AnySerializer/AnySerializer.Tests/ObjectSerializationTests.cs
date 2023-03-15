using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class ObjectSerializationTests
    {
        [Test]
        public void ShouldSerialize_BasicObject()
        {
            var test = new BasicObject() {
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
        public void ShouldDeserialize_ModifiedExtraPropertyObject2()
        {
            var test = new Project {
                ProjectId = 1,
                Color = 2,
                Description = "test",
                Location = "",
                Name = "",
                UserId = null,
                DateCreatedUtc = new DateTime(2022, 1, 1),
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.EmbedTypes);
            var restoredTest = provider.Deserialize<Project2>(bytes, SerializerOptions.EmbedTypes);

            Assert.NotNull(bytes);
            Assert.NotNull(restoredTest);
        }

        [Test]
        public void ShouldDeserialize_ModifiedExtraPropertyObject2Collection()
        {
            var provider = new SerializerProvider();

            var testCollection = new List<Project> {
                new Project { ProjectId = 1, Color = 2, Description = "test", Location = "", Name = "test", UserId = null, DateCreatedUtc = new DateTime(2022, 1, 1) },
                new Project { ProjectId = 2, Color = 3, Description = "test2", Location = "", Name = "test2", UserId = 1, DateCreatedUtc = new DateTime(2022, 1, 2) },
                new Project { ProjectId = 3, Color = 4, Description = "test3", Location = "", Name = "test3", UserId = null, DateCreatedUtc = new DateTime(2022, 1, 3) },
            };
            var bytes = provider.Serialize(testCollection, SerializerOptions.EmbedTypes);
            var restoredTest = provider.Deserialize<List<Project2>>(bytes, SerializerOptions.EmbedTypes);

            Assert.NotNull(bytes);
            Assert.NotNull(restoredTest);
            Assert.That(testCollection.Count, Is.EqualTo(restoredTest.Count));
            for (var i = 0; i < testCollection.Count; i++)
            {
                var source = testCollection.Skip(i).First();
                var target = restoredTest.Skip(i).First();
                Assert.That(source.ProjectId, Is.EqualTo(target.ProjectId));
                Assert.That(source.Color, Is.EqualTo(target.Color));
                Assert.That(source.Description, Is.EqualTo(target.Description));
                Assert.That(source.Location, Is.EqualTo(target.Location));
                Assert.That(source.Name, Is.EqualTo(target.Name));
                Assert.That(source.UserId, Is.EqualTo(target.UserId));
                Assert.That(source.DateCreatedUtc, Is.EqualTo(target.DateCreatedUtc));
            }
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
