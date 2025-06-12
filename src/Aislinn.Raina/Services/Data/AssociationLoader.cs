using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Aislinn.Core.Models;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Configuration;

namespace RAINA.Services.Data
{
    /// <summary>
    /// Loads association chunks from JSON with ID remapping and duplicate prevention
    /// </summary>
    public class AssociationLoader
    {
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;

        public AssociationLoader(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            AislinnConfiguration config)
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _chunkCollectionId = config.ChunkCollectionId;
            _associationCollectionId = config.AssociationCollectionId;
        }

        /// <summary>
        /// Loads associations from JSON with optional ID remapping and duplicate prevention
        /// </summary>
        /// <param name="jsonData">JSON string containing association array</param>
        /// <param name="skipDuplicates">Whether to skip associations that already exist</param>
        /// <returns>List of loaded associations</returns>
        public async Task<List<ChunkAssociation>> LoadAssociationsAsync(
            string jsonData,
            List<Chunk> loadedChunks,
            bool skipDuplicates = true)
        {
            if (string.IsNullOrEmpty(jsonData))
                throw new ArgumentException("JSON data cannot be null or empty", nameof(jsonData));

            // Build the ID mapping from the loaded chunks
            var chunkIdMapping = BuildIdMappingFromChunks(loadedChunks);
            // Deserialize associations from JSON
            var associations = JsonSerializer.Deserialize<List<ChunkAssociation>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (associations == null || associations.Count == 0)
                return new List<ChunkAssociation>();

            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                throw new InvalidOperationException($"Association collection '{_associationCollectionId}' not found");

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            var loadedAssociations = new List<ChunkAssociation>();

            foreach (var association in associations)
            {
                try
                {
                    // Remap chunk IDs if mapping provided
                    var chunkAId = RemapChunkId(association.ChunkAId, chunkIdMapping);
                    var chunkBId = RemapChunkId(association.ChunkBId, chunkIdMapping);

                    // Verify both chunks exist
                    var chunkA = await chunkCollection.GetChunkAsync(chunkAId);
                    var chunkB = await chunkCollection.GetChunkAsync(chunkBId);

                    if (chunkA == null)
                    {
                        Console.WriteLine($"Warning: ChunkA {chunkAId} not found, skipping association");
                        continue;
                    }

                    if (chunkB == null)
                    {
                        Console.WriteLine($"Warning: ChunkB {chunkBId} not found, skipping association");
                        continue;
                    }

                    // Check for existing association
                    if (skipDuplicates && await AssociationExistsAsync(associationCollection, chunkAId, chunkBId, association.RelationAtoB, association.RelationBtoA))
                    {
                        Console.WriteLine($"Skipping duplicate association: {chunkA.Name} -{association.RelationAtoB}-> {chunkB.Name}");
                        continue;
                    }

                    // Update the association with remapped IDs
                    association.ChunkAId = chunkAId;
                    association.ChunkBId = chunkBId;

                    // Add the association
                    var savedAssociation = await associationCollection.AddAssociationAsync(association);
                    loadedAssociations.Add(savedAssociation);



                    Console.WriteLine($"Loaded association: {chunkA.Name} -{association.RelationAtoB}-> {chunkB.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load association between {association.ChunkAId} and {association.ChunkBId}: {ex.Message}");

                }
            }

            return loadedAssociations;
        }

        /// <summary>
        /// Loads multiple association JSON files in sequence
        /// </summary>
        public async Task<List<ChunkAssociation>> LoadMultipleFilesAsync(
            string[] jsonFiles,
            List<Chunk> loadedChunks,
            bool skipDuplicates = true)
        {
            var allLoadedAssociations = new List<ChunkAssociation>();

            foreach (var jsonData in jsonFiles)
            {
                var loadedAssociations = await LoadAssociationsAsync(jsonData, loadedChunks, skipDuplicates);
                allLoadedAssociations.AddRange(loadedAssociations);
            }

            return allLoadedAssociations;
        }

        /// <summary>
        /// Remaps a chunk ID using the provided mapping, or returns original if no mapping
        /// </summary>
        private Guid RemapChunkId(Guid originalId, Dictionary<Guid, Guid> chunkIdMapping)
        {
            if (chunkIdMapping != null && chunkIdMapping.TryGetValue(originalId, out var newId))
            {
                return newId;
            }
            return originalId;
        }

        /// <summary>
        /// Checks if an association already exists between two chunks with the same relationship types
        /// </summary>
        private async Task<bool> AssociationExistsAsync(
            IChunkAssociationCollection associationCollection,
            Guid chunkAId,
            Guid chunkBId,
            string relationAtoB,
            string relationBtoA)
        {
            var existingAssociations = await associationCollection.GetAssociationsForChunkAsync(chunkAId);

            return existingAssociations.Any(a =>
                ((a.ChunkAId == chunkAId && a.ChunkBId == chunkBId &&
                  a.RelationAtoB == relationAtoB && a.RelationBtoA == relationBtoA) ||
                 (a.ChunkAId == chunkBId && a.ChunkBId == chunkAId &&
                  a.RelationAtoB == relationBtoA && a.RelationBtoA == relationAtoB)));
        }

        /// <summary>
        /// Find a chunk by its SemanticType
        /// </summary>
        private async Task<Chunk> FindChunkBySemanticTypeAsync(IChunkCollection chunkCollection, string semanticType)
        {
            var allChunks = await chunkCollection.GetAllChunksAsync();
            return allChunks.FirstOrDefault(c => c.SemanticType == semanticType);
        }



        /// <summary>
        /// Gets statistics about loaded associations
        /// </summary>
        public async Task<AssociationStats> GetAssociationStatsAsync()
        {
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null) return new AssociationStats();

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            var ontologyChunks = (await chunkCollection.GetAllChunksAsync())
                .Where(c => c.CognitiveCategory == "Ontology")
                .ToList();

            var stats = new AssociationStats
            {
                TotalAssociations = 0,
                RelationshipTypes = new Dictionary<string, int>(),
                OntologyAssociations = 0
            };

            foreach (var chunk in ontologyChunks)
            {
                var associations = await associationCollection.GetAssociationsForChunkAsync(chunk.ID);
                stats.TotalAssociations += associations.Count();

                foreach (var assoc in associations)
                {
                    // Count as ontology association if both chunks are ontology chunks
                    var otherChunkId = assoc.ChunkAId == chunk.ID ? assoc.ChunkBId : assoc.ChunkAId;
                    var otherChunk = ontologyChunks.FirstOrDefault(c => c.ID == otherChunkId);

                    if (otherChunk != null)
                    {
                        stats.OntologyAssociations++;

                        // Count relationship types
                        string relationType = assoc.ChunkAId == chunk.ID ? assoc.RelationAtoB : assoc.RelationBtoA;
                        if (!stats.RelationshipTypes.ContainsKey(relationType))
                            stats.RelationshipTypes[relationType] = 0;
                        stats.RelationshipTypes[relationType]++;
                    }
                }
            }

            // Avoid double counting (each association appears twice in the loop above)
            stats.TotalAssociations /= 2;
            stats.OntologyAssociations /= 2;
            foreach (var key in stats.RelationshipTypes.Keys.ToList())
            {
                stats.RelationshipTypes[key] /= 2;
            }

            return stats;
        }
        /// <summary>
        /// Builds ID mapping from loaded chunks using their $OldId slots
        /// </summary>
        /// <param name="loadedChunks">Chunks returned from OntologyLoader</param>
        /// <returns>Dictionary mapping old IDs to new IDs</returns>
        private Dictionary<Guid, Guid> BuildIdMappingFromChunks(List<Chunk> loadedChunks)
        {
            var idMapping = new Dictionary<Guid, Guid>();

            foreach (var chunk in loadedChunks)
            {
                if (chunk.Slots.TryGetValue("$OldId", out var oldIdSlot) &&
                    oldIdSlot.Value is Guid oldId)
                {
                    idMapping[oldId] = chunk.ID;
                }
            }

            return idMapping;
        }
    }



    /// <summary>
    /// Statistics about loaded associations
    /// </summary>
    public class AssociationStats
    {
        public int TotalAssociations { get; set; }
        public int OntologyAssociations { get; set; }
        public Dictionary<string, int> RelationshipTypes { get; set; } = new Dictionary<string, int>();

        public override string ToString()
        {
            var result = $"Total Associations: {TotalAssociations}\n";
            result += $"Ontology Associations: {OntologyAssociations}\n";

            result += "Relationship Types:\n";
            foreach (var kvp in RelationshipTypes.OrderByDescending(x => x.Value))
            {
                result += $"  {kvp.Key}: {kvp.Value} associations\n";
            }

            return result;
        }
    }
}