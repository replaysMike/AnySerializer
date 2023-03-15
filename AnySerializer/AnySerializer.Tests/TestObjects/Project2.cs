using System;

namespace AnySerializer.Tests.TestObjects
{
    /// <summary>
    /// A user defined project
    /// </summary>
    public class Project2 : IEntity, IEquatable<Project>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Project description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Project location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Project color
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Custom notes for the project (BOM)
        /// </summary>
        [SerializeAs("xNotes")]
        public string Notes { get; set; }

        /// <summary>
        /// Modification date
        /// </summary>
        [SerializeAs("xDateModifiedUtc")]
        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Project project)
                return Equals(project);
            return false;
        }

        public bool Equals(Project other)
        {
            return other != null && ProjectId == other.ProjectId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return ProjectId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
        }

        public override string ToString()
        {
            return $"{ProjectId}: {Name}";
        }
    }
}
