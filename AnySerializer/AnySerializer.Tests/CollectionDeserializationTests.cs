using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System.Collections.Generic;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class CollectionDeserializationTests
    {
        [Test]
        public void ShouldDeserialize_ArrayOfInts()
        {
            var test = new int[4] { 1, 2, 3, 4 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<int[]>(bytes);

            CollectionAssert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ListOfInts()
        {
            var test = new List<int> { 1, 2, 3, 4 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<List<int>>(bytes);

            CollectionAssert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ListOfIntsWithNullValue()
        {
            var test = new List<int?> { 1, 2, null, 4 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<List<int?>>(bytes);

            CollectionAssert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_DictionaryOfInts()
        {
            var test = new Dictionary<int, int> {
                { 1, 100 },
                { 2, 200 },
                { 3, 300 },
                { 4, 400 },
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Dictionary<int, int>>(bytes);

            CollectionAssert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_DictionaryOfIntsWithNullValue()
        {
            var test = new Dictionary<int, int?> {
                { 1, 100 },
                { 2, null },
                { 3, 300 },
                { 4, 400 },
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Dictionary<int, int?>>(bytes);

            CollectionAssert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_CustomCollectionWithIndexer()
        {
            var test = new CustomCollectionObject(100, 200);
            test.Add("test", 1);
            test.Add("test 2", 2);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<CustomCollectionObject>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_CustomCollectionAlt()
        {
            var test = new CustomCollectionAltObject<BasicObject>(100, "Test");
            test.Add(new BasicObject { Id = 1, Description = "Description", IsEnabled = true });
            test.Add(new BasicObject { Id = 2, Description = "Description 2", IsEnabled = true });
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<CustomCollectionAltObject<BasicObject>>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }
    }
}
