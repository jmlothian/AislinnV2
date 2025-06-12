using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using Microsoft.Data.Sqlite;

namespace Aislinn.Core.Storage.SQLite
{

    public class SQLiteChunkCollection : IChunkCollection, IDisposable
    {
        public string Id { get; }

        private readonly string _connectionString;
        private readonly string _tableName;

        // Memory cache for recently accessed chunks
        private readonly ConcurrentDictionary<Guid, CachedChunk> _cache;
        private readonly int _maxCacheSize;
        private readonly TimeSpan _cacheExpiry;

        private class CachedChunk
        {
            public Chunk Chunk { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime CachedAt { get; set; }
        }

        public SQLiteChunkCollection(string collectionId, string connectionString, int maxCacheSize = 1000, TimeSpan? cacheExpiry = null)
        {
            Id = collectionId;
            _connectionString = connectionString;
            _tableName = $"Chunks_{SanitizeTableName(collectionId)}";
            _cache = new ConcurrentDictionary<Guid, CachedChunk>();
            _maxCacheSize = maxCacheSize;
            _cacheExpiry = cacheExpiry ?? TimeSpan.FromMinutes(30);
        }

        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var createTable = $@"
            CREATE TABLE IF NOT EXISTS {_tableName} (
                Id TEXT PRIMARY KEY,
                ChunkType TEXT,
                CognitiveCategory TEXT,
                SemanticType TEXT,
                Name TEXT,
                Vector BLOB,
                ActivationLevel REAL,
                SlotsJson TEXT,
                ActivationHistoryJson TEXT,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            )";

            using var cmd = new SqliteCommand(createTable, connection);
            await cmd.ExecuteNonQueryAsync();

            // Create indexes for common queries
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_Id", _tableName, "Id");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_ChunkType", _tableName, "ChunkType");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_SemanticType", _tableName, "SemanticType");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_CognitiveCategory", _tableName, "CognitiveCategory");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_Name", _tableName, "Name");
            await CreateIndexIfNotExists(connection, $"idx_{_tableName}_ActivationLevel", _tableName, "ActivationLevel");
        }

        private async Task CreateIndexIfNotExists(SqliteConnection connection, string indexName, string tableName, string columnName)
        {
            var createIndex = $"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName} ({columnName})";
            using var cmd = new SqliteCommand(createIndex, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Chunk> AddChunkAsync(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            // Ensure chunk has an ID
            if (chunk.ID == Guid.Empty)
                chunk.ID = Guid.NewGuid();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var insert = $@"
                INSERT INTO {_tableName} 
                (Id, ChunkType, CognitiveCategory, SemanticType, Name, Vector, ActivationLevel, SlotsJson, ActivationHistoryJson, UpdatedAt)
                VALUES (@id, @chunkType, @cognitiveCategory, @semanticType, @name, @vector, @activationLevel, @slots, @history, @updatedAt)";

            using var cmd = new SqliteCommand(insert, connection);
            cmd.Parameters.AddWithValue("@id", chunk.ID.ToString());
            cmd.Parameters.AddWithValue("@chunkType", chunk.ChunkType ?? string.Empty);
            cmd.Parameters.AddWithValue("@cognitiveCategory", chunk.CognitiveCategory ?? string.Empty);
            cmd.Parameters.AddWithValue("@semanticType", chunk.SemanticType ?? string.Empty);
            cmd.Parameters.AddWithValue("@name", chunk.Name ?? string.Empty);
            cmd.Parameters.AddWithValue("@vector", SerializeVector(chunk.Vector) ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@activationLevel", chunk.ActivationLevel);
            cmd.Parameters.AddWithValue("@slots", JsonSerializer.Serialize(chunk.Slots ?? new Dictionary<string, ModelSlot>()));
            cmd.Parameters.AddWithValue("@history", JsonSerializer.Serialize(chunk.ActivationHistory ?? new List<ActivationHistoryItem>()));
            cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();

            // Add to cache
            CacheChunk(chunk);

            return chunk;
        }

        public async Task<Chunk> GetChunkAsync(Guid chunkId)
        {
            // Check cache first
            if (_cache.TryGetValue(chunkId, out var cachedChunk))
            {
                if (DateTime.Now - cachedChunk.CachedAt < _cacheExpiry)
                {
                    cachedChunk.LastAccessed = DateTime.Now;
                    return cachedChunk.Chunk;
                }
                else
                {
                    // Remove expired cache entry
                    _cache.TryRemove(chunkId, out _);
                }
            }

            // Load from database
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
            SELECT Id, ChunkType, CognitiveCategory, SemanticType, Name, Vector, ActivationLevel, SlotsJson, ActivationHistoryJson
            FROM {_tableName} 
            WHERE Id = @id";

            using var cmd = new SqliteCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", chunkId.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            var chunk = DeserializeChunk(reader);

            // Add to cache
            CacheChunk(chunk);

            return chunk;
        }

        public async Task<bool> UpdateChunkAsync(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var update = $@"
UPDATE {_tableName} 
SET ChunkType = @chunkType, CognitiveCategory = @cognitiveCategory, SemanticType = @semanticType, 
    Name = @name, Vector = @vector, ActivationLevel = @activationLevel, 
    SlotsJson = @slots, ActivationHistoryJson = @history, UpdatedAt = @updatedAt
WHERE Id = @id";

                using var cmd = new SqliteCommand(update, connection);
                cmd.Parameters.AddWithValue("@id", chunk.ID.ToString());
                cmd.Parameters.AddWithValue("@chunkType", chunk.ChunkType ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", chunk.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@vector", SerializeVector(chunk.Vector) ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@activationLevel", chunk.ActivationLevel);
                cmd.Parameters.AddWithValue("@slots", JsonSerializer.Serialize(chunk.Slots));
                cmd.Parameters.AddWithValue("@history", JsonSerializer.Serialize(chunk.ActivationHistory));
                cmd.Parameters.AddWithValue("@cognitiveCategory", chunk.CognitiveCategory ?? string.Empty);
                cmd.Parameters.AddWithValue("@semanticType", chunk.SemanticType ?? string.Empty);
                cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    // Update cache
                    CacheChunk(chunk);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }

        public async Task<bool> DeleteChunkAsync(Guid chunkId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var delete = $"DELETE FROM {_tableName} WHERE Id = @id";
            using var cmd = new SqliteCommand(delete, connection);
            cmd.Parameters.AddWithValue("@id", chunkId.ToString());

            var rowsAffected = await cmd.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                // Remove from cache
                _cache.TryRemove(chunkId, out _);
                return true;
            }

            return false;
        }

        public async Task<int> GetChunkCountAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $"SELECT COUNT(*) FROM {_tableName}";
            using var cmd = new SqliteCommand(query, connection);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<List<Chunk>> GetAllChunksAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
                SELECT Id, ChunkType, CognitiveCategory, SemanticType, Name, Vector, ActivationLevel, SlotsJson, ActivationHistoryJson
                FROM {_tableName}
                ORDER BY UpdatedAt DESC";

            using var cmd = new SqliteCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            var chunks = new List<Chunk>();
            while (await reader.ReadAsync())
            {
                var chunk = DeserializeChunk(reader);
                chunks.Add(chunk);

                // Cache recently loaded chunks
                CacheChunk(chunk);
            }

            return chunks;
        }

        private void CacheChunk(Chunk chunk)
        {
            // Clean cache if it's getting too large
            if (_cache.Count >= _maxCacheSize)
            {
                CleanCache();
            }

            var cachedChunk = new CachedChunk
            {
                Chunk = chunk,
                LastAccessed = DateTime.Now,
                CachedAt = DateTime.Now
            };

            _cache.AddOrUpdate(chunk.ID, cachedChunk, (key, existing) =>
            {
                existing.Chunk = chunk;
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
            // Replace any characters that aren't safe for table names
            return System.Text.RegularExpressions.Regex.Replace(collectionId, @"[^a-zA-Z0-9_]", "_");
        }

        private static byte[] SerializeVector(double[] vector)
        {
            if (vector == null || vector.Length == 0)
                return null;

            return JsonSerializer.SerializeToUtf8Bytes(vector);
        }

        private static double[] DeserializeVector(byte[] vectorBytes)
        {
            if (vectorBytes == null || vectorBytes.Length == 0)
                return null;

            return JsonSerializer.Deserialize<double[]>(vectorBytes);
        }

        private static Chunk DeserializeChunk(SqliteDataReader reader)
        {
            var chunk = new Chunk
            {
                ID = Guid.Parse(reader.GetString("Id")),
                ChunkType = reader.GetString("ChunkType"),
                CognitiveCategory = reader.GetString("CognitiveCategory"),
                SemanticType = reader.GetString("SemanticType"),
                Name = reader.GetString("Name"),
                ActivationLevel = reader.GetDouble("ActivationLevel")
            };

            // Deserialize vector
            if (!reader.IsDBNull("Vector"))
            {
                var vectorBytes = (byte[])reader["Vector"];
                chunk.Vector = DeserializeVector(vectorBytes);
            }

            // Deserialize slots
            var slotsJson = reader.GetString("SlotsJson");
            if (!string.IsNullOrEmpty(slotsJson))
            {
                chunk.Slots = JsonSerializer.Deserialize<Dictionary<string, ModelSlot>>(slotsJson);
            }

            // Deserialize activation history
            var historyJson = reader.GetString("ActivationHistoryJson");
            if (!string.IsNullOrEmpty(historyJson))
            {
                chunk.ActivationHistory = JsonSerializer.Deserialize<List<ActivationHistoryItem>>(historyJson);
            }

            return chunk;
        }

        public void Dispose()
        {
            _cache.Clear();
        }
    }

}
