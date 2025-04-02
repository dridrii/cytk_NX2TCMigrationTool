using System;
using System.IO;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace cytk_NX2TCMigrationTool.src.Core.Database
{
    public class SQLiteManager
    {
        private readonly string _connectionString;
        private readonly string _schemaFilePath;

        public SQLiteManager(string dbPath, string schemaFilePath = null)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";

            // Default to looking in the src/Core/Database folder if no path is provided
            if (schemaFilePath == null)
            {
                _schemaFilePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "src", "Core", "Database",
                    "database_schema.sql");
            }
            else
            {
                _schemaFilePath = schemaFilePath;
            }
        }

        public void Initialize()
        {
            // Extract the database file path from the connection string
            string dbFilePath = GetDatabaseFilePath();
            bool dbExists = File.Exists(dbFilePath);

            // Create database file if it doesn't exist
            if (!dbExists)
            {
                SQLiteConnection.CreateFile(dbFilePath);
            }

            // Create necessary tables from schema file
            if (File.Exists(_schemaFilePath))
            {
                ExecuteSchemaFile();
            }
            else
            {
                CreateBasicSchema();
                Console.WriteLine($"Warning: Schema file not found at {_schemaFilePath}. Using basic schema.");
            }
        }

        private string GetDatabaseFilePath()
        {
            // Parse the connection string to extract the database file path
            Match match = Regex.Match(_connectionString, @"Data Source=([^;]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Default to PLMIntegration.db in the current directory
            return "PLMIntegration.db";
        }

        private void ExecuteSchemaFile()
        {
            try
            {
                // Read schema file content
                string schemaContent = File.ReadAllText(_schemaFilePath);

                // Split the content into SQL statements more intelligently
                List<string> sqlStatements = SplitSqlStatements(schemaContent);

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    foreach (string sql in sqlStatements)
                    {
                        if (string.IsNullOrWhiteSpace(sql))
                            continue;

                        using (var command = new SQLiteCommand(sql, connection))
                        {
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error executing SQL: {sql}");
                                Console.WriteLine($"Error: {ex.Message}");
                                // Optionally, throw the exception to fail fast
                                // throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExecuteSchemaFile: {ex.Message}");
                throw;
            }
        }

        private List<string> SplitSqlStatements(string sqlScript)
        {
            List<string> statements = new List<string>();

            // Remove comments
            sqlScript = RemoveSqlComments(sqlScript);

            // Split by semicolons, but handle cases where semicolons might appear in quotes
            int startPos = 0;
            bool inQuote = false;
            char quoteChar = '"';

            for (int i = 0; i < sqlScript.Length; i++)
            {
                char c = sqlScript[i];

                // Check for quotes
                if (c == '"' || c == '\'')
                {
                    if (!inQuote)
                    {
                        inQuote = true;
                        quoteChar = c;
                    }
                    else if (c == quoteChar)
                    {
                        inQuote = false;
                    }
                }

                // Check for statement end (semicolon)
                if (c == ';' && !inQuote)
                {
                    string statement = sqlScript.Substring(startPos, i - startPos + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        statements.Add(statement);
                    }
                    startPos = i + 1;
                }
            }

            // Add the last statement if it doesn't end with a semicolon
            if (startPos < sqlScript.Length)
            {
                string lastStatement = sqlScript.Substring(startPos).Trim();
                if (!string.IsNullOrWhiteSpace(lastStatement))
                {
                    statements.Add(lastStatement);
                }
            }

            return statements;
        }

        private string RemoveSqlComments(string sql)
        {
            // Remove multi-line comments (/* ... */)
            sql = Regex.Replace(sql, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Remove single-line comments (-- ...)
            sql = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);

            return sql;
        }

        private void CreateBasicSchema()
        {
            // Fallback to hardcoded basic schema if schema file is not available
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
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_parts_name ON Parts(Name);
                    CREATE INDEX IF NOT EXISTS idx_parts_source ON Parts(Source);
                ";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}