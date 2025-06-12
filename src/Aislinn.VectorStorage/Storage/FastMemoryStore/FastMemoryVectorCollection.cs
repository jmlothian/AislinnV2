using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Models;

namespace Aislinn.VectorStorage.Implementations
{
    public class FastMemoryVectorCollection : IVectorCollection, IDisposable
    {
        private volatile VectorData _data = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly IVectorizer _vectorizer;
        private Timer _autoSaveTimer;
        private string _autoSaveFilePath;

        public string Id { get; }

        public FastMemoryVectorCollection(string id, IVectorizer vectorizer)
        {
            Id = id;
            _vectorizer = vectorizer ?? throw new ArgumentNullException(nameof(vectorizer));
        }

        public async Task<VectorItem> AddVectorAsync(string text, Dictionary<string, string> metadata)
        {
            var vectorId = Guid.NewGuid().ToString();
            return await AddVectorAsync(vectorId, text, metadata);
        }

        public async Task<VectorItem> AddVectorAsync(string vectorId, string text, Dictionary<string, string> metadata)
        {
            var vector = await _vectorizer.StringToVectorAsync(text, "document");
            var floatVector = vector.Select(x => (float)x).ToArray();

            _lock.EnterWriteLock();
            try
            {
                var newData = _data.Add(vectorId, floatVector, text, metadata ?? new Dictionary<string, string>());
                _data = newData; // Atomic swap
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return new VectorItem(vectorId, text, vector, metadata);
        }

        public async Task<List<SearchResult>> SearchVectorsAsync(string query, int topN, double minSimilarity = 0.0)
        {
            var queryVector = await _vectorizer.StringToVectorAsync(query, "query");
            return await SearchVectorsAsync(queryVector, topN, minSimilarity);
        }

        public Task<List<SearchResult>> SearchVectorsAsync(double[] queryVector, int topN, double minSimilarity = 0.0)
        {
            var floatQuery = queryVector.Select(x => (float)x).ToArray();
            var data = _data; // Snapshot for thread safety

            var results = new List<SearchResult>();

            // SIMD-optimized dot product (since vectors are pre-normalized, cosine = dot product)
            for (int i = 0; i < data.Count; i++)
            {
                var similarity = (double)SIMDDotProduct(floatQuery, data.Vectors[i]);

                if (similarity >= minSimilarity)
                {
                    var vectorItem = new VectorItem(
                        data.Ids[i],
                        data.Texts[i],
                        data.Vectors[i].Select(x => (double)x).ToArray(),
                        data.Metadata[i]
                    );

                    results.Add(new SearchResult(vectorItem, similarity, Id));
                }
            }

            // Return top N results sorted by similarity
            var topResults = results
                .OrderByDescending(r => r.Similarity)
                .Take(topN)
                .ToList();

            return Task.FromResult(topResults);
        }

        public Task<bool> DeleteVectorAsync(string vectorId)
        {
            _lock.EnterWriteLock();
            try
            {
                var newData = _data.Remove(vectorId);
                if (newData == _data) return Task.FromResult(false); // Not found

                _data = newData;
                return Task.FromResult(true);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Task<Dictionary<string, string>> GetMetadataAsync(string vectorId)
        {
            var data = _data;
            var index = Array.IndexOf(data.Ids, vectorId);

            if (index == -1)
                return Task.FromResult<Dictionary<string, string>>(null);

            return Task.FromResult(data.Metadata[index]);
        }

        public Task<int> GetVectorCountAsync()
        {
            return Task.FromResult(_data.Count);
        }

        public Task<VectorItem> GetVectorAsync(string vectorId)
        {
            var data = _data;
            var index = Array.IndexOf(data.Ids, vectorId);

            if (index == -1)
                return Task.FromResult<VectorItem>(null);

            return Task.FromResult(new VectorItem(
                data.Ids[index],
                data.Texts[index],
                data.Vectors[index].Select(x => (double)x).ToArray(),
                data.Metadata[index]
            ));
        }

        // Save/Load functionality
        public async Task SaveAsync(string filePath)
        {
            var data = _data; // Snapshot

            using var fileStream = File.Create(filePath);
            using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
            using var writer = new BinaryWriter(gzipStream);

            // Write header
            writer.Write("FVSTORE1.0"); // Version marker
            writer.Write(data.Count);

            if (data.Count == 0) return;

            // Write vector dimensions
            writer.Write(data.Vectors[0].Length);

            // Write all data
            for (int i = 0; i < data.Count; i++)
            {
                // ID
                writer.Write(data.Ids[i]);

                // Text
                writer.Write(data.Texts[i]);

                // Vector
                foreach (var value in data.Vectors[i])
                    writer.Write(value);

                // Metadata as JSON
                var metadataJson = JsonSerializer.Serialize(data.Metadata[i]);
                writer.Write(metadataJson);
            }
        }

        public async Task<bool> LoadAsync(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            _lock.EnterWriteLock();
            try
            {
                using var fileStream = File.OpenRead(filePath);
                using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
                using var reader = new BinaryReader(gzipStream);

                // Read header
                var version = reader.ReadString();
                if (version != "FVSTORE1.0")
                    throw new InvalidDataException("Unsupported file version");

                var count = reader.ReadInt32();
                if (count == 0)
                {
                    _data = new VectorData();
                    return true;
                }

                var dimensions = reader.ReadInt32();

                var ids = new string[count];
                var texts = new string[count];
                var vectors = new float[count][];
                var metadata = new Dictionary<string, string>[count];

                // Read all data
                for (int i = 0; i < count; i++)
                {
                    // ID
                    ids[i] = reader.ReadString();

                    // Text
                    texts[i] = reader.ReadString();

                    // Vector
                    vectors[i] = new float[dimensions];
                    for (int j = 0; j < dimensions; j++)
                        vectors[i][j] = reader.ReadSingle();

                    // Metadata
                    var metadataJson = reader.ReadString();
                    metadata[i] = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);
                }

                _data = new VectorData(ids, texts, vectors, metadata);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // Auto-save functionality
        public void EnableAutoSave(string filePath, TimeSpan interval)
        {
            _autoSaveFilePath = filePath;
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = new Timer(async _ => await SaveAsync(_autoSaveFilePath),
                                      null, interval, interval);
        }

        public void DisableAutoSave()
        {
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = null;
        }

        // Helper methods
        private static float SIMDDotProduct(float[] a, float[] b)
        {
            float sum = 0f;
            int vectorSize = Vector<float>.Count;
            int i = 0;

            // Process in SIMD chunks (4-8 floats at once)
            for (; i <= a.Length - vectorSize; i += vectorSize)
            {
                var va = new Vector<float>(a, i);
                var vb = new Vector<float>(b, i);
                sum += Vector.Dot(va, vb);
            }

            // Handle remainder
            for (; i < a.Length; i++)
                sum += a[i] * b[i];

            return sum;
        }

        public void Dispose()
        {
            _autoSaveTimer?.Dispose();
            _lock?.Dispose();
        }
    }

    // Immutable data structure for thread safety
    internal class VectorData
    {
        public string[] Ids { get; }
        public string[] Texts { get; }
        public float[][] Vectors { get; }
        public Dictionary<string, string>[] Metadata { get; }
        public int Count { get; }

        public VectorData() : this(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<float[]>(), Array.Empty<Dictionary<string, string>>()) { }

        internal VectorData(string[] ids, string[] texts, float[][] vectors, Dictionary<string, string>[] metadata)
        {
            Ids = ids;
            Texts = texts;
            Vectors = vectors;
            Metadata = metadata;
            Count = ids.Length;
        }

        public VectorData Add(string id, float[] vector, string text, Dictionary<string, string> metadata)
        {
            var newIds = new string[Count + 1];
            var newTexts = new string[Count + 1];
            var newVectors = new float[Count + 1][];
            var newMetadata = new Dictionary<string, string>[Count + 1];

            Array.Copy(Ids, newIds, Count);
            Array.Copy(Texts, newTexts, Count);
            Array.Copy(Vectors, newVectors, Count);
            Array.Copy(Metadata, newMetadata, Count);

            newIds[Count] = id;
            newTexts[Count] = text;
            newVectors[Count] = vector;
            newMetadata[Count] = metadata;

            return new VectorData(newIds, newTexts, newVectors, newMetadata);
        }

        public VectorData Remove(string vectorId)
        {
            var index = Array.IndexOf(Ids, vectorId);
            if (index == -1) return this; // Not found, return unchanged

            var newIds = new string[Count - 1];
            var newTexts = new string[Count - 1];
            var newVectors = new float[Count - 1][];
            var newMetadata = new Dictionary<string, string>[Count - 1];

            // Copy everything except the item at index
            Array.Copy(Ids, 0, newIds, 0, index);
            Array.Copy(Ids, index + 1, newIds, index, Count - index - 1);

            Array.Copy(Texts, 0, newTexts, 0, index);
            Array.Copy(Texts, index + 1, newTexts, index, Count - index - 1);

            Array.Copy(Vectors, 0, newVectors, 0, index);
            Array.Copy(Vectors, index + 1, newVectors, index, Count - index - 1);

            Array.Copy(Metadata, 0, newMetadata, 0, index);
            Array.Copy(Metadata, index + 1, newMetadata, index, Count - index - 1);

            return new VectorData(newIds, newTexts, newVectors, newMetadata);
        }
    }
}