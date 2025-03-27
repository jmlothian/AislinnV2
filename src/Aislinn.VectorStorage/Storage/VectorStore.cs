using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Models;

namespace Aislinn.VectorStorage.Storage
{
    public class VectorStore
    {
        private readonly ConcurrentDictionary<string, IVectorCollection> collections;

        public VectorStore()
        {
            collections = new ConcurrentDictionary<string, IVectorCollection>();
        }

        public Task<IVectorCollection> CreateCollectionAsync(string collectionId, IVectorizer vectorizer)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            if (vectorizer == null)
                throw new ArgumentNullException(nameof(vectorizer));

            var collection = new InMemoryVectorCollection(collectionId, vectorizer);

            if (!collections.TryAdd(collectionId, collection))
                throw new InvalidOperationException($"Collection with ID '{collectionId}' already exists");

            return Task.FromResult<IVectorCollection>(collection);
        }

        public Task<IVectorCollection> GetCollectionAsync(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            if (collections.TryGetValue(collectionId, out var collection))
                return Task.FromResult(collection);

            return Task.FromResult<IVectorCollection>(null);
        }

        public async Task<IVectorCollection> GetOrCreateCollectionAsync(string collectionId, IVectorizer vectorizer)
        {
            if (string.IsNullOrEmpty(collectionId))
                throw new ArgumentException("Collection ID cannot be null or empty", nameof(collectionId));

            if (vectorizer == null)
                throw new ArgumentNullException(nameof(vectorizer));

            var collection = await GetCollectionAsync(collectionId);
            if (collection != null)
                return collection;

            return await CreateCollectionAsync(collectionId, vectorizer);
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

        public async Task<List<SearchResult>> SearchAllCollectionsAsync(string query, int topN, double minSimilarity = 0.0)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty", nameof(query));

            if (topN <= 0)
                throw new ArgumentException("TopN must be greater than zero", nameof(topN));

            var allResults = new List<SearchResult>();

            foreach (var collection in collections.Values)
            {
                var results = await collection.SearchVectorsAsync(query, topN, minSimilarity);
                foreach (var result in results)
                {
                    //probably want to include the collection ID in the result?  do we want this thing touching metadata?

                    allResults.Add(result);
                }
            }

            return allResults
                .OrderByDescending(r => r.Similarity)
                .Take(topN)
                .ToList();
        }
    }
}