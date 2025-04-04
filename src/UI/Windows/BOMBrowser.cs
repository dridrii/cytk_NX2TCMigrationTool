using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;
using cytk_NX2TCMigrationTool.src.Core.Database.Repositories;
using cytk_NX2TCMigrationTool.src.Core.Settings;
using cytk_NX2TCMigrationTool.src.PLM.NX;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public partial class BOMBrowser : Form
    {
        private readonly PartRepository _partRepository;
        private readonly BOMRelationshipRepository _bomRepository;
        private readonly AssemblyStatsRepository _statsRepository;
        private readonly SettingsManager _settingsManager;
        private readonly BOMAnalyzerService _bomAnalyzer;
        private readonly Logger _logger;

        // Make _analyzeButton public to enable external triggering of analysis
        public Button AnalyzeButton => _analyzeButton;

        // Cache for part data
        private Dictionary<string, Part> _partsCache = new Dictionary<string, Part>();
        private Dictionary<string, AssemblyStats> _statsCache = new Dictionary<string, AssemblyStats>();
        private Dictionary<string, List<BOMRelationship>> _parentChildCache = new Dictionary<string, List<BOMRelationship>>();

        // Constructor
        public BOMBrowser(PartRepository partRepository, BOMRelationshipRepository bomRepository,
                          AssemblyStatsRepository statsRepository, SettingsManager settingsManager)
        {
            // Initialize UI components first (to create the controls)
            InitializeComponent();

            // Validate parameters
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _bomRepository = bomRepository ?? throw new ArgumentNullException(nameof(bomRepository));
            _statsRepository = statsRepository ?? throw new ArgumentNullException(nameof(statsRepository));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _logger = Logger.Instance;

            // Initialize BOM analyzer
            _bomAnalyzer = new BOMAnalyzerService(_partRepository, _bomRepository, _statsRepository, _settingsManager);
            _bomAnalyzer.AnalysisProgress += OnAnalysisProgress;

            // Register form closing handler to clean up event handlers
            this.FormClosing += BOMBrowser_FormClosing;

            // Initialize data
            LoadData();
        }

        /// <summary>
        /// Clean up event handlers when form is closing
        /// </summary>
        private void BOMBrowser_FormClosing(object sender, FormClosingEventArgs e)
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
                _logger.Error("BOMBrowser", $"Error during cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads initial data for the BOM browser
        /// </summary>
        private void LoadData()
        {
            try
            {
                // Show loading message and progress
                _statusLabel.Text = "Loading data...";
                _progressBar.Visible = true;
                _progressBar.Style = ProgressBarStyle.Marquee;

                // Clear existing data
                _assemblyTreeView.Nodes.Clear();
                _componentsGrid.Rows.Clear();
                _topLevelAssemblyComboBox.Items.Clear();

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

                // Populate top-level assembly dropdown
                var assemblies = _statsCache.Values
                    .Where(s => s.IsAssembly)
                    .OrderByDescending(s => s.TotalComponentCount)
                    .ToList();

                foreach (var assembly in assemblies)
                {
                    if (_partsCache.TryGetValue(assembly.PartId, out var part))
                    {
                        _topLevelAssemblyComboBox.Items.Add(new AssemblyListItem(part, assembly));
                    }
                }

                // Load the hierarchy tab data
                LoadHierarchyView();

                // If there's a top-level assembly, select it
                if (_topLevelAssemblyComboBox.Items.Count > 0)
                {
                    _topLevelAssemblyComboBox.SelectedIndex = 0;
                }
                else
                {
                    // No assemblies found - clear tree view
                    _assemblyTreeView.Nodes.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error loading BOM data: {ex.Message}");
                MessageBox.Show($"Error loading BOM data: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Hide progress
                _progressBar.Visible = false;
                _statusLabel.Text = "Ready";
            }
        }

        /// <summary>
        /// Handles the selection change in the top-level assembly combo box
        /// </summary>
        private void TopLevelAssemblyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_topLevelAssemblyComboBox.SelectedItem is AssemblyListItem selectedItem)
                {
                    LoadAssemblyTree(selectedItem.Part.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error handling assembly selection: {ex.Message}");
                MessageBox.Show($"Error loading assembly: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads the assembly tree view for a specific assembly
        /// </summary>
        private void LoadAssemblyTree(string assemblyId)
        {
            try
            {
                _assemblyTreeView.Nodes.Clear();

                if (!_partsCache.TryGetValue(assemblyId, out var part))
                {
                    return;
                }

                // Create the root node
                var rootNode = CreateTreeNode(part);
                _assemblyTreeView.Nodes.Add(rootNode);

                // Populate child nodes recursively
                PopulateChildNodes(rootNode, assemblyId);

                // Expand the root node
                rootNode.Expand();
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error loading assembly tree: {ex.Message}");
                MessageBox.Show($"Error loading assembly tree: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Recursively populates child nodes for an assembly
        /// </summary>
        private void PopulateChildNodes(TreeNode parentNode, string assemblyId)
        {
            // Get children for this assembly
            var children = GetChildRelationships(assemblyId);
            bool showDraftings = _showDraftingsCheckBox.Checked;

            foreach (var relationship in children)
            {
                // Skip drafting relationships if not showing them
                if (!showDraftings && relationship.RelationType == BOMRelationType.MASTER_MODEL.ToString())
                {
                    continue;
                }

                if (!_partsCache.TryGetValue(relationship.ChildId, out var childPart))
                {
                    continue;
                }

                // Create node for this child
                var childNode = CreateTreeNode(childPart, relationship);
                parentNode.Nodes.Add(childNode);

                // If this child is also an assembly, populate its children
                if (_statsCache.TryGetValue(childPart.Id, out var stats) && stats.IsAssembly)
                {
                    PopulateChildNodes(childNode, childPart.Id);
                }
            }
        }

        /// <summary>
        /// Creates a tree node for a part
        /// </summary>
        private TreeNode CreateTreeNode(Part part, BOMRelationship relationship = null)
        {
            string nodeText = part.Name;

            // Add relationship info if available
            if (relationship != null)
            {
                if (relationship.Quantity > 1)
                {
                    nodeText += $" ({relationship.Quantity}x)";
                }

                if (relationship.RelationType == BOMRelationType.MASTER_MODEL.ToString())
                {
                    nodeText += " [Drafting]";
                }
            }

            // Add assembly info if available
            if (_statsCache.TryGetValue(part.Id, out var stats))
            {
                if (stats.IsAssembly)
                {
                    nodeText += $" - Assembly ({stats.ComponentCount} direct, {stats.TotalComponentCount} total)";
                }
                else if (stats.IsDrafting)
                {
                    nodeText += " - Drafting";
                }
            }

            var node = new TreeNode(nodeText);
            node.Tag = new NodeData { PartId = part.Id, Relationship = relationship };

            // Set icon based on part type
            if (_statsCache.TryGetValue(part.Id, out var nodeStats))
            {
                if (nodeStats.IsDrafting)
                {
                    node.ForeColor = Color.Blue;
                }
                else if (nodeStats.IsAssembly)
                {
                    node.ForeColor = Color.Green;
                }
            }

            return node;
        }

        /// <summary>
        /// Handles the selection of a node in the assembly tree
        /// </summary>
        private void AssemblyTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (e.Node?.Tag is NodeData nodeData)
                {
                    LoadComponentDetails(nodeData.PartId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error handling tree node selection: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads component details for a selected assembly
        /// </summary>
        private void LoadComponentDetails(string assemblyId)
        {
            try
            {
                _componentsGrid.Rows.Clear();

                var children = GetChildRelationships(assemblyId);
                bool showDraftings = _showDraftingsCheckBox.Checked;

                foreach (var relationship in children)
                {
                    // Skip drafting relationships if not showing them
                    if (!showDraftings && relationship.RelationType == BOMRelationType.MASTER_MODEL.ToString())
                    {
                        continue;
                    }

                    if (!_partsCache.TryGetValue(relationship.ChildId, out var childPart))
                    {
                        continue;
                    }

                    // Determine type description
                    string typeDescription = childPart.Type;
                    if (_statsCache.TryGetValue(childPart.Id, out var stats))
                    {
                        if (stats.IsAssembly)
                        {
                            typeDescription = "Assembly";
                        }
                        else if (stats.IsDrafting)
                        {
                            typeDescription = "Drafting";
                        }
                    }

                    // Add row to grid
                    _componentsGrid.Rows.Add(
                        childPart.Id,
                        childPart.Name,
                        typeDescription,
                        relationship.RelationType,
                        relationship.Quantity,
                        relationship.InstanceName,
                        relationship.Verified ? "Yes" : "No"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error loading component details: {ex.Message}");
                MessageBox.Show($"Error loading component details: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads the hierarchy visualization based on the selected sort method
        /// </summary>
        private void LoadHierarchyView()
        {
            try
            {
                // Clear existing controls
                foreach (Control control in _hierarchyPanel.Controls)
                {
                    if (control != _sortMethodLabel && control != _sortMethodComboBox)
                    {
                        control.Dispose();
                    }
                }

                // Get sorting method
                int sortMethod = _sortMethodComboBox.SelectedIndex;
                if (sortMethod < 0) sortMethod = 0; // Default to first option if none selected

                // Get assemblies
                var assemblies = _statsCache.Values.Where(s => s.IsAssembly).ToList();
                if (assemblies.Count == 0)
                {
                    // No assemblies found
                    Label noDataLabel = new Label();
                    noDataLabel.Text = "No assembly data available. Please run BOM Analysis first.";
                    noDataLabel.Location = new Point(10, 50);
                    noDataLabel.AutoSize = true;
                    _hierarchyPanel.Controls.Add(noDataLabel);
                    return;
                }

                // Apply sorting
                switch (sortMethod)
                {
                    case 0: // Total Component Count (Largest First)
                        assemblies = assemblies.OrderByDescending(s => s.TotalComponentCount).ToList();
                        break;
                    case 1: // Total Component Count (Smallest First)
                        assemblies = assemblies.OrderBy(s => s.TotalComponentCount).ToList();
                        break;
                    case 2: // Direct Component Count (Largest First)
                        assemblies = assemblies.OrderByDescending(s => s.ComponentCount).ToList();
                        break;
                    case 3: // Direct Component Count (Smallest First)
                        assemblies = assemblies.OrderBy(s => s.ComponentCount).ToList();
                        break;
                    case 4: // Assembly Depth (Deepest First)
                        assemblies = assemblies.OrderByDescending(s => s.AssemblyDepth).ToList();
                        break;
                    case 5: // Assembly Depth (Shallowest First)
                        assemblies = assemblies.OrderBy(s => s.AssemblyDepth).ToList();
                        break;
                    case 6: // Alphabetical (A-Z)
                        assemblies = assemblies
                            .Where(s => _partsCache.ContainsKey(s.PartId))
                            .OrderBy(s => _partsCache[s.PartId].Name)
                            .ToList();
                        break;
                    case 7: // Alphabetical (Z-A)
                        assemblies = assemblies
                            .Where(s => _partsCache.ContainsKey(s.PartId))
                            .OrderByDescending(s => _partsCache[s.PartId].Name)
                            .ToList();
                        break;
                }

                // Create the visualization panel
                Panel visualizationPanel = new Panel();
                visualizationPanel.Location = new Point(10, 50);
                visualizationPanel.Size = new Size(960, 525);
                visualizationPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                visualizationPanel.AutoScroll = true;
                _hierarchyPanel.Controls.Add(visualizationPanel);

                // Create header
                Label headerLabel = new Label();
                headerLabel.Text = "Assembly Hierarchy (Sorted by " + _sortMethodComboBox.Text + ")";
                headerLabel.Font = new Font(headerLabel.Font, FontStyle.Bold);
                headerLabel.Location = new Point(0, 0);
                headerLabel.AutoSize = true;
                visualizationPanel.Controls.Add(headerLabel);

                // Create visualization
                int yPos = 30;
                foreach (var assembly in assemblies)
                {
                    if (!_partsCache.TryGetValue(assembly.PartId, out var part))
                    {
                        continue;
                    }

                    // Create assembly label
                    Label assemblyLabel = new Label();
                    assemblyLabel.Text = $"{part.Name} - {assembly.ComponentCount} direct components, " +
                                        $"{assembly.TotalComponentCount} total components, " +
                                        $"Depth: {assembly.AssemblyDepth}";
                    assemblyLabel.Location = new Point(10, yPos);
                    assemblyLabel.AutoSize = true;
                    assemblyLabel.Tag = part.Id;
                    assemblyLabel.ForeColor = Color.Green;
                    assemblyLabel.Font = new Font(assemblyLabel.Font, FontStyle.Bold);
                    assemblyLabel.Cursor = Cursors.Hand;
                    assemblyLabel.Click += AssemblyLabel_Click;
                    visualizationPanel.Controls.Add(assemblyLabel);

                    // Create bar visualization
                    int barWidth = Math.Max(1, (int)(Math.Min(assembly.TotalComponentCount, 1000) * 0.5));
                    Panel barPanel = new Panel();
                    barPanel.Location = new Point(10, yPos + 20);
                    barPanel.Size = new Size(barWidth, 15);
                    barPanel.BackColor = GetBarColor(assembly);
                    visualizationPanel.Controls.Add(barPanel);

                    // Create count label next to bar
                    Label countLabel = new Label();
                    countLabel.Text = assembly.TotalComponentCount.ToString();
                    countLabel.Location = new Point(barWidth + 15, yPos + 20);
                    countLabel.AutoSize = true;
                    visualizationPanel.Controls.Add(countLabel);

                    yPos += 50;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error loading hierarchy view: {ex.Message}");
                MessageBox.Show($"Error loading hierarchy view: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets a color for a bar based on assembly size
        /// </summary>
        private Color GetBarColor(AssemblyStats stats)
        {
            // Scale from light blue to dark blue based on total component count
            int maxComponents = 1000; // Cap for display purposes
            int components = Math.Min(stats.TotalComponentCount, maxComponents);
            int intensity = (int)(255 * (1 - (double)components / maxComponents));
            intensity = Math.Max(0, Math.Min(255, intensity)); // Clamp to valid range

            return Color.FromArgb(255, intensity, intensity, 255);
        }

        /// <summary>
        /// Handles click on an assembly label in the hierarchy view
        /// </summary>
        private void AssemblyLabel_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is Label label && label.Tag is string partId)
                {
                    // Switch to the Components tab
                    _tabControl.SelectedTab = _componentTabPage;

                    // Find and select the assembly in the combobox
                    for (int i = 0; i < _topLevelAssemblyComboBox.Items.Count; i++)
                    {
                        if (_topLevelAssemblyComboBox.Items[i] is AssemblyListItem item && item.Part.Id == partId)
                        {
                            _topLevelAssemblyComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error handling assembly label click: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles change to sort method dropdown
        /// </summary>
        private void SortMethodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LoadHierarchyView();
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error handling sort method change: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles change to "Show Draftings" checkbox
        /// </summary>
        private void ShowDraftingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Reload current assembly tree
                if (_topLevelAssemblyComboBox.SelectedItem is AssemblyListItem selectedItem)
                {
                    LoadAssemblyTree(selectedItem.Part.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error handling show draftings change: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Analyze button click
        /// </summary>
        private async void AnalyzeButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Confirm analysis
                if (MessageBox.Show("This will analyze all parts in the database for BOM relationships. " +
                                  "This may take a while. Do you want to continue?", "Confirm Analysis",
                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                // Disable UI during analysis
                _analyzeButton.Enabled = false;
                _progressBar.Visible = true;
                _progressBar.Style = ProgressBarStyle.Marquee;
                _statusLabel.Text = "Analyzing parts...";

                // Run analysis
                var results = await _bomAnalyzer.AnalyzeAllPartsAsync();

                // Show results
                MessageBox.Show($"Analysis complete!\n\n" +
                              $"Parts analyzed: {results.AnalyzedParts}\n" +
                              $"Assemblies found: {results.Assemblies}\n" +
                              $"Drafting files found: {results.Draftings}\n" +
                              $"Failed parts: {results.FailedParts}\n" +
                              $"Invalid parts: {results.InvalidParts}\n" +
                              $"Errors: {results.Errors.Count}",
                              "Analysis Results", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reload data
                LoadData();
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error analyzing parts: {ex.Message}");
                MessageBox.Show($"Error analyzing parts: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable UI
                _analyzeButton.Enabled = true;
                _progressBar.Visible = false;
                _statusLabel.Text = "Ready";
            }
        }

        /// <summary>
        /// Handles progress updates from the BOM analyzer
        /// </summary>
        private void OnAnalysisProgress(object sender, BOMAnalysisProgressEventArgs e)
        {
            // Make sure we're on the UI thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<BOMAnalysisProgressEventArgs>(OnAnalysisProgress), sender, e);
                return;
            }

            try
            {
                // Update status
                _statusLabel.Text = $"{e.Operation} - {e.CurrentItem} ({e.ProgressPercentage:F1}%)";

                // Update progress bar
                if (_progressBar.Style == ProgressBarStyle.Marquee)
                {
                    _progressBar.Style = ProgressBarStyle.Blocks;
                }

                // Ensure progress value is in valid range (0-100)
                int progressValue = (int)Math.Max(0, Math.Min(100, e.ProgressPercentage));
                _progressBar.Value = progressValue;
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error updating progress: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles close button click
        /// </summary>
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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
                _logger.Error("BOMBrowser", $"Error getting child relationships: {ex.Message}");
                return new List<BOMRelationship>();
            }
        }

        /// <summary>
        /// Helper class for tree node data
        /// </summary>
        private class NodeData
        {
            public string PartId { get; set; }
            public BOMRelationship Relationship { get; set; }
        }

        /// <summary>
        /// Helper class for assembly list items
        /// </summary>
        private class AssemblyListItem
        {
            public Part Part { get; }
            public AssemblyStats Stats { get; }

            public AssemblyListItem(Part part, AssemblyStats stats)
            {
                Part = part;
                Stats = stats;
            }

            public override string ToString()
            {
                return $"{Part.Name} ({Stats.ComponentCount} direct, {Stats.TotalComponentCount} total)";
            }
        }
    }
}