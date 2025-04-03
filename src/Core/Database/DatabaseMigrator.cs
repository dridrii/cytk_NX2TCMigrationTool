using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Text;
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
                // First check if we need to add the new columns
                if (!DoesColumnExist("Parts", "FilePath"))
                {
                    _logger.Info("DatabaseMigrator", "Adding new columns to Parts table");

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

                    _logger.Info("DatabaseMigrator", "Database migration completed successfully");
                    return true;
                }

                _logger.Info("DatabaseMigrator", "Database already up to date, no migration needed");
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