using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using static AnySerializer.TypeManagement;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class TypeUtilTests
    {
        [Test]
        public void ShouldTypes_MapCorrectly()
        {
            Assert.AreEqual(new TypeSupport(typeof(Array)), TypeUtil.GetType(TypeId.Array));
            Assert.AreEqual(new TypeSupport(typeof(bool)), TypeUtil.GetType(TypeId.Bool));
            Assert.AreEqual(new TypeSupport(typeof(byte)), TypeUtil.GetType(TypeId.Byte));
            Assert.AreEqual(new TypeSupport(typeof(char)), TypeUtil.GetType(TypeId.Char));
            Assert.AreEqual(new TypeSupport(typeof(decimal)), TypeUtil.GetType(TypeId.Decimal));
            Assert.AreEqual(new TypeSupport(typeof(double)), TypeUtil.GetType(TypeId.Double));
            Assert.AreEqual(new TypeSupport(typeof(float)), TypeUtil.GetType(TypeId.Float));
            Assert.AreEqual(new TypeSupport(typeof(IDictionary<,>)), TypeUtil.GetType(TypeId.IDictionary));
            Assert.AreEqual(new TypeSupport(typeof(IEnumerable)), TypeUtil.GetType(TypeId.IEnumerable));
            Assert.AreEqual(new TypeSupport(typeof(int)), TypeUtil.GetType(TypeId.Int));
            Assert.AreEqual(new TypeSupport(typeof(long)), TypeUtil.GetType(TypeId.Long));
            Assert.AreEqual(new TypeSupport(typeof(object)), TypeUtil.GetType(TypeId.Object));
            Assert.AreEqual(new TypeSupport(typeof(short)), TypeUtil.GetType(TypeId.Short));
            Assert.AreEqual(new TypeSupport(typeof(string)), TypeUtil.GetType(TypeId.String));
        }

        [Test]
        public void TypeSupportAndType_Should_BeEqual()
        {
            Assert.AreEqual(new TypeSupport(typeof(bool)), typeof(bool));
        }

        [Test]
        public void TypeSupportAndTypeSupport_Should_BeEqual()
        {
            Assert.AreEqual(new TypeSupport(typeof(bool)), new TypeSupport(typeof(bool)));
        }

        [Test]
        public void TypeSupportAndTypeSupport_ShouldNot_BeEqual()
        {
            Assert.AreNotEqual(new TypeSupport(typeof(bool)), new TypeSupport(typeof(int)));
        }

        [Test]
        public void Should_CreateNewArray()
        {
            var test = TypeUtil.CreateEmptyObject<byte[]>();
            Assert.AreEqual(test.GetType(), typeof(byte[]));
        }

        [Test]
        public void Should_CreateNewEnumerable()
        {
            var test = TypeUtil.CreateEmptyObject<IEnumerable>();
            Assert.AreEqual(test.GetType(), typeof(object[]));
        }

        [Test]
        public void Should_CreateNewList()
        {
            var test = TypeUtil.CreateEmptyObject<List<int>>();
            Assert.AreEqual(test.GetType(), typeof(List<int>));
        }

        [Test]
        public void Should_CreateNewDictionary()
        {
            var test = TypeUtil.CreateEmptyObject<Dictionary<int, string>>();
            Assert.AreEqual(test.GetType(), typeof(Dictionary<int, string>));
        }

        [Test]
        public void Should_CreateNewString()
        {
            var test = TypeUtil.CreateEmptyObject<string>();
            Assert.IsNull(test);
        }

        [Test]
        public void Should_CreateNewStringUsingInitializer()
        {
            var test = TypeUtil.CreateEmptyObject<string>(() => string.Empty);
            Assert.AreEqual(test.GetType(), typeof(string));
            Assert.AreEqual(test, string.Empty);
        }

        [Test]
        public void Should_CreateNewBasicObject()
        {
            var test = TypeUtil.CreateEmptyObject<BasicObject>();
            Assert.AreEqual(test.GetType(), typeof(BasicObject));
        }
    }
}
