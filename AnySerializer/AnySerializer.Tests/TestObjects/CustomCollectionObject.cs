using System;
using System.Collections.Generic;

namespace AnySerializer.Tests.TestObjects
{
    public class CustomCollectionObject : IEquatable<CustomCollectionObject>
    {
        private readonly Dictionary<string, int> _innerStorage = new Dictionary<string, int>();

        public ICollection<string> Keys
        {
            get { return _innerStorage.Keys; }
        }

        public ICollection<int> Values
        {
            get { return _innerStorage.Values; }
        }

        public void Add(string key, int value)
        {
            if (!_innerStorage.ContainsKey(key))
            {
                _innerStorage.Add(key, value);
            }
        }

        public int this[string key]
        {
            get
            {
                if (!_innerStorage.TryGetValue(key, out int item))
                {
                    _innerStorage.Add(key, item);
                }
                return item;
            }
            set
            {
                _innerStorage[key] = value;
            }
        }

        public override bool Equals(object obj)
        {
            var basicObject = (CustomCollectionObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(CustomCollectionObject other)
        {
            var dictionaryComparer = new DictionaryComparer<string, int>();
            return ((_innerStorage == null && other._innerStorage == null) || dictionaryComparer.Equals(_innerStorage, other._innerStorage))
                ;
        }
    }
}
