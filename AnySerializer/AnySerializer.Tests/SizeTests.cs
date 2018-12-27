using AnySerializer.Extensions;
using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class SizeTests
    {
        [Test]
        public void Should_Person_CorrectSize()
        {
            var person = new Person
            {
                Id = 12345,
                Name = "Fred",
                Address = new Address
                {
                    Line1 = "Flat 1",
                    Line2 = "The Meadows"
                }
            };

            var bytes = person.Serialize();
            Assert.AreEqual(71, bytes.Length);
        }

        [Test]
        public void Should_Compact_Person_CorrectSize()
        {
            var person = new Person
            {
                Id = 12345,
                Name = "Fred",
                Address = new Address
                {
                    Line1 = "Flat 1",
                    Line2 = "The Meadows"
                }
            };

            var bytes = person.Serialize(SerializerOptions.Compact);
            Assert.AreEqual(59, bytes.Length);
        }

        [Test]
        public void Should_Compact_Bool_CorrectSize()
        {
            var test = true;
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compact);
            Assert.AreEqual(7, bytes.Length);
        }

        [Test]
        public void Should_Compact_String_CorrectSize()
        {
            var test = "Test string";
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compact);
            Assert.AreEqual(18, bytes.Length);
        }

        [Test]
        public void Should_Compact_BasicObject_CorrectSize()
        {
            var test = new BasicObject()
            {
                Id = 1,
                IsEnabled = true,
                Description = "Test",
            };
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test, SerializerOptions.Compact);
            Assert.AreEqual(31, bytes.Length);
        }

        [Test]
        public void Should_Compact_ExceedMaxSize()
        {
            // generate an approximate value greater than max size for compact
            var testString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent vulputate nisl eros, sit amet dapibus neque convallis nec. Fusce vitae odio vitae mi tincidunt elementum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus sed congue felis, interdum gravida erat. Nam id ultrices ante, at fermentum tellus. Morbi dictum nec orci non gravida. Duis sagittis, ligula a vestibulum feugiat, leo quam sagittis nunc, maximus condimentum nisi leo vitae eros. Duis pellentesque commodo tortor sed tristique. Nulla facilisi. Vestibulum tincidunt nec ligula id luctus. Quisque blandit quis lorem a semper. Nunc interdum tellus a cursus gravida. Praesent hendrerit maximus magna, nec rutrum sapien pharetra sit amet. Vivamus quis iaculis leo. Morbi semper lectus ipsum, nec pharetra magna ullamcorper in. Nunc interdum tellus a cursus gravida. Praesent hendrerit maximus magna, nec rutrum sapien pharetra sit amet. Vivamus quis iaculis leo. Morbi semper lectus ipsum, nec pharetra magna ullamcor. Morbi semper lectus ipsum, nec pharetra magna ullamcor";
            var test = new List<string>();
            var lengthToExceed = ushort.MaxValue;
            var stringLength = 1000;
            var copies = (int)((lengthToExceed / stringLength) + 1);
            var generatedLength = stringLength * copies;
            var random = new Random();
            for (var i = 0; i < copies; i++)
                test.Add($"{random.Next(1, 100000)}-{testString}".Substring(0, stringLength));
            var provider = new SerializerProvider();

            // make sure we are at least generating a data size greater than a ushort
            Assert.Greater(generatedLength, lengthToExceed);

            // should exceed max compact size
            Assert.Throws<ExceedsMaxSizeException>(() => provider.Serialize(test, SerializerOptions.Compact));

            // ensure regular mode does not have a problem with this size
            Assert.DoesNotThrow(() => provider.Serialize(test));
        }

#if FEATURE_COMPRESSION
        [Test]
        public void Should_Compress_BeSmaller_OnLargeObjects()
        {
            // generate a large list of somewhat unique strings
            var testString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent vulputate nisl eros, sit amet dapibus neque convallis nec. Fusce vitae odio vitae mi tincidunt elementum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus sed congue felis, interdum gravida erat. Nam id ultrices ante, at fermentum tellus. Morbi dictum nec orci non gravida. Duis sagittis, ligula a vestibulum feugiat, leo quam sagittis nunc, maximus condimentum nisi leo vitae eros. Duis pellentesque commodo tortor sed tristique. Nulla facilisi. Vestibulum tincidunt nec ligula id luctus. Quisque blandit quis lorem a semper. Nunc interdum tellus a cursus gravida. Praesent hendrerit maximus magna, nec rutrum sapien pharetra sit amet. Vivamus quis iaculis leo. Morbi semper lectus ipsum, nec pharetra magna ullamcorper in. Nunc interdum tellus a cursus gravida. Praesent hendrerit maximus magna, nec rutrum sapien pharetra sit amet. Vivamus quis iaculis leo. Morbi semper lectus ipsum, nec pharetra magna ullamcor. Morbi semper lectus ipsum, nec pharetra magna ullamcor";
            var test = new List<string>();
            var stringLength = 1000;
            var copies = 5000;
            var generatedLength = stringLength * copies;
            var random = new Random();
            for (var i = 0; i < copies; i++)
                test.Add($"{random.Next(1, 100000)}-{testString}".Substring(0, stringLength));
            var provider = new SerializerProvider();

            var uncompressedBytes = provider.Serialize(test);
            var compressedBytes = provider.Serialize(test, SerializerOptions.Compress);
            var compressionRatio = (double)uncompressedBytes.Length / compressedBytes.Length;

            // ensure it can be deserialized
            var restored = provider.Deserialize<List<string>>(compressedBytes);
            Assert.NotNull(restored);

            Assert.Greater(compressionRatio, 25.0); // we expect a large gain here
            Assert.Greater(uncompressedBytes.Length, compressedBytes.Length);
        }

        [Test]
        public void Should_Compress_BeSmaller_OnMediumObjects()
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
            var uncompressedBytes = provider.Serialize(test);
            var compressedBytes = provider.Serialize(test, SerializerOptions.Compress);
            var compressionRatio = (double)uncompressedBytes.Length / compressedBytes.Length;

            // ensure it can be deserialized
            var restored = provider.Deserialize<ComplexObject>(compressedBytes);
            Assert.NotNull(restored);

            Assert.GreaterOrEqual(compressionRatio, 1.0); // we expect a small gain here
            Assert.Greater(uncompressedBytes.Length, compressedBytes.Length);
        }
#endif
    }
}
