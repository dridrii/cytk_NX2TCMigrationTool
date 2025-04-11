using System;
using System.IO;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Settings;
using cytk_NX2TCMigrationTool.src.PLM.NX;
using cytk_NX2TCMigrationTool.src.PLM.Teamcenter;
using cytk_NX2TCMigrationTool.src.UI.Windows;
using cytk_NX2TCMigrationTool.src.Core.Database.Repositories;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using cytk_NX2TCMigrationTool.src.Core.Database;
using cytk_NX2TCMigrationTool.src.Core.Common.NXCommunication;

namespace cytk_NX2TCMigrationTool
{
    public partial class cytk_nx2tcmigtool_form1 : Form
    {
        private readonly SettingsManager _settingsManager;
        private NXConnection _nxConnection;
        private TCConnection _tcConnection;
        private PartRepository _partRepository;
        private BOMRelationshipRepository _bomRepository;
        private AssemblyStatsRepository _statsRepository;
        private DatabaseMigrator _databaseMigrator;
        private NXWorkerClient _nxWorkerClient;
        private readonly Logger _logger;

        public cytk_nx2tcmigtool_form1(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            // Initialize the logger
            _logger = Logger.Instance;

            InitializeComponent();

            // Initialize repositories
            InitializeRepositories();

            // Ensure databases are up to date
            EnsureDatabaseStructure();

            // Hook up event handlers for the menu items
            if (configToolStripMenuItem?.DropDownItems != null &&
                configToolStripMenuItem.DropDownItems.Count > 0 &&
                configToolStripMenuItem.DropDownItems[0] is ToolStripMenuItem sessionSettings)
            {
                sessionSettings.Click += OnSessionSettingsClick;
            }

            // Connect menu items to event handlers
            if (fileToolStripMenuItem?.DropDownItems != null)
            {
                foreach (ToolStripItem item in fileToolStripMenuItem.DropDownItems)
                {
                    if (item is ToolStripMenuItem menuItem)
                    {
                        if (menuItem.Text == "Connect to NX")
                        {
                            menuItem.Click += OnConnectNXClick;
                        }
                        else if (menuItem.Text == "Browse Parts")
                        {
                            menuItem.Click += OnBrowsePartsClick;
                        }
                        else if (menuItem.Text == "Exit")
                        {
                            menuItem.Click += (s, e) => this.Close();
                        }
                    }
                }
            }

            // Add BOM menu
            InitializeBOMMenu();

            // Add log menu
            InitializeLogMenu();

            // You can add additional event handlers here
            this.Load += MainForm_Load;
        }

        private void InitializeRepositories()
        {
            try
            {
                // Get database path from settings
                string dbPath = _settingsManager.GetSetting("/Settings/Database/Path");

                // Make path absolute if it's relative
                if (!Path.IsPathRooted(dbPath))
                {
                    dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbPath);
                }

                // Ensure directory exists
                string dbDir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);
                }

                // Create connection string and repositories
                string connectionString = $"Data Source={dbPath};Version=3;";
                _partRepository = new PartRepository(connectionString);
                _bomRepository = new BOMRelationshipRepository(connectionString);
                _statsRepository = new AssemblyStatsRepository(connectionString);
                _databaseMigrator = new DatabaseMigrator(connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing repositories: {ex.Message}", "Initialization Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureDatabaseStructure()
        {
            try
            {
                bool result = _databaseMigrator.MigrateDatabase();
                if (!result)
                {
                    MessageBox.Show("Database migration failed. Some features may not work correctly.",
                                    "Database Migration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error migrating database: {ex.Message}", "Database Migration Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize connections
            try
            {
                string nxInstallPath = _settingsManager.GetSetting("/Settings/NX/InstallPath");
                string nxVersion = _settingsManager.GetSetting("/Settings/NX/Version");
                _nxConnection = new NXConnection(nxInstallPath, nxVersion);

                string tcServer = _settingsManager.GetSetting("/Settings/Teamcenter/Server");
                int tcPort;
                if (!int.TryParse(_settingsManager.GetSetting("/Settings/Teamcenter/Port"), out tcPort))
                {
                    tcPort = 7001; // Default port if parsing fails
                }
                string tcUser = _settingsManager.GetSetting("/Settings/Teamcenter/User");
                string tcPassword = _settingsManager.GetSetting("/Settings/Teamcenter/EncryptedPassword");
                _tcConnection = new TCConnection(tcServer, tcPort, tcUser, tcPassword);

                // Set window title to include version from settings or use default
                string version = _settingsManager.GetSetting("/Settings/Application/Version") ?? "0.0.1";
                this.Text = $"NX 2 TC Migration tool V{version}";


                StartNXWorker();

                // Ensure application settings exist
                EnsureApplicationSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing connections: {ex.Message}", "Initialization Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // In your MainForm_Load or similar startup method
        private async void StartNXWorker()
        {
            try
            {
                string nxInstallPath = _settingsManager.GetSetting("/Settings/NX/InstallPath");
                string nxWorkerPath = _settingsManager.GetSetting("/Settings/NX/NXWorkerPath");

                if (!string.IsNullOrEmpty(nxInstallPath) && !string.IsNullOrEmpty(nxWorkerPath))
                {
                    _logger.Info("Application", "Starting NX Worker...");
                    _nxWorkerClient = new NXWorkerClient(nxInstallPath, nxWorkerPath);
                    await _nxWorkerClient.StartWorkerAsync();
                    _logger.Info("Application", "NX Worker started successfully");
                }
                else
                {
                    _logger.Warning("Application", "Could not start NX Worker - missing NX installation path or worker path");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Application", $"Error starting NX Worker: {ex.Message}");
                MessageBox.Show($"Error starting NX Worker: {ex.Message}\n\nSome functionality may be limited.",
                               "NX Worker Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void EnsureApplicationSettings()
        {
            // Check if the Application section exists
            if (_settingsManager.GetSetting("/Settings/Application") == null)
            {
                // If it doesn't exist, try to add it with default values
                try
                {
                    _settingsManager.AddElement("/Settings", "Application", "");
                    _settingsManager.AddElement("/Settings/Application", "Salt", "CYTKdefault123");
                    _settingsManager.AddElement("/Settings/Application", "Version", "0.1.0");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating application settings: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Ensure the RootDirectories element exists
            if (_settingsManager.GetSetting("/Settings/NX/RootDirectories") == null)
            {
                try
                {
                    _settingsManager.AddElement("/Settings/NX", "RootDirectories", "");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating NX root directories setting: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // BOM menu integration
        private void InitializeBOMMenu()
        {
            try
            {
                // Create a BOM menu item
                ToolStripMenuItem bomMenuItem = new ToolStripMenuItem("BOM");

                // Add BOM Browser menu item
                ToolStripMenuItem browseMenuItem = new ToolStripMenuItem("BOM Browser");
                browseMenuItem.Click += OnBrowseBOMClick;
                bomMenuItem.DropDownItems.Add(browseMenuItem);

                // Add BOM Analysis menu item
                ToolStripMenuItem analysisMenuItem = new ToolStripMenuItem("Run BOM Analysis");
                analysisMenuItem.Click += OnRunBOMAnalysisClick;
                bomMenuItem.DropDownItems.Add(analysisMenuItem);

                // Add to main menu - insert after File
                int insertIndex = menuStrip1.Items.IndexOf(fileToolStripMenuItem) + 1;
                menuStrip1.Items.Insert(insertIndex, bomMenuItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing BOM menu: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_nxWorkerClient != null)
                {
                    _logger.Info("Application", "Stopping NX Worker...");
                    await _nxWorkerClient.StopWorkerAsync();
                    _logger.Info("Application", "NX Worker stopped successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Application", $"Error stopping NX Worker: {ex.Message}");
            }
        }

        // Update the BOM Browser methods to use the existing worker
        private async void OnBrowseBOMClick(object sender, EventArgs e)
        {
            try
            {
                // Open the BOM Browser
                using (BOMBrowser browser = new BOMBrowser(
                    _partRepository,
                    _bomRepository,
                    _statsRepository,
                    _settingsManager,
                    _nxWorkerClient)) // Pass the existing worker client
                {
                    browser.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening BOM Browser: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnRunBOMAnalysisClick(object sender, EventArgs e)
        {
            try
            {
                // Open the BOM Browser directly to run analysis
                using (BOMBrowser browser = new BOMBrowser(
                    _partRepository,
                    _bomRepository,
                    _statsRepository,
                    _settingsManager,
                    _nxWorkerClient)) // Pass the existing worker client
                {
                    browser.AnalyzeButton.PerformClick();
                    browser.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running BOM analysis: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnConnectTCClick(object sender, EventArgs e)
        {
            try
            {
                if (_tcConnection == null)
                {
                    MessageBox.Show("Teamcenter connection is not initialized.", "Connection Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool connected = _tcConnection.Connect();
                if (connected)
                {
                    MessageBox.Show("Connected to Teamcenter successfully!", "Connection Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to connect to Teamcenter. Please check your settings.", "Connection Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to Teamcenter: {ex.Message}", "Connection Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnConnectNXClick(object sender, EventArgs e)
        {
            try
            {
                if (_nxConnection == null)
                {
                    MessageBox.Show("NX connection is not initialized.", "Connection Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool connected = _nxConnection.Connect();
                if (connected)
                {
                    MessageBox.Show("Connected to NX successfully!", "Connection Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to connect to NX. Please check your settings.", "Connection Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to NX: {ex.Message}", "Connection Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSessionSettingsClick(object sender, EventArgs e)
        {
            try
            {
                using (var settingsDialog = new src.UI.Windows.SettingsDialog(_settingsManager))
                {
                    settingsDialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening settings dialog: {ex.Message}", "Settings Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnBrowsePartsClick(object sender, EventArgs e)
        {
            try
            {
                if (_partRepository == null)
                {
                    InitializeRepositories();
                }

                using (PartBrowser browser = new PartBrowser(_nxConnection, _tcConnection, _settingsManager, _partRepository))
                {
                    browser.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Part Browser: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnManageDirectoriesClick(object sender, EventArgs e)
        {
            try
            {
                if (_partRepository == null)
                {
                    InitializeRepositories();
                }

                // Open the Part Browser directly to the Directory Management tab
                using (PartBrowser browser = new PartBrowser(_nxConnection, _tcConnection, _settingsManager, _partRepository))
                {
                    browser.ShowTab(0); // Show the Directory Management tab
                    browser.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Directory Management: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Log menu integration
        private void InitializeLogMenu()
        {
            try
            {
                // Create a Log menu item
                ToolStripMenuItem logMenuItem = new ToolStripMenuItem("Logs");

                // Add View Logs menu item
                ToolStripMenuItem viewLogsMenuItem = new ToolStripMenuItem("View Logs");
                viewLogsMenuItem.Click += OnViewLogsClick;
                logMenuItem.DropDownItems.Add(viewLogsMenuItem);

                // Add Log Level submenu
                ToolStripMenuItem logLevelMenuItem = new ToolStripMenuItem("Log Level");
                logMenuItem.DropDownItems.Add(logLevelMenuItem);

                // Add menu items for each log level
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    ToolStripMenuItem levelItem = new ToolStripMenuItem(level.ToString());
                    levelItem.Tag = level;
                    levelItem.Click += OnLogLevelClick;

                    // Check the current log level
                    if (level == Logger.Instance.LogLevel)
                    {
                        levelItem.Checked = true;
                    }

                    logLevelMenuItem.DropDownItems.Add(levelItem);
                }

                // Add to main menu
                menuStrip1.Items.Add(logMenuItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing log menu: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnViewLogsClick(object sender, EventArgs e)
        {
            try
            {
                using (var logViewer = new LogViewerForm())
                {
                    logViewer.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log viewer: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void OnLogLevelClick(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolStripMenuItem menuItem && menuItem.Tag is LogLevel level)
                {
                    // Uncheck all items
                    ToolStripMenuItem parent = ((ToolStripMenuItem)menuItem.OwnerItem);
                    foreach (ToolStripItem item in parent.DropDownItems)
                    {
                        if (item is ToolStripMenuItem mItem)
                        {
                            mItem.Checked = false;
                        }
                    }

                    // Check the selected item
                    menuItem.Checked = true;

                    // Set the log level
                    Logger.Instance.LogLevel = level;
                    Logger.Instance.Info("Application", $"Log level changed to {level}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing log level: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}