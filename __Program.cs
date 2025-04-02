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
                // Application paths
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string settingsPath = Path.Combine(baseDir, "Settings.xml");
                string schemaPath = Path.Combine(baseDir, "SettingsSchema.xsd");
                string dbSchemaPath = Path.Combine(baseDir, "src", "Core", "Database", "database_schema.sql");

                // Create missing files if needed
                EnsureRequiredFiles(settingsPath, dbSchemaPath);

                // Initialize settings
                SettingsManager settingsManager = new SettingsManager(settingsPath, schemaPath);
                settingsManager.Initialize();

                // Initialize database
                string dbPath = settingsManager.GetSetting("/Settings/Database/Path");
                SQLiteManager dbManager = new SQLiteManager(dbPath, dbSchemaPath);
                dbManager.Initialize();

                // Start main form
                Application.Run(new cytk_nx2tcmigtool_form1(settingsManager));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void EnsureRequiredFiles(string settingsPath, string dbSchemaPath)
        {
            // Create the settings file from DefaultSettings.xml if it doesn't exist
            if (!File.Exists(settingsPath) && File.Exists("DefaultSettings.xml"))
            {
                File.Copy("DefaultSettings.xml", settingsPath);
            }

            // Create schema file if it doesn't exist
            if (!File.Exists(dbSchemaPath))
            {
                // Ensure directory exists
                string dbSchemaDir = Path.GetDirectoryName(dbSchemaPath);
                if (!Directory.Exists(dbSchemaDir))
                {
                    Directory.CreateDirectory(dbSchemaDir);
                }

                string schemaContent = @"-- PLMIntegration Database Schema

-- Parts table for storing part information
CREATE TABLE IF NOT EXISTS Parts (
    ID TEXT PRIMARY KEY,      -- Unique identifier for the part
    Name TEXT NOT NULL,       -- Name of the part
    Type TEXT NOT NULL,       -- Type/category of the part
    Source TEXT NOT NULL,     -- Source system (NX, Teamcenter, etc.)
    Metadata TEXT             -- JSON serialized metadata/attributes
);

-- Index for fast lookup by part name
CREATE INDEX IF NOT EXISTS idx_parts_name ON Parts(Name);

-- Index for filtering by source system
CREATE INDEX IF NOT EXISTS idx_parts_source ON Parts(Source);";

                File.WriteAllText(dbSchemaPath, schemaContent);
            }
        }
    }
}