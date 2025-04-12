using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using cytk_NX2TCMigrationTool.src.Core.Common.NXCommunication;

namespace cytk_NX2TCMigrationTool.src.PLM.NX
{
    /// <summary>
    /// Class to analyze NX parts and identify their types
    /// </summary>
    public class NXTypeAnalyzer
    {
        private readonly Logger _logger;
        private readonly NXWorkerClient _nxWorkerClient;
        private readonly bool _useNxWorker;

        public NXTypeAnalyzer(NXWorkerClient nxWorkerClient = null, string nxPath = null, string nxWorkerPath = null)
        {
            _logger = Logger.Instance;

            if (nxWorkerClient != null)
            {
                _nxWorkerClient = nxWorkerClient;
                _useNxWorker = true;
                _logger.Debug("NXTypeAnalyzer", "Using provided NX Worker client");
            }
            else if (!string.IsNullOrEmpty(nxPath) && !string.IsNullOrEmpty(nxWorkerPath))
            {
                _logger.Debug("NXTypeAnalyzer", "Creating new NX Worker client because none was provided");
                _useNxWorker = true;
                _nxWorkerClient = new NXWorkerClient(nxPath, nxWorkerPath);
            }
            else
            {
                _useNxWorker = false;
                _logger.Debug("NXTypeAnalyzer", "Not using NX Worker (insufficient parameters)");
            }
        }

        /// <summary>
        /// Analyzes an NX part file to determine its family type only (master/member)
        /// </summary>
        /// <param name="filePath">Path to the NX part file</param>
        /// <returns>Dictionary with type flags (IsPartFamilyMaster, IsPartFamilyMember)</returns>
        public async Task<Dictionary<string, bool>> AnalyzePartFamilyTypeAsync(string filePath)
        {
            var result = new Dictionary<string, bool>
            {
                { "IsPartFamilyMaster", false },
                { "IsPartFamilyMember", false }
            };

            try
            {
                // Check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.Error("NXTypeAnalyzer", $"File does not exist: {filePath}");
                    return result;
                }

                if (_useNxWorker)
                {
                    try
                    {
                        // Only use NXWorker for part family detection
                        return await _nxWorkerClient.SendCommandAsync<Dictionary<string, bool>>("AnalyzePartFamilyType", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("NXTypeAnalyzer", $"NXWorker part family analysis failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error analyzing part family type: {ex.Message}");
            }

            return result;
        }
    }
}