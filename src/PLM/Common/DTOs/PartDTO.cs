using System;
using System.Collections.Generic;

namespace cytk_NX2TCMigrationTool.src.PLM.Common.DTOs
{
    public class PartDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Revision { get; set; }
        public string Source { get; set; } // NX, Teamcenter, etc.
        public Dictionary<string, object> Metadata { get; set; }

        public PartDTO()
        {
            Metadata = new Dictionary<string, object>();
        }
    }
}