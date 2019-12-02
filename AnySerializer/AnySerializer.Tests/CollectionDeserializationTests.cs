using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        [Ignore("This is for understanding how multi-dimensional arrays can be reconstructed")]
        public void Test_MultidimensionalArrayOfInts()
        {
            /*var array = new int[4]
            {
                1,2,3,4
            };*/
            var array = new int[4, 2] {
                { 1, 2 },
                { 3, 4 },
                { 5, 6 },
                { 7, 8 }
            };
            /*var array = new int[2, 3, 3] {
                // row 1
                { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } },
                // row 2
                { { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 } }
            };*/

            /*var array = new int[2, 2, 2, 2] {
                // row 1
                { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } },
                // row 2
                { { { 9, 10 }, { 11, 12 } }, { { 13, 14 }, { 15, 16 } } },
            };*/
            /*var array = new int[2, 2, 2, 2, 2] {
                // row 1
                { { { { 1, 2 }, { 3, 4} }, { { 5, 6 }, { 7, 8 } } }, { { { 9, 10 }, { 11, 12 } }, { { 13, 14 }, { 15, 16 } } } },
                // row 2
                { { { { 17, 18 }, { 19, 20} }, { { 21, 22 }, { 23, 24 } } }, { { { 25, 26 }, { 27, 28 } }, { { 29, 30 }, { 31, 32 } } } },
            };*/
            var arrayRank = array.Rank;
            var arrayDimensions = new List<int>();
            for (var dimension = 0; dimension < arrayRank; dimension++)
                arrayDimensions.Add(array.GetLength(dimension));
            var i = 0;
            foreach (var val in array)
            {
                var indicies = new List<int>();
                var indicies2 = new List<int>();
                indicies.Add(i % arrayDimensions[arrayRank - 1]);
                // populate indicies2 based on a formula derived from the logic below which populates indicies.
                // the results should match
                for (var r = 1; r <= arrayRank; r++)
                {
                    var multi = 1;
                    for (var p = 1; p < r; p++)
                    {
                        multi *= arrayDimensions[arrayRank - p];
                    }
                    var b = (i / multi) % arrayDimensions[arrayRank - r];
                    indicies2.Add(b);
                }

                if (arrayRank >= 2)
                {
                    indicies.Add((i / arrayDimensions[arrayRank - 1]) % arrayDimensions[arrayRank - 2]);
                }
                if (arrayRank >= 3)
                {
                    indicies.Add((i / (arrayDimensions[arrayRank - 1] * arrayDimensions[arrayRank - 2])) % arrayDimensions[arrayRank - 3]);
                }
                if (arrayRank >= 4)
                {
                    indicies.Add((i / (arrayDimensions[arrayRank - 1] * arrayDimensions[arrayRank - 2] * arrayDimensions[arrayRank - 3])) % arrayDimensions[arrayRank - 4]);
                }
                if (arrayRank >= 5)
                {
                    indicies.Add((i / (arrayDimensions[arrayRank - 1] * arrayDimensions[arrayRank - 2] * arrayDimensions[arrayRank - 3] * arrayDimensions[arrayRank - 4])) % arrayDimensions[arrayRank - 5]);
                }
                if (arrayRank >= 6)
                {
                    indicies.Add((i / (arrayDimensions[arrayRank - 1] * arrayDimensions[arrayRank - 2] * arrayDimensions[arrayRank - 3] * arrayDimensions[arrayRank - 4] * arrayDimensions[arrayRank - 5])) % arrayDimensions[arrayRank - 6]);
                }
                indicies.Reverse();
                indicies2.Reverse();
                var v = array.GetValue(indicies.ToArray());
                var v2 = array.GetValue(indicies2.ToArray());
                Debug.WriteLine($"({string.Join(",", indicies)}) = {v} ({i})");
                Debug.WriteLine($"({string.Join(",", indicies2)}) = {v2} ({i})");
                i++;
            }
        }

        [Test]
        public void ShouldDeserialize_2dMultidimensionalArrayOfInts()
        {
            var array = new int[4, 2] {
                { 1, 2 },
                { 3, 4 },
                { 5, 6 },
                { 7, 8 }
            };

            var provider = new SerializerProvider();
            var bytes = provider.Serialize(array, SerializerOptions.EmbedTypes);
            var deserializedTest = provider.Deserialize<int[,]>(bytes);

            CollectionAssert.AreEqual(array, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_3dMultidimensionalArrayOfInts()
        {
            var array = new int[2, 3, 3] {
                // row 1
                { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } },
                // row 2
                { { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 } }
            };

            var provider = new SerializerProvider();
            var bytes = provider.Serialize(array, SerializerOptions.EmbedTypes);
            var deserializedTest = provider.Deserialize<int[,,]>(bytes);

            CollectionAssert.AreEqual(array, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_4dMultidimensionalArrayOfInts()
        {
            var array = new int[2, 2, 2, 2] {
                // row 1
                { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } },
                // row 2
                { { { 9, 10 }, { 11, 12 } }, { { 13, 14 }, { 15, 16 } } },
            };

            var provider = new SerializerProvider();
            var bytes = provider.Serialize(array, SerializerOptions.EmbedTypes);
            var deserializedTest = provider.Deserialize<int[,,,]>(bytes);

            CollectionAssert.AreEqual(array, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_JaggedArrayOfInts()
        {
            var array = new int[][] {
                new int[] { 1, 2 },
                new int[] { 3, 4 },
                new int[] { 5, 6 },
                new int[] { 7, 8 }
            };

            var provider = new SerializerProvider();
            var bytes = provider.Serialize(array, SerializerOptions.EmbedTypes);
            var deserializedTest = provider.Deserialize<int[][]>(bytes);

            CollectionAssert.AreEqual(array, deserializedTest);
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
        public void ShouldDeserialize_CustomWrappedCollection()
        {
            // this is an oddball case but we will support it!
            var test = new CustomWrappedCollection<string>();
            test.Add(new GenericObject<string>("test 1"));
            test.Add(new GenericObject<string>("test 2"));
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<CustomWrappedCollection<string>>(bytes);

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

        [Test]
        public void ShouldDeserialize_EmptyHashtable()
        {
            var hashTable = new Hashtable();
            var bytes = Serializer.Serialize(hashTable);
            Assert.AreEqual(8, bytes.Length);
        }

        [Test]
        public void ShouldDeserialize_Hashtable()
        {
            var test = new Hashtable { { 5, "Test" }, { 7, "Another test" } };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Hashtable>(bytes);
            Assert.AreEqual(test, deserializedTest);
        }
    }
}
