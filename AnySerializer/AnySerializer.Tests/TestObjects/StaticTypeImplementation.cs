namespace AnySerializer.Tests.TestObjects
{
    public class StaticTypeImplementation : IStaticTypeInterface
    {
        public static IStaticTypeInterface Instance { get; } = new StaticTypeImplementation(string.Empty);

        public string Name { get; }

        public StaticTypeImplementation(string name)
        {
            Name = name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var typedObj = obj as StaticTypeImplementation;
            if (typedObj == null)
                return false;
            return typedObj.Name.Equals(Name);
        }
    }

    public interface IStaticTypeInterface { }
}
