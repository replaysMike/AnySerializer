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
        private readonly Dictionary<int, ObjectReference> _objectTree = new Dictionary<int, ObjectReference>();

        public ushort GetNextReferenceId()
        {
            return _currentReferenceId;
        }

        /// <summary>
        /// Add a tracked object reference
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="hashCode"></param>
        /// <returns>The reference id</returns>
        public ushort AddObject(object obj, int hashCode)
        {
            _objectTree.Add(hashCode, new ObjectReference(obj, _currentReferenceId));
            return _currentReferenceId++;
        }

        /// <summary>
        /// True if the tracker is tracking the hashcode
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public bool ContainsHashcode(int hashCode, Type objectType)
        {
            var containsKey = _objectTree.ContainsKey(hashCode);
            if (containsKey)
            {
                var reference = _objectTree[hashCode];
                return reference.Object.GetType() == objectType;
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
            var reference = _objectTree[hashCode];
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
        public object GetObject(int hashCode)
        {
            return _objectTree[hashCode].Object;
        }

        /// <summary>
        /// Get an object from a tracked hashcode
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public object GetObject(int hashCode, Type type)
        {
            var reference = _objectTree[hashCode];
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
            var dictionaryComparer = new DictionaryComparer<int, ObjectReference>();
            return _currentReferenceId == other._currentReferenceId
                && dictionaryComparer.Equals(_objectTree, other._objectTree);
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
