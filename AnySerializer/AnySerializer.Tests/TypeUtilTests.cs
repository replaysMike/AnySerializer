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
            Assert.AreEqual(new TypeLoader(typeof(Array)), TypeUtil.GetType(TypeId.Array));
            Assert.AreEqual(new TypeLoader(typeof(bool)), TypeUtil.GetType(TypeId.Bool));
            Assert.AreEqual(new TypeLoader(typeof(byte)), TypeUtil.GetType(TypeId.Byte));
            Assert.AreEqual(new TypeLoader(typeof(char)), TypeUtil.GetType(TypeId.Char));
            Assert.AreEqual(new TypeLoader(typeof(decimal)), TypeUtil.GetType(TypeId.Decimal));
            Assert.AreEqual(new TypeLoader(typeof(double)), TypeUtil.GetType(TypeId.Double));
            Assert.AreEqual(new TypeLoader(typeof(float)), TypeUtil.GetType(TypeId.Float));
            Assert.AreEqual(new TypeLoader(typeof(IDictionary<,>)), TypeUtil.GetType(TypeId.IDictionary));
            Assert.AreEqual(new TypeLoader(typeof(IEnumerable)), TypeUtil.GetType(TypeId.IEnumerable));
            Assert.AreEqual(new TypeLoader(typeof(int)), TypeUtil.GetType(TypeId.Int));
            Assert.AreEqual(new TypeLoader(typeof(long)), TypeUtil.GetType(TypeId.Long));
            Assert.AreEqual(new TypeLoader(typeof(object)), TypeUtil.GetType(TypeId.Object));
            Assert.AreEqual(new TypeLoader(typeof(short)), TypeUtil.GetType(TypeId.Short));
            Assert.AreEqual(new TypeLoader(typeof(string)), TypeUtil.GetType(TypeId.String));
        }

        [Test]
        public void TypeLoaderAndType_Should_BeEqual()
        {
            Assert.AreEqual(new TypeLoader(typeof(bool)), typeof(bool));
        }

        [Test]
        public void TypeLoaderAndTypeLoader_Should_BeEqual()
        {
            Assert.AreEqual(new TypeLoader(typeof(bool)), new TypeLoader(typeof(bool)));
        }

        [Test]
        public void TypeLoaderAndTypeLoader_ShouldNot_BeEqual()
        {
            Assert.AreNotEqual(new TypeLoader(typeof(bool)), new TypeLoader(typeof(int)));
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
