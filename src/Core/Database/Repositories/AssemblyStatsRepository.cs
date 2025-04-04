using System;
using System.Collections.Generic;
using System.Data.SQLite;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Repositories
{
    // Remove any private modifier from the class declaration
    public class AssemblyStatsRepository
    {
        private readonly string _connectionString;
        private readonly Logger _logger;

        public AssemblyStatsRepository(string connectionString)
        {
            _connectionString = connectionString;
            _logger = Logger.Instance;
        }

        /// <summary>
        /// Add new assembly statistics
        /// </summary>
        public void Add(AssemblyStats stats)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO ""AssemblyStats"" (
                        ""PartID"", ""IsAssembly"", ""IsDrafting"", ""ComponentCount"", 
                        ""TotalComponentCount"", ""AssemblyDepth"", ""ParentCount"", ""LastAnalyzed""
                    ) VALUES (
                        @PartId, @IsAssembly, @IsDrafting, @ComponentCount, 
                        @TotalComponentCount, @AssemblyDepth, @ParentCount, @LastAnalyzed
                    )";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PartId", stats.PartId);
                    command.Parameters.AddWithValue("@IsAssembly", stats.IsAssembly ? 1 : 0);
                    command.Parameters.AddWithValue("@IsDrafting", stats.IsDrafting ? 1 : 0);
                    command.Parameters.AddWithValue("@ComponentCount", stats.ComponentCount);
                    command.Parameters.AddWithValue("@TotalComponentCount", stats.TotalComponentCount);
                    command.Parameters.AddWithValue("@AssemblyDepth", stats.AssemblyDepth);
                    command.Parameters.AddWithValue("@ParentCount", stats.ParentCount);
                    command.Parameters.AddWithValue("@LastAnalyzed", stats.LastAnalyzed);

                    command.ExecuteNonQuery();
                    _logger.Debug("AssemblyStatsRepository", $"Added assembly stats for part {stats.PartId}");
                }
            }
        }

        /// <summary>
        /// Update existing assembly statistics
        /// </summary>
        public void Update(AssemblyStats stats)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    UPDATE ""AssemblyStats"" SET
                        ""IsAssembly"" = @IsAssembly,
                        ""IsDrafting"" = @IsDrafting,
                        ""ComponentCount"" = @ComponentCount,
                        ""TotalComponentCount"" = @TotalComponentCount,
                        ""AssemblyDepth"" = @AssemblyDepth,
                        ""ParentCount"" = @ParentCount,
                        ""LastAnalyzed"" = @LastAnalyzed
                    WHERE ""PartID"" = @PartId";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PartId", stats.PartId);
                    command.Parameters.AddWithValue("@IsAssembly", stats.IsAssembly ? 1 : 0);
                    command.Parameters.AddWithValue("@IsDrafting", stats.IsDrafting ? 1 : 0);
                    command.Parameters.AddWithValue("@ComponentCount", stats.ComponentCount);
                    command.Parameters.AddWithValue("@TotalComponentCount", stats.TotalComponentCount);
                    command.Parameters.AddWithValue("@AssemblyDepth", stats.AssemblyDepth);
                    command.Parameters.AddWithValue("@ParentCount", stats.ParentCount);
                    command.Parameters.AddWithValue("@LastAnalyzed", stats.LastAnalyzed);

                    command.ExecuteNonQuery();
                    _logger.Debug("AssemblyStatsRepository", $"Updated assembly stats for part {stats.PartId}");
                }
            }
        }

        /// <summary>
        /// Get assembly statistics by part ID
        /// </summary>
        public AssemblyStats GetByPartId(string partId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""PartID"", ""IsAssembly"", ""IsDrafting"", ""ComponentCount"", 
                           ""TotalComponentCount"", ""AssemblyDepth"", ""ParentCount"", ""LastAnalyzed""
                    FROM ""AssemblyStats"" 
                    WHERE ""PartID"" = @PartId";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PartId", partId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapAssemblyStats(reader);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get all assembly statistics
        /// </summary>
        public IEnumerable<AssemblyStats> GetAll()
        {
            List<AssemblyStats> allStats = new List<AssemblyStats>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""PartID"", ""IsAssembly"", ""IsDrafting"", ""ComponentCount"", 
                           ""TotalComponentCount"", ""AssemblyDepth"", ""ParentCount"", ""LastAnalyzed""
                    FROM ""AssemblyStats""";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allStats.Add(MapAssemblyStats(reader));
                        }
                    }
                }
            }

            return allStats;
        }

        /// <summary>
        /// Get assemblies sorted by component count (largest first)
        /// </summary>
        public IEnumerable<AssemblyStats> GetAssembliesBySize(bool useDirectComponentCount = false)
        {
            List<AssemblyStats> assemblies = new List<AssemblyStats>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string countField = useDirectComponentCount ? "ComponentCount" : "TotalComponentCount";

                string sql = $@"
                    SELECT ""PartID"", ""IsAssembly"", ""IsDrafting"", ""ComponentCount"", 
                           ""TotalComponentCount"", ""AssemblyDepth"", ""ParentCount"", ""LastAnalyzed""
                    FROM ""AssemblyStats""
                    WHERE ""IsAssembly"" = 1
                    ORDER BY ""{countField}"" DESC";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            assemblies.Add(MapAssemblyStats(reader));
                        }
                    }
                }
            }

            return assemblies;
        }

        /// <summary>
        /// Get drafting files
        /// </summary>
        public IEnumerable<AssemblyStats> GetDraftings()
        {
            List<AssemblyStats> draftings = new List<AssemblyStats>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""PartID"", ""IsAssembly"", ""IsDrafting"", ""ComponentCount"", 
                           ""TotalComponentCount"", ""AssemblyDepth"", ""ParentCount"", ""LastAnalyzed""
                    FROM ""AssemblyStats""
                    WHERE ""IsDrafting"" = 1";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            draftings.Add(MapAssemblyStats(reader));
                        }
                    }
                }
            }

            return draftings;
        }

        /// <summary>
        /// Delete assembly statistics for a part
        /// </summary>
        public void Delete(string partId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM \"AssemblyStats\" WHERE \"PartID\" = @PartId";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PartId", partId);
                    command.ExecuteNonQuery();
                    _logger.Debug("AssemblyStatsRepository", $"Deleted assembly stats for part {partId}");
                }
            }
        }

        /// <summary>
        /// Helper method to map a database reader to an AssemblyStats object
        /// </summary>
        private AssemblyStats MapAssemblyStats(SQLiteDataReader reader)
        {
            return new AssemblyStats
            {
                PartId = reader.GetString(0),
                IsAssembly = reader.GetInt32(1) == 1,
                IsDrafting = reader.GetInt32(2) == 1,
                ComponentCount = reader.GetInt32(3),
                TotalComponentCount = reader.GetInt32(4),
                AssemblyDepth = reader.GetInt32(5),
                ParentCount = reader.GetInt32(6),
                LastAnalyzed = reader.GetString(7)
            };
        }
    }
}