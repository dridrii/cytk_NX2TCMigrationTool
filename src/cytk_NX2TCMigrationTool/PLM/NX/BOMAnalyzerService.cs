using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using cytk_NX2TCMigrationTool.src.Core.Common.NXCommunication;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;
using cytk_NX2TCMigrationTool.src.Core.Database.Repositories;
using cytk_NX2TCMigrationTool.src.Core.Settings;

namespace cytk_NX2TCMigrationTool.src.PLM.NX
{
    /// <summary>
    /// Service to analyze NX part files for BOM structure using ugpc.exe tool
    /// </summary>
    public class BOMAnalyzerService
    {
        private readonly PartRepository _partRepository;
        private readonly BOMRelationshipRepository _bomRepository;
        private readonly AssemblyStatsRepository _statsRepository;
        private readonly SettingsManager _settingsManager;
        private readonly NXTypeAnalyzer _typeAnalyzer;
        private readonly string _nxInstallPath;
        private readonly string _nxWorkerPath;
        private readonly Logger _logger;
        private readonly string _salt;

        // Event for progress reporting
        public event EventHandler<BOMAnalysisProgressEventArgs> AnalysisProgress;

        // Constructor
        // In src/cytk_NX2TCMigrationTool/PLM/NX/BOMAnalyzerService.cs
        public BOMAnalyzerService(PartRepository partRepository, BOMRelationshipRepository bomRepository,
                                  AssemblyStatsRepository statsRepository, SettingsManager settingsManager,
                                  NXWorkerClient nxWorkerClient = null)
        {
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _bomRepository = bomRepository ?? throw new ArgumentNullException(nameof(bomRepository));
            _statsRepository = statsRepository ?? throw new ArgumentNullException(nameof(statsRepository));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _logger = Logger.Instance;

            // Get NX installation path from settings
            _nxInstallPath = _settingsManager.GetSetting("/Settings/NX/InstallPath");

            // Get NX Worker path from settings or use default
            _nxWorkerPath = _settingsManager.GetSetting("/Settings/NX/NXWorkerPath");
            if (string.IsNullOrEmpty(_nxWorkerPath))
            {
                // Default to a location relative to the application
                _nxWorkerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cytk_NX2TC_NXWorker.exe");
            }

            if (nxWorkerClient != null)
            {
                _logger.Debug("BOMAnalyzerService", "Using provided NX Worker client");
            }
            else
            {
                _logger.Debug("BOMAnalyzerService", "No NX Worker client provided");
            }

            // Initialize the type analyzer with NX worker client if available
            _typeAnalyzer = new NXTypeAnalyzer(nxWorkerClient, _nxInstallPath, _nxWorkerPath);

            // Get salt from settings
            _salt = _settingsManager.GetSetting("/Settings/Application/Salt") ?? "CYTKdefault123";
        }

        /// <summary>
        /// Analyzes all parts in the database for BOM relationships
        /// </summary>
        public async Task<AnalysisResults> AnalyzeAllPartsAsync()
        {
            _logger.Info("BOMAnalyzer", "Starting analysis of all parts");
            var results = new AnalysisResults();

            // Get all parts from the repository
            var allParts = _partRepository.GetAll().ToList();
            int totalParts = allParts.Count;
            int currentPart = 0;

            _logger.Info("BOMAnalyzer", $"Found {totalParts} parts to analyze");

            foreach (var part in allParts)
            {
                currentPart++;

                // Report progress
                ReportProgress($"Analyzing part {currentPart}/{totalParts}", part.Name,
                               (double)currentPart / totalParts * 100);

                if (string.IsNullOrEmpty(part.FilePath) || !File.Exists(part.FilePath))
                {
                    _logger.Warning("BOMAnalyzer", $"Part {part.Id} ({part.Name}) has no valid file path");
                    results.InvalidParts++;
                    continue;
                }

                try
                {
                    await AnalyzePartAsync(part);
                    results.AnalyzedParts++;

                    // Determine if this was an assembly or a drafting
                    var stats = _statsRepository.GetByPartId(part.Id);
                    if (stats != null)
                    {
                        if (stats.IsAssembly)
                            results.Assemblies++;
                        if (stats.IsDrafting)
                            results.Draftings++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("BOMAnalyzer", $"Error analyzing part {part.Id} ({part.Name}): {ex.Message}");
                    results.Errors.Add($"Part {part.Name}: {ex.Message}");
                    results.FailedParts++;
                }
            }

            // Update parent counts and assembly depths after all parts have been analyzed
            await UpdateAssemblyHierarchyInfoAsync();

            _logger.Info("BOMAnalyzer", $"Analysis complete. Parts analyzed: {results.AnalyzedParts}, " +
                                       $"Assemblies: {results.Assemblies}, " +
                                       $"Draftings: {results.Draftings}, " +
                                       $"Failed: {results.FailedParts}, " +
                                       $"Invalid: {results.InvalidParts}");

            return results;
        }

        /// <summary>
        /// Analyzes a specific part for BOM relationships
        /// </summary>
        public async Task AnalyzePartAsync(Part part)
        {
            _logger.Debug("BOMAnalyzer", $"Analyzing part {part.Id} ({part.Name})");

            if (string.IsNullOrEmpty(part.FilePath) || !File.Exists(part.FilePath))
            {
                throw new FileNotFoundException($"Part file not found: {part.FilePath}");
            }

            try
            {
                // Step 1: Analyze the part family type using NXTypeAnalyzer
                var partFamilyInfo = await _typeAnalyzer.AnalyzePartFamilyTypeAsync(part.FilePath);
                _logger.Debug("BOMAnalyzer", $"Part family analysis results: Master={partFamilyInfo["IsPartFamilyMaster"]}, Member={partFamilyInfo["IsPartFamilyMember"]}");

                // Step 2: Use ugpc to determine assembly structure and drafting status
                string ugpcPath = Path.Combine(_nxInstallPath, "NXBIN", "ugpc.exe");
                var ugpcResults = await RunUgpcToolAsync(ugpcPath, part.FilePath);

                // Check if the part has assembly structure based on ugpc output
                bool isAssemblyByUgpc = !ugpcResults.Contains("has no assembly structure") && ugpcResults.Contains(" x ");
                _logger.Debug("BOMAnalyzer", $"Initial ugpc assembly determination: isAssemblyByUgpc={isAssemblyByUgpc}");

                // Determine if it's a drafting (based on filename or other criteria)
                bool isDrafting = part.FilePath.ToLower().Contains("draft") ||
                                  part.FilePath.ToLower().Contains("drawing") ||
                                  part.FilePath.ToLower().Contains("dwg");
                _logger.Debug("BOMAnalyzer", $"Initial drafting determination: isDrafting={isDrafting}");

                // Extract relationships information
                var relationships = ParseUgpcOutput(ugpcResults, part);
                _logger.Debug("BOMAnalyzer", $"Found {relationships.Count} component relationships");

                // Create assembly stats
                var stats = new AssemblyStats
                {
                    PartId = part.Id,
                    ComponentCount = relationships.Count,
                    LastAnalyzed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // PRESERVE EXISTING INFORMATION FIRST
                // Save the existing part family information
                bool isPartFamilyMaster = part.IsPartFamilyMaster ?? partFamilyInfo["IsPartFamilyMaster"];
                bool isPartFamilyMember = part.IsPartFamilyMember ?? partFamilyInfo["IsPartFamilyMember"];

                // Update part family info from analysis if the existing values are false or null
                if (!isPartFamilyMaster && partFamilyInfo["IsPartFamilyMaster"])
                {
                    isPartFamilyMaster = true;
                    _logger.Debug("BOMAnalyzer", $"Updated part family master status to true for part {part.Id}");
                }

                if (!isPartFamilyMember && partFamilyInfo["IsPartFamilyMember"])
                {
                    isPartFamilyMember = true;
                    _logger.Debug("BOMAnalyzer", $"Updated part family member status to true for part {part.Id}");
                }

                // FINAL DETERMINATION OF PART TYPE
                bool isAssembly = relationships.Count > 0 || isAssemblyByUgpc;

                // Update part with all information
                part.IsPartFamilyMaster = isPartFamilyMaster;
                part.IsPartFamilyMember = isPartFamilyMember;

                // Set part type flags
                if (isAssembly)
                {
                    _logger.Debug("BOMAnalyzer", $"FINAL: Part {part.Id} ({part.Name}) is an assembly");
                    part.IsAssembly = true;
                    part.IsPart = false;
                    part.IsDrafting = false;
                    stats.IsAssembly = true;
                    stats.IsDrafting = false;
                }
                else if (isDrafting)
                {
                    _logger.Debug("BOMAnalyzer", $"FINAL: Part {part.Id} ({part.Name}) is a drafting");
                    part.IsDrafting = true;
                    part.IsAssembly = false;
                    part.IsPart = false;
                    stats.IsDrafting = true;
                    stats.IsAssembly = false;
                }
                else
                {
                    _logger.Debug("BOMAnalyzer", $"FINAL: Part {part.Id} ({part.Name}) is a regular part");
                    part.IsPart = true;
                    part.IsAssembly = false;
                    part.IsDrafting = false;
                }

                // Now perform the update
                _logger.Debug("BOMAnalyzer", $"Updating part {part.Id} with final values: " +
                                            $"IsPart={part.IsPart}, " +
                                            $"IsAssembly={part.IsAssembly}, " +
                                            $"IsDrafting={part.IsDrafting}, " +
                                            $"IsPartFamilyMaster={part.IsPartFamilyMaster}, " +
                                            $"IsPartFamilyMember={part.IsPartFamilyMember}");
                _partRepository.Update(part);

                // Process relationships and stats as before
                // Save all relationships to database
                foreach (var relationship in relationships)
                {
                    // Check if this is a master model relationship (drafting-to-model)
                    if (part.IsDrafting == true && relationship.RelationType == BOMRelationType.ASSEMBLY.ToString())
                    {
                        // For drafting files, change the relationship type to MASTER_MODEL
                        relationship.RelationType = BOMRelationType.MASTER_MODEL.ToString();
                    }

                    if (!_bomRepository.RelationshipExists(relationship.ParentId, relationship.ChildId, relationship.RelationType))
                    {
                        _bomRepository.Add(relationship);
                    }
                }

                // Save assembly stats to database
                var existingStats = _statsRepository.GetByPartId(part.Id);
                if (existingStats != null)
                {
                    stats.ParentCount = existingStats.ParentCount; // Preserve parent count when updating
                    stats.TotalComponentCount = existingStats.TotalComponentCount; // Preserve total count until recalculation
                    stats.AssemblyDepth = existingStats.AssemblyDepth; // Preserve assembly depth until recalculation
                    _statsRepository.Update(stats);
                }
                else
                {
                    _statsRepository.Add(stats);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMAnalyzer", $"Error during part analysis: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs the ugpc.exe tool to analyze a part
        /// </summary>
        private async Task<string> RunUgpcToolAsync(string ugpcPath, string partFilePath)
        {
            _logger.Debug("BOMAnalyzer", $"Running ugpc.exe for part: {partFilePath}");

            // Create process start info
            var startInfo = new ProcessStartInfo
            {
                FileName = ugpcPath,
                Arguments = $"-s4 -n \"{partFilePath}\"", // Use -s for structure and -n for counts
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

            // Prepare result string
            string output = string.Empty;
            string error = string.Empty;

            // Start process and read output asynchronously
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(
                Task.Run(() => process.WaitForExit()),
                Task.Run(async () => output = await outputTask),
                Task.Run(async () => error = await errorTask)
            );

            if (!string.IsNullOrEmpty(error))
            {
                _logger.Warning("BOMAnalyzer", $"ugpc.exe reported errors: {error}");
            }

            _logger.Debug("BOMAnalyzer", $"ugpc.exe completed with exit code: {process.ExitCode}");
            return output;
        }

        /// <summary>
        /// Processes the output from ugpc.exe to extract BOM relationships and stats
        /// </summary>
        private async Task ProcessUgpcResultsAsync(Part part, string ugpcOutput)
        {
            _logger.Debug("BOMAnalyzer", $"Processing ugpc.exe output for part {part.Id} ({part.Name})");

            // Extract relationships information
            var relationships = ParseUgpcOutput(ugpcOutput, part);

            // Calculate assembly stats
            var stats = new AssemblyStats
            {
                PartId = part.Id,
                ComponentCount = relationships.Count,
                LastAnalyzed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Save all relationships to database
            foreach (var relationship in relationships)
            {
                // Check if this is a master model relationship (drafting-to-model)
                if (part.IsDrafting == true && relationship.RelationType == BOMRelationType.ASSEMBLY.ToString())
                {
                    // For drafting files, change the relationship type to MASTER_MODEL
                    relationship.RelationType = BOMRelationType.MASTER_MODEL.ToString();
                }

                if (!_bomRepository.RelationshipExists(relationship.ParentId, relationship.ChildId, relationship.RelationType))
                {
                    _bomRepository.Add(relationship);
                }
            }

            // KEY FIX: If the part has relationships (components), it's an assembly regardless of what NXTypeAnalyzer determined
            if (relationships.Count > 0)
            {
                _logger.Debug("BOMAnalyzer", $"Setting IsAssembly flag for part {part.Id} ({part.Name}) with {relationships.Count} components");
                part.IsAssembly = true;
                part.IsPart = false;  // A part cannot be both an assembly and a simple part

                // Also update stats
                stats.IsAssembly = true;
            }
            else
            {
                // If this part has no components, it's a simple part (not an assembly)
                _logger.Debug("BOMAnalyzer", $"Setting IsPart flag for part {part.Id} ({part.Name}) with no components");
                part.IsPart = true;
                part.IsAssembly = false;
            }

            // Update the part in the database after type determination
            _partRepository.Update(part);

            // Save assembly stats to database
            var existingStats = _statsRepository.GetByPartId(part.Id);
            if (existingStats != null)
            {
                stats.ParentCount = existingStats.ParentCount; // Preserve parent count when updating
                stats.TotalComponentCount = existingStats.TotalComponentCount; // Preserve total count until recalculation
                stats.AssemblyDepth = existingStats.AssemblyDepth; // Preserve assembly depth until recalculation
                _statsRepository.Update(stats);
            }
            else
            {
                _statsRepository.Add(stats);
            }

            await Task.CompletedTask; // For async method
        }

        /// <summary>
        /// Parses the output from ugpc.exe to extract BOM relationships
        /// </summary>
        private List<BOMRelationship> ParseUgpcOutput(string ugpcOutput, Part parentPart)
        {
            var relationships = new List<BOMRelationship>();

            if (string.IsNullOrEmpty(ugpcOutput))
            {
                _logger.Debug("BOMAnalyzer", $"No output from ugpc.exe for part {parentPart.Name}");
                return relationships;
            }

            // Log full output for debugging
            _logger.Debug("BOMAnalyzer", $"Parsing ugpc output for {parentPart.Name}:\n{ugpcOutput}");

            // Split output into lines
            string[] lines = ugpcOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Skip if lines count is less than 2 (we need at least command and result)
            if (lines.Length < 2)
            {
                _logger.Debug("BOMAnalyzer", $"Unexpected output format from ugpc.exe for part {parentPart.Name}");
                return relationships;
            }

            // Check if the part has no assembly structure
            if (lines.Length >= 2 && lines[1].Contains("has no assembly structure"))
            {
                _logger.Debug("BOMAnalyzer", $"Part {parentPart.Name} has no assembly structure");
                return relationships;
            }

            // Extract parent part name from the output
            string parentFileName = Path.GetFileName(parentPart.FilePath);
            _logger.Debug("BOMAnalyzer", $"Parent file name: {parentFileName}");

            // Skip the command line (first line)
            // Also skip the second line which often repeats the part itself with "x 1"
            int startLine = 1;
            if (lines.Length > 1 && lines[1].Contains(parentFileName) && lines[1].Contains(" x "))
            {
                startLine = 2;
                _logger.Debug("BOMAnalyzer", $"Skipping line that contains the part itself: {lines[1]}");
            }

            // Process the component lines
            int position = 0;
            for (int i = startLine; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Skip empty or irrelevant lines
                if (string.IsNullOrWhiteSpace(line) || !line.Contains(" x "))
                {
                    continue;
                }

                _logger.Debug("BOMAnalyzer", $"Processing component line: {line}");

                // Split at " x " to separate path and quantity
                string[] parts = line.Split(new[] { " x " }, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    _logger.Warning("BOMAnalyzer", $"Invalid component line format: {line}");
                    continue;
                }

                string componentPath = parts[0].Trim();
                string componentFileName = Path.GetFileName(componentPath);

                // Parse quantity
                int quantity;
                if (!int.TryParse(parts[1].Trim(), out quantity))
                {
                    quantity = 1; // Default to 1 if parsing fails
                }

                _logger.Debug("BOMAnalyzer", $"Found component: {componentFileName}, Quantity: {quantity}");

                // Look up the child part in the database
                var childParts = _partRepository.GetAll()
                    .Where(p => p.FileName != null && p.FileName.Equals(componentFileName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (childParts.Count == 0)
                {
                    _logger.Warning("BOMAnalyzer", $"Child part not found in database: {componentFileName}");
                    continue;
                }

                // Take the first match
                var childPart = childParts.First();

                // Create a new relationship
                var relationship = new BOMRelationship
                {
                    Id = FileUtils.GenerateUniqueId(_salt),
                    ParentId = parentPart.Id,
                    ChildId = childPart.Id,
                    RelationType = BOMRelationType.ASSEMBLY.ToString(),
                    InstanceName = $"Instance_{position + 1}",
                    Position = position++,
                    Quantity = quantity,
                    Verified = true,
                    LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                relationships.Add(relationship);
                _logger.Debug("BOMAnalyzer", $"Added relationship: {parentPart.Name} -> {childPart.Name} (Qty: {quantity})");
            }

            _logger.Info("BOMAnalyzer", $"Found {relationships.Count} components for part {parentPart.Name}");
            return relationships;
        }


        /// <summary>
        /// Calculates assembly statistics based on BOM relationships
        /// </summary>
        private AssemblyStats CalculateAssemblyStats(Part part, List<BOMRelationship> directRelationships)
        {
            var stats = new AssemblyStats
            {
                PartId = part.Id,
                // This is the key line - only mark as assembly if it has child relationships
                IsAssembly = directRelationships.Count > 0,
                ComponentCount = directRelationships.Count,
                LastAnalyzed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // We'll calculate total component count and depth later
            // after all parts have been processed

            return stats;
        }

        /// <summary>
        /// Updates assembly hierarchy information (parent counts, depths, total component counts)
        /// </summary>
        private async Task UpdateAssemblyHierarchyInfoAsync()
        {
            _logger.Info("BOMAnalyzer", "Updating assembly hierarchy information");

            // Get all relationships
            var allRelationships = _bomRepository.GetAll().ToList();

            // Update parent counts
            var childIdGroups = allRelationships.GroupBy(r => r.ChildId);
            foreach (var group in childIdGroups)
            {
                string childId = group.Key;
                int parentCount = group.Count();

                var stats = _statsRepository.GetByPartId(childId);
                if (stats != null)
                {
                    stats.ParentCount = parentCount;
                    _statsRepository.Update(stats);
                }
            }

            // Update assembly depths and total component counts
            var allStats = _statsRepository.GetAll().ToList();
            var assemblyStats = allStats.Where(s => s.IsAssembly).ToList();

            foreach (var assembly in assemblyStats)
            {
                // Calculate total component count and max depth
                var (totalCount, maxDepth) = CalculateHierarchyInfo(assembly.PartId, allRelationships, allStats);

                assembly.TotalComponentCount = totalCount;
                assembly.AssemblyDepth = maxDepth;
                _statsRepository.Update(assembly);

                // Report progress periodically
                if (assemblyStats.IndexOf(assembly) % 10 == 0)
                {
                    double progress = (double)assemblyStats.IndexOf(assembly) / assemblyStats.Count * 100;
                    ReportProgress("Updating assembly hierarchy information",
                                 $"Assembly {assemblyStats.IndexOf(assembly) + 1}/{assemblyStats.Count}",
                                 progress);
                }
            }

            await Task.CompletedTask; // For async method
        }

        /// <summary>
        /// Counts the indentation level in a string (number of leading spaces/tabs)
        /// </summary>
        private int CountIndentation(string line)
        {
            if (string.IsNullOrEmpty(line))
                return 0;

            int i = 0;
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
            {
                i++;
            }

            // Normalize by assuming 2 spaces per level
            return i / 2;
        }

        /// <summary>
        /// Calculates total component count and maximum depth for an assembly
        /// </summary>
        private (int TotalCount, int MaxDepth) CalculateHierarchyInfo(string assemblyId,
                                                                   List<BOMRelationship> allRelationships,
                                                                   List<AssemblyStats> allStats,
                                                                   HashSet<string> processedAssemblies = null)
        {
            // Initialize the HashSet to track processed assemblies if it hasn't been created yet
            if (processedAssemblies == null)
            {
                processedAssemblies = new HashSet<string>();
            }

            // Check if we've already processed this assembly to prevent infinite recursion
            if (processedAssemblies.Contains(assemblyId))
            {
                // Return zeros to avoid double-counting in circular references
                return (0, 0);
            }

            // Add this assembly to the processed set
            processedAssemblies.Add(assemblyId);

            // Get all direct children
            var directChildren = allRelationships
                .Where(r => r.ParentId == assemblyId && r.RelationType == BOMRelationType.ASSEMBLY.ToString())
                .Select(r => r.ChildId)
                .ToList();

            int totalCount = directChildren.Count;
            int maxDepth = 0;

            // Recursively process each child
            foreach (var childId in directChildren)
            {
                // Get stats for the child
                var childStats = allStats.FirstOrDefault(s => s.PartId == childId);

                // If the child is also an assembly
                if (childStats != null && childStats.IsAssembly)
                {
                    // Recursively calculate its hierarchy info, passing the processed set
                    var (childTotalCount, childMaxDepth) = CalculateHierarchyInfo(childId, allRelationships, allStats, processedAssemblies);

                    // Add the child's components to the total count
                    totalCount += childTotalCount;

                    // Update max depth if this child has a deeper hierarchy
                    maxDepth = Math.Max(maxDepth, childMaxDepth + 1);
                }
            }

            return (totalCount, maxDepth);
        }

        ///// <summary>
        ///// Determines if a file is a drafting based on its characteristics
        ///// </summary>
        //private bool IsDrafting(string filePath)
        //{
        //    // In a real implementation, we might check the file header or other characteristics
        //    // For this example, we'll use a simplistic approach: look for "drawing" or "draft" in the filename
        //    string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
        //    return fileName.Contains("drawing") || fileName.Contains("draft") || fileName.Contains("dwg");
        //}

        ///// <summary>
        ///// Counts the indentation level in a string (number of leading spaces/tabs)
        ///// </summary>
        //private int CountIndentation(string line)
        //{
        //    if (string.IsNullOrEmpty(line))
        //        return 0;

        //    int i = 0;
        //    while (i < line.Length && (line[i] == ' ' || line[i] == '\t'))
        //    {
        //        i++;
        //    }

        //    // Normalize by assuming 2 spaces per level
        //    return i / 2;
        //}

        /// <summary>
        /// Reports progress to subscribers
        /// </summary>
        private void ReportProgress(string operation, string currentItem, double progressPercentage)
        {
            AnalysisProgress?.Invoke(this, new BOMAnalysisProgressEventArgs
            {
                Operation = operation,
                CurrentItem = currentItem,
                ProgressPercentage = progressPercentage
            });
        }

        /// <summary>
        /// Represents the results of a BOM analysis operation
        /// </summary>
        public class AnalysisResults
        {
            /// <summary>
            /// Number of parts successfully analyzed
            /// </summary>
            public int AnalyzedParts { get; set; }

            /// <summary>
            /// Number of assemblies found
            /// </summary>
            public int Assemblies { get; set; }

            /// <summary>
            /// Number of drafting files found
            /// </summary>
            public int Draftings { get; set; }

            /// <summary>
            /// Number of parts that failed to analyze
            /// </summary>
            public int FailedParts { get; set; }

            /// <summary>
            /// Number of parts that were invalid (no file path)
            /// </summary>
            public int InvalidParts { get; set; }

            /// <summary>
            /// List of errors that occurred during analysis
            /// </summary>
            public List<string> Errors { get; set; } = new List<string>();
        }
    }

    /// <summary>
    /// Event arguments for BOM analysis progress reporting
    /// </summary>
    public class BOMAnalysisProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Current operation being performed
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Current item being processed
        /// </summary>
        public string CurrentItem { get; set; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public double ProgressPercentage { get; set; }
    }
}