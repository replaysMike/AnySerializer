using System;

namespace AnySerializer.Tests.TestObjects
{
    public class CustomerPaymentRecord : IEquatable<CustomerPaymentRecord>
    {
        public int RecordId { get; set; }
        public decimal PaymentAmount { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var basicObject = (CustomerPaymentRecord)obj;
            return Equals(basicObject);
        }

        public bool Equals(CustomerPaymentRecord other)
        {
            return RecordId == other.RecordId
                && PaymentAmount.Equals(other.PaymentAmount)
                ;
        }
    }
}
