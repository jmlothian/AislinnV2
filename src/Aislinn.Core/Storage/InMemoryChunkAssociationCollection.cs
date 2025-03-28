using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;

namespace Aislinn.Storage.AssociationStore
{
    // Interface for ChunkAssociation collection
    public class InMemoryChunkAssociationCollection : IChunkAssociationCollection
    {
        private readonly ConcurrentDictionary<string, ChunkAssociation> associationStore;

        public string Id { get; }

        public InMemoryChunkAssociationCollection(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            associationStore = new ConcurrentDictionary<string, ChunkAssociation>();
        }

        // Generate a consistent key for the association regardless of the order of IDs
        private string GetAssociationKey(Guid chunkAId, Guid chunkBId)
        {
            // Always put the smaller ID first to ensure consistency
            if (chunkAId.CompareTo(chunkBId) <= 0)
                return $"{chunkAId}_{chunkBId}";
            else
                return $"{chunkBId}_{chunkAId}";
        }

        public Task<ChunkAssociation> AddAssociationAsync(ChunkAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            if (association.ChunkAId == Guid.Empty || association.ChunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            // Create a deep copy
            var associationCopy = DeepCopyAssociation(association);

            // Generate the key
            var key = GetAssociationKey(association.ChunkAId, association.ChunkBId);

            // Add to store
            if (!associationStore.TryAdd(key, associationCopy))
                throw new InvalidOperationException("Association already exists");

            return Task.FromResult(DeepCopyAssociation(associationCopy));
        }

        public Task<ChunkAssociation> GetAssociationAsync(Guid chunkAId, Guid chunkBId)
        {
            if (chunkAId == Guid.Empty || chunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            var key = GetAssociationKey(chunkAId, chunkBId);

            if (associationStore.TryGetValue(key, out var association))
                return Task.FromResult(DeepCopyAssociation(association));

            return Task.FromResult<ChunkAssociation>(null);
        }

        public Task<bool> UpdateAssociationAsync(ChunkAssociation association)
        {
            if (association == null)
                throw new ArgumentNullException(nameof(association));

            if (association.ChunkAId == Guid.Empty || association.ChunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            var key = GetAssociationKey(association.ChunkAId, association.ChunkBId);

            // Check if exists
            if (!associationStore.ContainsKey(key))
                return Task.FromResult(false);

            // Update
            associationStore[key] = DeepCopyAssociation(association);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAssociationAsync(Guid chunkAId, Guid chunkBId)
        {
            if (chunkAId == Guid.Empty || chunkBId == Guid.Empty)
                throw new ArgumentException("Both chunk IDs must be valid");

            var key = GetAssociationKey(chunkAId, chunkBId);
            return Task.FromResult(associationStore.TryRemove(key, out _));
        }

        public Task<List<ChunkAssociation>> GetAssociationsForChunkAsync(Guid chunkId)
        {
            if (chunkId == Guid.Empty)
                throw new ArgumentException("Chunk ID must be valid");

            var associations = associationStore.Values
                .Where(a => a.ChunkAId == chunkId || a.ChunkBId == chunkId)
                .Select(DeepCopyAssociation)
                .ToList();

            return Task.FromResult(associations);
        }

        public Task<int> GetAssociationCountAsync()
        {
            return Task.FromResult(associationStore.Count);
        }

        // Create a deep copy to prevent accidental modification of the stored data
        private ChunkAssociation DeepCopyAssociation(ChunkAssociation association)
        {
            if (association == null)
                return null;

            var copy = new ChunkAssociation
            {
                ChunkAId = association.ChunkAId,
                ChunkBId = association.ChunkBId,
                RelationAtoB = association.RelationAtoB,
                RelationBtoA = association.RelationBtoA,
                WeightAtoB = association.WeightAtoB,
                WeightBtoA = association.WeightBtoA,
                LastActivated = association.LastActivated
            };

            // Copy activation history
            if (association.ActivationHistory != null)
            {
                foreach (var item in association.ActivationHistory)
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