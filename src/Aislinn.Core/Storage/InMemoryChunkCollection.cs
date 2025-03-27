using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;

namespace Aislinn.ChunkStorage.Storage
{
    public class InMemoryChunkCollection : IChunkCollection
    {
        private readonly ConcurrentDictionary<Guid, Chunk> chunkStore;

        public string Id { get; }

        public InMemoryChunkCollection(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            this.chunkStore = new ConcurrentDictionary<Guid, Chunk>();
        }

        public Task<Chunk> AddChunkAsync(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            if (chunk.ID == Guid.Empty)
                chunk.ID = Guid.NewGuid();

            // Store a deep copy to prevent external modification
            var chunkCopy = DeepCopyChunk(chunk);
            chunkStore[chunk.ID] = chunkCopy;

            // Return a copy so clients don't accidentally modify the stored version
            return Task.FromResult(DeepCopyChunk(chunkCopy));
        }

        public Task<Chunk> GetChunkAsync(Guid chunkId)
        {
            if (chunkId == Guid.Empty)
                throw new ArgumentException("ChunkId cannot be empty", nameof(chunkId));

            if (chunkStore.TryGetValue(chunkId, out var chunk))
            {
                // Return a copy to prevent external modification
                return Task.FromResult(DeepCopyChunk(chunk));
            }

            return Task.FromResult<Chunk>(null);
        }

        public Task<bool> UpdateChunkAsync(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            if (chunk.ID == Guid.Empty)
                throw new ArgumentException("Chunk must have a valid ID to update", nameof(chunk));

            // Check if the chunk exists
            if (!chunkStore.ContainsKey(chunk.ID))
                return Task.FromResult(false);

            // Replace with a deep copy
            chunkStore[chunk.ID] = DeepCopyChunk(chunk);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteChunkAsync(Guid chunkId)
        {
            if (chunkId == Guid.Empty)
                throw new ArgumentException("ChunkId cannot be empty", nameof(chunkId));

            return Task.FromResult(chunkStore.TryRemove(chunkId, out _));
        }

        public Task<int> GetChunkCountAsync()
        {
            return Task.FromResult(chunkStore.Count);
        }

        public Task<List<Chunk>> GetAllChunksAsync()
        {
            var chunks = chunkStore.Values
                .Select(DeepCopyChunk)
                .ToList();

            return Task.FromResult(chunks);
        }

        // Create a deep copy of a chunk to prevent accidental modification
        private Chunk DeepCopyChunk(Chunk chunk)
        {
            if (chunk == null)
                return null;

            var copy = new Chunk
            {
                ID = chunk.ID,
                ChunkType = chunk.ChunkType,
                Name = chunk.Name,
                ActivationLevel = chunk.ActivationLevel
            };

            // Copy vector
            if (chunk.Vector != null)
            {
                copy.Vector = new double[chunk.Vector.Length];
                Array.Copy(chunk.Vector, copy.Vector, chunk.Vector.Length);
            }

            // Copy slots
            foreach (var slot in chunk.Slots)
            {
                copy.Slots[slot.Key] = new ModelSlot
                {
                    Name = slot.Value.Name,
                    Value = slot.Value.Value // Note: might need deep copying for complex objects
                };
            }

            // Copy activation history
            if (chunk.ActivationHistory != null)
            {
                foreach (var item in chunk.ActivationHistory)
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
    }


}