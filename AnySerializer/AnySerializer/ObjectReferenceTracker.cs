using System;
using System.Collections.Generic;

namespace AnySerializer
{
    /// <summary>
    /// Tracks object references
    /// </summary>
    public class ObjectReferenceTracker : IEquatable<ObjectReferenceTracker>
    {
        private ushort _currentReferenceId;
        private readonly Dictionary<ObjectKey, ObjectReference> _objectTree = new Dictionary<ObjectKey, ObjectReference>();

        public ushort GetNextReferenceId()
        {
            return _currentReferenceId;
        }

        /// <summary>
        /// Add a tracked object reference
        /// </summary>
        /// <param name="hashCode">The hashcode of the object</param>
        /// <param name="obj">The object to add a reference to</param>
        /// <returns>The reference id</returns>
        public ushort AddObject(int hashCode, object obj)
        {
            var key = new ObjectKey(hashCode, obj.GetType().GetHashCode());
            var reference = new ObjectReference(obj, _currentReferenceId);
            _objectTree.Add(key, reference);
            return _currentReferenceId++;
        }

        /// <summary>
        /// True if the tracker is tracking the hashcode
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public bool ContainsHashcode(int hashCode, Type type)
        {
            var key = new ObjectKey(hashCode, type.GetHashCode());
            var containsKey = _objectTree.ContainsKey(key);
            if (containsKey)
            {
                var reference = _objectTree[key];
                return reference.Object.GetType() == type;
            }
            return false;
        }

        /// <summary>
        /// Get the reference id based on the tracked objects hashCode
        /// </summary>
        /// <param name="hashCode">The object's hashcode</param>
        /// <param name="type">The object's type</param>
        /// <returns></returns>
        public ushort GetObjectReferenceId(int hashCode, Type type)
        {
            var key = new ObjectKey(hashCode, type.GetHashCode());
            var reference = _objectTree[key];
            if (reference.Object.GetType() == type)
            {
                return reference.ReferenceId;
            }
            throw new InvalidOperationException($"Hashcode '{hashCode}' and type '{type.Name}' not found.");
        }

        /// <summary>
        /// Get an object from a tracked hashcode
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public object GetObject(int hashCode, Type type)
        {
            var key = new ObjectKey(hashCode, type.GetHashCode());
            var reference = _objectTree[key];
            if (reference.Object.GetType() == type)
                return reference.Object;
            return null;
        }

        public override int GetHashCode()
        {
            return _objectTree.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(ObjectReferenceTracker))
                return false;
            return Equals((ObjectReferenceTracker)obj);
        }

        public bool Equals(ObjectReferenceTracker other)
        {
            if (other == null)
                return false;
            var dictionaryComparer = new DictionaryComparer<ObjectKey, ObjectReference>();
            return _currentReferenceId == other._currentReferenceId
                && dictionaryComparer.Equals(_objectTree, other._objectTree);
        }
    }

    /// <summary>
    /// An object composite key
    /// </summary>
    public struct ObjectKey : IEquatable<ObjectKey>
    {
        /// <summary>
        /// The hashcode of the object itself
        /// </summary>
        public int Hashcode { get; set; }

        /// <summary>
        /// The hashcode of the object type
        /// </summary>
        public int TypeHashcode { get; set; }

        public ObjectKey(int hashCode, int typeHashcode)
        {
            Hashcode = hashCode;
            TypeHashcode = typeHashcode;
        }

        public override int GetHashCode()
        {
            var hashCode = 23;
            hashCode = hashCode * 31 + Hashcode;
            hashCode = hashCode * 31 + TypeHashcode;
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ObjectKey))
                return false;
            return Equals((ObjectKey)obj);
        }

        public bool Equals(ObjectKey other)
        {
            return Hashcode == other.Hashcode
                && TypeHashcode == other.TypeHashcode;
        }
    }

    /// <summary>
    /// An object reference
    /// </summary>
    public struct ObjectReference : IEquatable<ObjectReference>
    {
        /// <summary>
        /// The object being referenced
        /// </summary>
        public object Object { get; }

        /// <summary>
        /// The unique reference id of the object
        /// </summary>
        public ushort ReferenceId { get; }

        public ObjectReference(object obj, ushort referenceId)
        {
            Object = obj;
            ReferenceId = referenceId;
        }

        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(ObjectReference))
                return false;
            return Equals((ObjectReference)obj);
        }

        public bool Equals(ObjectReference other)
        {
            return ReferenceId == other.ReferenceId
                && Object == other.Object;
        }
    }
}
