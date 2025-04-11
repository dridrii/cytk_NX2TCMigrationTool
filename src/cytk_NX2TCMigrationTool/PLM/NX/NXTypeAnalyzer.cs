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
                // Add debug logging here
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
        /// Analyzes an NX part file to determine its type
        /// </summary>
        /// <param name="filePath">Path to the NX part file</param>
        /// <returns>Dictionary with type flags (IsPart, IsAssembly, IsDrafting, IsPartFamilyMaster, IsPartFamilyMember)</returns>
        public async Task<Dictionary<string, bool>> AnalyzePartTypeAsync(string filePath)
        {
            var result = new Dictionary<string, bool>
            {
                { "IsPart", false },
                { "IsAssembly", false },
                { "IsDrafting", false },
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
                        // Try to use NXWorker for accurate type analysis
                        return await _nxWorkerClient.SendCommandAsync<Dictionary<string, bool>>("AnalyzePartType", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("NXTypeAnalyzer", $"NXWorker analysis failed, falling back to content analysis: {ex.Message}");
                        // Fall back to content analysis
                    }
                }

                // If NXWorker is not available or failed, analyze file content
                return AnalyzePartTypeByContent(filePath);
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error analyzing part type: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Analyzes a part's type based on file content
        /// </summary>
        private Dictionary<string, bool> AnalyzePartTypeByContent(string filePath)
        {
            var result = new Dictionary<string, bool>
            {
                { "IsPart", false },
                { "IsAssembly", false },
                { "IsDrafting", false },
                { "IsPartFamilyMaster", false },
                { "IsPartFamilyMember", false }
            };

            try
            {
                // Read a small portion of the file to check for signatures
                byte[] buffer = new byte[4096];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                string fileSignature = System.Text.Encoding.ASCII.GetString(buffer);

                // Check for part family characteristics
                result["IsPartFamilyMaster"] = IsPartFamilyMasterByContent(fileSignature);
                result["IsPartFamilyMember"] = IsPartFamilyMemberByContent(fileSignature);

                // Check for drafting characteristics
                result["IsDrafting"] = fileSignature.Contains("DRAWING_SHEET") ||
                                     fileSignature.Contains("DRAFT") ||
                                     fileSignature.Contains("DRAWING");

                // Check for assembly indicators
                result["IsAssembly"] = fileSignature.Contains("COMPONENT_ASSEMBLY") ||
                                    fileSignature.Contains("ASSEMBLY_CONSTRAINTS") ||
                                    fileSignature.Contains("ASSEMBLY_ROOT") ||
                                    fileSignature.Contains("ug_component_") ||
                                    fileSignature.Contains("ug_member_of_assembly") ||
                                    fileSignature.Contains("COMPONENT_DATA") ||
                                    fileSignature.Contains("UG_COMPONENT");

                // If none of the special types, it's a simple part
                if (!result["IsAssembly"] && !result["IsDrafting"] &&
                    !result["IsPartFamilyMaster"] && !result["IsPartFamilyMember"])
                {
                    result["IsPart"] = true;
                }

                _logger.Debug("NXTypeAnalyzer", $"Content analysis result for {filePath}: IsPart: {result["IsPart"]}, " +
                                              $"IsAssembly: {result["IsAssembly"]}, IsDrafting: {result["IsDrafting"]}, " +
                                              $"IsPartFamilyMaster: {result["IsPartFamilyMaster"]}, " +
                                              $"IsPartFamilyMember: {result["IsPartFamilyMember"]}");
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error analyzing part content: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Determines if a part is a part family master
        /// </summary>
        private bool IsPartFamilyMasterByContent(string fileContent)
        {
            // Check for part family master-related keywords
            return fileContent.Contains("FAMILY_MASTER") ||
                   fileContent.Contains("FAMILY_TABLE_MASTER") ||
                   fileContent.Contains("MASTER_MODEL");
        }

        /// <summary>
        /// Determines if a part is a part family member
        /// </summary>
        private bool IsPartFamilyMemberByContent(string fileContent)
        {
            // Check for part family member-related keywords
            return fileContent.Contains("FAMILY_MEMBER") ||
                   fileContent.Contains("FAMILY_TABLE_MEMBER") ||
                   fileContent.Contains("INSTANCE_OF_MASTER");
        }

        /// <summary>
        /// Checks if a part file is an assembly by examining its structure
        /// </summary>
        public async Task<bool> IsAssemblyByStructureAsync(string filePath, string nxInstallPath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.Error("NXTypeAnalyzer", $"File does not exist: {filePath}");
                    return false;
                }

                if (_useNxWorker)
                {
                    try
                    {
                        // Try to use NXWorker for accurate assembly detection
                        return await _nxWorkerClient.SendCommandAsync<bool>("IsAssemblyByStructure", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning("NXTypeAnalyzer", $"NXWorker assembly check failed, falling back to ugpc: {ex.Message}");
                        // Fall back to ugpc utility
                    }
                }

                // Use ugpc.exe for assembly structure detection as a fallback
                return await RunUgpcForAssemblyCheckAsync(filePath, nxInstallPath);
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error checking assembly structure: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Runs the ugpc.exe utility to check if a part is an assembly
        /// </summary>
        private async Task<bool> RunUgpcForAssemblyCheckAsync(string filePath, string nxInstallPath)
        {
            try
            {
                // Construct the path to ugpc.exe
                string ugpcPath = Path.Combine(nxInstallPath, "NXBIN", "ugpc.exe");

                if (!File.Exists(ugpcPath))
                {
                    _logger.Warning("NXTypeAnalyzer", $"ugpc.exe not found at: {ugpcPath}. Using file content analysis instead.");

                    // Read the file content and check for assembly indicators
                    byte[] buffer = new byte[4096];
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        fs.Read(buffer, 0, buffer.Length);
                    }
                    string fileContent = System.Text.Encoding.ASCII.GetString(buffer);

                    return fileContent.Contains("COMPONENT_ASSEMBLY") ||
                           fileContent.Contains("ASSEMBLY_CONSTRAINTS");
                }

                // Create process start info
                var startInfo = new ProcessStartInfo
                {
                    FileName = ugpcPath,
                    Arguments = $"-s4 -n \"{filePath}\"", // Use -s for structure and -n for counts
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Create process
                var process = new Process
                {
                    StartInfo = startInfo
                };

                // Start process and read output
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();

                // Use Task.Run for .NET Framework compatibility (which doesn't have WaitForExitAsync)
                await Task.Run(() => process.WaitForExit());

                // Check if the output indicates the part has an assembly structure
                // If the output contains component information, it's an assembly
                bool isAssembly = !output.Contains("has no assembly structure") &&
                                 (output.Contains(" x ") || output.Contains("Assembly structure"));

                _logger.Debug("NXTypeAnalyzer", $"ugpc check for {filePath}: IsAssembly={isAssembly}");
                return isAssembly;
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error checking assembly structure with ugpc: {ex.Message}");
                return false;
            }
        }
    }
}