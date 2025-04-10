using System;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Models
{
    /// <summary>
    /// Represents the relationship types in the BOM structure
    /// </summary>
    public enum BOMRelationType
    {
        ASSEMBLY,     // Standard parent-child relationship in an assembly
        MASTER_MODEL  // Relationship between a drafting and its master model
    }

    /// <summary>
    /// Represents a relationship between two parts in a BOM structure
    /// </summary>
    public class BOMRelationship
    {
        /// <summary>
        /// Unique identifier for the relationship
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the parent part (assembly)
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// ID of the child part (component)
        /// </summary>
        public string ChildId { get; set; }

        /// <summary>
        /// Type of relationship (ASSEMBLY, MASTER_MODEL)
        /// </summary>
        public string RelationType { get; set; }

        /// <summary>
        /// Instance name of the component
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Position/order in the assembly
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// Quantity of this component in the assembly
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Flag indicating if the relationship has been verified
        /// </summary>
        public bool Verified { get; set; }

        /// <summary>
        /// Timestamp of when this relationship was last updated
        /// </summary>
        public string LastUpdated { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BOMRelationship()
        {
            Quantity = 1;
            Verified = false;
            LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Helper method to convert string relation type to enum
        /// </summary>
        public BOMRelationType GetRelationTypeEnum()
        {
            if (Enum.TryParse<BOMRelationType>(RelationType, out var result))
            {
                return result;
            }
            return BOMRelationType.ASSEMBLY; // Default to assembly
        }
    }
}