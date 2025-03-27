using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.ChunkStorage.Storage;
using Aislinn.Core.Models;
public class ChunkStore : IChunkStore
{
    private readonly ConcurrentDictionary<string, IChunkCollection> collections;

    public ChunkStore()
    {
        collections = new ConcurrentDictionary<string, IChunkCollection>();
    }

    public Task<IChunkCollection> CreateCollectionAsync(string collectionId)
    {
        if (string.IsNullOrEmpty(collectionId))
            throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

        var collection = new InMemoryChunkCollection(collectionId);

        if (!collections.TryAdd(collectionId, collection))
            throw new InvalidOperationException($"Collection with ID '{collectionId}' already exists");

        return Task.FromResult<IChunkCollection>(collection);
    }

    public Task<IChunkCollection> GetCollectionAsync(string collectionId)
    {
        if (string.IsNullOrEmpty(collectionId))
            throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

        if (collections.TryGetValue(collectionId, out var collection))
            return Task.FromResult(collection);

        return Task.FromResult<IChunkCollection>(null);
    }

    public async Task<IChunkCollection> GetOrCreateCollectionAsync(string collectionId)
    {
        if (string.IsNullOrEmpty(collectionId))
            throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

        var collection = await GetCollectionAsync(collectionId);
        if (collection != null)
            return collection;

        return await CreateCollectionAsync(collectionId);
    }

    public Task<bool> DeleteCollectionAsync(string collectionId)
    {
        if (string.IsNullOrEmpty(collectionId))
            throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

        return Task.FromResult(collections.TryRemove(collectionId, out _));
    }

    public Task<List<string>> GetCollectionIdsAsync()
    {
        return Task.FromResult(collections.Keys.ToList());
    }
}