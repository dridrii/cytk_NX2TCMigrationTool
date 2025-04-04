namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    partial class BOMBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // UI Controls
        private System.Windows.Forms.SplitContainer _mainSplitContainer;
        private System.Windows.Forms.TreeView _assemblyTreeView;
        private System.Windows.Forms.DataGridView _componentsGrid;
        private System.Windows.Forms.Button _analyzeButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.ComboBox _topLevelAssemblyComboBox;
        private System.Windows.Forms.Label _topLevelAssemblyLabel;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.TabControl _tabControl;
        private System.Windows.Forms.TabPage _componentTabPage;
        private System.Windows.Forms.TabPage _hierarchyTabPage;
        private System.Windows.Forms.Panel _hierarchyPanel;
        private System.Windows.Forms.CheckBox _showDraftingsCheckBox;
        private System.Windows.Forms.RadioButton _largestFirstRadioButton;
        private System.Windows.Forms.RadioButton _smallestFirstRadioButton;
        private System.Windows.Forms.RadioButton _alphabeticalRadioButton;
        private System.Windows.Forms.ComboBox _sortMethodComboBox;
        private System.Windows.Forms.Label _sortMethodLabel;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.Text = "BOM Browser";
            this.Size = new System.Drawing.Size(1000, 700);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // Create tab control
            _tabControl = new System.Windows.Forms.TabControl();
            _tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(_tabControl);

            // Create component tab
            _componentTabPage = new System.Windows.Forms.TabPage("Components");
            _tabControl.TabPages.Add(_componentTabPage);
            InitializeComponentTab();

            // Create hierarchy tab
            _hierarchyTabPage = new System.Windows.Forms.TabPage("Assembly Hierarchy");
            _tabControl.TabPages.Add(_hierarchyTabPage);
            InitializeHierarchyTab();

            // Create status bar
            _statusLabel = new System.Windows.Forms.Label();
            _statusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            _statusLabel.Text = "Ready";
            _statusLabel.Height = 20;
            this.Controls.Add(_statusLabel);

            // Create progress bar
            _progressBar = new System.Windows.Forms.ProgressBar();
            _progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            _progressBar.Visible = false;
            this.Controls.Add(_progressBar);

            // Create close button
            _closeButton = new System.Windows.Forms.Button();
            _closeButton.Text = "Close";
            _closeButton.Location = new System.Drawing.Point(900, 630);
            _closeButton.Size = new System.Drawing.Size(75, 23);
            _closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _closeButton.Click += new System.EventHandler(CloseButton_Click);
            this.Controls.Add(_closeButton);

            // Create analyze button
            _analyzeButton = new System.Windows.Forms.Button();
            _analyzeButton.Text = "Analyze All Parts";
            _analyzeButton.Location = new System.Drawing.Point(800, 630);
            _analyzeButton.Size = new System.Drawing.Size(95, 23);
            _analyzeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _analyzeButton.Click += new System.EventHandler(AnalyzeButton_Click);
            this.Controls.Add(_analyzeButton);

            // Set as the form's cancel button
            this.CancelButton = _closeButton;
        }

        private void InitializeComponentTab()
        {
            // Create main split container
            _mainSplitContainer = new System.Windows.Forms.SplitContainer();
            _mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            _mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
            _mainSplitContainer.SplitterDistance = 300;
            _componentTabPage.Controls.Add(_mainSplitContainer);

            // Top panel - Assembly tree view
            System.Windows.Forms.Panel topPanel = new System.Windows.Forms.Panel();
            topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _mainSplitContainer.Panel1.Controls.Add(topPanel);

            // Assembly selection
            _topLevelAssemblyLabel = new System.Windows.Forms.Label();
            _topLevelAssemblyLabel.Text = "Top-level Assembly:";
            _topLevelAssemblyLabel.Location = new System.Drawing.Point(10, 15);
            _topLevelAssemblyLabel.AutoSize = true;
            topPanel.Controls.Add(_topLevelAssemblyLabel);

            _topLevelAssemblyComboBox = new System.Windows.Forms.ComboBox();
            _topLevelAssemblyComboBox.Location = new System.Drawing.Point(140, 12);
            _topLevelAssemblyComboBox.Width = 300;
            _topLevelAssemblyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _topLevelAssemblyComboBox.SelectedIndexChanged += new System.EventHandler(TopLevelAssemblyComboBox_SelectedIndexChanged);
            topPanel.Controls.Add(_topLevelAssemblyComboBox);

            // Create tree view for assembly structure
            _assemblyTreeView = new System.Windows.Forms.TreeView();
            _assemblyTreeView.Location = new System.Drawing.Point(10, 45);
            _assemblyTreeView.Size = new System.Drawing.Size(450, 245);
            _assemblyTreeView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _assemblyTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(AssemblyTreeView_AfterSelect);
            _assemblyTreeView.HideSelection = false;
            topPanel.Controls.Add(_assemblyTreeView);

            // Add "Show Draftings" checkbox
            _showDraftingsCheckBox = new System.Windows.Forms.CheckBox();
            _showDraftingsCheckBox.Text = "Show Draftings";
            _showDraftingsCheckBox.Location = new System.Drawing.Point(500, 12);
            _showDraftingsCheckBox.AutoSize = true;
            _showDraftingsCheckBox.Checked = true;
            _showDraftingsCheckBox.CheckedChanged += new System.EventHandler(ShowDraftingsCheckBox_CheckedChanged);
            topPanel.Controls.Add(_showDraftingsCheckBox);

            // Bottom panel - Component details grid
            System.Windows.Forms.Panel bottomPanel = new System.Windows.Forms.Panel();
            bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _mainSplitContainer.Panel2.Controls.Add(bottomPanel);

            // Add label
            System.Windows.Forms.Label componentsLabel = new System.Windows.Forms.Label();
            componentsLabel.Text = "Components:";
            componentsLabel.Location = new System.Drawing.Point(10, 10);
            componentsLabel.AutoSize = true;
            bottomPanel.Controls.Add(componentsLabel);

            // Create data grid for components
            _componentsGrid = new System.Windows.Forms.DataGridView();
            _componentsGrid.Location = new System.Drawing.Point(10, 35);
            _componentsGrid.Size = new System.Drawing.Size(950, 225);
            _componentsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _componentsGrid.AllowUserToAddRows = false;
            _componentsGrid.AllowUserToDeleteRows = false;
            _componentsGrid.ReadOnly = true;
            _componentsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            _componentsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            _componentsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            bottomPanel.Controls.Add(_componentsGrid);

            // Configure columns for the grid
            System.Windows.Forms.DataGridViewTextBoxColumn idColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            idColumn.HeaderText = "ID";
            idColumn.Name = "ID";
            _componentsGrid.Columns.Add(idColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            nameColumn.HeaderText = "Name";
            nameColumn.Name = "Name";
            _componentsGrid.Columns.Add(nameColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn typeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            typeColumn.HeaderText = "Type";
            typeColumn.Name = "Type";
            _componentsGrid.Columns.Add(typeColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn relationTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            relationTypeColumn.HeaderText = "Relation Type";
            relationTypeColumn.Name = "RelationType";
            _componentsGrid.Columns.Add(relationTypeColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn quantityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            quantityColumn.HeaderText = "Quantity";
            quantityColumn.Name = "Quantity";
            _componentsGrid.Columns.Add(quantityColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn instanceNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            instanceNameColumn.HeaderText = "Instance Name";
            instanceNameColumn.Name = "InstanceName";
            _componentsGrid.Columns.Add(instanceNameColumn);

            System.Windows.Forms.DataGridViewTextBoxColumn verifiedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            verifiedColumn.HeaderText = "Verified";
            verifiedColumn.Name = "Verified";
            _componentsGrid.Columns.Add(verifiedColumn);
        }

        private void InitializeHierarchyTab()
        {
            // Create panel for hierarchy view
            _hierarchyPanel = new System.Windows.Forms.Panel();
            _hierarchyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            _hierarchyTabPage.Controls.Add(_hierarchyPanel);

            // Add sort method label and combo box
            _sortMethodLabel = new System.Windows.Forms.Label();
            _sortMethodLabel.Text = "Sort Assemblies By:";
            _sortMethodLabel.Location = new System.Drawing.Point(10, 15);
            _sortMethodLabel.AutoSize = true;
            _hierarchyPanel.Controls.Add(_sortMethodLabel);

            _sortMethodComboBox = new System.Windows.Forms.ComboBox();
            _sortMethodComboBox.Location = new System.Drawing.Point(140, 12);
            _sortMethodComboBox.Width = 200;
            _sortMethodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            _sortMethodComboBox.SelectedIndex = 0;
            _sortMethodComboBox.SelectedIndexChanged += new System.EventHandler(SortMethodComboBox_SelectedIndexChanged);
            _hierarchyPanel.Controls.Add(_sortMethodComboBox);

            // The actual hierarchy visualization will be rendered dynamically
            // based on the data and selected sort method
        }

        #endregion
    }
}