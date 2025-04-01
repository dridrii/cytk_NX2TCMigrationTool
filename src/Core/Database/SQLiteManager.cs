using System;
using System.Data.SQLite;

namespace cytk_NX2TCMigrationTool.src.Core.Database
{
    public class SQLiteManager
    {
        private readonly string _connectionString;

        public SQLiteManager(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        public void Initialize()
        {
            // Create database if it doesn't exist
            SQLiteConnection.CreateFile("PLMIntegration.db");

            // Create necessary tables
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS Parts (
                        ID TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Type TEXT NOT NULL,
                        Source TEXT NOT NULL,
                        Metadata TEXT
                    );";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}