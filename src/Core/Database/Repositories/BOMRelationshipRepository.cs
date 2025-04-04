using System;
using System.Collections.Generic;
using System.Data.SQLite;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;

namespace cytk_NX2TCMigrationTool.src.Core.Database.Repositories
{
    public class BOMRelationshipRepository
    {
        private readonly string _connectionString;
        private readonly Logger _logger;

        public BOMRelationshipRepository(string connectionString)
        {
            _connectionString = connectionString;
            _logger = Logger.Instance;
        }

        /// <summary>
        /// Add a new BOM relationship
        /// </summary>
        public void Add(BOMRelationship relationship)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    INSERT INTO ""BOM_Relationships"" (
                        ""ID"", ""ParentID"", ""ChildID"", ""RelationType"", 
                        ""InstanceName"", ""Position"", ""Quantity"", ""Verified"", ""LastUpdated""
                    ) VALUES (
                        @Id, @ParentId, @ChildId, @RelationType,
                        @InstanceName, @Position, @Quantity, @Verified, @LastUpdated
                    )";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", relationship.Id);
                    command.Parameters.AddWithValue("@ParentId", relationship.ParentId);
                    command.Parameters.AddWithValue("@ChildId", relationship.ChildId);
                    command.Parameters.AddWithValue("@RelationType", relationship.RelationType);
                    command.Parameters.AddWithValue("@InstanceName", relationship.InstanceName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Position", relationship.Position.HasValue ? (object)relationship.Position.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Quantity", relationship.Quantity);
                    command.Parameters.AddWithValue("@Verified", relationship.Verified ? 1 : 0);
                    command.Parameters.AddWithValue("@LastUpdated", relationship.LastUpdated);

                    command.ExecuteNonQuery();
                    _logger.Debug("BOMRelationshipRepository", $"Added relationship {relationship.Id} between {relationship.ParentId} and {relationship.ChildId}");
                }
            }
        }

        /// <summary>
        /// Update an existing BOM relationship
        /// </summary>
        public void Update(BOMRelationship relationship)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    UPDATE ""BOM_Relationships"" SET
                        ""ParentID"" = @ParentId,
                        ""ChildID"" = @ChildId,
                        ""RelationType"" = @RelationType,
                        ""InstanceName"" = @InstanceName,
                        ""Position"" = @Position,
                        ""Quantity"" = @Quantity,
                        ""Verified"" = @Verified,
                        ""LastUpdated"" = @LastUpdated
                    WHERE ""ID"" = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", relationship.Id);
                    command.Parameters.AddWithValue("@ParentId", relationship.ParentId);
                    command.Parameters.AddWithValue("@ChildId", relationship.ChildId);
                    command.Parameters.AddWithValue("@RelationType", relationship.RelationType);
                    command.Parameters.AddWithValue("@InstanceName", relationship.InstanceName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Position", relationship.Position.HasValue ? (object)relationship.Position.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Quantity", relationship.Quantity);
                    command.Parameters.AddWithValue("@Verified", relationship.Verified ? 1 : 0);
                    command.Parameters.AddWithValue("@LastUpdated", relationship.LastUpdated);

                    command.ExecuteNonQuery();
                    _logger.Debug("BOMRelationshipRepository", $"Updated relationship {relationship.Id}");
                }
            }
        }

        /// <summary>
        /// Delete a BOM relationship by ID
        /// </summary>
        public void Delete(string id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM \"BOM_Relationships\" WHERE \"ID\" = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                    _logger.Debug("BOMRelationshipRepository", $"Deleted relationship {id}");
                }
            }
        }

        /// <summary>
        /// Get a BOM relationship by ID
        /// </summary>
        public BOMRelationship GetById(string id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""ID"", ""ParentID"", ""ChildID"", ""RelationType"", 
                           ""InstanceName"", ""Position"", ""Quantity"", ""Verified"", ""LastUpdated""
                    FROM ""BOM_Relationships"" 
                    WHERE ""ID"" = @Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapRelationship(reader);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get all BOM relationships for a parent part
        /// </summary>
        public IEnumerable<BOMRelationship> GetByParent(string parentId)
        {
            List<BOMRelationship> relationships = new List<BOMRelationship>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""ID"", ""ParentID"", ""ChildID"", ""RelationType"", 
                           ""InstanceName"", ""Position"", ""Quantity"", ""Verified"", ""LastUpdated""
                    FROM ""BOM_Relationships"" 
                    WHERE ""ParentID"" = @ParentId
                    ORDER BY ""Position""";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ParentId", parentId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            relationships.Add(MapRelationship(reader));
                        }
                    }
                }
            }

            return relationships;
        }

        /// <summary>
        /// Get all BOM relationships for a child part
        /// </summary>
        public IEnumerable<BOMRelationship> GetByChild(string childId)
        {
            List<BOMRelationship> relationships = new List<BOMRelationship>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""ID"", ""ParentID"", ""ChildID"", ""RelationType"", 
                           ""InstanceName"", ""Position"", ""Quantity"", ""Verified"", ""LastUpdated""
                    FROM ""BOM_Relationships"" 
                    WHERE ""ChildID"" = @ChildId";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ChildId", childId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            relationships.Add(MapRelationship(reader));
                        }
                    }
                }
            }

            return relationships;
        }

        /// <summary>
        /// Get all BOM relationships by relation type
        /// </summary>
        public IEnumerable<BOMRelationship> GetByRelationType(BOMRelationType relationType)
        {
            List<BOMRelationship> relationships = new List<BOMRelationship>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""ID"", ""ParentID"", ""ChildID"", ""RelationType"", 
                           ""InstanceName"", ""Position"", ""Quantity"", ""Verified"", ""LastUpdated""
                    FROM ""BOM_Relationships"" 
                    WHERE ""RelationType"" = @RelationType";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@RelationType", relationType.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            relationships.Add(MapRelationship(reader));
                        }
                    }
                }
            }

            return relationships;
        }

        /// <summary>
        /// Get all BOM relationships
        /// </summary>
        public IEnumerable<BOMRelationship> GetAll()
        {
            List<BOMRelationship> relationships = new List<BOMRelationship>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT ""ID"", ""ParentID"", ""ChildID"", ""RelationType"", 
                           ""InstanceName"", ""Position"", ""Quantity"", ""Verified"", ""LastUpdated""
                    FROM ""BOM_Relationships""";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            relationships.Add(MapRelationship(reader));
                        }
                    }
                }
            }

            return relationships;
        }

        /// <summary>
        /// Delete all relationships for a part (both as parent and child)
        /// </summary>
        public void DeleteAllForPart(string partId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "DELETE FROM \"BOM_Relationships\" WHERE \"ParentID\" = @PartId OR \"ChildID\" = @PartId";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PartId", partId);
                    int count = command.ExecuteNonQuery();
                    _logger.Debug("BOMRelationshipRepository", $"Deleted {count} relationships for part {partId}");
                }
            }
        }

        /// <summary>
        /// Check if a relationship already exists between parent and child
        /// </summary>
        public bool RelationshipExists(string parentId, string childId, string relationType)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = @"
                    SELECT COUNT(*) 
                    FROM ""BOM_Relationships"" 
                    WHERE ""ParentID"" = @ParentId 
                    AND ""ChildID"" = @ChildId 
                    AND ""RelationType"" = @RelationType";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ParentId", parentId);
                    command.Parameters.AddWithValue("@ChildId", childId);
                    command.Parameters.AddWithValue("@RelationType", relationType);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Helper method to map a database reader to a BOMRelationship object
        /// </summary>
        private BOMRelationship MapRelationship(SQLiteDataReader reader)
        {
            return new BOMRelationship
            {
                Id = reader.GetString(0),
                ParentId = reader.GetString(1),
                ChildId = reader.GetString(2),
                RelationType = reader.GetString(3),
                InstanceName = reader.IsDBNull(4) ? null : reader.GetString(4),
                Position = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                Quantity = reader.GetInt32(6),
                Verified = reader.GetInt32(7) == 1,
                LastUpdated = reader.GetString(8)
            };
        }
    }
}