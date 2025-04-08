using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using NXOpen;
using NXOpen.UF;

namespace cytk_NX2TCMigrationTool.src.PLM.NX
{
    /// <summary>
    /// Class to analyze NX parts and identify their types using NXOpen API
    /// </summary>
    public class NXTypeAnalyzer
    {
        private readonly Logger _logger;

        public NXTypeAnalyzer()
        {
            _logger = Logger.Instance;
        }

        /// <summary>
        /// Analyzes an NX part file to determine its type using NXOpen API
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

            try
            {
                // Check if file exists
                if (!File.Exists(filePath))
                {
                    _logger.Error("NXTypeAnalyzer", $"File does not exist: {filePath}");
                    return result;
                }

                // Use NXOpen API for accurate part information
                Session? theSession = null;
                UFSession? ufSession = null;
                bool isFamilyTemplate = false;

                try
                {
                    // Initialize NX session
                    theSession = Session.GetSession();
                    ufSession = UFSession.GetUFSession();

                    _logger.Debug("NXTypeAnalyzer", $"Opening part base: {filePath}");

                    // Use OpenBase to access part metadata without fully loading it
                    PartLoadStatus loadStatus;
                    BasePart basePart = theSession.Parts.OpenBase(filePath, out loadStatus);

                    Tag partTag = basePart.Tag;

                    try
                    {
                        // Call the UF API method to check if the part is a family template.
                        ufSession.Part.IsFamilyTemplate(partTag, out isFamilyTemplate);

                        // Output the result to the console for non-interactive execution.
                        if (isFamilyTemplate)
                        {
                            
                        }
                        else
                        {
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error while checking part family template: " + ex.Message);
                        
                    }

                }
                catch (NXOpen.NXException nxEx)
                {
                    _logger.Error("NXTypeAnalyzer", $"NXOpen error analyzing part: {nxEx.Message}");
                    throw;
                }
                finally
                {
                    // Clean up resources if needed
                    // Note: Don't terminate the NX session as it may be used elsewhere
                }

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
        /// Determines if a part is a drafting using NXOpen API
        /// </summary>
        private bool IsDraftingWithNXOpen(Part part)
        {
            try
            {
                // In NX, drafting parts typically have drawing sheets
                // This is one approach to detect drafting files
                bool hasDraftingSheets = false;
        
                // Get the drawing sheets collection
                var drawingSheets = part.DrawingSheets;
                if (drawingSheets != null && drawingSheets.Count > 0)
                {
                    hasDraftingSheets = true;
                }
        
                // Alternatively, check part type directly if available
                // NX stores part type information that can be queried
                bool isDraftingType = (part.Prototype == NXOpen.BasePart.PrototypeType.Drafting);
        
                return hasDraftingSheets || isDraftingType;
            }
            catch (Exception ex)
            {
                _logger.Warning("NXTypeAnalyzer", $"Error checking if part is drafting: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Determines if a part is an assembly using NXOpen API
        /// </summary>
        private bool IsAssemblyWithNXOpen(Part part)
        {
            try
            {
                // In NX, assemblies have components
                bool hasComponents = false;
        
                // Get the component collection
                var components = part.ComponentAssembly?.RootComponent?.GetChildren();
                if (components != null && components.Length > 0)
                {
                    hasComponents = true;
                }
        
                // Alternatively, check part type directly
                bool isAssemblyType = (part.Prototype == NXOpen.BasePart.PrototypeType.Assembly);
        
                return hasComponents || isAssemblyType;
            }
            catch (Exception ex)
            {
                _logger.Warning("NXTypeAnalyzer", $"Error checking if part is assembly: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a part file is an assembly by examining its structure using NXOpen
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

                Session theSession = null;
                try
                {
                    // Initialize NX session
                    theSession = Session.GetSession();

                    // Use OpenBase to efficiently check part structure
                    PartLoadStatus loadStatus;
                    BasePart basePart = theSession.Parts.OpenBase(filePath, out loadStatus);

                    if (basePart == null)
                    {
                        _logger.Warning("NXTypeAnalyzer", $"Failed to open part: {filePath}, Status: {loadStatus}");
                        return false;
                    }

                    bool isAssembly = false;

                    if (basePart is Part part)
                    {
                        // First check if it's defined as an assembly type
                        if (part.Prototype == NXOpen.BasePart.PrototypeType.Assembly)
                        {
                            isAssembly = true;
                        }

                        // Then check if it has components, regardless of its official type
                        var components = part.ComponentAssembly?.RootComponent?.GetChildren();
                        if (components != null && components.Length > 0)
                        {
                            isAssembly = true;
                        }
                    }

                    // Close the part without saving
                    //theSession.Parts.Close(basePart, false, true);

                    return isAssembly;
                }
                catch (NXOpen.NXException nxEx)
                {
                    _logger.Error("NXTypeAnalyzer", $"NXOpen error checking assembly structure: {nxEx.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("NXTypeAnalyzer", $"Error checking assembly structure: {ex.Message}");
                return false;
            }

            // For compatibility with the interface
            await Task.CompletedTask;
            return false;
        }
    }
}