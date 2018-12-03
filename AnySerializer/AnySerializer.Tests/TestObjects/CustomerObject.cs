using System;
using System.Collections.Generic;
using System.Linq;

namespace AnySerializer.Tests.TestObjects
{
    public class CustomerObject : IEquatable<CustomerObject>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CustomerPaymentRecord> CustomerPaymentRecords { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var basicObject = (CustomerObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(CustomerObject other)
        {
            return Id == other.Id
                && (Name == null && other.Name == null) || Name.Equals(other.Name)
                && CustomerPaymentRecords.SequenceEqual(other.CustomerPaymentRecords)
                ;
        }
    }
}
