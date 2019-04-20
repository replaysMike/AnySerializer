using NUnit.Framework;
using System.Collections.Generic;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class CollectionSerializationTests
    {
        [Test]
        public void ShouldSerialize_ArrayOfInts_CorrectSize()
        {
            var test = new int[4] { 0x01, 0x02, 0x03, 0x04 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + serialized object size
            var dimensionDefinition = 0;
            for (var dimension = 0; dimension < test.Rank; dimension++)
                dimensionDefinition += sizeof(int);
            var expectedSize = Constants.DataSettingsSize + Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize +
                sizeof(int) + dimensionDefinition + (
                (test.Length *
                (
                    Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + sizeof(int)) /*valuesize*/
                )
            );
            Assert.AreEqual(expectedSize, bytes.Length);
        }

        [Test]
        public void ShouldSerialize_MultidimensionalArrayOfInts_CorrectSize()
        {
            var test = new int[2, 4] { { 0x01, 0x02, 0x03, 0x04 }, { 0x05, 0x06, 0x07, 0x08 } };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + serialized object size
            var dimensionDefinition = 0;
            for (var dimension = 0; dimension < test.Rank; dimension++)
                dimensionDefinition += sizeof(int);
            var expectedSize = Constants.DataSettingsSize + Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize +
                sizeof(int) + dimensionDefinition + (
                (test.Length *
                (
                    Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + sizeof(int)) /*valuesize*/
                )
            );
            Assert.AreEqual(expectedSize, bytes.Length);
        }

        [Test]
        public void ShouldSerialize_ListOfInts_CorrectSize()
        {
            var test = new List<int> { 1, 2, 3, 4 };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);

            Assert.NotNull(bytes);
            // should be equal to the size header + serialized object size
            var expectedSize = Constants.DataSettingsSize + Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + (
                (test.Count *
                (
                    Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + sizeof(int)) /*valuesize*/
                ) 
            );
            Assert.AreEqual(expectedSize, bytes.Length);
        }

        [Test]
        public void ShouldSerialize_DictionaryOfInts_CorrectSize()
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
            var expectedSize = Constants.DataSettingsSize + Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + (
                (test.Count *
                (
                    (Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + sizeof(int)) /*keysize*/
                    + (Constants.TypeHeaderSize + Constants.LengthHeaderSize + Constants.ObjectReferenceIdSize + sizeof(int)) /*valuesize*/
                ) 
            ));
            Assert.AreEqual(expectedSize, bytes.Length);
        }
    }
}
