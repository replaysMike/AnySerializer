using System;

namespace AnySerializer.Tests.TestObjects
{
    public class GenericObject<T> : IEquatable<GenericObject<T>>
    {
        public T Value { get; set; }
        public GenericObject(T value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(GenericObject<T>)) return false;
            var objectTyped = obj as GenericObject<T>;

            return Equals(objectTyped);
        }

        public bool Equals(GenericObject<T> other)
        {
            if (other == null) return false;
            if (other.Value == null) return false;

            return Value.Equals(other.Value);
        }
    }
}
