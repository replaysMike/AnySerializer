using AnySerializer.Tests.TestObjects;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TypeSupport.Extensions;
using static AnySerializer.TypeManagement;

namespace AnySerializer.Tests
{
    [TestFixture]
    public class TypeUtilTests
    {
        [Test]
        public void ShouldTypes_MapCorrectly()
        {
            Assert.AreEqual(typeof(Array), TypeUtil.GetType(TypeId.Array));
            Assert.AreEqual(typeof(Enum), TypeUtil.GetType(TypeId.Enum));
            Assert.AreEqual(typeof(bool), TypeUtil.GetType(TypeId.Bool));
            Assert.AreEqual(typeof(byte), TypeUtil.GetType(TypeId.Byte));
            Assert.AreEqual(typeof(char), TypeUtil.GetType(TypeId.Char));
            Assert.AreEqual(typeof(decimal), TypeUtil.GetType(TypeId.Decimal));
            Assert.AreEqual(typeof(double), TypeUtil.GetType(TypeId.Double));
            Assert.AreEqual(typeof(float), TypeUtil.GetType(TypeId.Float));
            Assert.AreEqual(typeof(IDictionary), TypeUtil.GetType(TypeId.IDictionary));
            Assert.AreEqual(typeof(KeyValuePair<,>), TypeUtil.GetType(TypeId.KeyValuePair));
            Assert.AreEqual(typeof(IEnumerable), TypeUtil.GetType(TypeId.IEnumerable));
            Assert.AreEqual(typeof(int), TypeUtil.GetType(TypeId.Int));
            Assert.AreEqual(typeof(long), TypeUtil.GetType(TypeId.Long));
            Assert.AreEqual(typeof(object), TypeUtil.GetType(TypeId.Object));
            Assert.AreEqual(typeof(short), TypeUtil.GetType(TypeId.Short));
            Assert.AreEqual(typeof(string), TypeUtil.GetType(TypeId.String));
        }

        [Test]
        public void ExtendedTypeAndType_Should_BeEqual()
        {
            Assert.AreEqual(typeof(bool).GetExtendedType(), typeof(bool));
        }

        [Test]
        public void ExtendedTypeAndExtendedType_Should_BeEqual()
        {
            Assert.AreEqual(typeof(bool).GetExtendedType(), typeof(bool).GetExtendedType());
        }

        [Test]
        public void ExtendedTypeAndExtendedType_ShouldNot_BeEqual()
        {
            Assert.AreNotEqual(typeof(bool).GetExtendedType(), typeof(int).GetExtendedType());
        }

        [Test]
        public void Should_CreateNewArray()
        {
            var test = new TypeSupport.ObjectFactory().CreateEmptyObject<byte[]>();
            Assert.AreEqual(test.GetType(), typeof(byte[]));
        }

        [Test]
        public void Should_CreateNewEnumerable()
        {
            var test = new TypeSupport.ObjectFactory().CreateEmptyObject<IEnumerable>();
            var isEnumerable = test is IEnumerable<object>;
            // .Net Core 3+ no longer guarantees this list to be a object[], its now an internal unaccessible type of EmptyPartition
            Assert.IsTrue(isEnumerable);
        }

        [Test]
        public void Should_CreateNewList()
        {
            var test = new TypeSupport.ObjectFactory().CreateEmptyObject<List<int>>();
            Assert.AreEqual(test.GetType(), typeof(List<int>));
        }

        [Test]
        public void Should_CreateNewDictionary()
        {
            var test = new TypeSupport.ObjectFactory().CreateEmptyObject<Dictionary<int, string>>();
            Assert.AreEqual(test.GetType(), typeof(Dictionary<int, string>));
        }

        [Test]
        public void Should_CreateNewString()
        {
            var test = new TypeSupport.ObjectFactory().CreateEmptyObject<string>();
            Assert.IsNull(test);
        }

        [Test]
        public void Should_CreateNewStringUsingInitializer()
        {
            var test = new TypeSupport.ObjectFactory().CreateEmptyObject<string>(() => string.Empty);
            Assert.AreEqual(test.GetType(), typeof(string));
            Assert.AreEqual(test, string.Empty);
        }

        [Test]
        public void Should_CreateNewBasicObject()
        {
            var test = new TypeSupport.ObjectFactory().CreateEmptyObject<BasicObject>();
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
