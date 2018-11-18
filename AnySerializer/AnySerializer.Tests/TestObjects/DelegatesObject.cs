using System;

namespace AnySerializer.Tests.TestObjects
{
    public class DelegatesEventsObject : IEquatable<DelegatesEventsObject>
    {
        private int _id;
        public delegate void ADelegate(int value);
        public event ADelegate OnEvent;

        public DelegatesEventsObject(int id, ADelegate someDelegate)
        {
            _id = id;
            OnEvent = someDelegate;
        }

        public override bool Equals(object obj)
        {
            var basicObject = (DelegatesEventsObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(DelegatesEventsObject other)
        {
            return _id == other._id
                ;
        }
    }
}
