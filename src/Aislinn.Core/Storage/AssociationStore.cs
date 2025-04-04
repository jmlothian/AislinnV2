using System.Collections.Concurrent;
using Aislinn.ChunkStorage.Interfaces;

namespace Aislinn.Storage.AssociationStore;
public class AssociationStore : IAssociationStore
{
    private readonly ConcurrentDictionary<string, IChunkAssociationCollection> collections;

    public AssociationStore()
    {
        collections = new ConcurrentDictionary<string, IChunkAssociationCollection>();
    }

    public Task<IChunkAssociationCollection> CreateCollectionAsync(string collectionId)
    {
        if (string.IsNullOrEmpty(collectionId))
            throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

        var collection = new InMemoryChunkAssociationCollection(collectionId);

        if (!collections.TryAdd(collectionId, collection))
            throw new InvalidOperationException($"Collection with ID '{collectionId}' already exists");

        return Task.FromResult<IChunkAssociationCollection>(collection);
    }

    public Task<IChunkAssociationCollection> GetCollectionAsync(string collectionId)
    {
        if (string.IsNullOrEmpty(collectionId))
            throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

        if (collections.TryGetValue(collectionId, out var collection))
            return Task.FromResult(collection);

        return Task.FromResult<IChunkAssociationCollection>(null);
    }

    public async Task<IChunkAssociationCollection> GetOrCreateCollectionAsync(string collectionId)
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