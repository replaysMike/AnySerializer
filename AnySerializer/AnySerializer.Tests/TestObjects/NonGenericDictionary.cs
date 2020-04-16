using System;
using System.Collections;
using System.Collections.Generic;

namespace AnySerializer.Tests.TestObjects
{
    public class NonGenericDictionary : IDictionary
    {
        private readonly Dictionary<object, object> _internal = new Dictionary<object, object>();

        public object this[object key]
        {
            get => _internal[key];
            set => _internal[key] = value;
        }

        public bool IsFixedSize { get; }

        public bool IsReadOnly { get; }

        public ICollection Keys => _internal.Keys;

        public ICollection Values => _internal.Values;

        public int Count => _internal.Count;

        public bool IsSynchronized { get; }

        public object SyncRoot => new object();

        public void Add(object key, object value) => _internal.Add(key, value);

        public void Clear() => _internal.Clear();

        public bool Contains(object key) => _internal.ContainsKey(key);

        public void CopyTo(Array array, int index)
        {
            // no implementation required
        }

        public IDictionaryEnumerator GetEnumerator() => _internal.GetEnumerator();

        public void Remove(object key)
        {
            _internal.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator() => _internal.GetEnumerator();
    }
}
