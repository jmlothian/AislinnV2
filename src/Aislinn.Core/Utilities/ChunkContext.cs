using System;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;

namespace Aislinn.ChunkStorage
{
    public static class ChunkContext
    {
        private static IChunkStore _store;
        private static string _defaultCollectionId;

        public static void Initialize(IChunkStore store, string defaultCollectionId = "")
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _defaultCollectionId = defaultCollectionId ?? throw new ArgumentNullException(nameof(defaultCollectionId));

            // Ensure default collection exists
            _store.GetOrCreateCollectionAsync(_defaultCollectionId).GetAwaiter().GetResult();
        }

        public static async Task<Chunk> GetChunkAsync(Guid chunkId, string collectionId = null)
        {
            EnsureInitialized();
            var collection = await _store.GetCollectionAsync(collectionId ?? _defaultCollectionId);
            if (collection == null)
                throw new InvalidOperationException($"Collection '{collectionId ?? _defaultCollectionId}' not found.");

            return await collection.GetChunkAsync(chunkId);
        }

        private static void EnsureInitialized()
        {
            if (_store == null)
                throw new InvalidOperationException("ChunkContext has not been initialized. Call Initialize method first.");
        }
    }

    // Extension method to allow implicit conversion from Guid to Chunk
    public static class GuidExtensions
    {
        public static async Task<Chunk> ToChunkAsync(this Guid chunkId, string collectionId = null)
        {
            return await ChunkContext.GetChunkAsync(chunkId, collectionId);
        }

        // Synchronous version that uses Task.Result (use with caution due to potential deadlocks)
        public static Chunk ToChunk(this Guid chunkId, string collectionId = null)
        {
            return ChunkContext.GetChunkAsync(chunkId, collectionId).GetAwaiter().GetResult();
        }
    }
}