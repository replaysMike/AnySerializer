using System.Collections.Generic;

namespace AnySerializer
{
    /// <summary>
    /// Tracks object references
    /// </summary>
    public class ObjectReferenceTracker
    {
        private ushort _currentReferenceId = 0;
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
        public bool ContainsHashcode(int hashCode)
        {
            return _objectTree.ContainsKey(hashCode);
        }

        /// <summary>
        /// Get the reference id based on the tracked objects hashCode
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public ushort GetObjectReferenceId(int hashCode)
        {
            return _objectTree[hashCode].ReferenceId;
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
    }

    /// <summary>
    /// An object reference
    /// </summary>
    public struct ObjectReference
    {
        /// <summary>
        /// The object being referenced
        /// </summary>
        public object Object;

        /// <summary>
        /// The unique reference id of the object
        /// </summary>
        public ushort ReferenceId;

        public ObjectReference(object obj, ushort referenceId)
        {
            Object = obj;
            ReferenceId = referenceId;
        }
    }
}
