using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TypeSupport;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class CollectionDeserializationTests
    {
        [Test]
        public void ShouldDeserialize_ArrayOfInts()
        {
            var test = new int[] { 1, 2, 3, 4 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<int[]>(bytes);

            CollectionAssert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_MultidimensionalArrayOfInts()
        {
            var array2Da = new int[4, 2] { 
                { 1, 2 },
                { 3, 4 },
                { 5, 6 },
                { 7, 8 }
            };
            var objectFactory = new ObjectFactory();
            var dimensions = new List<object> { 4, 2 };
            var newArray2Da = objectFactory.CreateEmptyObject<int[,]>(dimensions);
            var t = newArray2Da.Rank;
            var array3Da = new int[2, 2, 3] { 
                { { 1, 2, 3 }, { 4, 5, 6 } }, { { 7, 8, 9 }, { 10, 11, 12 } }
            };
            var tr = array2Da.Rank; // dimensions (number of ,) = 2
            var tr1 = array2Da.GetLength(0); // 4
            var tr2 = array2Da.GetLength(1); // 2
            var r = array3Da.Rank; // dimensions (number of ,) = 3
            var r1 = array3Da.GetLength(0); // 2
            var r2 = array3Da.GetLength(1); // 2
            var r3 = array3Da.GetLength(2); // 3

            var provider = new SerializerProvider();
            var bytes = provider.Serialize(array2Da, SerializerOptions.EmbedTypes | SerializerOptions.WriteDiagnosticLog);
            var deserializedTest = provider.Deserialize<int[,]>(bytes);

            CollectionAssert.AreEqual(array2Da, deserializedTest);
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

        [Test]
        public void ShouldDeserialize_KeyValuePair()
        {
            var test = new KeyValuePair<int, string>(100, "Test");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<KeyValuePair<int, string>>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }
    }
}
