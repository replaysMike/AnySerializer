using System.Collections.Generic;
using System.Linq;

namespace AnySerializer
{
    /// <summary>
    /// Provide a version tag to skip when deserializing data.
    /// All properties attributed with a matching SkipTagAttribute will not be deserialized.
    /// </summary>
    public class PropertyVersion
    {
        public ICollection<string> Tags { get; set; } = new List<string>();

        public PropertyVersion(params string[] tagsToSkip)
        {
            if (tagsToSkip != null)
            {
                foreach (var tag in tagsToSkip)
                {
                    Tags.Add(tag);
                }
            }
        }

        /// <summary>
        /// Returns true if tags list contains the specified tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Contains(string tag) => Tags.Any(x => x.Equals(tag, System.StringComparison.InvariantCultureIgnoreCase));
    }
}
