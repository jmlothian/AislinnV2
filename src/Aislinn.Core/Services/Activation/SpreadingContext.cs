using System;
using System.Collections.Generic;
using Aislinn.Core.Models;

namespace Aislinn.Core.Activation
{
    /// <summary>
    /// Context information for filtering spreading activation in a generic, domain-agnostic way.
    /// Allows callers to control which associations are followed during spreading activation
    /// without the memory system needing to know about conversations, tasks, or other higher-level concepts.
    /// </summary>
    public class SpreadingContext
    {
        /// <summary>
        /// Chunk IDs that are currently in cognitive focus.
        /// Spreading may prefer paths that connect to these chunks.
        /// </summary>
        public HashSet<Guid> FocusedChunkIds { get; set; } = new HashSet<Guid>();

        /// <summary>
        /// Relation types to allow during spreading.
        /// null = allow all relation types
        /// empty set = allow no relation types (blocks all spreading)
        /// populated set = allow only these specific relation types
        /// </summary>
        public HashSet<string> AllowedRelationTypes { get; set; } = null;

        /// <summary>
        /// Relation types to explicitly block during spreading.
        /// Applied after AllowedRelationTypes check.
        /// </summary>
        public HashSet<string> BlockedRelationTypes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Semantic types of target chunks to allow during spreading.
        /// null = allow all semantic types
        /// empty set = allow no semantic types (blocks all spreading)
        /// populated set = allow only these specific semantic types
        /// </summary>
        public HashSet<string> AllowedSemanticTypes { get; set; } = null;

        /// <summary>
        /// Semantic types of target chunks to explicitly block during spreading.
        /// Applied after AllowedSemanticTypes check.
        /// </summary>
        public HashSet<string> BlockedSemanticTypes { get; set; } = new HashSet<string>();

        /// <summary>
        /// Override the maximum spreading depth for this context.
        /// If null, uses the default depth calculation.
        /// </summary>
        public int? MaxDepthOverride { get; set; }

        /// <summary>
        /// Generic cognitive mode or state that could influence spreading behavior.
        /// Examples: "task-focused", "social", "creative", "analytical"
        /// </summary>
        public string CognitiveMode { get; set; }

        /// <summary>
        /// Activation boost reduction factor for chunks not in working memory or focus.
        /// 1.0 = no reduction, 0.5 = half boost, 0.0 = no boost for non-contextual chunks.
        /// Default is 0.3 (significant reduction for non-contextual chunks).
        /// </summary>
        public double NonContextualBoostFactor { get; set; } = 0.3;

        /// <summary>
        /// Allow spreading to chunks that have this many or more associations 
        /// with chunks in FocusedChunkIds, even if other filters would block them.
        /// This enables discovery of well-connected contextually relevant chunks.
        /// null = disabled
        /// </summary>
        public int? MinAssociationCountForDiscovery { get; set; } = null;

        /// <summary>
        /// Checks if a relation type should be allowed based on the current context filters.
        /// </summary>
        /// <param name="relationType">The relation type to check</param>
        /// <returns>True if the relation type should be allowed, false otherwise</returns>
        public bool IsRelationTypeAllowed(string relationType)
        {
            if (string.IsNullOrEmpty(relationType))
                return false;

            // Check allowlist first
            if (AllowedRelationTypes != null && !AllowedRelationTypes.Contains(relationType))
                return false;

            // Check blocklist
            if (BlockedRelationTypes.Contains(relationType))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if a semantic type should be allowed based on the current context filters.
        /// </summary>
        /// <param name="semanticType">The semantic type to check</param>
        /// <returns>True if the semantic type should be allowed, false otherwise</returns>
        public bool IsSemanticTypeAllowed(string semanticType)
        {
            if (string.IsNullOrEmpty(semanticType))
                return false;

            // Check allowlist first
            if (AllowedSemanticTypes != null && !AllowedSemanticTypes.Contains(semanticType))
                return false;

            // Check blocklist
            if (BlockedSemanticTypes.Contains(semanticType))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if a target chunk should be allowed based on all current context filters.
        /// </summary>
        /// <param name="targetChunk">The target chunk to check</param>
        /// <param name="association">The association leading to this chunk</param>
        /// <param name="isSourceA">Whether the source chunk is ChunkA in the association</param>
        /// <param name="allAssociations">All associations for the target chunk (for discovery counting)</param>
        /// <returns>True if spreading should continue to this target, false otherwise</returns>
        public bool ShouldSpreadToTarget(Chunk targetChunk, ChunkAssociation association, bool isSourceA, IEnumerable<ChunkAssociation> allAssociations = null)
        {
            if (targetChunk == null || association == null)
                return false;

            // Check relation type
            string relationType = isSourceA ? association.RelationAtoB : association.RelationBtoA;
            bool relationAllowed = IsRelationTypeAllowed(relationType);

            // Check semantic type
            bool semanticAllowed = IsSemanticTypeAllowed(targetChunk.SemanticType);

            // If normal filtering passes, allow
            if (relationAllowed && semanticAllowed)
                return true;

            // If normal filtering fails, check discovery mechanism
            if (MinAssociationCountForDiscovery.HasValue && allAssociations != null)
            {
                int contextualAssociationCount = CountContextualAssociations(targetChunk.ID, allAssociations);
                if (contextualAssociationCount >= MinAssociationCountForDiscovery.Value)
                    return true; // Discovery override
            }

            return false;
        }

        /// <summary>
        /// Counts how many associations the target chunk has with chunks in FocusedChunkIds.
        /// </summary>
        /// <param name="targetChunkId">The chunk to count associations for</param>
        /// <param name="allAssociations">All associations for the target chunk</param>
        /// <returns>Number of associations with focused chunks</returns>
        public int CountContextualAssociations(Guid targetChunkId, IEnumerable<ChunkAssociation> allAssociations)
        {
            if (FocusedChunkIds.Count == 0 || allAssociations == null)
                return 0;

            int count = 0;
            foreach (var association in allAssociations)
            {
                // Check if this association connects to a focused chunk
                if (association.ChunkAId == targetChunkId && FocusedChunkIds.Contains(association.ChunkBId))
                    count++;
                else if (association.ChunkBId == targetChunkId && FocusedChunkIds.Contains(association.ChunkAId))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Determines if a chunk is contextual (should get full activation boost).
        /// Currently based on whether the chunk is in FocusedChunkIds.
        /// </summary>
        /// <param name="chunkId">The chunk to check</param>
        /// <returns>True if chunk should get full boost, false if boost should be reduced</returns>
        public bool IsChunkContextual(Guid chunkId)
        {
            return FocusedChunkIds.Contains(chunkId);
        }

        /// <summary>
        /// Calculates the boost factor to apply for a given chunk.
        /// </summary>
        /// <param name="chunkId">The chunk to calculate boost for</param>
        /// <returns>Boost factor (1.0 for contextual chunks, NonContextualBoostFactor for others)</returns>
        public double GetBoostFactor(Guid chunkId)
        {
            return IsChunkContextual(chunkId) ? 1.0 : NonContextualBoostFactor;
        }

        /// <summary>
        /// Creates a context that allows all spreading (equivalent to no context filtering).
        /// </summary>
        public static SpreadingContext AllowAll()
        {
            return new SpreadingContext();
        }

        /// <summary>
        /// Creates a context that blocks all spreading.
        /// </summary>
        public static SpreadingContext AllowNone()
        {
            return new SpreadingContext
            {
                AllowedRelationTypes = new HashSet<string>(),
                AllowedSemanticTypes = new HashSet<string>()
            };
        }

        /// <summary>
        /// Creates a context that only allows specific relation types.
        /// </summary>
        public static SpreadingContext AllowRelationTypes(params string[] relationTypes)
        {
            return new SpreadingContext
            {
                AllowedRelationTypes = new HashSet<string>(relationTypes)
            };
        }

        /// <summary>
        /// Creates a context that only allows specific semantic types.
        /// </summary>
        public static SpreadingContext AllowSemanticTypes(params string[] semanticTypes)
        {
            return new SpreadingContext
            {
                AllowedSemanticTypes = new HashSet<string>(semanticTypes)
            };
        }

        /// <summary>
        /// Creates a context focused on specific chunks, with optional filtering.
        /// </summary>
        public static SpreadingContext FocusedOn(IEnumerable<Guid> focusedChunkIds, string cognitiveMode = null)
        {
            return new SpreadingContext
            {
                FocusedChunkIds = new HashSet<Guid>(focusedChunkIds),
                CognitiveMode = cognitiveMode
            };
        }
    }
}