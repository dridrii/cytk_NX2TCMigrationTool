using System;
using System.Collections.Generic;

namespace cytk_NX2TCMigrationTool.src.PLM.Common.Interfaces
{
    public interface IPLMPartManager
    {
        bool LoadPart(string partIdentifier);
        Dictionary<string, object> GetPartAttributes(string partIdentifier);
        bool ExportPart(string partIdentifier, string targetPath, string format);
    }
}