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
            components = new System.ComponentModel.Container();
            mainPanel = new TableLayoutPanel();
            topPanel = new Panel();
            logFileLabel = new Label();
            _logFileComboBox = new ComboBox();
            logLevelLabel = new Label();
            _logLevelComboBox = new ComboBox();
            _autoRefreshCheckBox = new CheckBox();
            _refreshButton = new Button();
            _logTextBox = new TextBox();
            bottomPanel = new Panel();
            _clearButton = new Button();
            _closeButton = new Button();
            _refreshTimer = new System.Windows.Forms.Timer(components);
            mainPanel.SuspendLayout();
            topPanel.SuspendLayout();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.ColumnCount = 1;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            mainPanel.Controls.Add(topPanel, 0, 0);
            mainPanel.Controls.Add(_logTextBox, 0, 1);
            mainPanel.Controls.Add(bottomPanel, 0, 2);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.RowCount = 3;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainPanel.Size = new Size(936, 585);
            mainPanel.TabIndex = 0;
            // 
            // topPanel
            // 
            topPanel.Controls.Add(logFileLabel);
            topPanel.Controls.Add(_logFileComboBox);
            topPanel.Controls.Add(logLevelLabel);
            topPanel.Controls.Add(_logLevelComboBox);
            topPanel.Controls.Add(_autoRefreshCheckBox);
            topPanel.Controls.Add(_refreshButton);
            topPanel.Dock = DockStyle.Fill;
            topPanel.Location = new Point(3, 3);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(930, 34);
            topPanel.TabIndex = 0;
            // 
            // logFileLabel
            // 
            logFileLabel.AutoSize = true;
            logFileLabel.Location = new Point(10, 12);
            logFileLabel.Name = "logFileLabel";
            logFileLabel.Size = new Size(51, 15);
            logFileLabel.TabIndex = 0;
            logFileLabel.Text = "Log File:";
            // 
            // _logFileComboBox
            // 
            _logFileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _logFileComboBox.Location = new Point(80, 8);
            _logFileComboBox.Name = "_logFileComboBox";
            _logFileComboBox.Size = new Size(250, 23);
            _logFileComboBox.TabIndex = 1;
            // 
            // logLevelLabel
            // 
            logLevelLabel.AutoSize = true;
            logLevelLabel.Location = new Point(350, 12);
            logLevelLabel.Name = "logLevelLabel";
            logLevelLabel.Size = new Size(60, 15);
            logLevelLabel.TabIndex = 2;
            logLevelLabel.Text = "Log Level:";
            // 
            // _logLevelComboBox
            // 
            _logLevelComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _logLevelComboBox.Items.AddRange(new object[] { "Trace", "Debug", "Info", "Warning", "Error", "Critical", "None" });
            _logLevelComboBox.Location = new Point(420, 8);
            _logLevelComboBox.Name = "_logLevelComboBox";
            _logLevelComboBox.Size = new Size(120, 23);
            _logLevelComboBox.TabIndex = 3;
            // 
            // _autoRefreshCheckBox
            // 
            _autoRefreshCheckBox.AutoSize = true;
            _autoRefreshCheckBox.Checked = true;
            _autoRefreshCheckBox.CheckState = CheckState.Checked;
            _autoRefreshCheckBox.Location = new Point(560, 10);
            _autoRefreshCheckBox.Name = "_autoRefreshCheckBox";
            _autoRefreshCheckBox.Size = new Size(94, 19);
            _autoRefreshCheckBox.TabIndex = 4;
            _autoRefreshCheckBox.Text = "Auto Refresh";
            // 
            // _refreshButton
            // 
            _refreshButton.Location = new Point(670, 8);
            _refreshButton.Name = "_refreshButton";
            _refreshButton.Size = new Size(80, 23);
            _refreshButton.TabIndex = 5;
            _refreshButton.Text = "Refresh";
            // 
            // _logTextBox
            // 
            _logTextBox.BackColor = Color.White;
            _logTextBox.Dock = DockStyle.Fill;
            _logTextBox.Font = new Font("Consolas", 9F);
            _logTextBox.Location = new Point(3, 43);
            _logTextBox.Multiline = true;
            _logTextBox.Name = "_logTextBox";
            _logTextBox.ReadOnly = true;
            _logTextBox.ScrollBars = ScrollBars.Both;
            _logTextBox.Size = new Size(930, 499);
            _logTextBox.TabIndex = 1;
            _logTextBox.WordWrap = false;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(_clearButton);
            bottomPanel.Controls.Add(_closeButton);
            bottomPanel.Dock = DockStyle.Fill;
            bottomPanel.Location = new Point(3, 548);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(930, 34);
            bottomPanel.TabIndex = 2;
            // 
            // _clearButton
            // 
            _clearButton.Location = new Point(670, 8);
            _clearButton.Name = "_clearButton";
            _clearButton.Size = new Size(80, 23);
            _clearButton.TabIndex = 0;
            _clearButton.Text = "Clear";
            // 
            // _closeButton
            // 
            _closeButton.DialogResult = DialogResult.Cancel;
            _closeButton.Location = new Point(770, 8);
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new Size(80, 23);
            _closeButton.TabIndex = 1;
            _closeButton.Text = "Close";
            // 
            // LogViewerForm
            // 
            CancelButton = _closeButton;
            ClientSize = new Size(936, 585);
            Controls.Add(mainPanel);
            Name = "LogViewerForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Log Viewer";
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            bottomPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel mainPanel;
        private Panel topPanel;
        private Label logFileLabel;
        private Label logLevelLabel;
        private Panel bottomPanel;
    }
}