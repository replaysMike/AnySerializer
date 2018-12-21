using NUnit.Framework;
using System.Xml.Linq;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class XmlSerializationTests
    {
        [Test]
        public void ShouldSerialize_Basic_Xml()
        {
            var test = XDocument.Parse(TestHelper.GetResourceFileText(@"TestData.basic.xml"));
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.EmbedTypes);
            var deserializedTest = provider.Deserialize<XDocument>(bytes);

            Assert.AreEqual(test.ToString(), deserializedTest.ToString());
        }

        [Test]
        public void ShouldSerialize_Complex_Xml()
        {
            var test = XDocument.Parse(TestHelper.GetResourceFileText(@"TestData.complex.xml"));
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.EmbedTypes);
            var deserializedTest = provider.Deserialize<XDocument>(bytes);

            Assert.AreEqual(test.ToString(), deserializedTest.ToString());
        }
    }
}
