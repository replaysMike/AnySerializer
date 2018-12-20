using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class XmlSerializationTests
    {
        [Test]
        public void ShouldSerialize_Xml()
        {
            var test = XDocument.Load(@".\TestData\basic.xml");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.EmbedTypes);
            var deserializedTest = provider.Deserialize<XDocument>(bytes);

            Assert.AreEqual(test, deserializedTest);
        }
    }
}
