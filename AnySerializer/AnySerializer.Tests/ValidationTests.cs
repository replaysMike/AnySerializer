﻿using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System.Collections.Generic;

namespace AnySerializer.Tests
{
    [TestFixture]
    [Category("Validation")]
    public class ValidationTests
    {
        [Test]
        public void ShouldValidate_BasicObject()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var isValid = provider.Validate(bytes);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void ShouldValidate_Compact_BasicObject()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compact);
            var isValid = provider.Validate(bytes);

            Assert.IsTrue(isValid);
        }

#if FEATURE_COMPRESSION
        [Test]
        public void ShouldValidate_Compressed_BasicObject()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compress);
            var isValid = provider.Validate(bytes);

            Assert.IsTrue(isValid);
        }
#endif

        [Test]
        public void ShouldValidate_BasicObjectWithEmptyEmbeddedTypeDescriptors()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, embedTypes: true);
            var isValid = provider.Validate(bytes);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void ShouldValidate_BasicObjectWithEmbeddedTypeDescriptors()
        {
            var test = new InterfaceWithMultipleConcreteObject()
            {
                UnknownClass = new TestInterfaceObject3()
                {
                    Id = 1
                }
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, embedTypes: true);
            var isValid = provider.Validate(bytes);

            Assert.IsTrue(isValid);
        }

        [Test]
        public void ShouldValidate_ComplexObject()
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
            var isValid = provider.Validate(bytes);

            Assert.IsTrue(isValid);
        }
    }
}
