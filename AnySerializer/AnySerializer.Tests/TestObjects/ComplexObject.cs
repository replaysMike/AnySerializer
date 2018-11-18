using System;
using System.Collections.Generic;
using System.Linq;

namespace AnySerializer.Tests.TestObjects
{
    public class ComplexObject : IEquatable<ComplexObject>
    {
        public int Id { get; set; }
        public string Department { get; set; }
        public bool IsEnabled { get; set; }
        public ICollection<int> NumbersList { get; set; }
        public IDictionary<int, CustomerObject> Customers { get; set; }


        public override bool Equals(object obj)
        {
            var basicObject = (ComplexObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(ComplexObject other)
        {
            var dictionaryComparer = new DictionaryComparer<int, CustomerObject>();
            return Id == other.Id
                && (Department == null && other.Department == null) || Department.Equals(other.Department)
                && IsEnabled == other.IsEnabled
                && NumbersList.SequenceEqual(other.NumbersList)
                && dictionaryComparer.Equals(Customers, other.Customers)
                ;
        }
    }
}
