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

        private void OnBrowseClick1(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "SQLite Database (*.db)|*.db";
            dialog.Title = "Select Database Location";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                databasePathSettings.Text = dialog.FileName;
            }
        }

        private void OnNXBrowseClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select NX Installation Directory";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                nxInstallPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void OnTestTCConnectionClick(object sender, EventArgs e)
        {
            try
            {
                // Store the current values temporarily in case they changed
                string server = tcServerTextBox.Text;
                int port = (int)tcPortNumeric.Value;
                string user = tcUserTextBox.Text;
                string password = tcPasswordTextBox.Text;

                // You would implement the actual connection test here
                MessageBox.Show($"Testing connection to Teamcenter server: {server}:{port} as user {user}...",
                               "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Placeholder for actual connection test
                // In a real implementation, you would create a TCConnection and try to connect
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection test failed: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            // Database settings
            databasePathSettings.Text = _settingsManager.GetSetting("/Settings/Database/Path");

            // NX settings
            nxInstallPathTextBox.Text = _settingsManager.GetSetting("/Settings/NX/InstallPath");
            nxVersionComboBox.Text = _settingsManager.GetSetting("/Settings/NX/Version");

            // Teamcenter settings
            tcServerTextBox.Text = _settingsManager.GetSetting("/Settings/Teamcenter/Server");

            int port;
            if (int.TryParse(_settingsManager.GetSetting("/Settings/Teamcenter/Port"), out port))
            {
                tcPortNumeric.Value = port;
            }
            else
            {
                tcPortNumeric.Value = 7001; // Default port
            }

            tcUserTextBox.Text = _settingsManager.GetSetting("/Settings/Teamcenter/User");
            // Note: Password will need special handling if it's encrypted

            nxWorkerPathTextBox.Text = _settingsManager.GetSetting("/Settings/NX/NXWorkerPath");
        }

        private void OnNXWorkerBrowseClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "DLL Files (*.dll)|*.dll|Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
            dialog.Title = "Select NX Worker File";
            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                nxWorkerPathTextBox.Text = dialog.FileName;
            }
        }
    }
}