using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;
using Aislinn.Core.Relationships;

namespace Aislinn.Core.Goals
{
    /// <summary>
    /// Extends goal evaluation with the ability to match semantic relationships,
    /// including transitive relationships across multiple hops.
    /// </summary>
    public class GoalRelationshipMatcher
    {
        private readonly IChunkStore _chunkStore;
        private readonly RelationshipTraversalService _relationshipService;
        private readonly string _chunkCollectionId;

        // Regular expression for matching relationship patterns
        private static readonly Regex RelationshipPattern = new Regex(
            @"(\w+)\s+(\w+)\s+(\w+)",
            RegexOptions.Compiled);

        // When true, logs verbose information about relationship matching
        public bool EnableVerboseLogging { get; set; } = false;

        public GoalRelationshipMatcher(
            IChunkStore chunkStore,
            RelationshipTraversalService relationshipService,
            string chunkCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _relationshipService = relationshipService ?? throw new ArgumentNullException(nameof(relationshipService));
            _chunkCollectionId = chunkCollectionId;
        }

        /// <summary>
        /// Evaluates a relationship pattern within a goal's completion criteria
        /// </summary>
        public async Task<bool> EvaluateRelationshipPatternAsync(string pattern, Chunk goal)
        {
            if (string.IsNullOrWhiteSpace(pattern) || goal == null)
                return false;

            // Check if this is a relationship pattern
            var match = RelationshipPattern.Match(pattern.Trim());
            if (!match.Success || match.Groups.Count < 4)
                return false;

            string sourceSlotName = match.Groups[1].Value;
            string relationshipType = match.Groups[2].Value;
            string targetName = match.Groups[3].Value;

            // Get the source object from the goal slots
            if (!goal.Slots.TryGetValue(sourceSlotName, out var sourceSlot) || sourceSlot.Value == null)
            {
                LogMessage($"Source slot '{sourceSlotName}' not found or null in goal {goal.ID}");
                return false;
            }

            // Convert source value to a Guid if possible
            if (!TryGetChunkId(sourceSlot.Value, out Guid sourceId))
            {
                LogMessage($"Source slot '{sourceSlotName}' value could not be converted to a chunk ID: {sourceSlot.Value}");
                return false;
            }

            // Get the target object (could be a slot name or a direct reference to a concept)
            Guid targetId;

            // First check if the target is a slot reference
            if (goal.Slots.TryGetValue(targetName, out var targetSlot) && targetSlot.Value != null)
            {
                if (!TryGetChunkId(targetSlot.Value, out targetId))
                {
                    LogMessage($"Target slot '{targetName}' value could not be converted to a chunk ID: {targetSlot.Value}");
                    return false;
                }
            }
            else
            {
                // Target might be a concept name - try to look it up
                targetId = await GetChunkIdByNameAsync(targetName);
                if (targetId == Guid.Empty)
                {
                    LogMessage($"Could not find a chunk with name '{targetName}'");
                    return false;
                }
            }

            // Now check the relationship
            bool relationshipExists = await _relationshipService.CheckRelationshipAsync(
                sourceId, relationshipType, targetId);

            LogMessage($"Relationship {sourceSlotName} {relationshipType} {targetName}: {relationshipExists}");
            return relationshipExists;
        }

        /// <summary>
        /// Attempts to get a chunk ID from a value, handling different representations
        /// </summary>
        private bool TryGetChunkId(object value, out Guid chunkId)
        {
            chunkId = Guid.Empty;

            if (value == null)
                return false;

            // Direct Guid value
            if (value is Guid guid)
            {
                chunkId = guid;
                return true;
            }

            // Chunk object
            if (value is Chunk chunk)
            {
                chunkId = chunk.ID;
                return true;
            }

            // String representation of a Guid
            if (value is string guidString && Guid.TryParse(guidString, out Guid parsedGuid))
            {
                chunkId = parsedGuid;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a chunk ID by looking up its name in the chunk store
        /// </summary>
        private async Task<Guid> GetChunkIdByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Guid.Empty;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return Guid.Empty;

            // Get all chunks and find one with the matching name
            // Note: In a production system, you'd want to index chunks by name for efficiency
            var allChunks = await chunkCollection.GetAllChunksAsync();
            var matchingChunk = allChunks.FirstOrDefault(c =>
                c.Name != null && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return matchingChunk?.ID ?? Guid.Empty;
        }

        /// <summary>
        /// Logs diagnostic information if verbose logging is enabled
        /// </summary>
        private void LogMessage(string message)
        {
            if (EnableVerboseLogging)
            {
                Console.WriteLine($"[RelationshipMatcher] {message}");
            }
        }
    }
}