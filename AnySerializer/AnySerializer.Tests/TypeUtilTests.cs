using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TypeSupport;
using static AnySerializer.TypeManagement;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class TypeUtilTests
    {
        [Test]
        public void ShouldTypes_MapCorrectly()
        {
            Assert.AreEqual(new ExtendedType(typeof(Array)), TypeUtil.GetType(TypeId.Array));
            Assert.AreEqual(new ExtendedType(typeof(Enum)), TypeUtil.GetType(TypeId.Enum));
            Assert.AreEqual(new ExtendedType(typeof(bool)), TypeUtil.GetType(TypeId.Bool));
            Assert.AreEqual(new ExtendedType(typeof(byte)), TypeUtil.GetType(TypeId.Byte));
            Assert.AreEqual(new ExtendedType(typeof(char)), TypeUtil.GetType(TypeId.Char));
            Assert.AreEqual(new ExtendedType(typeof(decimal)), TypeUtil.GetType(TypeId.Decimal));
            Assert.AreEqual(new ExtendedType(typeof(double)), TypeUtil.GetType(TypeId.Double));
            Assert.AreEqual(new ExtendedType(typeof(float)), TypeUtil.GetType(TypeId.Float));
            Assert.AreEqual(new ExtendedType(typeof(IDictionary)), TypeUtil.GetType(TypeId.IDictionary));
            Assert.AreEqual(new ExtendedType(typeof(KeyValuePair<,>)), TypeUtil.GetType(TypeId.KeyValuePair));
            Assert.AreEqual(new ExtendedType(typeof(IEnumerable)), TypeUtil.GetType(TypeId.IEnumerable));
            Assert.AreEqual(new ExtendedType(typeof(int)), TypeUtil.GetType(TypeId.Int));
            Assert.AreEqual(new ExtendedType(typeof(long)), TypeUtil.GetType(TypeId.Long));
            Assert.AreEqual(new ExtendedType(typeof(object)), TypeUtil.GetType(TypeId.Object));
            Assert.AreEqual(new ExtendedType(typeof(short)), TypeUtil.GetType(TypeId.Short));
            Assert.AreEqual(new ExtendedType(typeof(string)), TypeUtil.GetType(TypeId.String));
        }

        [Test]
        public void ExtendedTypeAndType_Should_BeEqual()
        {
            Assert.AreEqual(new ExtendedType(typeof(bool)), typeof(bool));
        }

        [Test]
        public void ExtendedTypeAndExtendedType_Should_BeEqual()
        {
            Assert.AreEqual(new ExtendedType(typeof(bool)), new ExtendedType(typeof(bool)));
        }

        [Test]
        public void ExtendedTypeAndExtendedType_ShouldNot_BeEqual()
        {
            Assert.AreNotEqual(new ExtendedType(typeof(bool)), new ExtendedType(typeof(int)));
        }

        [Test]
        public void Should_CreateNewArray()
        {
            var test = new ObjectFactory().CreateEmptyObject<byte[]>();
            Assert.AreEqual(test.GetType(), typeof(byte[]));
        }

        [Test]
        public void Should_CreateNewEnumerable()
        {
            var test = new ObjectFactory().CreateEmptyObject<IEnumerable>();
            Assert.AreEqual(test.GetType(), typeof(object[]));
        }

        [Test]
        public void Should_CreateNewList()
        {
            var test = new ObjectFactory().CreateEmptyObject<List<int>>();
            Assert.AreEqual(test.GetType(), typeof(List<int>));
        }

        [Test]
        public void Should_CreateNewDictionary()
        {
            var test = new ObjectFactory().CreateEmptyObject<Dictionary<int, string>>();
            Assert.AreEqual(test.GetType(), typeof(Dictionary<int, string>));
        }

        [Test]
        public void Should_CreateNewString()
        {
            var test = new ObjectFactory().CreateEmptyObject<string>();
            Assert.IsNull(test);
        }

        [Test]
        public void Should_CreateNewStringUsingInitializer()
        {
            var test = new ObjectFactory().CreateEmptyObject<string>(() => string.Empty);
            Assert.AreEqual(test.GetType(), typeof(string));
            Assert.AreEqual(test, string.Empty);
        }

        [Test]
        public void Should_CreateNewBasicObject()
        {
            var test = new ObjectFactory().CreateEmptyObject<BasicObject>();
            Assert.AreEqual(test.GetType(), typeof(BasicObject));
        }

        [Test]
        public void Should_TypeId_ContainNullValue()
        {
            var typeId = TypeId.Double | TypeId.NullValue;
            Assert.AreEqual(true, TypeUtil.IsNullValue(typeId));
        }

        [Test]
        public void Should_TypeId_NotContainNullValue()
        {
            var typeId = TypeId.Double;
            Assert.AreEqual(false, TypeUtil.IsNullValue(typeId));
        }

        [Test]
        public void Should_TypeId_ShouldRemoveNullValue()
        {
            var typeId = TypeUtil.GetTypeId(TypeId.Double | TypeId.NullValue);
            Assert.AreEqual(false, TypeUtil.IsNullValue(typeId));
            Assert.AreEqual(TypeId.Double, typeId);
        }

    }
}
