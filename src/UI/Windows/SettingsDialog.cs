using System;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Settings;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public partial class SettingsDialog : Form
    {
        private readonly SettingsManager _settingsManager;

        public SettingsDialog(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Create tabbed interface
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // Database settings tab
            TabPage databaseTab = new TabPage("Database");
            CreateDatabaseSettings(databaseTab);

            // NX settings tab
            TabPage nxTab = new TabPage("NX");
            CreateNXSettings(nxTab);

            // Teamcenter settings tab
            TabPage tcTab = new TabPage("Teamcenter");
            CreateTeamcenterSettings(tcTab);

            tabControl.TabPages.Add(databaseTab);
            tabControl.TabPages.Add(nxTab);
            tabControl.TabPages.Add(tcTab);

            this.Controls.Add(tabControl);

            // Add OK and Cancel buttons
            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new System.Drawing.Point(330, 330);
            okButton.Click += OnOKClick;

            Button cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(410, 330);

            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        private void CreateDatabaseSettings(TabPage tab)
        {
            Label pathLabel = new Label();
            pathLabel.Text = "Database Path:";
            pathLabel.Location = new System.Drawing.Point(10, 20);

            TextBox pathTextBox = new TextBox();
            pathTextBox.Location = new System.Drawing.Point(150, 20);
            pathTextBox.Width = 250;
            pathTextBox.Text = _settingsManager.GetSetting("/Settings/Database/Path");
            pathTextBox.Tag = "/Settings/Database/Path"; // Store XPath for later use

            Button browseButton = new Button();
            browseButton.Text = "Browse...";
            browseButton.Location = new System.Drawing.Point(410, 20);
            browseButton.Click += (sender, e) =>
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "SQLite Database (*.db)|*.db";
                dialog.Title = "Select Database Location";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pathTextBox.Text = dialog.FileName;
                }
            };

            tab.Controls.Add(pathLabel);
            tab.Controls.Add(pathTextBox);
            tab.Controls.Add(browseButton);
        }

        private void CreateNXSettings(TabPage tab)
        {
            // NX Install Path
            Label installPathLabel = new Label();
            installPathLabel.Text = "Install Path:";
            installPathLabel.Location = new System.Drawing.Point(10, 20);

            TextBox installPathTextBox = new TextBox();
            installPathTextBox.Location = new System.Drawing.Point(150, 20);
            installPathTextBox.Width = 250;
            installPathTextBox.Text = _settingsManager.GetSetting("/Settings/NX/InstallPath");
            installPathTextBox.Tag = "/Settings/NX/InstallPath";

            Button browseButton = new Button();
            browseButton.Text = "Browse...";
            browseButton.Location = new System.Drawing.Point(410, 20);
            browseButton.Click += (sender, e) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "Select NX Installation Directory";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    installPathTextBox.Text = dialog.SelectedPath;
                }
            };

            // NX Version
            Label versionLabel = new Label();
            versionLabel.Text = "Version:";
            versionLabel.Location = new System.Drawing.Point(10, 50);

            ComboBox versionComboBox = new ComboBox();
            versionComboBox.Location = new System.Drawing.Point(150, 50);
            versionComboBox.Width = 250;
            versionComboBox.Items.AddRange(new object[] { "NX 1847", "NX 1872", "NX 1899", "NX 1919", "NX 1926", "NX 1980" });
            versionComboBox.Text = _settingsManager.GetSetting("/Settings/NX/Version");
            versionComboBox.Tag = "/Settings/NX/Version";

            tab.Controls.Add(installPathLabel);
            tab.Controls.Add(installPathTextBox);
            tab.Controls.Add(browseButton);
            tab.Controls.Add(versionLabel);
            tab.Controls.Add(versionComboBox);
        }

        private void CreateTeamcenterSettings(TabPage tab)
        {
            // Server
            Label serverLabel = new Label();
            serverLabel.Text = "Server:";
            serverLabel.Location = new System.Drawing.Point(10, 20);

            TextBox serverTextBox = new TextBox();
            serverTextBox.Location = new System.Drawing.Point(150, 20);
            serverTextBox.Width = 250;
            serverTextBox.Text = _settingsManager.GetSetting("/Settings/Teamcenter/Server");
            serverTextBox.Tag = "/Settings/Teamcenter/Server";

            // Port
            Label portLabel = new Label();
            portLabel.Text = "Port:";
            portLabel.Location = new System.Drawing.Point(10, 50);

            NumericUpDown portNumeric = new NumericUpDown();
            portNumeric.Location = new System.Drawing.Point(150, 50);
            portNumeric.Width = 100;
            portNumeric.Minimum = 0;
            portNumeric.Maximum = 65535;
            int port;
            if (int.TryParse(_settingsManager.GetSetting("/Settings/Teamcenter/Port"), out port))
            {
                portNumeric.Value = port;
            }
            portNumeric.Tag = "/Settings/Teamcenter/Port";

            // Username
            Label userLabel = new Label();
            userLabel.Text = "Username:";
            userLabel.Location = new System.Drawing.Point(10, 80);

            TextBox userTextBox = new TextBox();
            userTextBox.Location = new System.Drawing.Point(150, 80);
            userTextBox.Width = 250;
            userTextBox.Text = _settingsManager.GetSetting("/Settings/Teamcenter/User");
            userTextBox.Tag = "/Settings/Teamcenter/User";

            // Password
            Label passwordLabel = new Label();
            passwordLabel.Text = "Password:";
            passwordLabel.Location = new System.Drawing.Point(10, 110);

            TextBox passwordTextBox = new TextBox();
            passwordTextBox.Location = new System.Drawing.Point(150, 110);
            passwordTextBox.Width = 250;
            passwordTextBox.PasswordChar = '*';
            passwordTextBox.Tag = "/Settings/Teamcenter/EncryptedPassword";

            // Test Connection button
            Button testButton = new Button();
            testButton.Text = "Test Connection";
            testButton.Location = new System.Drawing.Point(150, 150);
            testButton.Width = 120;
            testButton.Click += (sender, e) =>
            {
                MessageBox.Show("Testing connection to Teamcenter...");
                // Implement actual connection test here
            };

            tab.Controls.Add(serverLabel);
            tab.Controls.Add(serverTextBox);
            tab.Controls.Add(portLabel);
            tab.Controls.Add(portNumeric);
            tab.Controls.Add(userLabel);
            tab.Controls.Add(userTextBox);
            tab.Controls.Add(passwordLabel);
            tab.Controls.Add(passwordTextBox);
            tab.Controls.Add(testButton);
        }

        private void OnOKClick(object sender, EventArgs e)
        {
            // Save all settings
            foreach (Control tab in this.Controls[0].Controls)
            {
                foreach (Control control in tab.Controls)
                {
                    if (control.Tag != null && control.Tag is string xpath)
                    {
                        string value;

                        if (control is TextBox textBox)
                        {
                            value = textBox.Text;
                        }
                        else if (control is ComboBox comboBox)
                        {
                            value = comboBox.Text;
                        }
                        else if (control is NumericUpDown numericUpDown)
                        {
                            value = numericUpDown.Value.ToString();
                        }
                        else
                        {
                            continue;
                        }

                        _settingsManager.SaveSetting(xpath, value);
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}