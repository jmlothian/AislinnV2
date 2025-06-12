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
    /// Loads ontology chunks from JSON with ID remapping and duplicate prevention
    /// </summary>
    public class OntologyLoader
    {
        private readonly IChunkStore _chunkStore;
        private readonly string _chunkCollectionId;

        public OntologyLoader(IChunkStore chunkStore, AislinnConfiguration config)
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _chunkCollectionId = config.ChunkCollectionId;
        }

        /// <summary>
        /// Loads chunks from JSON with optional ID remapping and duplicate prevention
        /// </summary>
        /// <param name="jsonData">JSON string containing chunk array</param>
        /// <param name="remapIds">Whether to generate new GUIDs for all chunks</param>
        /// <param name="skipDuplicates">Whether to skip chunks that already exist</param>
        /// <returns>List of loaded chunks with their final IDs</returns>
        public async Task<List<Chunk>> LoadChunksAsync(string jsonData, bool remapIds = true, bool skipDuplicates = true)
        {
            if (string.IsNullOrEmpty(jsonData))
                throw new ArgumentException("JSON data cannot be null or empty", nameof(jsonData));

            // Deserialize chunks from JSON
            var chunks = JsonSerializer.Deserialize<List<Chunk>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (chunks == null || chunks.Count == 0)
                return new List<Chunk>();

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Track ID mappings for remapping references
            var idMapping = new Dictionary<Guid, Guid>();
            var loadedChunks = new List<Chunk>();
            var existingChunks = new List<Chunk>();

            // First pass: Check for duplicates and create ID mappings
            foreach (var chunk in chunks)
            {
                if (skipDuplicates && await ChunkExistsAsync(chunkCollection, chunk))
                {
                    // Still need to track the mapping even for skipped chunks
                    // so parent references can be resolved
                    var existingChunk = await FindExistingChunkAsync(chunkCollection, chunk);
                    idMapping[chunk.ID] = existingChunk.ID;
                    existingChunks.Add(existingChunk);
                    continue;
                }

                Guid originalPlaceholderId = chunk.ID; // e.g., 00000000-0000-0000-0000-000000000001
                Guid newId = remapIds ? Guid.NewGuid() : originalPlaceholderId;

                idMapping[originalPlaceholderId] = newId;
                chunk.ID = newId;
                ConvertSlotTypes(chunk);
                if (chunk.Slots == null)
                    chunk.Slots = new Dictionary<string, ModelSlot>();
                chunk.Slots["$OldId"] = new ModelSlot
                {
                    Name = "$OldId",
                    Value = originalPlaceholderId
                };
                loadedChunks.Add(chunk);
            }

            // Second pass: Remap parent references in slots
            foreach (var chunk in loadedChunks)
            {
                RemapParentReferences(chunk, idMapping);
            }

            // Third pass: Add chunks to storage
            var savedChunks = new List<Chunk>();
            foreach (var chunk in loadedChunks)
            {
                try
                {
                    var savedChunk = await chunkCollection.AddChunkAsync(chunk);
                    savedChunks.Add(savedChunk);
                    Console.WriteLine($"Loaded chunk: {savedChunk.SemanticType} ({savedChunk.Name})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save chunk {chunk.SemanticType}: {ex.Message}");
                }
            }
            savedChunks.AddRange(existingChunks);
            return savedChunks;
        }
        private void ConvertSlotTypes(Chunk chunk)
        {
            if (chunk.Slots == null) return;

            foreach (var slot in chunk.Slots.Values)
            {
                if (slot.Value is JsonElement jsonElement)
                {
                    slot.Value = ConvertJsonElement(jsonElement);
                }
            }
        }
        private object ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out int intVal) ? intVal : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(e => e.GetString()).ToList(),
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
                _ => element.ToString()
            };
        }
        private async Task<Chunk> FindExistingChunkAsync(IChunkCollection chunkCollection, Chunk chunk)
        {
            var existingChunks = await chunkCollection.GetAllChunksAsync();
            return existingChunks.FirstOrDefault(c =>
                c.ChunkType == "Declarative" &&
                c.CognitiveCategory == "Ontology" &&
                c.SemanticType == chunk.SemanticType);
        }
        /// <summary>
        /// Checks if a chunk with the same SemanticType already exists
        /// </summary>
        private async Task<bool> ChunkExistsAsync(IChunkCollection chunkCollection, Chunk chunk)
        {
            // Check by SemanticType since that's the unique identifier for ontology chunks
            var existingChunks = await chunkCollection.GetAllChunksAsync();
            return existingChunks.Any(c =>
                c.ChunkType == "Declarative" &&
                c.CognitiveCategory == "Ontology" &&
                c.SemanticType == chunk.SemanticType);
        }

        /// <summary>
        /// Remaps parent references in chunk slots using the ID mapping
        /// </summary>
        private void RemapParentReferences(Chunk chunk, Dictionary<Guid, Guid> idMapping)
        {
            if (chunk.Slots == null) return;

            // Look for Parent slot that contains GUID references
            if (chunk.Slots.TryGetValue("Parent", out var parentSlot) && parentSlot.Value != null)
            {
                // Handle GUID parent references
                if (parentSlot.Value is Guid parentGuid && idMapping.TryGetValue(parentGuid, out var newParentGuid))
                {
                    parentSlot.Value = newParentGuid;
                }
                // Handle string parent references that can be parsed as GUID
                else if (parentSlot.Value is string parentString &&
                         Guid.TryParse(parentString, out var parsedParentGuid) &&
                         idMapping.TryGetValue(parsedParentGuid, out var newStringParentGuid))
                {
                    parentSlot.Value = newStringParentGuid.ToString();
                }
            }

            // Could extend this to handle other types of ID references in slots
            // For example, if there were "Children" arrays or other reference slots
        }

        /// <summary>
        /// Loads multiple JSON files in sequence
        /// </summary>
        /// <param name="jsonFiles">Array of JSON strings to load in order</param>
        /// <param name="remapIds">Whether to generate new GUIDs for all chunks</param>
        /// <param name="skipDuplicates">Whether to skip chunks that already exist</param>
        /// <returns>Combined list of all loaded chunks</returns>
        public async Task<List<Chunk>> LoadMultipleFilesAsync(string[] jsonFiles, bool remapIds = true, bool skipDuplicates = true)
        {
            var allLoadedChunks = new List<Chunk>();

            foreach (var jsonData in jsonFiles)
            {
                var loadedChunks = await LoadChunksAsync(jsonData, remapIds, skipDuplicates);
                allLoadedChunks.AddRange(loadedChunks);
            }

            return allLoadedChunks;
        }

        /// <summary>
        /// Gets statistics about the loaded ontology
        /// </summary>
        public async Task<OntologyStats> GetOntologyStatsAsync()
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null) return new OntologyStats();

            var allChunks = await chunkCollection.GetAllChunksAsync();
            var ontologyChunks = allChunks.Where(c => c.CognitiveCategory == "Ontology").ToList();

            var stats = new OntologyStats
            {
                TotalOntologyChunks = ontologyChunks.Count,
                ChunksByLevel = new Dictionary<int, int>(),
                ChunksByCategory = new Dictionary<string, int>()
            };

            foreach (var chunk in ontologyChunks)
            {
                // Count by level
                if (chunk.Slots.TryGetValue("Level", out var levelSlot) &&
                    levelSlot.Value is int level)
                {
                    if (!stats.ChunksByLevel.ContainsKey(level))
                        stats.ChunksByLevel[level] = 0;
                    stats.ChunksByLevel[level]++;
                }

                // Count by top-level category
                var semanticParts = chunk.SemanticType?.Split('.') ?? new string[0];
                if (semanticParts.Length >= 2)
                {
                    string category = semanticParts[1]; // e.g., "entity", "property", "relation"
                    if (!stats.ChunksByCategory.ContainsKey(category))
                        stats.ChunksByCategory[category] = 0;
                    stats.ChunksByCategory[category]++;
                }
            }

            return stats;
        }
    }

    /// <summary>
    /// Statistics about the loaded ontology
    /// </summary>
    public class OntologyStats
    {
        public int TotalOntologyChunks { get; set; }
        public Dictionary<int, int> ChunksByLevel { get; set; } = new Dictionary<int, int>();
        public Dictionary<string, int> ChunksByCategory { get; set; } = new Dictionary<string, int>();

        public override string ToString()
        {
            var result = $"Total Ontology Chunks: {TotalOntologyChunks}\n";

            result += "Chunks by Level:\n";
            foreach (var kvp in ChunksByLevel.OrderBy(x => x.Key))
            {
                result += $"  Level {kvp.Key}: {kvp.Value} chunks\n";
            }

            result += "Chunks by Category:\n";
            foreach (var kvp in ChunksByCategory.OrderBy(x => x.Key))
            {
                result += $"  {kvp.Key}: {kvp.Value} chunks\n";
            }

            return result;
        }
    }
}