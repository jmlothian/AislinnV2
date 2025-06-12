using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using System.Data;
using Aislinn.Core.Storage.SQLite;
using Aislinn.Configuration;

namespace Aislinn.Core.Storage.SQLite
{
    public class SQLiteChunkStore : IChunkStore, IDisposable
    {
        private readonly string _connectionString;
        private readonly ConcurrentDictionary<string, SQLiteChunkCollection> _collections;
        private bool _disposed = false;

        public SQLiteChunkStore(AislinnConfiguration config)
        {
            _connectionString = $"Data Source={config.ChunkDatabasePath}";
            _collections = new ConcurrentDictionary<string, SQLiteChunkCollection>();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create collections table
            var createCollectionsTable = @"
                CREATE TABLE IF NOT EXISTS Collections (
                    Id TEXT PRIMARY KEY,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            using var cmd = new SqliteCommand(createCollectionsTable, connection);
            cmd.ExecuteNonQuery();
        }

        public async Task<IChunkCollection> CreateCollectionAsync(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Check if collection already exists
            var checkExists = "SELECT COUNT(*) FROM Collections WHERE Id = @id";
            using var checkCmd = new SqliteCommand(checkExists, connection);
            checkCmd.Parameters.AddWithValue("@id", collectionId);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (exists)
                throw new InvalidOperationException($"Collection with ID '{collectionId}' already exists");

            // Insert collection record
            var insertCollection = "INSERT INTO Collections (Id) VALUES (@id)";
            using var insertCmd = new SqliteCommand(insertCollection, connection);
            insertCmd.Parameters.AddWithValue("@id", collectionId);
            await insertCmd.ExecuteNonQueryAsync();

            // Create the collection object
            var collection = new SQLiteChunkCollection(collectionId, _connectionString);
            await collection.InitializeAsync();

            _collections.TryAdd(collectionId, collection);
            return collection;
        }

        public async Task<IChunkCollection> GetCollectionAsync(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            // Check if already loaded
            if (_collections.TryGetValue(collectionId, out var existingCollection))
                return existingCollection;

            // Check if exists in database
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var checkExists = "SELECT COUNT(*) FROM Collections WHERE Id = @id";
            using var checkCmd = new SqliteCommand(checkExists, connection);
            checkCmd.Parameters.AddWithValue("@id", collectionId);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (!exists)
                return null;

            // Create and cache the collection
            var collection = new SQLiteChunkCollection(collectionId, _connectionString);
            await collection.InitializeAsync();
            _collections.TryAdd(collectionId, collection);

            return collection;
        }

        public async Task<IChunkCollection> GetOrCreateCollectionAsync(string collectionId)
        {
            var collection = await GetCollectionAsync(collectionId);
            if (collection != null)
                return collection;

            return await CreateCollectionAsync(collectionId);
        }

        public async Task<bool> DeleteCollectionAsync(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Drop the chunks table for this collection
                var dropTable = $"DROP TABLE IF EXISTS Chunks_{SanitizeTableName(collectionId)}";
                using var dropCmd = new SqliteCommand(dropTable, connection, transaction);
                await dropCmd.ExecuteNonQueryAsync();

                // Remove from collections table
                var deleteCollection = "DELETE FROM Collections WHERE Id = @id";
                using var deleteCmd = new SqliteCommand(deleteCollection, connection, transaction);
                deleteCmd.Parameters.AddWithValue("@id", collectionId);
                var rowsAffected = await deleteCmd.ExecuteNonQueryAsync();

                transaction.Commit();

                // Remove from memory cache
                _collections.TryRemove(collectionId, out var removedCollection);
                removedCollection?.Dispose();

                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<string>> GetCollectionIdsAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT Id FROM Collections ORDER BY Id";
            using var cmd = new SqliteCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            var collectionIds = new List<string>();
            while (await reader.ReadAsync())
            {
                collectionIds.Add(reader.GetString("Id"));
            }

            return collectionIds;
        }

        private static string SanitizeTableName(string collectionId)
        {
            // Replace any characters that aren't safe for table names
            return System.Text.RegularExpressions.Regex.Replace(collectionId, @"[^a-zA-Z0-9_]", "_");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var collection in _collections.Values)
                {
                    collection.Dispose();
                }
                _collections.Clear();
                _disposed = true;
            }
        }
    }
}