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
            this.components = new System.ComponentModel.Container();

            this.Text = "Part Browser";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // Create tab control
            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;
            this.Controls.Add(_tabControl);

            // Create directory management tab
            _directoryTab = new TabPage("Directory Management");
            _tabControl.TabPages.Add(_directoryTab);
            InitializeDirectoryTab();

            // Create parts tab
            _partsTab = new TabPage("Parts");
            _tabControl.TabPages.Add(_partsTab);
            InitializePartsTab();

            // Create duplicates tab
            _duplicatesTab = new TabPage("Duplicates");
            _tabControl.TabPages.Add(_duplicatesTab);
            InitializeDuplicatesTab();

            // Create status bar
            _statusLabel = new Label();
            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Text = "Ready";
            _statusLabel.Height = 20;
            this.Controls.Add(_statusLabel);

            // Create progress bar
            _progressBar = new ProgressBar();
            _progressBar.Dock = DockStyle.Bottom;
            _progressBar.Visible = false;
            this.Controls.Add(_progressBar);

            // Create close button
            _closeButton = new Button();
            _closeButton.Text = "Close";
            _closeButton.Location = new Point(680, 500);
            _closeButton.Click += new EventHandler(CloseButton_Click);
            this.Controls.Add(_closeButton);

            // Set as the form's cancel button
            this.CancelButton = _closeButton;
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