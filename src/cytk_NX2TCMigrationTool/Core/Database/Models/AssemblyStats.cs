using System;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Models
{
    /// <summary>
    /// Represents statistics about a part's assembly properties
    /// </summary>
    public class AssemblyStats
    {
        /// <summary>
        /// ID of the part (references Parts table)
        /// </summary>
        public string PartId { get; set; }

        /// <summary>
        /// Flag indicating if this is an assembly
        /// </summary>
        public bool IsAssembly { get; set; }

        /// <summary>
        /// Flag indicating if this is a drafting
        /// </summary>
        public bool IsDrafting { get; set; }

        /// <summary>
        /// Number of direct components in this assembly
        /// </summary>
        public int ComponentCount { get; set; }

        /// <summary>
        /// Total number of components in the assembly hierarchy
        /// </summary>
        public int TotalComponentCount { get; set; }

        /// <summary>
        /// Depth of this assembly in the overall structure
        /// </summary>
        public int AssemblyDepth { get; set; }

        /// <summary>
        /// Number of assemblies this part appears in
        /// </summary>
        public int ParentCount { get; set; }

        /// <summary>
        /// Timestamp of when this assembly was last analyzed
        /// </summary>
        public string LastAnalyzed { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AssemblyStats()
        {
            IsAssembly = false;
            IsDrafting = false;
            ComponentCount = 0;
            TotalComponentCount = 0;
            AssemblyDepth = 0;
            ParentCount = 0;
            LastAnalyzed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}