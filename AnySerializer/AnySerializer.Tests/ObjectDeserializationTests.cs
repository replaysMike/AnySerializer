using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class ObjectDeserializationTests
    {
        [Test]
        public void ShouldDeserialize_BasicObject()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Name = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<BasicObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }

        [Test]
        public void ShouldDeserialize_ComplexObject()
        {
            var test = new ComplexObject()
            {
                Id = 1,
                Customers = new Dictionary<int, CustomerObject>
                {
                    { 1, new CustomerObject
                        {
                            Id = 1,
                            Name = "John Doe",
                            CustomerPaymentRecords = new List<CustomerPaymentRecord> { { new CustomerPaymentRecord { RecordId = 1, PaymentAmount = 10.00M } } },
                        }
                    },
                    { 2, new CustomerObject
                        {
                            Id = 2,
                            Name = "Jane Doe",
                            CustomerPaymentRecords = new List<CustomerPaymentRecord> { { new CustomerPaymentRecord { RecordId = 2, PaymentAmount = 20.00M } } },
                        }
                    },
                },
                Department = "Sales",
                IsEnabled = true,
                NumbersList = new List<int> { 1, 2, 3, 4, 5 }
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<ComplexObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }

        [Test]
        public void ShouldDeserialize_PrivateSetters()
        {
            var test = new PrivateSettersObject(1);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<PrivateSettersObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }

        [Test]
        public void ShouldDeserialize_NoEmptyConstructors()
        {
            var test = new NoConstructorObject() { Id = 1};
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<NoConstructorObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }

        [Test]
        public void ShouldDeserialize_PrivateReadonlyFields()
        {
            var test = new ReadOnlyFieldsObject(1);
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<ReadOnlyFieldsObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }

        [Test]
        public void ShouldDeserialize_ShouldIgnoreProperties()
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
        public void ShouldDeserialize_ShouldIgnoreFields()
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
        public void ShouldDeserialize_DelegatesShouldNotBeSerialized()
        {
            var test = new DelegatesEventsObject(1, new DelegatesEventsObject.ADelegate(DelegateTarget));
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var testDeserialized = provider.Deserialize<DelegatesEventsObject>(bytes);

            Assert.AreEqual(test, testDeserialized);
        }

        private void DelegateTarget(int value)
        {

        }
    }
}
