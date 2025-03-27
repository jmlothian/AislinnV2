using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Models;

namespace Aislinn.VectorStorage.Storage
{
    public class InMemoryVectorCollection : IVectorCollection
    {
        private readonly ConcurrentDictionary<string, VectorItem> vectorStore;
        private readonly IVectorizer vectorizer;

        public string Id { get; }

        public InMemoryVectorCollection(string id, IVectorizer vectorizer)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            this.vectorStore = new ConcurrentDictionary<string, VectorItem>();
            this.vectorizer = vectorizer ?? throw new ArgumentNullException(nameof(vectorizer));
        }
        public async Task<VectorItem> AddVectorAsync(string text, Dictionary<string, string> metadata)
        {
            return await this.AddVectorAsync(Guid.NewGuid().ToString(), text, metadata);
        }
        public async Task<VectorItem> AddVectorAsync(string vectorId, string text, Dictionary<string, string> metadata)
        {
            if (string.IsNullOrEmpty(vectorId))
                throw new ArgumentException("vectorId cannot be null or empty", nameof(vectorId));

            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            double[] vector = await vectorizer.StringToVectorAsync(text);
            var vectorItem = new VectorItem(vectorId, text, vector, metadata);
            vectorStore[vectorId] = vectorItem;
            return vectorItem.Clone(); //return a copy, so we don't mess up the store
        }

        public async Task<List<SearchResult>> SearchVectorsAsync(string query, int topN, double minSimilarity = 0.0)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("Query cannot be null or empty", nameof(query));

            if (topN <= 0)
                throw new ArgumentException("TopN must be greater than zero", nameof(topN));

            double[] queryVector = await vectorizer.StringToVectorAsync(query);
            return await SearchVectorsAsync(queryVector, topN, minSimilarity);
        }

        public Task<List<SearchResult>> SearchVectorsAsync(double[] queryVector, int topN, double minSimilarity = 0.0)
        {
            if (queryVector == null || queryVector.Length == 0)
                throw new ArgumentException("QueryVector cannot be null or empty", nameof(queryVector));

            if (topN <= 0)
                throw new ArgumentException("TopN must be greater than zero", nameof(topN));

            var similarities = new ConcurrentBag<SearchResult>();

            Parallel.ForEach(vectorStore, (kvp) =>
            {
                string vectorId = kvp.Key;
                double[] vector = kvp.Value.Vector;

                double similarity = CosineSimilarity(queryVector, vector);

                if (similarity >= minSimilarity)
                {
                    similarities.Add(new SearchResult(
                        kvp.Value,
                        similarity,
                        this.Id
                    ));
                }
            });

            var topResults =
                similarities
                    .OrderByDescending(x => x.Similarity)
                    .Take(topN)
                    .ToList();

            // Create deep copies of only the top results
            var deepCopies = topResults.Select(result => new SearchResult(
                result.Value.Clone(),
                result.Similarity,
                result.Collection
            )).ToList();
            return Task.FromResult(deepCopies);
        }

        public Task<bool> DeleteVectorAsync(string vectorId)
        {
            if (string.IsNullOrEmpty(vectorId))
                throw new ArgumentException("vectorId cannot be null or empty", nameof(vectorId));

            return Task.FromResult(vectorStore.TryRemove(vectorId, out _));
        }

        public Task<Dictionary<string, string>> GetMetadataAsync(string vectorId)
        {
            if (string.IsNullOrEmpty(vectorId))
                throw new ArgumentException("vectorId cannot be null or empty", nameof(vectorId));

            if (vectorStore.TryGetValue(vectorId, out var vectorItem))
            {
                return Task.FromResult(new Dictionary<string, string>(vectorItem.Metadata));
            }

            return Task.FromResult<Dictionary<string, string>>(null);
        }

        public Task<int> GetVectorCountAsync()
        {
            return Task.FromResult(vectorStore.Count);
        }

        public Task<VectorItem> GetVectorAsync(string vectorId)
        {
            if (string.IsNullOrEmpty(vectorId))
                throw new ArgumentException("vectorId cannot be null or empty", nameof(vectorId));

            if (vectorStore.TryGetValue(vectorId, out var vectorItem))
            {
                return Task.FromResult(vectorItem.Clone());
            }

            return Task.FromResult<VectorItem>(null);
        }

        private double CosineSimilarity(double[] vector1, double[] vector2)
        {
            // Check if vectors have the same dimensions
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same dimensions");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }


    }
}