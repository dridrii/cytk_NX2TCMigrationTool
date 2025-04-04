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
            tcTab = new TabPage();
            buttonPanel = new FlowLayoutPanel();
            cancelButton = new Button();
            okButton = new Button();
            mainLayout.SuspendLayout();
            tabControl.SuspendLayout();
            databaseTab.SuspendLayout();
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
            databaseSettingsBrowseButton.Location = new Point(629, 32);
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
            databaseSettingsLabel.Location = new Point(6, 36);
            databaseSettingsLabel.Name = "databaseSettingsLabel";
            databaseSettingsLabel.Size = new Size(85, 15);
            databaseSettingsLabel.TabIndex = 0;
            databaseSettingsLabel.Text = "Database Path:";
            // 
            // nxTab
            // 
            nxTab.Location = new Point(4, 24);
            nxTab.Name = "nxTab";
            nxTab.Padding = new Padding(3);
            nxTab.Size = new Size(751, 404);
            nxTab.TabIndex = 1;
            nxTab.Text = "NX";
            nxTab.UseVisualStyleBackColor = true;
            // 
            // tcTab
            // 
            tcTab.Location = new Point(4, 24);
            tcTab.Name = "tcTab";
            tcTab.Padding = new Padding(3);
            tcTab.Size = new Size(751, 404);
            tcTab.TabIndex = 2;
            tcTab.Text = "Teamcenter";
            tcTab.UseVisualStyleBackColor = true;
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
    }
}