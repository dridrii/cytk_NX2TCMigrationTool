using System;
using System.Collections.Generic;
using System.Data.SQLite;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Repositories
{
    public class PartRepository : IDataRepository<Part>
    {
        private readonly string _connectionString;

        public PartRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Part GetById(string id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"SELECT ""ID"", ""Name"", ""Type"", ""Source"", ""FilePath"", ""FileName"", 
                               ""Checksum"", ""IsDuplicate"", ""DuplicateOf"", ""Metadata"" 
                               FROM ""Parts"" WHERE ""ID"" = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Part
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                Source = reader.GetString(3),
                                FilePath = reader.IsDBNull(4) ? null : reader.GetString(4),
                                FileName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Checksum = reader.IsDBNull(6) ? null : reader.GetString(6),
                                IsDuplicate = reader.IsDBNull(7) ? false : reader.GetInt32(7) == 1,
                                DuplicateOf = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Metadata = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public IEnumerable<Part> GetAll()
        {
            List<Part> parts = new List<Part>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"SELECT ""ID"", ""Name"", ""Type"", ""Source"", ""FilePath"", ""FileName"", 
                               ""Checksum"", ""IsDuplicate"", ""DuplicateOf"", ""Metadata"" 
                               FROM ""Parts""";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            parts.Add(new Part
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                Source = reader.GetString(3),
                                FilePath = reader.IsDBNull(4) ? null : reader.GetString(4),
                                FileName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Checksum = reader.IsDBNull(6) ? null : reader.GetString(6),
                                IsDuplicate = reader.IsDBNull(7) ? false : reader.GetInt32(7) == 1,
                                DuplicateOf = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Metadata = reader.IsDBNull(9) ? null : reader.GetString(9)
                            });
                        }
                    }
                }
            }

            return parts;
        }

        public void Add(Part entity)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO ""Parts"" (""ID"", ""Name"", ""Type"", ""Source"", ""FilePath"", ""FileName"", 
                    ""Checksum"", ""IsDuplicate"", ""DuplicateOf"", ""Metadata"")
                    VALUES (@Id, @Name, @Type, @Source, @FilePath, @FileName, 
                    @Checksum, @IsDuplicate, @DuplicateOf, @Metadata)";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", entity.Id);
                    command.Parameters.AddWithValue("@Name", entity.Name);
                    command.Parameters.AddWithValue("@Type", entity.Type);
                    command.Parameters.AddWithValue("@Source", entity.Source);
                    command.Parameters.AddWithValue("@FilePath", entity.FilePath ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FileName", entity.FileName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Checksum", entity.Checksum ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsDuplicate", entity.IsDuplicate ? 1 : 0);
                    command.Parameters.AddWithValue("@DuplicateOf", entity.DuplicateOf ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Metadata", entity.Metadata ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Part entity)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    UPDATE ""Parts""
                    SET ""Name"" = @Name, ""Type"" = @Type, ""Source"" = @Source, 
                    ""FilePath"" = @FilePath, ""FileName"" = @FileName, ""Checksum"" = @Checksum,
                    ""IsDuplicate"" = @IsDuplicate, ""DuplicateOf"" = @DuplicateOf, ""Metadata"" = @Metadata
                    WHERE ""ID"" = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", entity.Id);
                    command.Parameters.AddWithValue("@Name", entity.Name);
                    command.Parameters.AddWithValue("@Type", entity.Type);
                    command.Parameters.AddWithValue("@Source", entity.Source);
                    command.Parameters.AddWithValue("@FilePath", entity.FilePath ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FileName", entity.FileName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Checksum", entity.Checksum ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsDuplicate", entity.IsDuplicate ? 1 : 0);
                    command.Parameters.AddWithValue("@DuplicateOf", entity.DuplicateOf ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Metadata", entity.Metadata ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(string id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM \"Parts\" WHERE \"ID\" = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Additional query methods

        public IEnumerable<Part> GetBySource(string source)
        {
            List<Part> parts = new List<Part>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"SELECT ""ID"", ""Name"", ""Type"", ""Source"", ""FilePath"", ""FileName"", 
                               ""Checksum"", ""IsDuplicate"", ""DuplicateOf"", ""Metadata"" 
                               FROM ""Parts"" WHERE ""Source"" = @Source";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Source", source);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            parts.Add(new Part
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                Source = reader.GetString(3),
                                FilePath = reader.IsDBNull(4) ? null : reader.GetString(4),
                                FileName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Checksum = reader.IsDBNull(6) ? null : reader.GetString(6),
                                IsDuplicate = reader.IsDBNull(7) ? false : reader.GetInt32(7) == 1,
                                DuplicateOf = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Metadata = reader.IsDBNull(9) ? null : reader.GetString(9)
                            });
                        }
                    }
                }
            }

            return parts;
        }

        public Part GetByChecksum(string checksum)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"SELECT ""ID"", ""Name"", ""Type"", ""Source"", ""FilePath"", ""FileName"", 
                               ""Checksum"", ""IsDuplicate"", ""DuplicateOf"", ""Metadata"" 
                               FROM ""Parts"" WHERE ""Checksum"" = @Checksum AND ""IsDuplicate"" = 0 LIMIT 1";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Checksum", checksum);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Part
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                Source = reader.GetString(3),
                                FilePath = reader.IsDBNull(4) ? null : reader.GetString(4),
                                FileName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Checksum = reader.IsDBNull(6) ? null : reader.GetString(6),
                                IsDuplicate = reader.IsDBNull(7) ? false : reader.GetInt32(7) == 1,
                                DuplicateOf = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Metadata = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public IEnumerable<Part> GetDuplicates()
        {
            List<Part> parts = new List<Part>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"SELECT ""ID"", ""Name"", ""Type"", ""Source"", ""FilePath"", ""FileName"", 
                               ""Checksum"", ""IsDuplicate"", ""DuplicateOf"", ""Metadata"" 
                               FROM ""Parts"" WHERE ""IsDuplicate"" = 1";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            parts.Add(new Part
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                Source = reader.GetString(3),
                                FilePath = reader.IsDBNull(4) ? null : reader.GetString(4),
                                FileName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Checksum = reader.IsDBNull(6) ? null : reader.GetString(6),
                                IsDuplicate = reader.IsDBNull(7) ? false : reader.GetInt32(7) == 1,
                                DuplicateOf = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Metadata = reader.IsDBNull(9) ? null : reader.GetString(9)
                            });
                        }
                    }
                }
            }

            return parts;
        }
    }
}