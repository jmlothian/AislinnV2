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
using Aislinn.Configuration;

namespace Aislinn.Core.Storage.SQLite
{
    public class SQLiteAssociationStore : IAssociationStore, IDisposable
    {
        private readonly string _connectionString;
        private readonly ConcurrentDictionary<string, SQLiteChunkAssociationCollection> _collections;
        private bool _disposed = false;

        public SQLiteAssociationStore(AislinnConfiguration config)
        {
            _connectionString = $"Data Source={config.AssociationDatabasePath}";
            _collections = new ConcurrentDictionary<string, SQLiteChunkAssociationCollection>();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Create collections table for associations
            var createCollectionsTable = @"
                CREATE TABLE IF NOT EXISTS AssociationCollections (
                    Id TEXT PRIMARY KEY,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            using var cmd = new SqliteCommand(createCollectionsTable, connection);
            cmd.ExecuteNonQuery();
        }

        public async Task<IChunkAssociationCollection> CreateCollectionAsync(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Check if collection already exists
            var checkExists = "SELECT COUNT(*) FROM AssociationCollections WHERE Id = @id";
            using var checkCmd = new SqliteCommand(checkExists, connection);
            checkCmd.Parameters.AddWithValue("@id", collectionId);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (exists)
                throw new InvalidOperationException($"Association collection with ID '{collectionId}' already exists");

            // Insert collection record
            var insertCollection = "INSERT INTO AssociationCollections (Id) VALUES (@id)";
            using var insertCmd = new SqliteCommand(insertCollection, connection);
            insertCmd.Parameters.AddWithValue("@id", collectionId);
            await insertCmd.ExecuteNonQueryAsync();

            // Create the collection object
            var collection = new SQLiteChunkAssociationCollection(collectionId, _connectionString);
            await collection.InitializeAsync();

            _collections.TryAdd(collectionId, collection);
            return collection;
        }

        public async Task<IChunkAssociationCollection> GetCollectionAsync(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            // Check if already loaded
            if (_collections.TryGetValue(collectionId, out var existingCollection))
                return existingCollection;

            // Check if exists in database
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var checkExists = "SELECT COUNT(*) FROM AssociationCollections WHERE Id = @id";
            using var checkCmd = new SqliteCommand(checkExists, connection);
            checkCmd.Parameters.AddWithValue("@id", collectionId);
            var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

            if (!exists)
                return null;

            // Create and cache the collection
            var collection = new SQLiteChunkAssociationCollection(collectionId, _connectionString);
            await collection.InitializeAsync();
            _collections.TryAdd(collectionId, collection);

            return collection;
        }

        public async Task<IChunkAssociationCollection> GetOrCreateCollectionAsync(string collectionId)
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
                // Drop the associations table for this collection
                var dropTable = $"DROP TABLE IF EXISTS Associations_{SanitizeTableName(collectionId)}";
                using var dropCmd = new SqliteCommand(dropTable, connection, transaction);
                await dropCmd.ExecuteNonQueryAsync();

                // Remove from collections table
                var deleteCollection = "DELETE FROM AssociationCollections WHERE Id = @id";
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

            var query = "SELECT Id FROM AssociationCollections ORDER BY Id";
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