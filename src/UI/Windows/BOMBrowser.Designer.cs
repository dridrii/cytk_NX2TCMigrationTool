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
            // Define UI controls
            this._mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this._tabControl = new System.Windows.Forms.TabControl();
            this._componentTabPage = new System.Windows.Forms.TabPage();
            this._hierarchyTabPage = new System.Windows.Forms.TabPage();
            this._assemblyTreeView = new System.Windows.Forms.TreeView();
            this._componentsGrid = new System.Windows.Forms.DataGridView();
            this._topLevelAssemblyComboBox = new System.Windows.Forms.ComboBox();
            this._topLevelAssemblyLabel = new System.Windows.Forms.Label();
            this._showDraftingsCheckBox = new System.Windows.Forms.CheckBox();
            this._analyzeButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._statusLabel = new System.Windows.Forms.Label();
            this._hierarchyPanel = new System.Windows.Forms.Panel();
            this._sortMethodComboBox = new System.Windows.Forms.ComboBox();
            this._sortMethodLabel = new System.Windows.Forms.Label();

            // Configure components
            ((System.ComponentModel.ISupportInitialize)(this._mainSplitContainer)).BeginInit();
            this._mainSplitContainer.Panel1.SuspendLayout();
            this._mainSplitContainer.Panel2.SuspendLayout();
            this._mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._componentsGrid)).BeginInit();
            this.SuspendLayout();

            // 
            // _tabControl
            // 
            this._tabControl.Controls.Add(this._componentTabPage);
            this._tabControl.Controls.Add(this._hierarchyTabPage);
            this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabControl.Location = new System.Drawing.Point(0, 0);
            this._tabControl.Name = "_tabControl";
            this._tabControl.SelectedIndex = 0;
            this._tabControl.Size = new System.Drawing.Size(800, 400);
            this._tabControl.TabIndex = 0;

            // 
            // _componentTabPage
            // 
            this._componentTabPage.Controls.Add(this._mainSplitContainer);
            this._componentTabPage.Location = new System.Drawing.Point(4, 22);
            this._componentTabPage.Name = "_componentTabPage";
            this._componentTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._componentTabPage.Size = new System.Drawing.Size(792, 374);
            this._componentTabPage.TabIndex = 0;
            this._componentTabPage.Text = "Components";
            this._componentTabPage.UseVisualStyleBackColor = true;

            // 
            // _hierarchyTabPage
            // 
            this._hierarchyTabPage.Controls.Add(this._hierarchyPanel);
            this._hierarchyTabPage.Location = new System.Drawing.Point(4, 22);
            this._hierarchyTabPage.Name = "_hierarchyTabPage";
            this._hierarchyTabPage.Padding = new System.Windows.Forms.Padding(3);
            this._hierarchyTabPage.Size = new System.Drawing.Size(792, 374);
            this._hierarchyTabPage.TabIndex = 1;
            this._hierarchyTabPage.Text = "Assembly Hierarchy";
            this._hierarchyTabPage.UseVisualStyleBackColor = true;

            // 
            // _mainSplitContainer
            // 
            this._mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainSplitContainer.Location = new System.Drawing.Point(3, 3);
            this._mainSplitContainer.Name = "_mainSplitContainer";
            this._mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;

            // 
            // _mainSplitContainer.Panel1
            // 
            this._mainSplitContainer.Panel1.Controls.Add(this._assemblyTreeView);
            this._mainSplitContainer.Panel1.Controls.Add(this._topLevelAssemblyComboBox);
            this._mainSplitContainer.Panel1.Controls.Add(this._topLevelAssemblyLabel);
            this._mainSplitContainer.Panel1.Controls.Add(this._showDraftingsCheckBox);

            // 
            // _mainSplitContainer.Panel2
            // 
            this._mainSplitContainer.Panel2.Controls.Add(this._componentsGrid);
            this._mainSplitContainer.Size = new System.Drawing.Size(786, 368);
            this._mainSplitContainer.SplitterDistance = 184;
            this._mainSplitContainer.TabIndex = 0;

            // 
            // _assemblyTreeView
            // 
            this._assemblyTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._assemblyTreeView.Location = new System.Drawing.Point(5, 30);
            this._assemblyTreeView.Name = "_assemblyTreeView";
            this._assemblyTreeView.Size = new System.Drawing.Size(778, 151);
            this._assemblyTreeView.TabIndex = 0;

            // 
            // _componentsGrid
            // 
            this._componentsGrid.AllowUserToAddRows = false;
            this._componentsGrid.AllowUserToDeleteRows = false;
            this._componentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._componentsGrid.Location = new System.Drawing.Point(0, 0);
            this._componentsGrid.Name = "_componentsGrid";
            this._componentsGrid.ReadOnly = true;
            this._componentsGrid.Size = new System.Drawing.Size(786, 180);
            this._componentsGrid.TabIndex = 0;

            // 
            // _topLevelAssemblyComboBox
            // 
            this._topLevelAssemblyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._topLevelAssemblyComboBox.FormattingEnabled = true;
            this._topLevelAssemblyComboBox.Location = new System.Drawing.Point(125, 3);
            this._topLevelAssemblyComboBox.Name = "_topLevelAssemblyComboBox";
            this._topLevelAssemblyComboBox.Size = new System.Drawing.Size(300, 21);
            this._topLevelAssemblyComboBox.TabIndex = 1;

            // 
            // _topLevelAssemblyLabel
            // 
            this._topLevelAssemblyLabel.AutoSize = true;
            this._topLevelAssemblyLabel.Location = new System.Drawing.Point(5, 6);
            this._topLevelAssemblyLabel.Name = "_topLevelAssemblyLabel";
            this._topLevelAssemblyLabel.Size = new System.Drawing.Size(114, 13);
            this._topLevelAssemblyLabel.TabIndex = 2;
            this._topLevelAssemblyLabel.Text = "Top-level Assembly:";

            // 
            // _showDraftingsCheckBox
            // 
            this._showDraftingsCheckBox.AutoSize = true;
            this._showDraftingsCheckBox.Location = new System.Drawing.Point(450, 5);
            this._showDraftingsCheckBox.Name = "_showDraftingsCheckBox";
            this._showDraftingsCheckBox.Size = new System.Drawing.Size(100, 17);
            this._showDraftingsCheckBox.TabIndex = 3;
            this._showDraftingsCheckBox.Text = "Show Draftings";
            this._showDraftingsCheckBox.UseVisualStyleBackColor = true;

            // 
            // _hierarchyPanel
            // 
            this._hierarchyPanel.Controls.Add(this._sortMethodLabel);
            this._hierarchyPanel.Controls.Add(this._sortMethodComboBox);
            this._hierarchyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._hierarchyPanel.Location = new System.Drawing.Point(3, 3);
            this._hierarchyPanel.Name = "_hierarchyPanel";
            this._hierarchyPanel.Size = new System.Drawing.Size(786, 368);
            this._hierarchyPanel.TabIndex = 0;

            // 
            // _sortMethodComboBox
            // 
            this._sortMethodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._sortMethodComboBox.FormattingEnabled = true;
            this._sortMethodComboBox.Location = new System.Drawing.Point(125, 10);
            this._sortMethodComboBox.Name = "_sortMethodComboBox";
            this._sortMethodComboBox.Size = new System.Drawing.Size(300, 21);
            this._sortMethodComboBox.TabIndex = 0;

            // 
            // _sortMethodLabel
            // 
            this._sortMethodLabel.AutoSize = true;
            this._sortMethodLabel.Location = new System.Drawing.Point(10, 13);
            this._sortMethodLabel.Name = "_sortMethodLabel";
            this._sortMethodLabel.Size = new System.Drawing.Size(109, 13);
            this._sortMethodLabel.TabIndex = 1;
            this._sortMethodLabel.Text = "Sort Assemblies By:";

            // 
            // _analyzeButton
            // 
            this._analyzeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._analyzeButton.Location = new System.Drawing.Point(625, 410);
            this._analyzeButton.Name = "_analyzeButton";
            this._analyzeButton.Size = new System.Drawing.Size(102, 23);
            this._analyzeButton.TabIndex = 1;
            this._analyzeButton.Text = "Analyze All Parts";
            this._analyzeButton.UseVisualStyleBackColor = true;

            // 
            // _closeButton
            // 
            this._closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._closeButton.Location = new System.Drawing.Point(733, 410);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(75, 23);
            this._closeButton.TabIndex = 2;
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;

            // 
            // _progressBar
            // 
            this._progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._progressBar.Location = new System.Drawing.Point(0, 434);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(800, 10);
            this._progressBar.TabIndex = 3;

            // 
            // _statusLabel
            // 
            this._statusLabel.AutoSize = true;
            this._statusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._statusLabel.Location = new System.Drawing.Point(0, 444);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Padding = new System.Windows.Forms.Padding(5, 3, 0, 3);
            this._statusLabel.Size = new System.Drawing.Size(44, 19);
            this._statusLabel.TabIndex = 4;
            this._statusLabel.Text = "Ready";

            // 
            // BOMBrowser
            // 
            this.AcceptButton = this._analyzeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._closeButton;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this._analyzeButton);
            this.Controls.Add(this._closeButton);
            this.Controls.Add(this._tabControl);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._statusLabel);
            this.Name = "BOMBrowser";
            this.Text = "BOM Browser";
            this._mainSplitContainer.Panel1.ResumeLayout(false);
            this._mainSplitContainer.Panel1.PerformLayout();
            this._mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._mainSplitContainer)).EndInit();
            this._mainSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._componentsGrid)).EndInit();
            this._hierarchyPanel.ResumeLayout(false);
            this._hierarchyPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
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
    }
}