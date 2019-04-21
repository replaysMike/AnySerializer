using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnySerializer.Tests.TestObjects
{
    /// <summary>
    /// This is a wrapper for a collection implementation.
    /// </summary>
    /// <remarks>
    /// This is a bit of a weird use case, but should be supported.
    /// Note that the collection is of GenericObject<T> and not <T>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class CustomWrappedCollection<T> : ICollection<GenericObject<T>>, IEquatable<CustomWrappedCollection<T>>
    {
        private readonly ICollection<GenericObject<T>> _values;

        public int Count => _values.Count;

        public bool IsReadOnly => _values.IsReadOnly;

        public CustomWrappedCollection()
        {
            _values = new List<GenericObject<T>>();
        }

        public void Add(GenericObject<T> item)
        {
            _values.Add(item);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(GenericObject<T> item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(GenericObject<T>[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<GenericObject<T>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public bool Remove(GenericObject<T> item)
        {
            return _values.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _values.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(CustomWrappedCollection<T>)) return false;
            var objectTyped = obj as CustomWrappedCollection<T>;

            return Equals(objectTyped);
        }

        public bool Equals(CustomWrappedCollection<T> other)
        {
            var isEqual = _values.SequenceEqual(other._values);
            return isEqual;
        }
    }
}
