using System;

namespace AnySerializer.Tests.TestObjects
{
    public class Complex2Object : IEquatable<Complex2Object>
    {
        public BasicEnum GameId { get; set; }
        public Guid GameGlobalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Complex2Object(BasicEnum gameId, Guid gameGlobalId, string name, string description)
        {
            GameId = gameId;
            GameGlobalId = gameGlobalId;
            Name = name;
            Description = description;
        }

        public override bool Equals(object obj)
        {
            var basicObject = (Complex2Object)obj;
            return Equals(basicObject);
        }

        public bool Equals(Complex2Object other)
        {
            return GameId == other.GameId
                && GameGlobalId == other.GameGlobalId
                && (Name == null && other.Name == null) || Name.Equals(other.Name)
                && (Description == null && other.Description == null) || Description.Equals(other.Description)
                ;
        }
    }

    public enum BasicEnum
    {
        ValueOne = 1,
        ValueTwo,
    }
}
