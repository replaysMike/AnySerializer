using System.Collections.ObjectModel;

namespace AnySerializer.Tests.TestObjects
{
    public class CustomCollectionAltObject<T> : Collection<T>
    {
        public int CustomId { get; set; }
        public string CustomName { get; set; }
        public CustomCollectionAltObject(int customId, string customName)
        {
            CustomId = customId;
            CustomName = customName;
        }
    }
}
