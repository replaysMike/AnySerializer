using NUnit.Framework;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class SpecializedCollectionDeserializationTests
    {
        [Test]
        public void ShouldDeserialize_Queue()
        {
            var capacity = 2;
            var test = new Queue<string>(capacity);
            test.Enqueue("Test");
            test.Enqueue("Test 2");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Queue<string>>(bytes);

            Assert.AreEqual(2, deserializedTest.Count);
            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_Stack()
        {
            var capacity = 2;
            var test = new Stack<string>(capacity);
            test.Push("Test");
            test.Push("Test 2");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<Stack<string>>(bytes);

            Assert.AreEqual(2, deserializedTest.Count);
            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ConcurrentDictionary()
        {
            var concurrencyLevel = 1; // 1 thread for testing
            var dictionarySize = 2;
            var test = new ConcurrentDictionary<int, string>(concurrencyLevel, dictionarySize);
            test[0] = "Test";
            test[1] = "Test 2";
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<ConcurrentDictionary<int, string>>(bytes);

            Assert.AreEqual(2, deserializedTest.Count);
            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ConcurrentQueue()
        {
            var test = new ConcurrentQueue<string>();
            test.Enqueue("Test");
            test.Enqueue("Test 2");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<ConcurrentQueue<string>>(bytes);

            Assert.AreEqual(2, deserializedTest.Count);
            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ConcurrentStack()
        {
            var test = new ConcurrentStack<string>();
            test.Push("Test");
            test.Push("Test 2");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<ConcurrentStack<string>>(bytes);

            Assert.AreEqual(2, deserializedTest.Count);
            Assert.AreEqual(test, deserializedTest);
        }

        [Test]
        public void ShouldDeserialize_ConcurrentBag()
        {
            var test = new ConcurrentBag<string>();
            test.Add("Test");
            test.Add("Test 2");
            var provider = new SerializerProvider();
            var bytes = provider.Serialize(test);
            var deserializedTest = provider.Deserialize<ConcurrentBag<string>>(bytes);

            Assert.AreEqual(2, deserializedTest.Count);
            Assert.AreEqual(test, deserializedTest);
        }
    }
}
