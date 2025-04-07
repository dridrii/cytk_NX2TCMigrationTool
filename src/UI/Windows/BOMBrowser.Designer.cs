namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    partial class BOMBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            _mainSplitContainer = new SplitContainer();
            _assemblyTreeView = new TreeView();
            _topLevelAssemblyComboBox = new ComboBox();
            _topLevelAssemblyLabel = new Label();
            _showDraftingsCheckBox = new CheckBox();
            _componentsGrid = new DataGridView();
            _analyzeButton = new Button();
            _closeButton = new Button();
            _tabControl = new TabControl();
            _componentTabPage = new TabPage();
            _hierarchyTabPage = new TabPage();
            _hierarchyPanel = new Panel();
            _sortMethodLabel = new Label();
            _sortMethodComboBox = new ComboBox();
            _progressBar = new ProgressBar();
            _statusLabel = new Label();
            panel1 = new Panel();
            ((System.ComponentModel.ISupportInitialize)_mainSplitContainer).BeginInit();
            _mainSplitContainer.Panel1.SuspendLayout();
            _mainSplitContainer.Panel2.SuspendLayout();
            _mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_componentsGrid).BeginInit();
            _tabControl.SuspendLayout();
            _componentTabPage.SuspendLayout();
            _hierarchyTabPage.SuspendLayout();
            _hierarchyPanel.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // _mainSplitContainer
            // 
            _mainSplitContainer.Dock = DockStyle.Fill;
            _mainSplitContainer.Location = new Point(4, 3);
            _mainSplitContainer.Margin = new Padding(4, 3, 4, 3);
            _mainSplitContainer.Name = "_mainSplitContainer";
            _mainSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // _mainSplitContainer.Panel1
            // 
            _mainSplitContainer.Panel1.Controls.Add(_assemblyTreeView);
            _mainSplitContainer.Panel1.Controls.Add(_topLevelAssemblyComboBox);
            _mainSplitContainer.Panel1.Controls.Add(_topLevelAssemblyLabel);
            _mainSplitContainer.Panel1.Controls.Add(_showDraftingsCheckBox);
            // 
            // _mainSplitContainer.Panel2
            // 
            _mainSplitContainer.Panel2.Controls.Add(_componentsGrid);
            _mainSplitContainer.Size = new Size(984, 474);
            _mainSplitContainer.SplitterDistance = 235;
            _mainSplitContainer.SplitterWidth = 5;
            _mainSplitContainer.TabIndex = 0;
            // 
            // _assemblyTreeView
            // 
            _assemblyTreeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _assemblyTreeView.Location = new Point(6, 35);
            _assemblyTreeView.Margin = new Padding(4, 3, 4, 3);
            _assemblyTreeView.Name = "_assemblyTreeView";
            _assemblyTreeView.Size = new Size(973, 197);
            _assemblyTreeView.TabIndex = 0;
            // 
            // _topLevelAssemblyComboBox
            // 
            _topLevelAssemblyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _topLevelAssemblyComboBox.FormattingEnabled = true;
            _topLevelAssemblyComboBox.Location = new Point(146, 3);
            _topLevelAssemblyComboBox.Margin = new Padding(4, 3, 4, 3);
            _topLevelAssemblyComboBox.Name = "_topLevelAssemblyComboBox";
            _topLevelAssemblyComboBox.Size = new Size(349, 23);
            _topLevelAssemblyComboBox.TabIndex = 1;
            // 
            // _topLevelAssemblyLabel
            // 
            _topLevelAssemblyLabel.AutoSize = true;
            _topLevelAssemblyLabel.Location = new Point(6, 7);
            _topLevelAssemblyLabel.Margin = new Padding(4, 0, 4, 0);
            _topLevelAssemblyLabel.Name = "_topLevelAssemblyLabel";
            _topLevelAssemblyLabel.Size = new Size(112, 15);
            _topLevelAssemblyLabel.TabIndex = 2;
            _topLevelAssemblyLabel.Text = "Top-level Assembly:";
            // 
            // _showDraftingsCheckBox
            // 
            _showDraftingsCheckBox.AutoSize = true;
            _showDraftingsCheckBox.Location = new Point(525, 6);
            _showDraftingsCheckBox.Margin = new Padding(4, 3, 4, 3);
            _showDraftingsCheckBox.Name = "_showDraftingsCheckBox";
            _showDraftingsCheckBox.Size = new Size(106, 19);
            _showDraftingsCheckBox.TabIndex = 3;
            _showDraftingsCheckBox.Text = "Show Draftings";
            _showDraftingsCheckBox.UseVisualStyleBackColor = true;
            // 
            // _componentsGrid
            // 
            _componentsGrid.AllowUserToAddRows = false;
            _componentsGrid.AllowUserToDeleteRows = false;
            _componentsGrid.Dock = DockStyle.Fill;
            _componentsGrid.Location = new Point(0, 0);
            _componentsGrid.Margin = new Padding(4, 3, 4, 3);
            _componentsGrid.Name = "_componentsGrid";
            _componentsGrid.ReadOnly = true;
            _componentsGrid.Size = new Size(984, 234);
            _componentsGrid.TabIndex = 0;
            // 
            // _analyzeButton
            // 
            _analyzeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _analyzeButton.Location = new Point(781, 8);
            _analyzeButton.Margin = new Padding(4, 3, 4, 3);
            _analyzeButton.Name = "_analyzeButton";
            _analyzeButton.Size = new Size(119, 27);
            _analyzeButton.TabIndex = 1;
            _analyzeButton.Text = "Analyze All Parts";
            _analyzeButton.UseVisualStyleBackColor = true;
            // 
            // _closeButton
            // 
            _closeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _closeButton.DialogResult = DialogResult.Cancel;
            _closeButton.Location = new Point(908, 7);
            _closeButton.Margin = new Padding(4, 3, 4, 3);
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new Size(88, 29);
            _closeButton.TabIndex = 2;
            _closeButton.Text = "Close";
            _closeButton.UseVisualStyleBackColor = true;
            _closeButton.Click += _closeButton_Click;
            // 
            // _tabControl
            // 
            _tabControl.Controls.Add(_componentTabPage);
            _tabControl.Controls.Add(_hierarchyTabPage);
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Location = new Point(0, 0);
            _tabControl.Margin = new Padding(4, 3, 4, 3);
            _tabControl.Name = "_tabControl";
            _tabControl.SelectedIndex = 0;
            _tabControl.Size = new Size(1000, 508);
            _tabControl.TabIndex = 0;
            // 
            // _componentTabPage
            // 
            _componentTabPage.Controls.Add(_mainSplitContainer);
            _componentTabPage.Location = new Point(4, 24);
            _componentTabPage.Margin = new Padding(4, 3, 4, 3);
            _componentTabPage.Name = "_componentTabPage";
            _componentTabPage.Padding = new Padding(4, 3, 4, 3);
            _componentTabPage.Size = new Size(992, 480);
            _componentTabPage.TabIndex = 0;
            _componentTabPage.Text = "Components";
            _componentTabPage.UseVisualStyleBackColor = true;
            // 
            // _hierarchyTabPage
            // 
            _hierarchyTabPage.Controls.Add(_hierarchyPanel);
            _hierarchyTabPage.Location = new Point(4, 24);
            _hierarchyTabPage.Margin = new Padding(4, 3, 4, 3);
            _hierarchyTabPage.Name = "_hierarchyTabPage";
            _hierarchyTabPage.Padding = new Padding(4, 3, 4, 3);
            _hierarchyTabPage.Size = new Size(929, 457);
            _hierarchyTabPage.TabIndex = 1;
            _hierarchyTabPage.Text = "Assembly Hierarchy";
            _hierarchyTabPage.UseVisualStyleBackColor = true;
            // 
            // _hierarchyPanel
            // 
            _hierarchyPanel.Controls.Add(_sortMethodLabel);
            _hierarchyPanel.Controls.Add(_sortMethodComboBox);
            _hierarchyPanel.Dock = DockStyle.Fill;
            _hierarchyPanel.Location = new Point(4, 3);
            _hierarchyPanel.Margin = new Padding(4, 3, 4, 3);
            _hierarchyPanel.Name = "_hierarchyPanel";
            _hierarchyPanel.Size = new Size(921, 451);
            _hierarchyPanel.TabIndex = 0;
            // 
            // _sortMethodLabel
            // 
            _sortMethodLabel.AutoSize = true;
            _sortMethodLabel.Location = new Point(12, 15);
            _sortMethodLabel.Margin = new Padding(4, 0, 4, 0);
            _sortMethodLabel.Name = "_sortMethodLabel";
            _sortMethodLabel.Size = new Size(109, 15);
            _sortMethodLabel.TabIndex = 1;
            _sortMethodLabel.Text = "Sort Assemblies By:";
            // 
            // _sortMethodComboBox
            // 
            _sortMethodComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _sortMethodComboBox.FormattingEnabled = true;
            _sortMethodComboBox.Location = new Point(146, 12);
            _sortMethodComboBox.Margin = new Padding(4, 3, 4, 3);
            _sortMethodComboBox.Name = "_sortMethodComboBox";
            _sortMethodComboBox.Size = new Size(349, 23);
            _sortMethodComboBox.TabIndex = 0;
            // 
            // _progressBar
            // 
            _progressBar.Dock = DockStyle.Bottom;
            _progressBar.Location = new Point(0, 508);
            _progressBar.Margin = new Padding(4, 3, 4, 3);
            _progressBar.Name = "_progressBar";
            _progressBar.Size = new Size(1000, 19);
            _progressBar.TabIndex = 3;
            // 
            // _statusLabel
            // 
            _statusLabel.AutoSize = true;
            _statusLabel.Location = new Point(4, 8);
            _statusLabel.Margin = new Padding(4, 0, 4, 0);
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Padding = new Padding(6, 3, 0, 3);
            _statusLabel.Size = new Size(45, 21);
            _statusLabel.TabIndex = 4;
            _statusLabel.Text = "Ready";
            _statusLabel.Click += _statusLabel_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(_analyzeButton);
            panel1.Controls.Add(_statusLabel);
            panel1.Controls.Add(_closeButton);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 527);
            panel1.Name = "panel1";
            panel1.Size = new Size(1000, 42);
            panel1.TabIndex = 5;
            // 
            // BOMBrowser
            // 
            AcceptButton = _analyzeButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _closeButton;
            ClientSize = new Size(1000, 569);
            Controls.Add(_tabControl);
            Controls.Add(_progressBar);
            Controls.Add(panel1);
            Margin = new Padding(4, 3, 4, 3);
            Name = "BOMBrowser";
            Text = "BOM Browser";
            _mainSplitContainer.Panel1.ResumeLayout(false);
            _mainSplitContainer.Panel1.PerformLayout();
            _mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_mainSplitContainer).EndInit();
            _mainSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_componentsGrid).EndInit();
            _tabControl.ResumeLayout(false);
            _componentTabPage.ResumeLayout(false);
            _hierarchyTabPage.ResumeLayout(false);
            _hierarchyPanel.ResumeLayout(false);
            _hierarchyPanel.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

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
        private System.Windows.Forms.ComboBox _sortMethodComboBox;
        private System.Windows.Forms.Label _sortMethodLabel;
        private Panel panel1;
    }
}