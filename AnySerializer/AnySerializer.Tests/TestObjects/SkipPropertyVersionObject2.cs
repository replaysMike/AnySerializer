namespace AnySerializer.Tests.TestObjects
{
    public class SkipPropertyVersionObject2
    {
        public int Id { get;set;}
        public string Name { get; set; }
        public string V4Property { get;set; }

        [PropertyVersion("v5")]
        public string V5Property { get;set; }
    }
}
