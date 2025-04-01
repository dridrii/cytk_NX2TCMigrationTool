using System;
using System.IO;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Database;
using cytk_NX2TCMigrationTool.src.Core.Settings;
using cytk_NX2TCMigrationTool.src.UI.Windows;

namespace cytk_NX2TCMigrationTool
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Initialize settings
                string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings.xml");
                string schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SettingsSchema.xsd");

                SettingsManager settingsManager = new SettingsManager(settingsPath, schemaPath);
                settingsManager.Initialize();

                // Initialize database
                string dbPath = settingsManager.GetSetting("/Settings/Database/Path");
                SQLiteManager dbManager = new SQLiteManager(dbPath);
                dbManager.Initialize();

                // Start main form
                Application.Run(new MainForm(settingsManager));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}