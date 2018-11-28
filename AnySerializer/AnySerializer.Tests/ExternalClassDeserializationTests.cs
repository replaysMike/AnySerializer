using AnySerializer.Extensions;
using NUnit.Framework;

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

            Assert.AreEqual(test.Arguments.Length, testRestored.Arguments.Length);
            Assert.AreEqual(test.OriginalArguments.Length, testRestored.OriginalArguments.Length);
            Assert.AreEqual(test.RunState, testRestored.RunState);
            Assert.AreEqual(test.TestName, testRestored.TestName);
            Assert.AreEqual(test.TypeArgs, testRestored.TypeArgs);
            Assert.AreEqual(test.Arguments[0], testRestored.Arguments[0]);
            Assert.AreEqual(test.OriginalArguments[0], testRestored.OriginalArguments[0]);
        }
    }
}
