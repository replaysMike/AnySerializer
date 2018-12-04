using AnySerializer.Tests.TestObjects;
using NUnit.Framework;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class IgnoreTests
    {
        [Test]
        public void ShouldNot_IgnoreProperty_ByAttribute()
        {
            var test = new IgnorePropertiesObject() { Id = 1, Name = "John Doe" };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.DisableIgnoreAttributes);
            var testDeserialized = provider.Deserialize<IgnorePropertiesObject>(bytes);

            Assert.AreNotEqual(test, testDeserialized);
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.IsNotNull(test.Name);
            Assert.IsNull(testDeserialized.Name);
        }

        [Test]
        public void Should_IgnoreProperty_ByAttribute()
        {
            var test = new IgnorePropertiesObject() { Id = 1, Name = "John Doe" };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<IgnorePropertiesObject>(bytes);

            Assert.AreNotEqual(test, testDeserialized);
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.IsNotNull(test.Name);
            Assert.IsNull(testDeserialized.Name);
        }

        [Test]
        public void Should_IgnoreField_ByAttribute()
        {
            var test = new IgnoreFieldsObject(100) { Id = 1, Name = "John Doe" };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<IgnoreFieldsObject>(bytes);

            Assert.AreNotEqual(test, testDeserialized);
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.AreEqual(test.Name, testDeserialized.Name);
            Assert.AreNotEqual(test.GetInternalField(), testDeserialized.GetInternalField());
        }

        [Test]
        public void Should_Ignore_ByJsonAttribute()
        {
            var test = new JsonIgnorePropertiesObject() { Id = 1, Name = "John Doe" };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<JsonIgnorePropertiesObject>(bytes);
            Assert.AreNotEqual(test, testDeserialized);
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.IsNull(testDeserialized.Name);
        }

        [Test]
        public void Should_Ignore_ByPropertyName()
        {
            var test = new BasicObject { Id = 1, Description = "Description", IsEnabled = true };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, "Description");
            var testDeserialized = provider.Deserialize<BasicObject>(bytes, "Description");
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.AreEqual(test.IsEnabled, testDeserialized.IsEnabled);
            Assert.IsNull(testDeserialized.Description);
        }

        [Test]
        public void Should_Ignore_ByPropertyPath()
        {
            var test = new BasicObject { Id = 1, Description = "Description", IsEnabled = true };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, ".Description");
            var testDeserialized = provider.Deserialize<BasicObject>(bytes, "Description");
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.AreEqual(test.IsEnabled, testDeserialized.IsEnabled);
            Assert.IsNull(testDeserialized.Description);
        }

        [Test]
        public void Should_Ignore_ByFieldName()
        {
            var test = new BasicObject { Id = 1, Description = "Description", IsEnabled = true };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, "<Description>k__BackingField");
            var testDeserialized = provider.Deserialize<BasicObject>(bytes, "<Description>k__BackingField");
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.AreEqual(test.IsEnabled, testDeserialized.IsEnabled);
            Assert.IsNull(testDeserialized.Description);
        }

        [Test]
        public void Should_Ignore_ByFieldPath()
        {
            var test = new BasicObject { Id = 1, Description = "Description", IsEnabled = true };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, ".<Description>k__BackingField");
            var testDeserialized = provider.Deserialize<BasicObject>(bytes, ".<Description>k__BackingField");
            Assert.AreEqual(test.Id, testDeserialized.Id);
            Assert.AreEqual(test.IsEnabled, testDeserialized.IsEnabled);
            Assert.IsNull(testDeserialized.Description);
        }
    }
}
