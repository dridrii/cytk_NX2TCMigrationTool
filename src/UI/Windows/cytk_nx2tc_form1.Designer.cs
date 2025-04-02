namespace cytk_NX2TCMigrationTool
{
    partial class cytk_nx2tcmigtool_form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            tabPage3 = new TabPage();
            tabPage4 = new TabPage();
            tabPage5 = new TabPage();
            tabPage6 = new TabPage();
            tabPage7 = new TabPage();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            connectToNXToolStripMenuItem = new ToolStripMenuItem();
            connectToTeamcenterToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            browsePartsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            configToolStripMenuItem = new ToolStripMenuItem();
            sessionSettingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            progressBar1 = new ProgressBar();
            tabControl1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Controls.Add(tabPage7);
            tabControl1.Location = new Point(70, 56);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(933, 470);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(925, 442);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "MAIN";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(925, 442);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "BOM";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(925, 442);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "ORGANISATION";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(925, 442);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "ATTRIBUTES";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(925, 442);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "VERIFICATION";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            tabPage6.Location = new Point(4, 24);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3);
            tabPage6.Size = new Size(925, 442);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "LOG FILE";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // tabPage7
            // 
            tabPage7.Location = new Point(4, 24);
            tabPage7.Name = "tabPage7";
            tabPage7.Padding = new Padding(3);
            tabPage7.Size = new Size(925, 442);
            tabPage7.TabIndex = 6;
            tabPage7.Text = "TERMINAL";
            tabPage7.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, configToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1064, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { connectToNXToolStripMenuItem, connectToTeamcenterToolStripMenuItem, toolStripSeparator1, browsePartsToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // connectToNXToolStripMenuItem
            // 
            connectToNXToolStripMenuItem.Name = "connectToNXToolStripMenuItem";
            connectToNXToolStripMenuItem.Size = new Size(197, 22);
            connectToNXToolStripMenuItem.Text = "Connect to NX";
            // 
            // connectToTeamcenterToolStripMenuItem
            // 
            connectToTeamcenterToolStripMenuItem.Name = "connectToTeamcenterToolStripMenuItem";
            connectToTeamcenterToolStripMenuItem.Size = new Size(197, 22);
            connectToTeamcenterToolStripMenuItem.Text = "Connect to Teamcenter";
            connectToTeamcenterToolStripMenuItem.Click += OnConnectTCClick;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(194, 6);
            // 
            // browsePartsToolStripMenuItem
            // 
            browsePartsToolStripMenuItem.Name = "browsePartsToolStripMenuItem";
            browsePartsToolStripMenuItem.Size = new Size(197, 22);
            browsePartsToolStripMenuItem.Text = "Browse Parts";
            browsePartsToolStripMenuItem.Click += OnBrowsePartsClick;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(194, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(197, 22);
            exitToolStripMenuItem.Text = "Exit";
            // 
            // configToolStripMenuItem
            // 
            configToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sessionSettingsToolStripMenuItem });
            configToolStripMenuItem.Name = "configToolStripMenuItem";
            configToolStripMenuItem.Size = new Size(55, 20);
            configToolStripMenuItem.Text = "Config";
            // 
            // sessionSettingsToolStripMenuItem
            // 
            sessionSettingsToolStripMenuItem.Name = "sessionSettingsToolStripMenuItem";
            sessionSettingsToolStripMenuItem.Size = new Size(180, 22);
            sessionSettingsToolStripMenuItem.Text = "Session Settings";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 27);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(1040, 23);
            progressBar1.Step = 100;
            progressBar1.TabIndex = 2;
            progressBar1.UseWaitCursor = true;
            // 
            // cytk_nx2tcmigtool_form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1064, 628);
            Controls.Add(progressBar1);
            Controls.Add(tabControl1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "cytk_nx2tcmigtool_form1";
            Text = "NX 2 TC Migration tool V0.0.1";
            tabControl1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem configToolStripMenuItem;
        private ToolStripMenuItem sessionSettingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ProgressBar progressBar1;
        private TabPage tabPage6;
        private TabPage tabPage7;
        private ToolStripMenuItem connectToNXToolStripMenuItem;
        private ToolStripMenuItem connectToTeamcenterToolStripMenuItem;
        private ToolStripMenuItem browsePartsToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
    }
}
