using System;

namespace cytk_NX2TCMigrationTool.src.PLM.Common.Interfaces
{
    public interface IPLMConnection
    {
        bool Connect();
        void Disconnect();
        bool IsConnected { get; }
        string ConnectionInfo { get; }
    }
}