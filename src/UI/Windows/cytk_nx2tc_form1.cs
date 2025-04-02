using System;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Settings;
using cytk_NX2TCMigrationTool.src.PLM.NX;
using cytk_NX2TCMigrationTool.src.PLM.Teamcenter;
using cytk_NX2TCMigrationTool.src.UI.Windows;

namespace cytk_NX2TCMigrationTool
{
    public partial class cytk_nx2tcmigtool_form1 : Form
    {
        private readonly SettingsManager _settingsManager;
        private NXConnection _nxConnection;
        private TCConnection _tcConnection;

        public cytk_nx2tcmigtool_form1(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            InitializeComponent();

            // Hook up event handlers for the menu items
            if (configToolStripMenuItem?.DropDownItems != null &&
                configToolStripMenuItem.DropDownItems.Count > 0 &&
                configToolStripMenuItem.DropDownItems[0] is ToolStripMenuItem sessionSettings)
            {
                sessionSettings.Click += OnSessionSettingsClick;
            }

            // You can add additional event handlers here
            this.Load += MainForm_Load;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing connections: {ex.Message}", "Initialization Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Implementation for the missing OnConnectTCClick method
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

        // Implementation for the NX connection click
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

        // Method to handle settings dialog
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

        private void browsePartsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void OnBrowsePartsClick(object sender, EventArgs e)
        {
            PartBrowser browser = new PartBrowser(_nxConnection, _tcConnection);
            browser.ShowDialog();
        }
    }
}