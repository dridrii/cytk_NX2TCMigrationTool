using System;
using System.IO;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using cytk_NX2TCMigrationTool.src.Core.Database;
using cytk_NX2TCMigrationTool.src.Core.Database.Repositories;
using cytk_NX2TCMigrationTool.src.Core.Settings;

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
                // Initialize logger first so we can log the startup process
                Logger logger = Logger.Instance;
                logger.LogLevel = LogLevel.Debug; // Set initial log level to Debug
                logger.Info("Application", "Application starting");

                // Application paths
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string settingsPath = Path.Combine(baseDir, "Settings.xml");
                string schemaPath = Path.Combine(baseDir, "SettingsSchema.xsd");
                string dbSchemaPath = Path.Combine(baseDir, "src", "Core", "Database", "database_schema.sql");

                logger.Debug("Application", $"Base directory: {baseDir}");
                logger.Debug("Application", $"Settings path: {settingsPath}");
                logger.Debug("Application", $"Schema path: {schemaPath}");
                logger.Debug("Application", $"DB Schema path: {dbSchemaPath}");

                // Create data directory if it doesn't exist
                string dataDir = Path.Combine(baseDir, "Data");
                if (!Directory.Exists(dataDir))
                {
                    logger.Info("Application", $"Creating data directory: {dataDir}");
                    Directory.CreateDirectory(dataDir);
                }

                // Create missing files if needed
                EnsureRequiredFiles(settingsPath, dbSchemaPath);

                // Initialize settings
                logger.Info("Application", "Initializing settings manager");
                SettingsManager settingsManager = new SettingsManager(settingsPath, schemaPath);
                settingsManager.Initialize();

                // Initialize database
                logger.Info("Application", "Initializing database");
                string dbPath = settingsManager.GetSetting("/Settings/Database/Path");

                // If dbPath is relative, make it absolute
                if (!Path.IsPathRooted(dbPath))
                {
                    dbPath = Path.Combine(baseDir, dbPath);
                }

                logger.Debug("Application", $"Database path: {dbPath}");

                // Ensure database directory exists
                string dbDir = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dbDir))
                {
                    logger.Info("Application", $"Creating database directory: {dbDir}");
                    Directory.CreateDirectory(dbDir);
                }

                // Initialize SQLite database
                logger.Info("Application", "Initializing SQLite database");
                SQLiteManager dbManager = new SQLiteManager(dbPath, dbSchemaPath);
                dbManager.Initialize();

                // Register application exit handler to clean up the logger
                Application.ApplicationExit += (sender, e) => {
                    logger.Info("Application", "Application shutting down");
                    logger.Shutdown();
                };

                // Start the application form
                logger.Info("Application", "Starting main form");
                Application.Run(new cytk_nx2tcmigtool_form1(settingsManager));
            }
            catch (Exception ex)
            {
                // Try to log the error if the logger is initialized
                try
                {
                    Logger.Instance.Critical("Application", $"Unhandled exception: {ex.Message}\n{ex.StackTrace}");
                }
                catch
                {
                    // Ignore errors from logger
                }

                MessageBox.Show($"Error initializing application: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void EnsureRequiredFiles(string settingsPath, string dbSchemaPath)
        {
            Logger logger = Logger.Instance;

            // Create the settings file from DefaultSettings.xml if it doesn't exist
            if (!File.Exists(settingsPath) && File.Exists("DefaultSettings.xml"))
            {
                logger.Info("Application", $"Creating settings file from default template: {settingsPath}");
                File.Copy("DefaultSettings.xml", settingsPath);
            }

            // Create schema file if it doesn't exist
            if (!File.Exists(dbSchemaPath))
            {
                // Ensure directory exists
                string dbSchemaDir = Path.GetDirectoryName(dbSchemaPath);
                if (!Directory.Exists(dbSchemaDir))
                {
                    logger.Info("Application", $"Creating database schema directory: {dbSchemaDir}");
                    Directory.CreateDirectory(dbSchemaDir);
                }

                logger.Info("Application", $"Creating database schema file: {dbSchemaPath}");

                // Make sure this schema has all the required columns
                string schemaContent = @"-- PLMIntegration Database Schema

-- Parts table for storing part information
CREATE TABLE IF NOT EXISTS ""Parts"" (
    ""ID"" TEXT PRIMARY KEY,      -- Unique identifier for the part
    ""Name"" TEXT NOT NULL,       -- Name of the part
    ""Type"" TEXT NOT NULL,       -- Type/category of the part
    ""Source"" TEXT NOT NULL,     -- Source system (NX, Teamcenter, etc.)
    ""FilePath"" TEXT,            -- Full path to the file
    ""FileName"" TEXT,            -- Name of the file with extension
    ""Checksum"" TEXT,            -- SHA-256 checksum of the file
    ""IsDuplicate"" INTEGER DEFAULT 0, -- Flag for duplicate files (0=no, 1=yes)
    ""DuplicateOf"" TEXT,         -- ID of the original part this is a duplicate of
    ""Metadata"" TEXT             -- JSON serialized metadata/attributes
);

-- Index for fast lookup by part name
CREATE INDEX IF NOT EXISTS ""idx_parts_name"" ON ""Parts""(""Name"");

-- Index for filtering by source system
CREATE INDEX IF NOT EXISTS ""idx_parts_source"" ON ""Parts""(""Source"");

-- Index for checksum lookups (for duplicate detection)
CREATE INDEX IF NOT EXISTS ""idx_parts_checksum"" ON ""Parts""(""Checksum"");

-- Index for duplicate flag
CREATE INDEX IF NOT EXISTS ""idx_parts_duplicate"" ON ""Parts""(""IsDuplicate"");";

                File.WriteAllText(dbSchemaPath, schemaContent);
            }
        }
    }
}