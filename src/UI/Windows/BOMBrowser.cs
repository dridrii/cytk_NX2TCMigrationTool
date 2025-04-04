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
using cytk_NX2TCMigrationTool.src.UI.ViewModels;
using static cytk_NX2TCMigrationTool.src.UI.ViewModels.BOMBrowserViewModel;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public partial class BOMBrowser : Form
    {
        private BOMBrowserViewModel _viewModel;
        private Logger _logger;

        // Make _analyzeButton public to enable external triggering of analysis
        public Button AnalyzeButton => _analyzeButton;

        // Constructor
        public BOMBrowser(PartRepository partRepository, BOMRelationshipRepository bomRepository,
                          AssemblyStatsRepository statsRepository, SettingsManager settingsManager)
        {
            _logger = Logger.Instance;

            // Initialize UI components (this will use the designer-generated method)
            InitializeComponent();

            // Create the view model
            _viewModel = new BOMBrowserViewModel(partRepository, bomRepository, statsRepository, settingsManager);
            _viewModel.ProgressChanged += ViewModel_ProgressChanged;

            // Register form events
            this.Load += BOMBrowser_Load;
            this.FormClosing += BOMBrowser_FormClosing;
        }

        #region Form Event Handlers

        /// <summary>
        /// Initialize the form when it loads
        /// </summary>
        private void BOMBrowser_Load(object sender, EventArgs e)
        {
            try
            {
                // Initialize UI elements
                InitializeComponentsTab();
                InitializeHierarchyTab();
                ConfigureEventHandlers();

                // Load data
                _viewModel.LoadData();
                PopulateAssemblyComboBox();
                LoadHierarchyView();
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error initializing form: {ex.Message}");
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Clean up event handlers when form is closing
        /// </summary>
        private void BOMBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Unregister event handlers to prevent memory leaks
                _viewModel.ProgressChanged -= ViewModel_ProgressChanged;
                _viewModel.Cleanup();
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error during cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Handler for ViewModel progress events
        /// </summary>
        private void ViewModel_ProgressChanged(object sender, BOMProgressEventArgs e)
        {
            // Make sure we're on the UI thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<BOMProgressEventArgs>(ViewModel_ProgressChanged), sender, e);
                return;
            }

            try
            {
                // Update status display
                _statusLabel.Text = e.Message;
                _progressBar.Visible = e.ShowProgress;

                if (e.ShowProgress)
                {
                    _progressBar.Style = e.ProgressStyle;
                    if (e.ProgressStyle == ProgressBarStyle.Blocks)
                    {
                        _progressBar.Value = e.ProgressValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error updating progress UI: {ex.Message}");
            }
        }

        #endregion

        #region UI Initialization

        /// <summary>
        /// Initialize components tab elements after form creation
        /// </summary>
        private void InitializeComponentsTab()
        {
            // Configure grid columns
            ConfigureComponentsGridColumns();

            // Set initial values for checkbox
            _showDraftingsCheckBox.Checked = true;
        }

        /// <summary>
        /// Configure columns for the components grid
        /// </summary>
        private void ConfigureComponentsGridColumns()
        {
            // Clear existing columns to avoid duplicates
            _componentsGrid.Columns.Clear();

            // Configure columns for the grid
            _componentsGrid.Columns.Add("ID", "ID");
            _componentsGrid.Columns.Add("Name", "Name");
            _componentsGrid.Columns.Add("Type", "Type");
            _componentsGrid.Columns.Add("RelationType", "Relation Type");
            _componentsGrid.Columns.Add("Quantity", "Quantity");
            _componentsGrid.Columns.Add("InstanceName", "Instance Name");
            _componentsGrid.Columns.Add("Verified", "Verified");
        }

        /// <summary>
        /// Initialize hierarchy tab elements after form creation
        /// </summary>
        private void InitializeHierarchyTab()
        {
            // Ensure sort method combo box is populated and has a selection
            if (_sortMethodComboBox.Items.Count == 0)
            {
                _sortMethodComboBox.Items.AddRange(new object[] {
                    "Total Component Count (Largest First)",
                    "Total Component Count (Smallest First)",
                    "Direct Component Count (Largest First)",
                    "Direct Component Count (Smallest First)",
                    "Assembly Depth (Deepest First)",
                    "Assembly Depth (Shallowest First)",
                    "Alphabetical (A-Z)",
                    "Alphabetical (Z-A)"
                });
            }

            if (_sortMethodComboBox.SelectedIndex < 0)
            {
                _sortMethodComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Configure event handlers for UI elements
        /// </summary>
        private void ConfigureEventHandlers()
        {
            // Assembly selection
            _topLevelAssemblyComboBox.SelectedIndexChanged += TopLevelAssemblyComboBox_SelectedIndexChanged;

            // Tree view
            _assemblyTreeView.AfterSelect += AssemblyTreeView_AfterSelect;

            // Checkbox
            _showDraftingsCheckBox.CheckedChanged += ShowDraftingsCheckBox_CheckedChanged;

            // Sort method
            _sortMethodComboBox.SelectedIndexChanged += SortMethodComboBox_SelectedIndexChanged;

            // Buttons
            _analyzeButton.Click += AnalyzeButton_Click;
            _closeButton.Click += CloseButton_Click;
        }

        #endregion

        #region UI Event Handlers

        private void TopLevelAssemblyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_topLevelAssemblyComboBox.SelectedItem is AssemblyData selectedItem)
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

        private void ShowDraftingsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Reload current assembly tree
                if (_topLevelAssemblyComboBox.SelectedItem is AssemblyData selectedItem)
                {
                    LoadAssemblyTree(selectedItem.Part.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("BOMBrowser", $"Error handling show draftings change: {ex.Message}");
            }
        }

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

                // Run analysis using view model
                var results = await _viewModel.AnalyzeAllPartsAsync();

                // Show results
                MessageBox.Show($"Analysis complete!\n\n" +
                              $"Parts analyzed: {results.AnalyzedParts}\n" +
                              $"Assemblies found: {results.Assemblies}\n" +
                              $"Drafting files found: {results.Draftings}\n" +
                              $"Failed parts: {results.FailedParts}\n" +
                              $"Invalid parts: {results.InvalidParts}\n" +
                              $"Errors: {results.Errors.Count}",
                              "Analysis Results", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reload UI after analysis
                PopulateAssemblyComboBox();
                LoadHierarchyView();
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
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

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
                        if (_topLevelAssemblyComboBox.Items[i] is AssemblyData item &&
                            item.Part.Id == partId)
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

        #endregion

        #region UI Update Methods

        /// <summary>
        /// Populates the assembly dropdown with available assemblies
        /// </summary>
        private void PopulateAssemblyComboBox()
        {
            // Remember selected item
            object selectedItem = _topLevelAssemblyComboBox.SelectedItem;
            string selectedId = selectedItem is AssemblyData selectedAssembly ? selectedAssembly.Part.Id : null;

            // Clear and repopulate
            _topLevelAssemblyComboBox.Items.Clear();

            var assemblies = _viewModel.GetSortedAssemblies();
            foreach (var assemblyItem in assemblies)
            {
                _topLevelAssemblyComboBox.Items.Add(assemblyItem);
            }

            // Try to reselect previous item or select first item
            if (!string.IsNullOrEmpty(selectedId))
            {
                for (int i = 0; i < _topLevelAssemblyComboBox.Items.Count; i++)
                {
                    if (_topLevelAssemblyComboBox.Items[i] is AssemblyData itemAssembly &&
                        itemAssembly.Part.Id == selectedId)
                    {
                        _topLevelAssemblyComboBox.SelectedIndex = i;
                        return;
                    }
                }
            }

            // If no previous selection or it's not found, select first item
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

        /// <summary>
        /// Loads the assembly tree view for a specific assembly
        /// </summary>
        private void LoadAssemblyTree(string assemblyId)
        {
            try
            {
                _assemblyTreeView.Nodes.Clear();

                // Get components from view model
                var components = _viewModel.GetAssemblyComponents(assemblyId, true);
                var rootComponent = components.FirstOrDefault(c => c.Part.Id == assemblyId);

                if (rootComponent == null)
                {
                    // Root component might not be in the children, get it directly
                    var assemblies = _viewModel.GetSortedAssemblies();
                    rootComponent = assemblies
                        .Where(a => a.Part.Id == assemblyId)
                        .Select(a => new ComponentData
                        {
                            Part = a.Part,
                            Stats = a.Stats,
                            IsAssembly = true
                        })
                        .FirstOrDefault();
                }

                if (rootComponent == null)
                    return;

                // Create the root node
                var rootNode = CreateTreeNode(rootComponent);
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

        private void PopulateChildNodes(TreeNode parentNode, string assemblyId)
        {
            // Add a HashSet to track already processed assemblies
            HashSet<string> processedAssemblies = new HashSet<string>();
            PopulateChildNodesInternal(parentNode, assemblyId, processedAssemblies);
        }

        private void PopulateChildNodesInternal(TreeNode parentNode, string assemblyId, HashSet<string> processedAssemblies)
        {
            // Check if we've already processed this assembly to prevent infinite recursion
            if (processedAssemblies.Contains(assemblyId))
            {
                // This assembly has already been processed, so we'll mark it as a circular reference
                var circularRefNode = new TreeNode("Circular reference detected");
                circularRefNode.ForeColor = Color.Red;
                parentNode.Nodes.Add(circularRefNode);
                return;
            }

            // Add this assembly to the processed set
            processedAssemblies.Add(assemblyId);

            // Get children for this assembly
            bool showDraftings = _showDraftingsCheckBox.Checked;
            var components = _viewModel.GetAssemblyComponents(assemblyId, showDraftings);

            foreach (var component in components)
            {
                // Create node for this child
                var childNode = CreateTreeNode(component);
                parentNode.Nodes.Add(childNode);

                // If this child is also an assembly, populate its children
                if (component.IsAssembly)
                {
                    PopulateChildNodesInternal(childNode, component.Part.Id, processedAssemblies);
                }
            }
        }

        /// <summary>
        /// Creates a tree node for a component
        /// </summary>
        private TreeNode CreateTreeNode(ComponentData component)
        {
            string nodeText = component.Part.Name;

            // Add relationship info if available
            if (component.Relationship != null)
            {
                if (component.Relationship.Quantity > 1)
                {
                    nodeText += $" ({component.Relationship.Quantity}x)";
                }

                if (component.Relationship.RelationType == BOMRelationType.MASTER_MODEL.ToString())
                {
                    nodeText += " [Drafting]";
                }
            }

            // Add assembly info if available
            if (component.Stats != null)
            {
                if (component.IsAssembly)
                {
                    nodeText += $" - Assembly ({component.Stats.ComponentCount} direct, {component.Stats.TotalComponentCount} total)";
                }
                else if (component.IsDrafting)
                {
                    nodeText += " - Drafting";
                }
            }

            var node = new TreeNode(nodeText);
            node.Tag = new NodeData { PartId = component.Part.Id, Relationship = component.Relationship };

            // Set icon based on part type
            if (component.IsDrafting)
            {
                node.ForeColor = Color.Blue;
            }
            else if (component.IsAssembly)
            {
                node.ForeColor = Color.Green;
            }

            return node;
        }

        /// <summary>
        /// Loads component details for a selected assembly
        /// </summary>
        private void LoadComponentDetails(string assemblyId)
        {
            try
            {
                _componentsGrid.Rows.Clear();

                // Get components from view model
                bool showDraftings = _showDraftingsCheckBox.Checked;
                var components = _viewModel.GetAssemblyComponents(assemblyId, showDraftings);

                foreach (var component in components)
                {
                    // Add row to grid
                    _componentsGrid.Rows.Add(
                        component.Part.Id,
                        component.Part.Name,
                        component.TypeDescription,
                        component.Relationship.RelationType,
                        component.Relationship.Quantity,
                        component.Relationship.InstanceName,
                        component.Relationship.Verified ? "Yes" : "No"
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
                var assemblies = _viewModel.GetSortedAssemblies(sortMethod);

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

                // Create the visualization panel
                Panel visualizationPanel = new Panel();
                visualizationPanel.Location = new Point(10, 50);
                visualizationPanel.Size = new Size(760, 300);
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
                    // Create assembly label
                    Label assemblyLabel = CreateAssemblyLabel(assembly, yPos);
                    visualizationPanel.Controls.Add(assemblyLabel);

                    // Create bar visualization
                    int barWidth = Math.Max(1, (int)(Math.Min(assembly.Stats.TotalComponentCount, 1000) * 0.5));
                    Panel barPanel = new Panel();
                    barPanel.Location = new Point(10, yPos + 20);
                    barPanel.Size = new Size(barWidth, 15);
                    barPanel.BackColor = _viewModel.GetBarColor(assembly.Stats);
                    visualizationPanel.Controls.Add(barPanel);

                    // Create count label next to bar
                    Label countLabel = new Label();
                    countLabel.Text = assembly.Stats.TotalComponentCount.ToString();
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
        /// Creates an assembly label for the hierarchy view
        /// </summary>
        private Label CreateAssemblyLabel(AssemblyData assembly, int yPos)
        {
            Label assemblyLabel = new Label();
            assemblyLabel.Text = $"{assembly.Part.Name} - {assembly.Stats.ComponentCount} direct components, " +
                               $"{assembly.Stats.TotalComponentCount} total components, " +
                               $"Depth: {assembly.Stats.AssemblyDepth}";
            assemblyLabel.Location = new Point(10, yPos);
            assemblyLabel.AutoSize = true;
            assemblyLabel.Tag = assembly.Part.Id;
            assemblyLabel.ForeColor = Color.Green;
            assemblyLabel.Font = new Font(assemblyLabel.Font, FontStyle.Bold);
            assemblyLabel.Cursor = Cursors.Hand;
            assemblyLabel.Click += AssemblyLabel_Click;

            return assemblyLabel;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Helper class for tree node data
        /// </summary>
        private class NodeData
        {
            public string PartId { get; set; }
            public BOMRelationship Relationship { get; set; }
        }

        #endregion
    }
}