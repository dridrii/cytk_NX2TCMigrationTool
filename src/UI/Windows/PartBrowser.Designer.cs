using System;
using System.Drawing;
using System.Windows.Forms;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    partial class PartBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // UI Controls
        private ListView _directoryListView;
        private Button _addDirectoryButton;
        private Button _removeDirectoryButton;
        private Button _scanButton;
        private TabControl _tabControl;
        private TabPage _directoryTab;
        private TabPage _partsTab;
        private TabPage _duplicatesTab;
        private ProgressBar _progressBar;
        private Label _statusLabel;
        private DataGridView _partsGrid;
        private DataGridView _duplicatesGrid;
        private Button _closeButton;

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
            _tabControl = new TabControl();
            _directoryTab = new TabPage();
            _partsTab = new TabPage();
            _duplicatesTab = new TabPage();
            _statusLabel = new Label();
            _progressBar = new ProgressBar();
            _closeButton = new Button();
            _tabControl.SuspendLayout();
            SuspendLayout();
            // 
            // _tabControl
            // 
            _tabControl.Controls.Add(_directoryTab);
            _tabControl.Controls.Add(_partsTab);
            _tabControl.Controls.Add(_duplicatesTab);
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Location = new Point(0, 0);
            _tabControl.Name = "_tabControl";
            _tabControl.SelectedIndex = 0;
            _tabControl.Size = new Size(865, 511);
            _tabControl.TabIndex = 0;
            // 
            // _directoryTab
            // 
            _directoryTab.Location = new Point(4, 24);
            _directoryTab.Name = "_directoryTab";
            _directoryTab.Size = new Size(857, 483);
            _directoryTab.TabIndex = 0;
            _directoryTab.Text = "Directory Management";
            // 
            // _partsTab
            // 
            _partsTab.Location = new Point(4, 24);
            _partsTab.Name = "_partsTab";
            _partsTab.Size = new Size(857, 483);
            _partsTab.TabIndex = 1;
            _partsTab.Text = "Parts";
            // 
            // _duplicatesTab
            // 
            _duplicatesTab.Location = new Point(4, 24);
            _duplicatesTab.Name = "_duplicatesTab";
            _duplicatesTab.Size = new Size(857, 483);
            _duplicatesTab.TabIndex = 2;
            _duplicatesTab.Text = "Duplicates";
            // 
            // _statusLabel
            // 
            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Location = new Point(0, 511);
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Size = new Size(865, 20);
            _statusLabel.TabIndex = 1;
            _statusLabel.Text = "Ready";
            // 
            // _progressBar
            // 
            _progressBar.Dock = DockStyle.Bottom;
            _progressBar.Location = new Point(0, 531);
            _progressBar.Name = "_progressBar";
            _progressBar.Size = new Size(865, 23);
            _progressBar.TabIndex = 2;
            _progressBar.Visible = false;
            // 
            // _closeButton
            // 
            _closeButton.Location = new Point(680, 500);
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new Size(75, 23);
            _closeButton.TabIndex = 3;
            _closeButton.Text = "Close";
            _closeButton.Click += CloseButton_Click;
            // 
            // PartBrowser
            // 
            CancelButton = _closeButton;
            ClientSize = new Size(865, 554);
            Controls.Add(_tabControl);
            Controls.Add(_statusLabel);
            Controls.Add(_progressBar);
            Controls.Add(_closeButton);
            Name = "PartBrowser";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Part Browser";
            _tabControl.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void InitializeDirectoryTab()
        {
            // Create directory list view
            _directoryListView = new ListView();
            _directoryListView.View = View.Details;
            _directoryListView.FullRowSelect = true;
            _directoryListView.Columns.Add("Directory Path", 500);
            _directoryListView.Dock = DockStyle.Fill;
            _directoryListView.SelectedIndexChanged += new EventHandler(OnDirectorySelectionChanged);
            _directoryTab.Controls.Add(_directoryListView);

            // Create button panel
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 40;
            _directoryTab.Controls.Add(buttonPanel);

            // Create buttons
            _addDirectoryButton = new Button();
            _addDirectoryButton.Text = "Add Directory";
            _addDirectoryButton.Location = new Point(10, 10);
            _addDirectoryButton.Click += new EventHandler(OnAddDirectoryClick);
            buttonPanel.Controls.Add(_addDirectoryButton);

            _removeDirectoryButton = new Button();
            _removeDirectoryButton.Text = "Remove Directory";
            _removeDirectoryButton.Location = new Point(120, 10);
            _removeDirectoryButton.Enabled = false;
            _removeDirectoryButton.Click += new EventHandler(OnRemoveDirectoryClick);
            buttonPanel.Controls.Add(_removeDirectoryButton);

            _scanButton = new Button();
            _scanButton.Text = "Scan Directories";
            _scanButton.Location = new Point(250, 10);
            _scanButton.Click += new EventHandler(OnScanDirectoriesClick);
            buttonPanel.Controls.Add(_scanButton);
        }

        private void InitializePartsTab()
        {
            // Create parts grid
            _partsGrid = new DataGridView();
            _partsGrid.Dock = DockStyle.Fill;
            _partsGrid.AllowUserToAddRows = false;
            _partsGrid.AllowUserToDeleteRows = false;
            _partsGrid.ReadOnly = true;
            _partsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _partsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _partsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _partsTab.Controls.Add(_partsGrid);

            DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
            rowNumberColumn.HeaderText = "#";
            rowNumberColumn.Width = 40;
            rowNumberColumn.ReadOnly = true;
            rowNumberColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            _partsGrid.Columns.Insert(0, rowNumberColumn);

            // Add search panel
            Panel searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Top;
            searchPanel.Height = 40;
            _partsTab.Controls.Add(searchPanel);

            TextBox searchBox = new TextBox();
            searchBox.Location = new Point(100, 10);
            searchBox.Width = 200;
            searchPanel.Controls.Add(searchBox);

            Label searchLabel = new Label();
            searchLabel.Text = "Search:";
            searchLabel.Location = new Point(10, 13);
            searchPanel.Controls.Add(searchLabel);

            Button searchButton = new Button();
            searchButton.Text = "Search";
            searchButton.Location = new Point(310, 8);
            searchPanel.Controls.Add(searchButton);

            // Configure grid columns
            _partsGrid.Columns.Add("ID", "ID");
            _partsGrid.Columns.Add("Name", "Name");
            _partsGrid.Columns.Add("Type", "Type");
            _partsGrid.Columns.Add("FileName", "File Name");
            _partsGrid.Columns.Add("FilePath", "File Path");
            _partsGrid.Columns.Add("Checksum", "Checksum");
            _partsGrid.Columns.Add("IsDuplicate", "Is Duplicate");
            _partsGrid.Columns.Add("IsPart", "Is Part");
            _partsGrid.Columns.Add("IsAssembly", "Is Assembly");
            _partsGrid.Columns.Add("IsDrafting", "Is Drafting");
            _partsGrid.Columns.Add("IsPartFamilyMaster", "Is Part Family Master");
            _partsGrid.Columns.Add("IsPartFamilyMember", "Is Part Family Member");
        }

        private void InitializeDuplicatesTab()
        {
            // Create duplicates grid
            _duplicatesGrid = new DataGridView();
            _duplicatesGrid.Dock = DockStyle.Fill;
            _duplicatesGrid.AllowUserToAddRows = false;
            _duplicatesGrid.AllowUserToDeleteRows = false;
            _duplicatesGrid.ReadOnly = true;
            _duplicatesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _duplicatesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _duplicatesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _duplicatesTab.Controls.Add(_duplicatesGrid);

            // Configure grid columns
            _duplicatesGrid.Columns.Add("ID", "ID");
            _duplicatesGrid.Columns.Add("Name", "Name");
            _duplicatesGrid.Columns.Add("FilePath", "File Path");
            _duplicatesGrid.Columns.Add("DuplicateOf", "Duplicate Of");
            _duplicatesGrid.Columns.Add("OriginalPath", "Original File Path");
        }

        #endregion
    }
}