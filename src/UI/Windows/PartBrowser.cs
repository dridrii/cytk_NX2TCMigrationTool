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

            // Initialize tab controls
            InitializeDirectoryTab();
            InitializePartsTab();
            InitializeDuplicatesTab();

            // Register event handlers
            _fileScanner.ScanProgress += OnScanProgress;

            // Add form closing handler
            this.FormClosing += PartBrowser_FormClosing;

            // Load data
            LoadDirectories();
            LoadParts();
            LoadDuplicates();
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
            int rowNumber = 1;
            foreach (var part in parts)
            {
                if (!part.IsDuplicate)
                {
                    _partsGrid.Rows.Add(
                        rowNumber,
                        part.Id,
                        part.Name,
                        part.Type,
                        part.FileName,
                        part.FilePath,
                        part.Checksum,
                        part.IsDuplicate ? "Yes" : "No",
                        part.IsPart.HasValue ? (part.IsPart.Value ? "Yes" : "No") : "Unknown",
                        part.IsAssembly.HasValue ? (part.IsAssembly.Value ? "Yes" : "No") : "Unknown",
                        part.IsDrafting.HasValue ? (part.IsDrafting.Value ? "Yes" : "No") : "Unknown",
                        part.IsPartFamilyMaster.HasValue ? (part.IsPartFamilyMaster.Value ? "Yes" : "No") : "Unknown",
                        part.IsPartFamilyMember.HasValue ? (part.IsPartFamilyMember.Value ? "Yes" : "No") : "Unknown"
                    );
                    rowNumber++;
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

        private void PartBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unregister event handlers to prevent memory leaks
            _fileScanner.ScanProgress -= OnScanProgress;
        }
    }
}