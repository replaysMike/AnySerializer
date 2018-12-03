namespace AnySerializer.Tests.TestObjects
{
    public class AnonymousTypeObject
    {
        public object AnonymousType { get; }
        public AnonymousTypeObject(object anonymousType)
        {
            AnonymousType = anonymousType;
        }
    }
}
