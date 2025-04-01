using System;
using System.Data.Common;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Settings;
using cytk_NX2TCMigrationTool.src.PLM.NX;
using cytk_NX2TCMigrationTool.src.PLM.Teamcenter;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public partial class MainForm : Form
    {
        private readonly SettingsManager _settingsManager;
        private NXConnection _nxConnection;
        private TCConnection _tcConnection;

        public MainForm(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "PLM Integration";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Add menu items, toolbars, etc.
            CreateMenus();
            CreateStatusBar();

            // Add event handlers
            this.Load += MainForm_Load;
        }

        private void CreateMenus()
        {
            MenuStrip mainMenu = new MenuStrip();

            // File menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Settings", null, OnSettingsClick);
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("Exit", null, OnExitClick);

            // PLM menu
            ToolStripMenuItem plmMenu = new ToolStripMenuItem("PLM");
            plmMenu.DropDownItems.Add("Connect to NX", null, OnConnectNXClick);
            plmMenu.DropDownItems.Add("Connect to Teamcenter", null, OnConnectTCClick);
            plmMenu.DropDownItems.Add("-");
            plmMenu.DropDownItems.Add("Browse Parts", null, OnBrowsePartsClick);

            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(plmMenu);

            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);
        }

        private void CreateStatusBar()
        {
            StatusStrip statusStrip = new StatusStrip();
            ToolStripStatusLabel statusLabel = new ToolStripStatusLabel("Ready");

            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize connections
            string nxInstallPath = _settingsManager.GetSetting("/Settings/NX/InstallPath");
            string nxVersion = _settingsManager.GetSetting("/Settings/NX/Version");
            _nxConnection = new NXConnection(nxInstallPath, nxVersion);

            string tcServer = _settingsManager.GetSetting("/Settings/Teamcenter/Server");
            int tcPort = int.Parse(_settingsManager.GetSetting("/Settings/Teamcenter/Port"));
            string tcUser = _settingsManager.GetSetting("/Settings/Teamcenter/User");
            string tcPassword = _settingsManager.GetSetting("/Settings/Teamcenter/EncryptedPassword");
            _tcConnection = new TCConnection(tcServer, tcPort, tcUser, tcPassword);
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            SettingsDialog dialog = new SettingsDialog(_settingsManager);
            dialog.ShowDialog();
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnConnectNXClick(object sender, EventArgs e)
        {
            bool connected = _nxConnection.Connect();
            if (connected)
            {
                MessageBox.Show("Connected to NX successfully!");
            }
            else
            {
                MessageBox.Show("Failed to connect to NX. Please check your settings.");
            }
        }

        private void OnConnectTCClick(object sender, EventArgs e)
        {
            bool connected = _tcConnection.Connect();
            if (connected)
            {
                MessageBox.Show("Connected to Teamcenter successfully!");
            }
            else
            {
                MessageBox.Show("Failed to connect to Teamcenter. Please check your settings.");
            }
        }

        private void OnBrowsePartsClick(object sender, EventArgs e)
        {
            PartBrowser browser = new PartBrowser(_nxConnection, _tcConnection);
            browser.ShowDialog();
        }
    }
}