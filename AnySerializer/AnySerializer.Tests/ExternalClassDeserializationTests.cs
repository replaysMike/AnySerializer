using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using AnySerializer.Extensions;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class ExternalClassDeserializationTests
    {
        [Test]
        public void Should_Deserialize()
        {
            var test = new NUnit.Framework.TestFixtureData(new { Test1 = 1 });
            var bytes = test.Serialize(true);
            var testRestored = bytes.Deserialize<TestFixtureData>();

            Assert.AreEqual(test, testRestored);
        }
    }
}
