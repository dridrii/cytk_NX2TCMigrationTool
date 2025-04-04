using System;
using System.Drawing;
using System.Windows.Forms;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    partial class SettingsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            mainLayout = new TableLayoutPanel();
            tabControl = new TabControl();
            databaseTab = new TabPage();
            databaseSettingsBrowseButton = new Button();
            databasePathSettings = new TextBox();
            databaseSettingsLabel = new Label();
            nxTab = new TabPage();
            nxInstallPathBrowseButton = new Button();
            nxVersionComboBox = new ComboBox();
            nxVersionLabel = new Label();
            nxInstallPathTextBox = new TextBox();
            nxInstallPathLabel = new Label();
            tcTab = new TabPage();
            tcTestConnectionButton = new Button();
            tcPasswordTextBox = new TextBox();
            tcPasswordLabel = new Label();
            tcUserTextBox = new TextBox();
            tcUserLabel = new Label();
            tcPortNumeric = new NumericUpDown();
            tcPortLabel = new Label();
            tcServerTextBox = new TextBox();
            tcServerLabel = new Label();
            buttonPanel = new FlowLayoutPanel();
            cancelButton = new Button();
            okButton = new Button();
            mainLayout.SuspendLayout();
            tabControl.SuspendLayout();
            databaseTab.SuspendLayout();
            nxTab.SuspendLayout();
            tcTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tcPortNumeric).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(tabControl, 0, 0);
            mainLayout.Controls.Add(buttonPanel, 0, 1);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.RowCount = 2;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 87.00787F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12.9921255F));
            mainLayout.Size = new Size(765, 504);
            mainLayout.TabIndex = 0;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(databaseTab);
            tabControl.Controls.Add(nxTab);
            tabControl.Controls.Add(tcTab);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(3, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(759, 432);
            tabControl.TabIndex = 0;
            // 
            // databaseTab
            // 
            databaseTab.Controls.Add(databaseSettingsBrowseButton);
            databaseTab.Controls.Add(databasePathSettings);
            databaseTab.Controls.Add(databaseSettingsLabel);
            databaseTab.Location = new Point(4, 24);
            databaseTab.Name = "databaseTab";
            databaseTab.Padding = new Padding(3);
            databaseTab.Size = new Size(751, 404);
            databaseTab.TabIndex = 0;
            databaseTab.Text = "Database";
            databaseTab.UseVisualStyleBackColor = true;
            // 
            // databaseSettingsBrowseButton
            // 
            databaseSettingsBrowseButton.Location = new Point(629, 33);
            databaseSettingsBrowseButton.Name = "databaseSettingsBrowseButton";
            databaseSettingsBrowseButton.Size = new Size(103, 23);
            databaseSettingsBrowseButton.TabIndex = 2;
            databaseSettingsBrowseButton.Text = "Browse...";
            databaseSettingsBrowseButton.UseVisualStyleBackColor = true;
            databaseSettingsBrowseButton.Click += OnBrowseClick1;
            // 
            // databasePathSettings
            // 
            databasePathSettings.Location = new Point(97, 33);
            databasePathSettings.Name = "databasePathSettings";
            databasePathSettings.Size = new Size(526, 23);
            databasePathSettings.TabIndex = 1;
            databasePathSettings.Tag = "/Settings/Database/Path";
            // 
            // databaseSettingsLabel
            // 
            databaseSettingsLabel.AutoSize = true;
            databaseSettingsLabel.Location = new Point(9, 39);
            databaseSettingsLabel.Name = "databaseSettingsLabel";
            databaseSettingsLabel.Size = new Size(85, 15);
            databaseSettingsLabel.TabIndex = 0;
            databaseSettingsLabel.Text = "Database Path:";
            // 
            // nxTab
            // 
            nxTab.Controls.Add(nxInstallPathBrowseButton);
            nxTab.Controls.Add(nxVersionComboBox);
            nxTab.Controls.Add(nxVersionLabel);
            nxTab.Controls.Add(nxInstallPathTextBox);
            nxTab.Controls.Add(nxInstallPathLabel);
            nxTab.Location = new Point(4, 24);
            nxTab.Name = "nxTab";
            nxTab.Padding = new Padding(3);
            nxTab.Size = new Size(751, 404);
            nxTab.TabIndex = 1;
            nxTab.Text = "NX";
            nxTab.UseVisualStyleBackColor = true;
            // 
            // nxInstallPathBrowseButton
            // 
            nxInstallPathBrowseButton.Location = new Point(629, 33);
            nxInstallPathBrowseButton.Name = "nxInstallPathBrowseButton";
            nxInstallPathBrowseButton.Size = new Size(103, 23);
            nxInstallPathBrowseButton.TabIndex = 2;
            nxInstallPathBrowseButton.Text = "Browse...";
            nxInstallPathBrowseButton.UseVisualStyleBackColor = true;
            nxInstallPathBrowseButton.Click += OnNXBrowseClick;
            // 
            // nxVersionComboBox
            // 
            nxVersionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            nxVersionComboBox.FormattingEnabled = true;
            nxVersionComboBox.Items.AddRange(new object[] { "NX 1847", "NX 1872", "NX 1899", "NX 1919", "NX 1926", "NX 1980" });
            nxVersionComboBox.Location = new Point(97, 63);
            nxVersionComboBox.Name = "nxVersionComboBox";
            nxVersionComboBox.Size = new Size(167, 23);
            nxVersionComboBox.TabIndex = 4;
            nxVersionComboBox.Tag = "/Settings/NX/Version";
            // 
            // nxVersionLabel
            // 
            nxVersionLabel.AutoSize = true;
            nxVersionLabel.Location = new Point(9, 69);
            nxVersionLabel.Name = "nxVersionLabel";
            nxVersionLabel.Size = new Size(48, 15);
            nxVersionLabel.TabIndex = 3;
            nxVersionLabel.Text = "Version:";
            // 
            // nxInstallPathTextBox
            // 
            nxInstallPathTextBox.Location = new Point(97, 33);
            nxInstallPathTextBox.Name = "nxInstallPathTextBox";
            nxInstallPathTextBox.Size = new Size(526, 23);
            nxInstallPathTextBox.TabIndex = 1;
            nxInstallPathTextBox.Tag = "/Settings/NX/InstallPath";
            // 
            // nxInstallPathLabel
            // 
            nxInstallPathLabel.AutoSize = true;
            nxInstallPathLabel.Location = new Point(9, 39);
            nxInstallPathLabel.Name = "nxInstallPathLabel";
            nxInstallPathLabel.Size = new Size(68, 15);
            nxInstallPathLabel.TabIndex = 0;
            nxInstallPathLabel.Text = "Install Path:";
            // 
            // tcTab
            // 
            tcTab.Controls.Add(tcTestConnectionButton);
            tcTab.Controls.Add(tcPasswordTextBox);
            tcTab.Controls.Add(tcPasswordLabel);
            tcTab.Controls.Add(tcUserTextBox);
            tcTab.Controls.Add(tcUserLabel);
            tcTab.Controls.Add(tcPortNumeric);
            tcTab.Controls.Add(tcPortLabel);
            tcTab.Controls.Add(tcServerTextBox);
            tcTab.Controls.Add(tcServerLabel);
            tcTab.Location = new Point(4, 24);
            tcTab.Name = "tcTab";
            tcTab.Padding = new Padding(3);
            tcTab.Size = new Size(751, 404);
            tcTab.TabIndex = 2;
            tcTab.Text = "Teamcenter";
            tcTab.UseVisualStyleBackColor = true;
            // 
            // tcTestConnectionButton
            // 
            tcTestConnectionButton.Location = new Point(97, 163);
            tcTestConnectionButton.Name = "tcTestConnectionButton";
            tcTestConnectionButton.Size = new Size(120, 23);
            tcTestConnectionButton.TabIndex = 8;
            tcTestConnectionButton.Text = "Test Connection";
            tcTestConnectionButton.UseVisualStyleBackColor = true;
            tcTestConnectionButton.Click += OnTestTCConnectionClick;
            // 
            // tcPasswordTextBox
            // 
            tcPasswordTextBox.Location = new Point(97, 123);
            tcPasswordTextBox.Name = "tcPasswordTextBox";
            tcPasswordTextBox.PasswordChar = '*';
            tcPasswordTextBox.Size = new Size(255, 23);
            tcPasswordTextBox.TabIndex = 7;
            tcPasswordTextBox.Tag = "/Settings/Teamcenter/EncryptedPassword";
            // 
            // tcPasswordLabel
            // 
            tcPasswordLabel.AutoSize = true;
            tcPasswordLabel.Location = new Point(9, 129);
            tcPasswordLabel.Name = "tcPasswordLabel";
            tcPasswordLabel.Size = new Size(60, 15);
            tcPasswordLabel.TabIndex = 6;
            tcPasswordLabel.Text = "Password:";
            // 
            // tcUserTextBox
            // 
            tcUserTextBox.Location = new Point(97, 93);
            tcUserTextBox.Name = "tcUserTextBox";
            tcUserTextBox.Size = new Size(255, 23);
            tcUserTextBox.TabIndex = 5;
            tcUserTextBox.Tag = "/Settings/Teamcenter/User";
            // 
            // tcUserLabel
            // 
            tcUserLabel.AutoSize = true;
            tcUserLabel.Location = new Point(9, 99);
            tcUserLabel.Name = "tcUserLabel";
            tcUserLabel.Size = new Size(63, 15);
            tcUserLabel.TabIndex = 4;
            tcUserLabel.Text = "Username:";
            // 
            // tcPortNumeric
            // 
            tcPortNumeric.Location = new Point(97, 64);
            tcPortNumeric.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            tcPortNumeric.Name = "tcPortNumeric";
            tcPortNumeric.Size = new Size(120, 23);
            tcPortNumeric.TabIndex = 3;
            tcPortNumeric.Tag = "/Settings/Teamcenter/Port";
            tcPortNumeric.Value = new decimal(new int[] { 7001, 0, 0, 0 });
            // 
            // tcPortLabel
            // 
            tcPortLabel.AutoSize = true;
            tcPortLabel.Location = new Point(9, 69);
            tcPortLabel.Name = "tcPortLabel";
            tcPortLabel.Size = new Size(32, 15);
            tcPortLabel.TabIndex = 2;
            tcPortLabel.Text = "Port:";
            // 
            // tcServerTextBox
            // 
            tcServerTextBox.Location = new Point(97, 33);
            tcServerTextBox.Name = "tcServerTextBox";
            tcServerTextBox.Size = new Size(255, 23);
            tcServerTextBox.TabIndex = 1;
            tcServerTextBox.Tag = "/Settings/Teamcenter/Server";
            // 
            // tcServerLabel
            // 
            tcServerLabel.AutoSize = true;
            tcServerLabel.Location = new Point(9, 39);
            tcServerLabel.Name = "tcServerLabel";
            tcServerLabel.Size = new Size(42, 15);
            tcServerLabel.TabIndex = 0;
            tcServerLabel.Text = "Server:";
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(3, 441);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(10);
            buttonPanel.Size = new Size(759, 60);
            buttonPanel.TabIndex = 1;
            // 
            // cancelButton
            // 
            cancelButton.AutoSize = true;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(661, 13);
            cancelButton.Name = "cancelButton";
            cancelButton.Padding = new Padding(5);
            cancelButton.Size = new Size(75, 35);
            cancelButton.TabIndex = 0;
            cancelButton.Text = "Cancel";
            // 
            // okButton
            // 
            okButton.AutoSize = true;
            okButton.DialogResult = DialogResult.OK;
            okButton.Location = new Point(580, 13);
            okButton.Name = "okButton";
            okButton.Padding = new Padding(5);
            okButton.Size = new Size(75, 35);
            okButton.TabIndex = 1;
            okButton.Text = "OK";
            okButton.Click += OnOKClick;
            // 
            // SettingsDialog
            // 
            AcceptButton = okButton;
            CancelButton = cancelButton;
            ClientSize = new Size(765, 504);
            Controls.Add(mainLayout);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            Load += SettingsDialog_Load;
            mainLayout.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            databaseTab.ResumeLayout(false);
            databaseTab.PerformLayout();
            nxTab.ResumeLayout(false);
            nxTab.PerformLayout();
            tcTab.ResumeLayout(false);
            tcTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)tcPortNumeric).EndInit();
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        // Fields accessible to the main code
        private TabControl tabControl;
        private Button okButton;
        private Button cancelButton;
        private TableLayoutPanel mainLayout;
        private TabPage databaseTab;
        private TabPage nxTab;
        private TabPage tcTab;
        private FlowLayoutPanel buttonPanel;
        private Label databaseSettingsLabel;
        private TextBox databasePathSettings;
        private Button databaseSettingsBrowseButton;

        // NX Tab Controls
        private Label nxInstallPathLabel;
        private TextBox nxInstallPathTextBox;
        private Button nxInstallPathBrowseButton;
        private Label nxVersionLabel;
        private ComboBox nxVersionComboBox;

        // Teamcenter Tab Controls
        private Label tcServerLabel;
        private TextBox tcServerTextBox;
        private Label tcPortLabel;
        private NumericUpDown tcPortNumeric;
        private Label tcUserLabel;
        private TextBox tcUserTextBox;
        private Label tcPasswordLabel;
        private TextBox tcPasswordTextBox;
        private Button tcTestConnectionButton;
    }
}