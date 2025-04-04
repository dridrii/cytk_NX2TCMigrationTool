using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;
using cytk_NX2TCMigrationTool.src.Core.Database.Repositories;
using cytk_NX2TCMigrationTool.src.Core.Settings;
using cytk_NX2TCMigrationTool.src.PLM.NX;

namespace cytk_NX2TCMigrationTool.src.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the BOMBrowser form
    /// </summary>
    public class BOMBrowserViewModel
    {
        private readonly PartRepository _partRepository;
        private readonly BOMRelationshipRepository _bomRepository;
        private readonly AssemblyStatsRepository _statsRepository;
        private readonly SettingsManager _settingsManager;
        private readonly BOMAnalyzerService _bomAnalyzer;
        private readonly Logger _logger;

        // Event for progress reporting
        public event EventHandler<BOMProgressEventArgs> ProgressChanged;

        // Cache for part data
        private Dictionary<string, Part> _partsCache = new Dictionary<string, Part>();
        private Dictionary<string, AssemblyStats> _statsCache = new Dictionary<string, AssemblyStats>();
        private Dictionary<string, List<BOMRelationship>> _parentChildCache = new Dictionary<string, List<BOMRelationship>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public BOMBrowserViewModel(
            PartRepository partRepository,
            BOMRelationshipRepository bomRepository,
            AssemblyStatsRepository statsRepository,
            SettingsManager settingsManager)
        {
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _bomRepository = bomRepository ?? throw new ArgumentNullException(nameof(bomRepository));
            _statsRepository = statsRepository ?? throw new ArgumentNullException(nameof(statsRepository));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _logger = Logger.Instance;

            // Initialize BOM analyzer
            _bomAnalyzer = new BOMAnalyzerService(_partRepository, _bomRepository, _statsRepository, _settingsManager);
            _bomAnalyzer.AnalysisProgress += OnAnalysisProgress;
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Cleanup()
        {
            try
            {
                // Unregister event handlers to prevent memory leaks
                if (_bomAnalyzer != null)
                {
                    _bomAnalyzer.AnalysisProgress -= OnAnalysisProgress;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowserViewModel", $"Error during cleanup: {ex.Message}");
            }
        }

        #region Data Loading

        /// <summary>
        /// Loads all data needed for the BOM browser
        /// </summary>
        public void LoadData()
        {
            try
            {
                // Show loading message
                ReportProgress("Loading data...", true, ProgressBarStyle.Marquee);

                // Clear caches
                _partsCache.Clear();
                _statsCache.Clear();
                _parentChildCache.Clear();

                // Load parts into cache
                var allParts = _partRepository.GetAll();
                foreach (var part in allParts)
                {
                    _partsCache[part.Id] = part;
                }

                // Load assembly stats into cache
                var allStats = _statsRepository.GetAll();
                foreach (var stats in allStats)
                {
                    _statsCache[stats.PartId] = stats;
                }

                // Report completion
                ReportProgress("Ready", false);
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowserViewModel", $"Error loading data: {ex.Message}");
                throw; // Re-throw to be handled by the UI
            }
        }

        /// <summary>
        /// Gets all assemblies sorted by the specified criteria
        /// </summary>
        public List<AssemblyData> GetSortedAssemblies(int sortMethod = 0)
        {
            var assemblies = _statsCache.Values
                .Where(s => s.IsAssembly)
                .ToList();

            if (assemblies.Count == 0)
                return new List<AssemblyData>();

            // Apply sorting
            assemblies = SortAssemblies(assemblies, sortMethod);

            // Convert to AssemblyData objects
            return assemblies
                .Where(stats => _partsCache.ContainsKey(stats.PartId))
                .Select(stats => new AssemblyData
                {
                    Part = _partsCache[stats.PartId],
                    Stats = stats
                })
                .ToList();
        }

        /// <summary>
        /// Gets the children of a specific assembly
        /// </summary>
        public List<ComponentData> GetAssemblyComponents(string assemblyId, bool includeDraftings)
        {
            var children = GetChildRelationships(assemblyId);
            var result = new List<ComponentData>();

            foreach (var relationship in children)
            {
                // Skip drafting relationships if not showing them
                if (!includeDraftings && relationship.RelationType == BOMRelationType.MASTER_MODEL.ToString())
                {
                    continue;
                }

                if (!_partsCache.TryGetValue(relationship.ChildId, out var childPart))
                {
                    continue;
                }

                // Determine type description
                string typeDescription = childPart.Type;
                bool isAssembly = false;
                bool isDrafting = false;

                if (_statsCache.TryGetValue(childPart.Id, out var stats))
                {
                    isAssembly = stats.IsAssembly;
                    isDrafting = stats.IsDrafting;

                    if (isAssembly)
                    {
                        typeDescription = "Assembly";
                    }
                    else if (isDrafting)
                    {
                        typeDescription = "Drafting";
                    }
                }

                // Create component data
                result.Add(new ComponentData
                {
                    Part = childPart,
                    Relationship = relationship,
                    TypeDescription = typeDescription,
                    IsAssembly = isAssembly,
                    IsDrafting = isDrafting,
                    Stats = stats
                });
            }

            return result;
        }

        /// <summary>
        /// Gets child relationships for a part, with caching
        /// </summary>
        private List<BOMRelationship> GetChildRelationships(string parentId)
        {
            try
            {
                if (!_parentChildCache.TryGetValue(parentId, out var relationships))
                {
                    relationships = _bomRepository.GetByParent(parentId)?.ToList() ?? new List<BOMRelationship>();
                    _parentChildCache[parentId] = relationships;
                }

                return relationships;
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowserViewModel", $"Error getting child relationships: {ex.Message}");
                return new List<BOMRelationship>();
            }
        }

        /// <summary>
        /// Sorts the assemblies based on the selected sort method
        /// </summary>
        private List<AssemblyStats> SortAssemblies(List<AssemblyStats> assemblies, int sortMethod)
        {
            switch (sortMethod)
            {
                case 0: // Total Component Count (Largest First)
                    return assemblies.OrderByDescending(s => s.TotalComponentCount).ToList();
                case 1: // Total Component Count (Smallest First)
                    return assemblies.OrderBy(s => s.TotalComponentCount).ToList();
                case 2: // Direct Component Count (Largest First)
                    return assemblies.OrderByDescending(s => s.ComponentCount).ToList();
                case 3: // Direct Component Count (Smallest First)
                    return assemblies.OrderBy(s => s.ComponentCount).ToList();
                case 4: // Assembly Depth (Deepest First)
                    return assemblies.OrderByDescending(s => s.AssemblyDepth).ToList();
                case 5: // Assembly Depth (Shallowest First)
                    return assemblies.OrderBy(s => s.AssemblyDepth).ToList();
                case 6: // Alphabetical (A-Z)
                    return assemblies
                        .Where(s => _partsCache.ContainsKey(s.PartId))
                        .OrderBy(s => _partsCache[s.PartId].Name)
                        .ToList();
                case 7: // Alphabetical (Z-A)
                    return assemblies
                        .Where(s => _partsCache.ContainsKey(s.PartId))
                        .OrderByDescending(s => _partsCache[s.PartId].Name)
                        .ToList();
                default:
                    return assemblies.OrderByDescending(s => s.TotalComponentCount).ToList();
            }
        }

        #endregion

        #region Analysis

        /// <summary>
        /// Analyzes all parts in the database for BOM relationships
        /// </summary>
        public async Task<BOMAnalyzerService.AnalysisResults> AnalyzeAllPartsAsync()
        {
            try
            {
                ReportProgress("Analyzing parts...", true, ProgressBarStyle.Marquee);

                // Run analysis
                var results = await _bomAnalyzer.AnalyzeAllPartsAsync();

                // Reload data after analysis
                LoadData();

                return results;
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowserViewModel", $"Error analyzing parts: {ex.Message}");
                throw; // Re-throw to be handled by the UI
            }
            finally
            {
                ReportProgress("Ready", false);
            }
        }

        /// <summary>
        /// Handler for analysis progress events
        /// </summary>
        private void OnAnalysisProgress(object sender, BOMAnalysisProgressEventArgs e)
        {
            try
            {
                // Update status
                string statusText = $"{e.Operation} - {e.CurrentItem} ({e.ProgressPercentage:F1}%)";

                // Ensure progress value is in valid range (0-100)
                int progressValue = (int)Math.Max(0, Math.Min(100, e.ProgressPercentage));

                ReportProgress(statusText, true, ProgressBarStyle.Blocks, progressValue);
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowserViewModel", $"Error updating progress: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets a color for a visualization bar based on assembly size
        /// </summary>
        public Color GetBarColor(AssemblyStats stats)
        {
            // Scale from light blue to dark blue based on total component count
            int maxComponents = 1000; // Cap for display purposes
            int components = Math.Min(stats.TotalComponentCount, maxComponents);
            int intensity = (int)(255 * (1 - (double)components / maxComponents));
            intensity = Math.Max(0, Math.Min(255, intensity)); // Clamp to valid range

            return Color.FromArgb(255, intensity, intensity, 255);
        }

        /// <summary>
        /// Reports progress to subscribers
        /// </summary>
        private void ReportProgress(string message, bool showProgress = false,
            ProgressBarStyle style = ProgressBarStyle.Blocks, int value = 0)
        {
            ProgressChanged?.Invoke(this, new BOMProgressEventArgs
            {
                Message = message,
                ShowProgress = showProgress,
                ProgressStyle = style,
                ProgressValue = value
            });
        }

        #endregion

        #region Data Classes

        /// <summary>
        /// Data class for assembly items
        /// </summary>
        public class AssemblyData
        {
            public Part Part { get; set; }
            public AssemblyStats Stats { get; set; }

            public override string ToString()
            {
                return $"{Part.Name} ({Stats.ComponentCount} direct, {Stats.TotalComponentCount} total)";
            }
        }

        /// <summary>
        /// Data class for component items
        /// </summary>
        public class ComponentData
        {
            public Part Part { get; set; }
            public BOMRelationship Relationship { get; set; }
            public string TypeDescription { get; set; }
            public bool IsAssembly { get; set; }
            public bool IsDrafting { get; set; }
            public AssemblyStats Stats { get; set; }
        }

        /// <summary>
        /// Event arguments for progress reporting
        /// </summary>
        public class BOMProgressEventArgs : EventArgs
        {
            public string Message { get; set; }
            public bool ShowProgress { get; set; }
            public ProgressBarStyle ProgressStyle { get; set; }
            public int ProgressValue { get; set; }
        }

        #endregion
    }
}