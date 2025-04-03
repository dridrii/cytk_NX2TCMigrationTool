using System;
using System.Collections.Generic;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Models
{
    public class Part
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Checksum { get; set; }
        public bool IsDuplicate { get; set; }
        public string DuplicateOf { get; set; }
        public string Metadata { get; set; }

        // Helper method to convert JSON metadata to Dictionary
        public Dictionary<string, object> GetMetadataDictionary()
        {
            if (string.IsNullOrEmpty(Metadata))
            {
                return new Dictionary<string, object>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(Metadata);
        }

        // Helper method to set metadata from Dictionary
        public void SetMetadata(Dictionary<string, object> metadata)
        {
            Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
        }
    }
}