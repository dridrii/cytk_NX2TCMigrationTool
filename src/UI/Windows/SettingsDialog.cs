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

            // Create tab content after InitializeComponent is called
            CreateDatabaseSettings(databaseTab);
            CreateNXSettings(nxTab);
            CreateTeamcenterSettings(tcTab);
        }

        private void OnOKClick(object sender, EventArgs e)
        {
            // Save all settings
            ProcessControlsRecursively(this);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ProcessControlsRecursively(Control container)
        {
            foreach (Control control in container.Controls)
            {
                // Check if this control has a tag with xpath
                if (control.Tag != null && control.Tag is string xpath)
                {
                    string value = null;
                    
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
                    
                    // Save the setting if we got a value
                    if (value != null)
                    {
                        _settingsManager.SaveSetting(xpath, value);
                        Console.WriteLine($"Saved setting: {xpath} = {value}");
                    }
                }
                
                // Process child controls recursively
                if (control.Controls.Count > 0)
                {
                    ProcessControlsRecursively(control);
                }
            }
        }

        // Move tab content creation methods to the main code file
        private void CreateDatabaseSettings(TabPage tab)
        {
            // Create layout panel
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 2;
            layout.ColumnCount = 3;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Padding = new Padding(10);
            tab.Controls.Add(layout);

            // Path label
            Label pathLabel = new Label();
            pathLabel.Text = "Database Path:";
            pathLabel.Dock = DockStyle.Fill;
            pathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            layout.Controls.Add(pathLabel, 0, 0);

            // Path textbox
            TextBox pathTextBox = new TextBox();
            pathTextBox.Dock = DockStyle.Fill;
            pathTextBox.Text = _settingsManager.GetSetting("/Settings/Database/Path");
            pathTextBox.Tag = "/Settings/Database/Path"; // Store XPath for later use
            layout.Controls.Add(pathTextBox, 1, 0);

            // Browse button
            Button browseButton = new Button();
            browseButton.Text = "Browse...";
            browseButton.Dock = DockStyle.Fill;
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
            layout.Controls.Add(browseButton, 2, 0);
        }

        private void CreateNXSettings(TabPage tab)
        {
            // Create layout panel
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 3;
            layout.ColumnCount = 3;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Padding = new Padding(10);
            tab.Controls.Add(layout);

            // NX Install Path
            Label installPathLabel = new Label();
            installPathLabel.Text = "Install Path:";
            installPathLabel.Dock = DockStyle.Fill;
            installPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            layout.Controls.Add(installPathLabel, 0, 0);

            TextBox installPathTextBox = new TextBox();
            installPathTextBox.Dock = DockStyle.Fill;
            installPathTextBox.Text = _settingsManager.GetSetting("/Settings/NX/InstallPath");
            installPathTextBox.Tag = "/Settings/NX/InstallPath";
            layout.Controls.Add(installPathTextBox, 1, 0);

            Button browseButton = new Button();
            browseButton.Text = "Browse...";
            browseButton.Dock = DockStyle.Fill;
            browseButton.Click += (sender, e) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "Select NX Installation Directory";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    installPathTextBox.Text = dialog.SelectedPath;
                }
            };
            layout.Controls.Add(browseButton, 2, 0);

            // NX Version
            Label versionLabel = new Label();
            versionLabel.Text = "Version:";
            versionLabel.Dock = DockStyle.Fill;
            versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            layout.Controls.Add(versionLabel, 0, 1);

            ComboBox versionComboBox = new ComboBox();
            versionComboBox.Dock = DockStyle.Fill;
            versionComboBox.Items.AddRange(new object[] { "NX 1847", "NX 1872", "NX 1899", "NX 1919", "NX 1926", "NX 1980" });
            versionComboBox.Text = _settingsManager.GetSetting("/Settings/NX/Version");
            versionComboBox.Tag = "/Settings/NX/Version";
            layout.Controls.Add(versionComboBox, 1, 1);
        }

        private void CreateTeamcenterSettings(TabPage tab)
        {
            // Create layout panel
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 6;
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Padding = new Padding(10);
            tab.Controls.Add(layout);

            // Server
            Label serverLabel = new Label();
            serverLabel.Text = "Server:";
            serverLabel.Dock = DockStyle.Fill;
            serverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            layout.Controls.Add(serverLabel, 0, 0);

            TextBox serverTextBox = new TextBox();
            serverTextBox.Dock = DockStyle.Fill;
            serverTextBox.Text = _settingsManager.GetSetting("/Settings/Teamcenter/Server");
            serverTextBox.Tag = "/Settings/Teamcenter/Server";
            layout.Controls.Add(serverTextBox, 1, 0);

            // Port
            Label portLabel = new Label();
            portLabel.Text = "Port:";
            portLabel.Dock = DockStyle.Fill;
            portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            layout.Controls.Add(portLabel, 0, 1);

            NumericUpDown portNumeric = new NumericUpDown();
            portNumeric.Dock = DockStyle.Left;
            portNumeric.Width = 100;
            portNumeric.Minimum = 0;
            portNumeric.Maximum = 65535;
            int port;
            if (int.TryParse(_settingsManager.GetSetting("/Settings/Teamcenter/Port"), out port))
            {
                portNumeric.Value = port;
            }
            portNumeric.Tag = "/Settings/Teamcenter/Port";
            layout.Controls.Add(portNumeric, 1, 1);

            // Username
            Label userLabel = new Label();
            userLabel.Text = "Username:";
            userLabel.Dock = DockStyle.Fill;
            userLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            layout.Controls.Add(userLabel, 0, 2);

            TextBox userTextBox = new TextBox();
            userTextBox.Dock = DockStyle.Fill;
            userTextBox.Text = _settingsManager.GetSetting("/Settings/Teamcenter/User");
            userTextBox.Tag = "/Settings/Teamcenter/User";
            layout.Controls.Add(userTextBox, 1, 2);

            // Password
            Label passwordLabel = new Label();
            passwordLabel.Text = "Password:";
            passwordLabel.Dock = DockStyle.Fill;
            passwordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            layout.Controls.Add(passwordLabel, 0, 3);

            TextBox passwordTextBox = new TextBox();
            passwordTextBox.Dock = DockStyle.Fill;
            passwordTextBox.PasswordChar = '*';
            passwordTextBox.Tag = "/Settings/Teamcenter/EncryptedPassword";
            layout.Controls.Add(passwordTextBox, 1, 3);

            // Test Connection button
            Button testButton = new Button();
            testButton.Text = "Test Connection";
            testButton.Width = 120;
            testButton.Click += (sender, e) =>
            {
                MessageBox.Show("Testing connection to Teamcenter...");
                // Implement actual connection test here
            };
            layout.Controls.Add(testButton, 1, 4);
        }
    }
}