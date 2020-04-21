using System.IO;
using NUnit.Framework;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class TypeDescriptorTests
    {
        [Test]
        public void Should_Serialize_TypeDescriptors()
        {
            var bytes = Serialize();
            Assert.Greater(bytes.Length, 0);
#if FEATURE_COMPRESSION
            Assert.Less(bytes.Length, 250);
#else
            Assert.Less(bytes.Length, 1000);
#endif
        }

        [Test]
        public void Should_Deserialize_TypeDescriptors()
        {
            var bytes = Serialize();
            var typeDescriptors = new TypeDescriptors();
            typeDescriptors.Deserialize(bytes);
            Assert.AreEqual(8, typeDescriptors.Types.Count);
            CollectionAssert.AreEqual(CreateTypeDescriptors().Types, typeDescriptors.Types);
        }

        [Test]
        public void Should_Serialize_EnsureCompression()
        {
            var bytes = Serialize();
            byte[] uncompressedBytes = null;
            var typeDescriptors = CreateTypeDescriptors();
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    foreach (var typeDescriptor in typeDescriptors.Types)
                        writer.Write($"{typeDescriptor.TypeId}|{typeDescriptor.FullName}\r\n");
                }
                uncompressedBytes = stream.ToArray();
            }

            // ensure the data is smaller
            Assert.Less(bytes.Length, uncompressedBytes.Length);
        }

        private byte[] Serialize()
        {
            var typeDescriptors = CreateTypeDescriptors();
            return typeDescriptors.Serialize();
        }

        private TypeDescriptors CreateTypeDescriptors()
        {
            var typeDescriptors = new TypeDescriptors();
            typeDescriptors.AddKnownType(typeof(string));
            typeDescriptors.AddKnownType(typeof(object));
            typeDescriptors.AddKnownType(typeof(short));
            typeDescriptors.AddKnownType(typeof(int));
            typeDescriptors.AddKnownType(typeof(long));
            typeDescriptors.AddKnownType(typeof(double));
            typeDescriptors.AddKnownType(typeof(decimal));
            typeDescriptors.AddKnownType(typeof(float));
            return typeDescriptors;
        }
    }
}
