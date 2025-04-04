using System;
using System.Data.SQLite;
using System.IO;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;

namespace cytk_NX2TCMigrationTool.src.Core.Database
{
    public class DatabaseMigrator
    {
        private readonly string _connectionString;
        private readonly Logger _logger;

        public DatabaseMigrator(string connectionString)
        {
            _connectionString = connectionString;
            _logger = Logger.Instance;
        }

        /// <summary>
        /// Migrates the database to the latest schema
        /// </summary>
        public bool MigrateDatabase()
        {
            _logger.Info("DatabaseMigrator", "Starting database migration");

            try
            {
                // Apply migrations in order
                if (!DoesColumnExist("Parts", "FilePath"))
                {
                    ApplyFilepathMigration();
                }

                if (!DoesTableExist("BOM_Relationships"))
                {
                    ApplyBOMMigration();
                }

                _logger.Info("DatabaseMigrator", "Database migration completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("DatabaseMigrator", $"Error migrating database: {ex.Message}");
                _logger.Debug("DatabaseMigrator", $"Exception details: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Applies the filepath migration (adds filepath, filename, etc. columns)
        /// </summary>
        private void ApplyFilepathMigration()
        {
            _logger.Info("DatabaseMigrator", "Adding file path columns to Parts table");

            // Add the new columns
            ExecuteSql(@"
                ALTER TABLE ""Parts"" ADD COLUMN ""FilePath"" TEXT;
                ALTER TABLE ""Parts"" ADD COLUMN ""FileName"" TEXT;
                ALTER TABLE ""Parts"" ADD COLUMN ""Checksum"" TEXT;
                ALTER TABLE ""Parts"" ADD COLUMN ""IsDuplicate"" INTEGER DEFAULT 0;
                ALTER TABLE ""Parts"" ADD COLUMN ""DuplicateOf"" TEXT;
            ");

            // Add the new indexes
            ExecuteSql(@"
                CREATE INDEX IF NOT EXISTS ""idx_parts_checksum"" ON ""Parts""(""Checksum"");
                CREATE INDEX IF NOT EXISTS ""idx_parts_duplicate"" ON ""Parts""(""IsDuplicate"");
            ");

            _logger.Info("DatabaseMigrator", "Filepath migration completed successfully");
        }

        /// <summary>
        /// Applies the BOM migration (adds BOM_Relationships and AssemblyStats tables)
        /// </summary>
        private void ApplyBOMMigration()
        {
            _logger.Info("DatabaseMigrator", "Adding BOM tables");

            // Create BOM_Relationships table
            ExecuteSql(@"
                CREATE TABLE IF NOT EXISTS ""BOM_Relationships"" (
                    ""ID"" TEXT PRIMARY KEY,
                    ""ParentID"" TEXT NOT NULL,
                    ""ChildID"" TEXT NOT NULL,
                    ""RelationType"" TEXT NOT NULL,
                    ""InstanceName"" TEXT,
                    ""Position"" INTEGER,
                    ""Quantity"" INTEGER DEFAULT 1,
                    ""Verified"" INTEGER DEFAULT 0,
                    ""LastUpdated"" TEXT,
                    FOREIGN KEY (""ParentID"") REFERENCES ""Parts""(""ID""),
                    FOREIGN KEY (""ChildID"") REFERENCES ""Parts""(""ID"")
                );
            ");

            // Create indexes for BOM_Relationships
            ExecuteSql(@"
                CREATE INDEX IF NOT EXISTS ""idx_bom_parent"" ON ""BOM_Relationships""(""ParentID"");
                CREATE INDEX IF NOT EXISTS ""idx_bom_child"" ON ""BOM_Relationships""(""ChildID"");
                CREATE INDEX IF NOT EXISTS ""idx_bom_reltype"" ON ""BOM_Relationships""(""RelationType"");
            ");

            // Create AssemblyStats table
            ExecuteSql(@"
                CREATE TABLE IF NOT EXISTS ""AssemblyStats"" (
                    ""PartID"" TEXT PRIMARY KEY,
                    ""IsAssembly"" INTEGER DEFAULT 0,
                    ""IsDrafting"" INTEGER DEFAULT 0,
                    ""ComponentCount"" INTEGER DEFAULT 0,
                    ""TotalComponentCount"" INTEGER DEFAULT 0,
                    ""AssemblyDepth"" INTEGER DEFAULT 0,
                    ""ParentCount"" INTEGER DEFAULT 0,
                    ""LastAnalyzed"" TEXT,
                    FOREIGN KEY (""PartID"") REFERENCES ""Parts""(""ID"")
                );
            ");

            // Create index for AssemblyStats
            ExecuteSql(@"
                CREATE INDEX IF NOT EXISTS ""idx_assembly_stats"" ON ""AssemblyStats""(""IsAssembly"", ""ComponentCount"");
            ");

            _logger.Info("DatabaseMigrator", "BOM migration completed successfully");
        }

        /// <summary>
        /// Checks if a column exists in a table
        /// </summary>
        private bool DoesColumnExist(string tableName, string columnName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Get table info
                using (var command = new SQLiteCommand($"PRAGMA table_info(\"{tableName}\")", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(1); // Column 1 is the name
                            if (name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a table exists in the database
        /// </summary>
        private bool DoesTableExist(string tableName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Query sqlite_master table to check if the table exists
                using (var command = new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName", connection))
                {
                    command.Parameters.AddWithValue("@tableName", tableName);
                    var result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }

        /// <summary>
        /// Executes an SQL statement
        /// </summary>
        private void ExecuteSql(string sql)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}