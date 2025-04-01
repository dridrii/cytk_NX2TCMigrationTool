using System;
using System.Data.Common;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.PLM.NX;
using cytk_NX2TCMigrationTool.src.PLM.Teamcenter;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public partial class PartBrowser : Form
    {
        private readonly NXConnection _nxConnection;
        private readonly TCConnection _tcConnection;

        public PartBrowser(NXConnection nxConnection, TCConnection tcConnection)
        {
            _nxConnection = nxConnection;
            _tcConnection = tcConnection;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Part Browser";
            this.Size = new System.Drawing.Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            // Create split container
            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Vertical;

            // Left panel - Tree view of part hierarchy
            TreeView treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            splitContainer.Panel1.Controls.Add(treeView);

            // Right panel - Part details
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // Basic info tab
            TabPage basicInfoTab = new TabPage("Basic Info");
            PropertyGrid propertyGrid = new PropertyGrid();
            propertyGrid.Dock = DockStyle.Fill;
            basicInfoTab.Controls.Add(propertyGrid);

            // Preview tab
            TabPage previewTab = new TabPage("Preview");

            tabControl.TabPages.Add(basicInfoTab);
            tabControl.TabPages.Add(previewTab);

            splitContainer.Panel2.Controls.Add(tabControl);

            this.Controls.Add(splitContainer);

            // Add toolbar
            ToolStrip toolStrip = new ToolStrip();
            toolStrip.Items.Add("Refresh", null, OnRefreshClick);
            toolStrip.Items.Add("Export", null, OnExportClick);

            this.Controls.Add(toolStrip);
            toolStrip.Dock = DockStyle.Top;

            // Position split container below toolbar
            splitContainer.Top = toolStrip.Height;
            splitContainer.Height -= toolStrip.Height;
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            // Refresh part list
            MessageBox.Show("Refreshing part list...");
        }

        private void OnExportClick(object sender, EventArgs e)
        {
            // Export selected part
            MessageBox.Show("Exporting selected part...");
        }
    }
}