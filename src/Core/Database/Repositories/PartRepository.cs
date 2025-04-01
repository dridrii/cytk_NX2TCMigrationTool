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

                string sql = "SELECT ID, Name, Type, Source, Metadata FROM Parts WHERE ID = @Id";

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
                                Metadata = reader.GetString(4)
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

                string sql = "SELECT ID, Name, Type, Source, Metadata FROM Parts";

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
                                Metadata = reader.GetString(4)
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
                    INSERT INTO Parts (ID, Name, Type, Source, Metadata)
                    VALUES (@Id, @Name, @Type, @Source, @Metadata)";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", entity.Id);
                    command.Parameters.AddWithValue("@Name", entity.Name);
                    command.Parameters.AddWithValue("@Type", entity.Type);
                    command.Parameters.AddWithValue("@Source", entity.Source);
                    command.Parameters.AddWithValue("@Metadata", entity.Metadata);

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
                    UPDATE Parts
                    SET Name = @Name, Type = @Type, Source = @Source, Metadata = @Metadata
                    WHERE ID = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", entity.Id);
                    command.Parameters.AddWithValue("@Name", entity.Name);
                    command.Parameters.AddWithValue("@Type", entity.Type);
                    command.Parameters.AddWithValue("@Source", entity.Source);
                    command.Parameters.AddWithValue("@Metadata", entity.Metadata);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(string id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM Parts WHERE ID = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}