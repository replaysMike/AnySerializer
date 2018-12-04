using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace AnySerializer.Tests.TestObjects
{
    public class JsonIgnorePropertiesObject : IEquatable<JsonIgnorePropertiesObject>
    {
        public int Id { get; set; }
        [JsonIgnore]
        public string Name { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var basicObject = (JsonIgnorePropertiesObject)obj;
            return Equals(basicObject);
        }

        public bool Equals(JsonIgnorePropertiesObject other)
        {
            return Id == other.Id
                && (Name == null && other.Name == null) || Name.Equals(other.Name)
                ;
        }
    }
}
