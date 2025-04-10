using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Models
{
    public class Part
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 
        /// </summary> 
        public string FileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDuplicate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DuplicateOf { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Metadata { get; set; }
        /// <summary>
        /// Flag indicating if this is a simple part
        /// </summary>
        public bool? IsPart { get; set; } = null;

        /// <summary>
        /// Flag indicating if this is an assembly
        /// </summary>
        public bool? IsAssembly { get; set; } = null;

        /// <summary>
        /// Flag indicating if this is a drafting
        /// </summary>
        public bool? IsDrafting { get; set; } = null;

        /// <summary>
        /// Flag indicating if this is a part family master
        /// </summary>
        public bool? IsPartFamilyMaster { get; set; } = null;

        /// <summary>
        /// Flag indicating if this is a part family member
        /// </summary>
        public bool? IsPartFamilyMember { get; set; } = null;

        // Helper method to convert JSON metadata to Dictionary
        public Dictionary<string, object> GetMetadataDictionary()
        {
            if (string.IsNullOrEmpty(Metadata))
            {
                return new Dictionary<string, object>();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(Metadata);
        }

        // Helper method to set metadata from Dictionary
        public void SetMetadata(Dictionary<string, object> metadata)
        {
            Metadata = JsonConvert.SerializeObject(metadata);
        }
    }
}