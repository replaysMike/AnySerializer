using AnySerializer.Extensions;
using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using TypeSupport.Extensions;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class AnonymousTests
    {
        [Test]
        public void Should_Deserialize_RootAnonymousType()
        {
            var originalObject = new { Id = 1, Name = "John Doe" };

            // serialize to binary data
            var bytes = originalObject.Serialize(SerializerOptions.EmbedTypes);

            // restore the object from binary data
            var restoredObject = bytes.Deserialize(originalObject.GetType());

            Assert.AreEqual(originalObject.Id, restoredObject.GetProperty("Id").GetValue(restoredObject, null));
            Assert.AreEqual(originalObject.Name, restoredObject.GetProperty("Name").GetValue(restoredObject, null));
        }

        [Test]
        public void Should_Deserialize_AnonymousType()
        {
            var originalObject = new AnonymousTypeObject(new { Id = 1, Name = "John Doe" });

            // serialize to binary data
            var bytes = originalObject.Serialize(SerializerOptions.EmbedTypes);

            // restore the object from binary data
            var restoredObject = bytes.Deserialize<AnonymousTypeObject>();

            Assert.AreEqual(originalObject.AnonymousType, restoredObject.AnonymousType);
        }
    }
}
