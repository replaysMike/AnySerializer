using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class CollectionSerializationTests
    {
        [Test]
        public void ShouldSerialize_ArrayOfInts()
        {
            var test = new int[4] { 0x01, 0x02, 0x03, 0x04 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + serialized object size
            var expectedSize = Constants.TypeHeaderSize + Constants.LengthHeaderSize + (
                (test.Length *
                (
                    Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(int)) /*valuesize*/
                )
            );
            Assert.AreEqual(expectedSize, bytes.Length);
        }

        [Test]
        public void ShouldSerialize_ListOfInts()
        {
            var test = new List<int> { 1, 2, 3, 4 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + serialized object size
            var expectedSize = Constants.TypeHeaderSize + Constants.LengthHeaderSize + (
                (test.Count *
                (
                    Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(int)) /*valuesize*/
                ) 
            );
            Assert.AreEqual(expectedSize, bytes.Length);
        }

        [Test]
        public void ShouldSerialize_DictionaryOfInts()
        {
            var test = new Dictionary<int, int> {
                { 1, 100 },
                { 2, 200 },
                { 3, 300 },
                { 4, 400 },
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + serialized object size
            var expectedSize = Constants.TypeHeaderSize + Constants.LengthHeaderSize + (
                (test.Count *
                (
                    (Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(int)) /*keysize*/
                    + (Constants.TypeHeaderSize + Constants.LengthHeaderSize + sizeof(int)) /*valuesize*/
                ) 
            ));
            Assert.AreEqual(expectedSize, bytes.Length);
        }
    }
}
