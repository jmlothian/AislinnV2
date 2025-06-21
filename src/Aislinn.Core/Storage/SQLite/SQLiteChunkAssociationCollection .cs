using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using Microsoft.Data.Sqlite;
namespace Aislinn.Core.Storage.SQLite
{
    public class SQLiteChunkAssociationCollection : IChunkAssociationCollection, IDisposable
    {
        public string Id { get; }

        private readonly string _connectionString;
        private readonly string _tableName;

        // Memory cache for recently accessed associations
        private readonly ConcurrentDictionary<string, CachedAssociation> _cache;
        private readonly int _maxCacheSize;
        private readonly TimeSpan _cacheExpiry;

        private class CachedAssociation
        {
            public ChunkAssociation Association { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime CachedAt { get; set; }
        }

        public SQLiteChunkAssociationCollection(string collectionId, string connectionString, int maxCacheSize = 1000, TimeSpan? cacheExpiry = null)
        {
            Id = collectionId;
            _connectionString = connectionString;
            _tableName = $"Associations_{SanitizeTableName(collectionId)}";
            _cache = new ConcurrentDictionary<string, CachedAssociation>();
            _maxCacheSize = maxCacheSize;
            _cacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(30);
        }

        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var createTable = $@"
                CREATE TABLE IF NOT EXISTS {_tableName} (
                    AssociationKey TEXT PRIMARY KEY,
                    ChunkAId TEXT NOT NULL,
                    ChunkBId TEXT NOT NULL,
                    RelationAtoB TEXT,
                    RelationBtoA TEXT,
                    WeightAtoB REAL,
                    WeightBtoA REAL,
                    LastActivated DATETIME,
                    ActivationHistoryJson TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            using var cmd = new SqliteCommand(createTable, connection);
            await cmd.ExecuteNonQueryAsync();

            // Create indexes for efficient queries
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_ChunkAId", _tableName, "ChunkAId");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_ChunkBId", _tableName, "ChunkBId");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_LastActivated", _tableName, "LastActivated");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_WeightAtoB", _tableName, "WeightAtoB");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_WeightBtoA", _tableName, "WeightBtoA");
        }

        private async Task CreateIndexIfNotExists(SqliteConnection connection, string indexName, string tableName, string columnName)
        {
            var createIndex = $"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName} ({columnName})";
            using var cmd = new SqliteCommand(createIndex, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        // Generate a consistent key for the association regardless of the order of IDs
        private string GetAssociationKey(Guid chunkAId, Guid chunkBId, string relationAtoB, string relationBtoA)
        {
            // Always put the smaller ID first to ensure consistency, but include relationship types
            if (chunkAId.CompareTo(chunkBId) <= 0)
                return $"{chunkAId}_{chunkBId}_{relationAtoB}_{relationBtoA}";
            else
                return $"{chunkBId}_{chunkAId}_{relationBtoA}_{relationAtoB}";
        }
        public async Task<ChunkAssociation> AddAssociationAsync(ChunkAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            if (association.ChunkAId == Guid.Empty || association.ChunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            var key = GetAssociationKey(association.ChunkAId, association.ChunkBId, association.RelationAtoB, association.RelationBtoA);

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Check if association already exists
            var checkExists = $"SELECT COUNT(*) FROM {_tableName} WHERE AssociationKey = @key";
            using var checkCmd = new SqliteCommand(checkExists, connection);
            checkCmd.Parameters.AddWithValue("@key", key);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (exists)
                throw new InvalidOperationException("Association already exists");

            var insert = $@"
                INSERT INTO {_tableName} 
                (AssociationKey, ChunkAId, ChunkBId, RelationAtoB, RelationBtoA, WeightAtoB, WeightBtoA, LastActivated, ActivationHistoryJson, UpdatedAt)
                VALUES (@key, @chunkAId, @chunkBId, @relationAtoB, @relationBtoA, @weightAtoB, @weightBtoA, @lastActivated, @history, @updatedAt)";

            using var cmd = new SqliteCommand(insert, connection);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@chunkAId", association.ChunkAId.ToString());
            cmd.Parameters.AddWithValue("@chunkBId", association.ChunkBId.ToString());
            cmd.Parameters.AddWithValue("@relationAtoB", association.RelationAtoB ?? string.Empty);
            cmd.Parameters.AddWithValue("@relationBtoA", association.RelationBtoA ?? string.Empty);
            cmd.Parameters.AddWithValue("@weightAtoB", association.WeightAtoB);
            cmd.Parameters.AddWithValue("@weightBtoA", association.WeightBtoA);
            cmd.Parameters.AddWithValue("@lastActivated", association.LastActivated);
            cmd.Parameters.AddWithValue("@history", JsonSerializer.Serialize(association.ActivationHistory));
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();

            // Create a copy for return and cache
            var associationCopy = DeepCopyAssociation(association);
            CacheAssociation(key, associationCopy);

            return associationCopy;
        }

        public async Task<ChunkAssociation> GetAssociationAsync(Guid chunkAId, Guid chunkBId, string relationAtoB, string relationBtoA)
        {
            if (chunkAId == Guid.Empty || chunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            var key = GetAssociationKey(chunkAId, chunkBId, relationAtoB, relationBtoA);

            // Check cache first
            if (_cache.TryGetValue(key, out var cachedAssociation))
            {
                if (DateTime.Now - cachedAssociation.CachedAt < _cacheExpiry)
                {
                    cachedAssociation.LastAccessed = DateTime.Now;
                    return DeepCopyAssociation(cachedAssociation.Association);
                }
                else
                {
                    // Remove expired cache entry
                    _cache.TryRemove(key, out _);
                }
            }

            // Load from database
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
                SELECT AssociationKey, ChunkAId, ChunkBId, RelationAtoB, RelationBtoA, WeightAtoB, WeightBtoA, LastActivated, ActivationHistoryJson
                FROM {_tableName} 
                WHERE AssociationKey = @key";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@key", key);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            var association = DeserializeAssociation(reader);

            // Add to cache
            CacheAssociation(key, association);

            return DeepCopyAssociation(association);
        }

        public async Task<bool> UpdateAssociationAsync(ChunkAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            if (association.ChunkAId == Guid.Empty || association.ChunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            var key = GetAssociationKey(association.ChunkAId, association.ChunkBId, association.RelationAtoB, association.RelationBtoA);

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var update = $@"
                UPDATE {_tableName} 
                SET RelationAtoB = @relationAtoB, RelationBtoA = @relationBtoA, 
                    WeightAtoB = @weightAtoB, WeightBtoA = @weightBtoA, 
                    LastActivated = @lastActivated, ActivationHistoryJson = @history, UpdatedAt = @updatedAt
                WHERE AssociationKey = @key";

            using var cmd = new SqliteCommand(update, connection);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@relationAtoB", association.RelationAtoB ?? string.Empty);
            cmd.Parameters.AddWithValue("@relationBtoA", association.RelationBtoA ?? string.Empty);
            cmd.Parameters.AddWithValue("@weightAtoB", association.WeightAtoB);
            cmd.Parameters.AddWithValue("@weightBtoA", association.WeightBtoA);
            cmd.Parameters.AddWithValue("@lastActivated", association.LastActivated);
            cmd.Parameters.AddWithValue("@history", JsonSerializer.Serialize(association.ActivationHistory));
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                // Update cache
                CacheAssociation(key, association);
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteAssociationAsync(Guid chunkAId, Guid chunkBId, string relationAtoB, string relationBtoA)
        {
            if (chunkAId == Guid.Empty || chunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            var key = GetAssociationKey(chunkAId, chunkBId, relationAtoB, relationBtoA);

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var delete = $"DELETE FROM {_tableName} WHERE AssociationKey = @key";
            using var cmd = new SqliteCommand(delete, connection);
            cmd.Parameters.AddWithValue("@key", key);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                // Remove from cache
                _cache.TryRemove(key, out _);
                return true;
            }

            return false;
        }

        public async Task<List<ChunkAssociation>> GetAssociationsForChunkAsync(Guid chunkId)
        {
            if (chunkId == Guid.Empty)
                throw new ArgumentException("Chunk ID must be valid");

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
                SELECT AssociationKey, ChunkAId, ChunkBId, RelationAtoB, RelationBtoA, WeightAtoB, WeightBtoA, LastActivated, ActivationHistoryJson
                FROM {_tableName} 
                WHERE ChunkAId = @chunkId OR ChunkBId = @chunkId
                ORDER BY LastActivated DESC";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@chunkId", chunkId.ToString());

            using var reader = await cmd.ExecuteReaderAsync();

            var associations = new List<ChunkAssociation>();
            while (await reader.ReadAsync())
            {
                var association = DeserializeAssociation(reader);
                associations.Add(DeepCopyAssociation(association));

                // Cache recently loaded associations
                var key = reader.GetString("AssociationKey");
                CacheAssociation(key, association);
            }

            return associations;
        }

        public async Task<int> GetAssociationCountAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $"SELECT COUNT(*) FROM {_tableName}";
            using var cmd = new SqliteCommand(query, connection);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        private void CacheAssociation(string key, ChunkAssociation association)
        {
            // Clean cache if it's getting too large
            if (_cache.Count >= _maxCacheSize)
            {
                CleanCache();
            }

            var cachedAssociation = new CachedAssociation
            {
                Association = DeepCopyAssociation(association),
                LastAccessed = DateTime.Now,
                CachedAt = DateTime.Now
            };

            _cache.AddOrUpdate(key, cachedAssociation, (k, existing) =>
            {
                existing.Association = DeepCopyAssociation(association);
                existing.LastAccessed = DateTime.Now;
                existing.CachedAt = DateTime.Now;
                return existing;
            });
        }

        private void CleanCache()
        {
            var now = DateTime.Now;
            var itemsToRemove = _cache
                .Where(kvp => now - kvp.Value.LastAccessed > _cacheExpiry)
                .Take(_maxCacheSize / 4) // Remove 25% of cache
                .ToList();

            foreach (var item in itemsToRemove)
            {
                _cache.TryRemove(item.Key, out _);
            }
        }

        private static string SanitizeTableName(string collectionId)
        {
            return System.Text.RegularExpressions.Regex.Replace(collectionId, @"[^a-zA-Z0-9_]", "_");
        }

        private static ChunkAssociation DeserializeAssociation(SqliteDataReader reader)
        {
            var association = new ChunkAssociation
            {
                ChunkAId = Guid.Parse(reader.GetString("ChunkAId")),
                ChunkBId = Guid.Parse(reader.GetString("ChunkBId")),
                RelationAtoB = reader.GetString("RelationAtoB"),
                RelationBtoA = reader.GetString("RelationBtoA"),
                WeightAtoB = reader.GetDouble("WeightAtoB"),
                WeightBtoA = reader.GetDouble("WeightBtoA"),
                LastActivated = reader.GetInt64("LastActivated")
            };

            // Deserialize activation history
            var historyJson = reader.GetString("ActivationHistoryJson");
            if (!string.IsNullOrEmpty(historyJson))
            {
                association.ActivationHistory = JsonSerializer.Deserialize<List<ActivationHistoryItem>>(historyJson);
            }

            return association;
        }

        // Create a deep copy to prevent accidental modification of the stored data
        private ChunkAssociation DeepCopyAssociation(ChunkAssociation association)
        {
            if (association == null)
                return null;

            var copy = new ChunkAssociation
            {
                ChunkAId = association.ChunkAId,
                ChunkBId = association.ChunkBId,
                RelationAtoB = association.RelationAtoB,
                RelationBtoA = association.RelationBtoA,
                WeightAtoB = association.WeightAtoB,
                WeightBtoA = association.WeightBtoA,
                LastActivated = association.LastActivated
            };

            // Copy activation history
            if (association.ActivationHistory != null)
            {
                foreach (var item in association.ActivationHistory)
                {
                    var historyCopy = new ActivationHistoryItem
                    {
                        PreviousValue = item.PreviousValue,
                        NewValue = item.NewValue,
                        Change = item.Change,
                        SequenceNumber = item.SequenceNumber,
                        ActivationDate = item.ActivationDate,
                        EmotionName = item.EmotionName,
                        ActivatedByChunk = item.ActivatedByChunk
                    };

                    if (item.Coordinates != null)
                    {
                        historyCopy.Coordinates = new List<double>(item.Coordinates);
                    }

                    if (item.ActivatedBy != null)
                    {
                        historyCopy.ActivatedBy = new List<Guid>(item.ActivatedBy);
                    }

                    copy.ActivationHistory.Add(historyCopy);
                }
            }

            return copy;
        }
        public async Task<List<ChunkAssociation>> GetAllAssociationsAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
                SELECT AssociationKey, ChunkAId, ChunkBId, RelationAtoB, RelationBtoA, WeightAtoB, WeightBtoA, LastActivated, ActivationHistoryJson
                FROM {_tableName} 
                ORDER BY LastActivated DESC";

            using var cmd = new SqliteCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            var associations = new List<ChunkAssociation>();
            while (await reader.ReadAsync())
            {
                var association = DeserializeAssociation(reader);
                associations.Add(DeepCopyAssociation(association));
            }

            return associations;
        }
        public void Dispose()
        {
            _cache.Clear();
        }
        /// <summary>
        /// Returns true if any association exists between two chunk IDs (any relation type)
        /// </summary>
        public async Task<bool> HasAssociationAsync(Guid chunkAId, Guid chunkBId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
        SELECT COUNT(*) FROM {_tableName}
        WHERE (ChunkAId = @idA AND ChunkBId = @idB)
           OR (ChunkAId = @idB AND ChunkBId = @idA)";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@idA", chunkAId.ToString());
            cmd.Parameters.AddWithValue("@idB", chunkBId.ToString());

            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        /// <summary>
        /// Returns all associations (all relation types) between two chunk IDs
        /// </summary>
        public async Task<List<ChunkAssociation>> GetAllAssociationsBetweenChunksAsync(Guid chunkAId, Guid chunkBId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
        SELECT AssociationKey, ChunkAId, ChunkBId, RelationAtoB, RelationBtoA, WeightAtoB, WeightBtoA, LastActivated, ActivationHistoryJson
        FROM {_tableName}
        WHERE (ChunkAId = @idA AND ChunkBId = @idB)
           OR (ChunkAId = @idB AND ChunkBId = @idA)";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@idA", chunkAId.ToString());
            cmd.Parameters.AddWithValue("@idB", chunkBId.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            var associations = new List<ChunkAssociation>();
            while (await reader.ReadAsync())
            {
                var association = DeserializeAssociation(reader);
                associations.Add(DeepCopyAssociation(association));
            }
            return associations;
        }
    }
}