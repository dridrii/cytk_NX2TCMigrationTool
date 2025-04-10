using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
//using NXOpen;
//using NXOpen.UF;


namespace cytk_NX2TCMigrationTool.src.PLM.NX
{
    /// <summary>
    /// Class to analyze NX parts and identify their types
    /// </summary>
    public class NXTypeAnalyzer
    {
        private readonly Logger _logger;

        public NXTypeAnalyzer()
        {
            _logger = Logger.Instance;
        }

        /// <summary>
        /// Analyzes an NX part file to determine its type
        /// </summary>
        /// <param name="filePath">Path to the NX part file</param>
        /// <returns>Dictionary with type flags (IsPart, IsAssembly, IsDrafting, IsPartFamilyMaster, IsPartFamilyMember)</returns>
        public Dictionary<string, bool> AnalyzePartType(string filePath)
        {
            var result = new Dictionary<string, bool>
            {
                { "IsPart", false },
                { "IsAssembly", false },
                { "IsDrafting", false },
                { "IsPartFamilyMaster", false },
                { "IsPartFamilyMember", false }
            };

            //UFSession? ufSession = null;

            try
            {
                // Check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.Error("NXTypeAnalyzer", $"File does not exist: {filePath}");
                    return result;
                }

                

                //Session theSession = Session.GetSession();
                //ufSession = UFSession.GetUFSession();
                
                //PartLoadStatus partLoadStatus;
                
                //BasePart basePart = theSession.Parts.OpenBase(filePath, out partLoadStatus);
                
                
                //bool parentTemplateTag = false;
                
                //ufSession.Part.IsFamilyTemplate(basePart.Tag, out parentTemplateTag);


                //ufSession.Part.AskPartFamilyTemplateTag(basePart.Tag, out parentTemplateTag);
                //isFamilyMember = parentTemplateTag != Tag.Null;
                //result["IsPartFamilyMember"] = isFamilyMember;

                // In a real implementation, we would use NXOpen API to analyze the file
                // For now, we'll use some heuristics based on file content

                // Read a small portion of the file to check for signatures
                byte[] buffer = new byte[4096];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                }

                string fileSignature = System.Text.Encoding.ASCII.GetString(buffer);




                // Check for part family characteristics
                bool isPartFamilyMaster = IsPartFamilyMasterByContent(fileSignature);
                result["IsPartFamilyMaster"] = isPartFamilyMaster;

                bool isPartFamilyMember = IsPartFamilyMemberByContent(fileSignature);
                result["IsPartFamilyMember"] = isPartFamilyMember;

                // If not any of the special types, it's a simple part
                // result["IsPart"] = !isDrafting && !isAssembly && !isPartFamilyMaster && !isPartFamilyMember;

                _logger.Debug("NXTypeAnalyzer", $"Analyzed part: {filePath}, IsPart: {result["IsPart"]}, " +
                                              $"IsAssembly: {result["IsAssembly"]}, IsDrafting: {result["IsDrafting"]}, " +
                                              $"IsPartFamilyMaster: {result["IsPartFamilyMaster"]}, " +
                                              $"IsPartFamilyMember: {result["IsPartFamilyMember"]}");
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error analyzing part type: {ex.Message}");
            }

            return result;
        }


        
        /// <summary>
        /// Determines if a part is an assembly based on file content
        /// </summary>
        //private bool IsAssemblyByContent(string fileContent)
        //{
        //    // In a real implementation, we would use NXOpen API
        //    // For now, we'll improve the detection heuristics
        //
        //    // Check for common assembly signatures
        //    bool hasAssemblySignatures = fileContent.Contains("COMPONENT_ASSEMBLY") ||
        //                               fileContent.Contains("ASSEMBLY_CONSTRAINTS") ||
        //                               fileContent.Contains("ASSEMBLY_ROOT");
        //
        //    // Additional assembly indicators often found in NX files
        //    bool hasComponentIndicators = fileContent.Contains("ug_component_") ||
        //                                fileContent.Contains("ug_member_of_assembly") ||
        //                                fileContent.Contains("COMPONENT_DATA") ||
        //                                fileContent.Contains("UG_COMPONENT");
        //
        //    return hasAssemblySignatures || hasComponentIndicators;
        //}

        /// <summary>
        /// Determines if a part is a part family master
        /// </summary>
        private bool IsPartFamilyMasterByContent(string fileContent)
        {
            // In a real implementation, we would use NXOpen API
            // For this example, we'll check for part family master-related keywords
            return fileContent.Contains("FAMILY_MASTER") ||
                   fileContent.Contains("FAMILY_TABLE_MASTER") ||
                   fileContent.Contains("MASTER_MODEL");
        }

        /// <summary>
        /// Determines if a part is a part family member
        /// </summary>
        private bool IsPartFamilyMemberByContent(string fileContent)
        {
            // In a real implementation, we would use NXOpen API
            // For this example, we'll check for part family member-related keywords
            return fileContent.Contains("FAMILY_MEMBER") ||
                   fileContent.Contains("FAMILY_TABLE_MEMBER") ||
                   fileContent.Contains("INSTANCE_OF_MASTER");
        }

        public async Task<bool> IsAssemblyByStructureAsync(string filePath, string nxInstallPath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.Error("NXTypeAnalyzer", $"File does not exist: {filePath}");
                    return false;
                }

                // Construct the path to ugpc.exe
                string ugpcPath = Path.Combine(nxInstallPath, "NXBIN", "ugpc.exe");

                if (!File.Exists(ugpcPath))
                {
                    _logger.Warning("NXTypeAnalyzer", $"ugpc.exe not found at: {ugpcPath}. Using file content analysis instead.");
                    //return IsAssemblyByContent(File.ReadAllText(filePath));
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
                process.WaitForExit();

                // Check if the output indicates the part has an assembly structure
                // If the output contains component information, it's an assembly
                return !output.Contains("has no assembly structure") &&
                       (output.Contains(" x ") || output.Contains("Assembly structure"));
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error checking assembly structure: {ex.Message}");
                return false;
            }
        }
    }
}