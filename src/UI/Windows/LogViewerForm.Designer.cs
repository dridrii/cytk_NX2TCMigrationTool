namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    partial class LogViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox _logTextBox;
        private System.Windows.Forms.ComboBox _logLevelComboBox;
        private System.Windows.Forms.Button _refreshButton;
        private System.Windows.Forms.Button _clearButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.CheckBox _autoRefreshCheckBox;
        private System.Windows.Forms.Timer _refreshTimer;
        private System.Windows.Forms.ComboBox _logFileComboBox;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (_refreshTimer != null)
                {
                    _refreshTimer.Stop();
                    _refreshTimer.Dispose();
                }
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

            this.Text = "Log Viewer";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // Main layout panel
            System.Windows.Forms.TableLayoutPanel mainPanel = new System.Windows.Forms.TableLayoutPanel();
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.RowCount = 3;
            mainPanel.ColumnCount = 1;

            mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));

            this.Controls.Add(mainPanel);

            // Top panel for controls
            System.Windows.Forms.Panel topPanel = new System.Windows.Forms.Panel();
            topPanel.Dock = System.Windows.Forms.DockStyle.Fill;

            // Log file selection
            System.Windows.Forms.Label logFileLabel = new System.Windows.Forms.Label();
            logFileLabel.Text = "Log File:";
            logFileLabel.AutoSize = true;
            logFileLabel.Location = new System.Drawing.Point(10, 12);
            topPanel.Controls.Add(logFileLabel);

            _logFileComboBox = new System.Windows.Forms.ComboBox();
            _logFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _logFileComboBox.Width = 250;
            _logFileComboBox.Location = new System.Drawing.Point(80, 8);
            topPanel.Controls.Add(_logFileComboBox);

            // Log level filter
            System.Windows.Forms.Label logLevelLabel = new System.Windows.Forms.Label();
            logLevelLabel.Text = "Log Level:";
            logLevelLabel.AutoSize = true;
            logLevelLabel.Location = new System.Drawing.Point(350, 12);
            topPanel.Controls.Add(logLevelLabel);

            _logLevelComboBox = new System.Windows.Forms.ComboBox();
            _logLevelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _logLevelComboBox.Width = 120;
            _logLevelComboBox.Location = new System.Drawing.Point(420, 8);

            // Add log levels to the combo box
            _logLevelComboBox.Items.Add("Trace");
            _logLevelComboBox.Items.Add("Debug");
            _logLevelComboBox.Items.Add("Info");
            _logLevelComboBox.Items.Add("Warning");
            _logLevelComboBox.Items.Add("Error");
            _logLevelComboBox.Items.Add("Critical");
            _logLevelComboBox.Items.Add("None");

            // Default to Info level
            _logLevelComboBox.SelectedIndex = 2; // Info
            topPanel.Controls.Add(_logLevelComboBox);

            // Auto refresh checkbox
            _autoRefreshCheckBox = new System.Windows.Forms.CheckBox();
            _autoRefreshCheckBox.Text = "Auto Refresh";
            _autoRefreshCheckBox.Checked = true;
            _autoRefreshCheckBox.Location = new System.Drawing.Point(560, 10);
            _autoRefreshCheckBox.AutoSize = true;
            topPanel.Controls.Add(_autoRefreshCheckBox);

            // Refresh button
            _refreshButton = new System.Windows.Forms.Button();
            _refreshButton.Text = "Refresh";
            _refreshButton.Location = new System.Drawing.Point(670, 8);
            _refreshButton.Width = 80;
            topPanel.Controls.Add(_refreshButton);

            mainPanel.Controls.Add(topPanel, 0, 0);

            // Log text box
            _logTextBox = new System.Windows.Forms.TextBox();
            _logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            _logTextBox.Multiline = true;
            _logTextBox.ReadOnly = true;
            _logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            _logTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            _logTextBox.BackColor = System.Drawing.Color.White;
            _logTextBox.WordWrap = false;

            mainPanel.Controls.Add(_logTextBox, 0, 1);

            // Bottom panel for buttons
            System.Windows.Forms.Panel bottomPanel = new System.Windows.Forms.Panel();
            bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;

            // Clear button
            _clearButton = new System.Windows.Forms.Button();
            _clearButton.Text = "Clear";
            _clearButton.Location = new System.Drawing.Point(670, 8);
            _clearButton.Width = 80;
            bottomPanel.Controls.Add(_clearButton);

            // Close button
            _closeButton = new System.Windows.Forms.Button();
            _closeButton.Text = "Close";
            _closeButton.Location = new System.Drawing.Point(770, 8);
            _closeButton.Width = 80;
            _closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            bottomPanel.Controls.Add(_closeButton);

            mainPanel.Controls.Add(bottomPanel, 0, 2);

            // Set as cancel button
            this.CancelButton = _closeButton;

            // Create timer for auto-refresh - this will be set up in the main form
            _refreshTimer = new System.Windows.Forms.Timer(this.components);
        }

        #endregion
    }
}