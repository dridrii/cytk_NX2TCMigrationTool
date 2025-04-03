using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;
using cytk_NX2TCMigrationTool.src.Core.Database.Repositories;
using cytk_NX2TCMigrationTool.src.Core.Settings;
using cytk_NX2TCMigrationTool.src.PLM.NX;
using cytk_NX2TCMigrationTool.src.PLM.Teamcenter;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public partial class PartBrowser : Form
    {
        private readonly NXConnection _nxConnection;
        private readonly TCConnection _tcConnection;
        private readonly SettingsManager _settingsManager;
        private readonly PartRepository _partRepository;
        private readonly NXFileScanner _fileScanner;

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

        // Constructor
        public PartBrowser(NXConnection nxConnection, TCConnection tcConnection, SettingsManager settingsManager, PartRepository partRepository)
        {
            _nxConnection = nxConnection;
            _tcConnection = tcConnection;
            _settingsManager = settingsManager;
            _partRepository = partRepository;
            _fileScanner = new NXFileScanner(_partRepository, _settingsManager);

            // Initialize the UI
            InitializeComponent();

            // Register event handlers
            _fileScanner.ScanProgress += OnScanProgress;
        }

        /// <summary>
        /// Optional parameter to specify which tab to show initially
        /// </summary>
        /// <param name="tabIndex">Index of tab to show (0 = Directories, 1 = Parts, 2 = Duplicates)</param>
        public void ShowTab(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex < _tabControl.TabPages.Count)
            {
                _tabControl.SelectedIndex = tabIndex;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Part Browser";
            this.Size = new System.Drawing.Size(800, 600);
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

            // Load data
            LoadDirectories();
            LoadParts();
            LoadDuplicates();
        }

        private void InitializeDirectoryTab()
        {
            // Create directory list view
            _directoryListView = new ListView();
            _directoryListView.View = View.Details;
            _directoryListView.FullRowSelect = true;
            _directoryListView.Columns.Add("Directory Path", 500);
            _directoryListView.Dock = DockStyle.Fill;
            _directoryListView.SelectedIndexChanged += OnDirectorySelectionChanged;
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
            _addDirectoryButton.Click += OnAddDirectoryClick;
            buttonPanel.Controls.Add(_addDirectoryButton);

            _removeDirectoryButton = new Button();
            _removeDirectoryButton.Text = "Remove Directory";
            _removeDirectoryButton.Location = new Point(120, 10);
            _removeDirectoryButton.Enabled = false;
            _removeDirectoryButton.Click += OnRemoveDirectoryClick;
            buttonPanel.Controls.Add(_removeDirectoryButton);

            _scanButton = new Button();
            _scanButton.Text = "Scan Directories";
            _scanButton.Location = new Point(250, 10);
            _scanButton.Click += OnScanDirectoriesClick;
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

        // Load directories from settings
        private void LoadDirectories()
        {
            _directoryListView.Items.Clear();

            var dirElements = _settingsManager.GetSettingElements("/Settings/NX/RootDirectories/Directory");
            foreach (var dirElement in dirElements)
            {
                string path = dirElement.InnerText;
                var item = new ListViewItem(path);
                _directoryListView.Items.Add(item);
            }
        }

        // Load parts from database
        private void LoadParts()
        {
            _partsGrid.Rows.Clear();

            var parts = _partRepository.GetAll();
            foreach (var part in parts)
            {
                if (!part.IsDuplicate)
                {
                    _partsGrid.Rows.Add(
                        part.Id,
                        part.Name,
                        part.Type,
                        part.FileName,
                        part.FilePath,
                        part.Checksum,
                        part.IsDuplicate
                    );
                }
            }
        }

        // Load duplicates from database
        private void LoadDuplicates()
        {
            _duplicatesGrid.Rows.Clear();

            var duplicates = _partRepository.GetDuplicates();
            foreach (var duplicate in duplicates)
            {
                var original = _partRepository.GetById(duplicate.DuplicateOf);
                string originalPath = original?.FilePath ?? "Unknown";

                _duplicatesGrid.Rows.Add(
                    duplicate.Id,
                    duplicate.Name,
                    duplicate.FilePath,
                    duplicate.DuplicateOf,
                    originalPath
                );
            }
        }

        // Event handlers
        private void OnDirectorySelectionChanged(object sender, EventArgs e)
        {
            _removeDirectoryButton.Enabled = _directoryListView.SelectedItems.Count > 0;
        }

        private void OnAddDirectoryClick(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a root directory for NX part files";
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string path = dialog.SelectedPath;

                    // Check if the directory already exists in the list
                    bool exists = false;
                    foreach (ListViewItem item in _directoryListView.Items)
                    {
                        if (item.Text.Equals(path, StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        // Add to ListView
                        var item = new ListViewItem(path);
                        _directoryListView.Items.Add(item);

                        // Add to settings
                        _settingsManager.AddElement("/Settings/NX/RootDirectories", "Directory", path);
                    }
                    else
                    {
                        MessageBox.Show("This directory is already in the list.", "Directory Exists",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void OnRemoveDirectoryClick(object sender, EventArgs e)
        {
            if (_directoryListView.SelectedItems.Count > 0)
            {
                var item = _directoryListView.SelectedItems[0];
                string path = item.Text;

                // Remove from ListView
                _directoryListView.Items.Remove(item);

                // Find and remove from settings
                var dirElements = _settingsManager.GetSettingElements("/Settings/NX/RootDirectories/Directory");
                for (int i = 0; i < dirElements.Count; i++)
                {
                    if (dirElements[i].InnerText.Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
                        // Note: This is a bit of a hack - we're using the index to construct the XPath
                        // A better implementation would be to have the SettingsManager handle this more cleanly
                        _settingsManager.RemoveElement($"/Settings/NX/RootDirectories/Directory[{i + 1}]");
                        break;
                    }
                }
            }
        }

        private async void OnScanDirectoriesClick(object sender, EventArgs e)
        {
            // Disable UI during scan
            _addDirectoryButton.Enabled = false;
            _removeDirectoryButton.Enabled = false;
            _scanButton.Enabled = false;
            _progressBar.Visible = true;
            _progressBar.Value = 0;
            _statusLabel.Text = "Preparing to scan...";

            try
            {
                // Perform the scan
                var results = await _fileScanner.ScanAllRootDirectoriesAsync();

                // Show results
                MessageBox.Show($"Scan complete!\n\nFiles scanned: {results.FilesScanned}\nFiles added: {results.FilesAdded}\nDuplicates found: {results.DuplicatesFound}\nErrors: {results.Errors.Count}",
                    "Scan Results", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh the UI
                LoadParts();
                LoadDuplicates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning directories: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable UI
                _addDirectoryButton.Enabled = true;
                _removeDirectoryButton.Enabled = _directoryListView.SelectedItems.Count > 0;
                _scanButton.Enabled = true;
                _progressBar.Visible = false;
                _statusLabel.Text = "Ready";
            }
        }

        private void OnScanProgress(object sender, FileScanProgressEventArgs e)
        {
            // Make sure we're on the UI thread
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new EventHandler<FileScanProgressEventArgs>(OnScanProgress), sender, e);
                return;
            }

            // Update the UI
            _statusLabel.Text = e.CurrentOperation;
            if (e.TotalFiles > 0)
            {
                _statusLabel.Text += $" - File {e.CurrentFileNumber} of {e.TotalFiles}: {Path.GetFileName(e.CurrentFile)}";
                _progressBar.Value = (int)((double)e.CurrentFileNumber / e.TotalFiles * 100);
            }
            else
            {
                _progressBar.Value = e.OverallProgress;
            }
        }
    }
}