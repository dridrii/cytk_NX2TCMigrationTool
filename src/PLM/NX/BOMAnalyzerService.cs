using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly Logger _logger;
        private readonly string _salt;

        // Event for progress reporting
        public event EventHandler<BOMAnalysisProgressEventArgs> AnalysisProgress;

        // Constructor
        public BOMAnalyzerService(PartRepository partRepository, BOMRelationshipRepository bomRepository,
                                  AssemblyStatsRepository statsRepository, SettingsManager settingsManager)
        {
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _bomRepository = bomRepository ?? throw new ArgumentNullException(nameof(bomRepository));
            _statsRepository = statsRepository ?? throw new ArgumentNullException(nameof(statsRepository));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _logger = Logger.Instance;

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

            // Get the NX installation directory from settings
            string nxInstallPath = _settingsManager.GetSetting("/Settings/NX/InstallPath");

            if (string.IsNullOrEmpty(nxInstallPath))
            {
                throw new Exception("NX installation path not configured in settings");
            }

            // Construct the path to ugpc.exe
            string ugpcPath = Path.Combine(nxInstallPath, "NXBIN", "ugpc.exe");

            if (!File.Exists(ugpcPath))
            {
                throw new FileNotFoundException($"ugpc.exe not found at: {ugpcPath}");
            }

            // Run the ugpc tool to analyze the part structure
            var results = await RunUgpcToolAsync(ugpcPath, part.FilePath);

            // Process the results to extract BOM relationships and stats
            await ProcessUgpcResultsAsync(part, results);
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
            var stats = CalculateAssemblyStats(part, relationships);

            // Check if this part is a drafting
            bool isDrafting = IsDrafting(part.FilePath);
            stats.IsDrafting = isDrafting;

            // Save all relationships to database
            foreach (var relationship in relationships)
            {
                // Check if this is a master model relationship (drafting-to-model)
                if (isDrafting && relationship.RelationType == BOMRelationType.ASSEMBLY.ToString())
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

            // Split output into lines
            string[] lines = ugpcOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Skip if lines count is less than 2 (we need at least command result and a response line)
            if (lines.Length < 2)
            {
                _logger.Debug("BOMAnalyzer", $"Unexpected output format from ugpc.exe for part {parentPart.Name}");
                return relationships;
            }

            // Check if the part has no assembly structure
            // Based on the screenshot, the "no assembly structure" message appears on the second line
            if (lines.Length >= 2 && lines[1].Contains("has no assembly structure"))
            {
                _logger.Debug("BOMAnalyzer", $"Part {parentPart.Name} has no assembly structure");
                return relationships; // Return empty list since this isn't an assembly
            }

            // First line after the command is the part path, so we start from the second line (index 1)
            int position = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                // Skip lines that are empty, contain notes, or are the part itself
                if (string.IsNullOrWhiteSpace(line) ||
                    line.StartsWith("Note:") ||
                    !line.Contains(" x ")) // Components are displayed with quantity as "x N"
                {
                    continue;
                }

                // Parse the component line
                // Format: <path/filename> x <quantity>
                string[] parts = line.Trim().Split(new[] { " x " }, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    _logger.Warning("BOMAnalyzer", $"Unexpected line format in ugpc output: {line}");
                    continue;
                }

                string componentPath = parts[0].Trim();
                string componentFileName = Path.GetFileName(componentPath);
                int quantity;
                if (!int.TryParse(parts[1].Trim(), out quantity))
                {
                    quantity = 1; // Default to 1 if parsing fails
                }

                // Count indentation to determine level in assembly hierarchy
                int indentationLevel = CountIndentation(line);

                // Only process direct children of the parent
                // Note: First-level components have indentation (usually 4 spaces)
                if (indentationLevel != 4) // Adjust this based on your actual output
                {
                    continue;
                }

                // Look up the child part in the database
                var childParts = _partRepository.GetAll()
                    .Where(p => p.FileName != null && p.FileName.Equals(componentFileName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (childParts.Count == 0)
                {
                    _logger.Warning("BOMAnalyzer", $"Child part not found in database: {componentFileName}");
                    continue;
                }

                // Take the first match (should be unique, but handle duplicates)
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
                _logger.Trace("BOMAnalyzer", $"Found relationship: {parentPart.Name} -> {childPart.Name} (Qty: {quantity})");
            }

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

        /// <summary>
        /// Determines if a file is a drafting based on its characteristics
        /// </summary>
        private bool IsDrafting(string filePath)
        {
            // In a real implementation, we might check the file header or other characteristics
            // For this example, we'll use a simplistic approach: look for "drawing" or "draft" in the filename
            string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
            return fileName.Contains("drawing") || fileName.Contains("draft") || fileName.Contains("dwg");
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