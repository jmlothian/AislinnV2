using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Models;

namespace Aislinn.Core.Relationships
{
    /// <summary>
    /// Service for traversing relationships between chunks, supporting transitive relationship checking
    /// and caching for performance optimization.
    /// </summary>
    public class RelationshipTraversalService
    {
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;

        // Cache for direct relationships - {sourceId}:{relationshipType}:{targetId} -> bool
        private readonly Dictionary<string, bool> _directRelationshipCache;

        // Cache for transitive relationships - {sourceId}:{relationshipType}:{targetId} -> bool
        private readonly Dictionary<string, bool> _transitiveRelationshipCache;

        // Track when caches were last updated
        private DateTime _lastCacheCleanup;

        // Cache expiration times
        private readonly TimeSpan _directCacheExpiration;
        private readonly TimeSpan _transitiveCacheExpiration;

        /// <summary>
        /// Initializes a new instance of the RelationshipTraversalService
        /// </summary>
        public RelationshipTraversalService(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            string chunkCollectionId = "default",
            string associationCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _associationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            _chunkCollectionId = chunkCollectionId;
            _associationCollectionId = associationCollectionId;

            _directRelationshipCache = new Dictionary<string, bool>();
            _transitiveRelationshipCache = new Dictionary<string, bool>();
            _lastCacheCleanup = DateTime.Now;

            // Set cache expiration times - direct cache has longer expiration since relationships rarely change
            _directCacheExpiration = TimeSpan.FromHours(1);
            _transitiveCacheExpiration = TimeSpan.FromMinutes(10);
        }

        /// <summary>
        /// Creates a cache key for relationship lookup
        /// </summary>
        private string GetCacheKey(Guid sourceId, string relationshipType, Guid targetId)
        {
            return $"{sourceId}:{relationshipType}:{targetId}";
        }

        /// <summary>
        /// Checks if a direct relationship exists between source and target
        /// </summary>
        public async Task<bool> HasDirectRelationshipAsync(Guid sourceId, string relationshipType, Guid targetId)
        {
            string cacheKey = GetCacheKey(sourceId, relationshipType, targetId);

            // Check cache first
            if (_directRelationshipCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                return false;

            // Get associations for the source
            var associations = await associationCollection.GetAssociationsForChunkAsync(sourceId);

            // Look for the specific relationship type
            bool hasRelationship = associations.Any(a =>
                (a.ChunkAId == sourceId && a.ChunkBId == targetId && a.RelationAtoB == relationshipType) ||
                (a.ChunkBId == sourceId && a.ChunkAId == targetId && a.RelationBtoA == relationshipType));

            // Cache the result
            _directRelationshipCache[cacheKey] = hasRelationship;

            return hasRelationship;
        }

        /// <summary>
        /// Gets all chunks directly related to the source by the specified relationship type
        /// </summary>
        public async Task<List<Guid>> GetDirectlyRelatedChunksAsync(Guid sourceId, string relationshipType)
        {
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null)
                return new List<Guid>();

            var associations = await associationCollection.GetAssociationsForChunkAsync(sourceId);
            var relatedChunks = new List<Guid>();

            foreach (var association in associations)
            {
                if (association.ChunkAId == sourceId && association.RelationAtoB == relationshipType)
                {
                    relatedChunks.Add(association.ChunkBId);
                }
                else if (association.ChunkBId == sourceId && association.RelationBtoA == relationshipType)
                {
                    relatedChunks.Add(association.ChunkAId);
                }
            }

            return relatedChunks;
        }

        /// <summary>
        /// Checks if a relationship exists between source and target, potentially through a chain of relationships
        /// Uses bidirectional breadth-first search for efficiency.
        /// </summary>
        public async Task<bool> CheckRelationshipAsync(
            Guid sourceId,
            string relationshipType,
            Guid targetId,
            int maxDepth = 5)
        {
            // Handle identity case
            if (sourceId == targetId)
                return relationshipType == "IsSameAs" || relationshipType == "Equals";

            string cacheKey = GetCacheKey(sourceId, relationshipType, targetId);

            // Check transitive cache
            if (_transitiveRelationshipCache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            // Check direct relationship first (and cache it)
            if (await HasDirectRelationshipAsync(sourceId, relationshipType, targetId))
            {
                _transitiveRelationshipCache[cacheKey] = true;
                return true;
            }

            // If max depth is 1, we've already checked the direct relationship
            if (maxDepth <= 1)
            {
                _transitiveRelationshipCache[cacheKey] = false;
                return false;
            }

            // Perform bidirectional BFS
            bool result = await BidirectionalBfsAsync(sourceId, relationshipType, targetId, maxDepth);

            // Cache the result
            _transitiveRelationshipCache[cacheKey] = result;

            // Periodically clean up cache
            CleanupCacheIfNeeded();

            return result;
        }

        /// <summary>
        /// Performs a bidirectional breadth-first search to find a path between source and target
        /// </summary>
        private async Task<bool> BidirectionalBfsAsync(
            Guid sourceId,
            string relationshipType,
            Guid targetId,
            int maxDepth)
        {
            // Early exit if either ID is empty
            if (sourceId == Guid.Empty || targetId == Guid.Empty)
                return false;

            // Initialize forward search (from source)
            var forwardQueue = new Queue<Guid>();
            var forwardVisited = new Dictionary<Guid, int>(); // node -> depth
            forwardQueue.Enqueue(sourceId);
            forwardVisited[sourceId] = 0;

            // Initialize backward search (from target)
            var backwardQueue = new Queue<Guid>();
            var backwardVisited = new Dictionary<Guid, int>(); // node -> depth
            backwardQueue.Enqueue(targetId);
            backwardVisited[targetId] = 0;

            // Bidirectional BFS
            while (forwardQueue.Count > 0 && backwardQueue.Count > 0)
            {
                // Check if we've found a meeting point
                foreach (var forwardNode in forwardVisited.Keys)
                {
                    if (backwardVisited.ContainsKey(forwardNode))
                    {
                        // Found a path - check if it's within our max depth
                        int totalDepth = forwardVisited[forwardNode] + backwardVisited[forwardNode];
                        if (totalDepth <= maxDepth)
                            return true;
                    }
                }

                // Expand forward search by one step
                if (forwardQueue.Count > 0)
                {
                    await ExpandBfsLevelAsync(forwardQueue, forwardVisited, relationshipType, maxDepth, true);
                }

                // Expand backward search by one step
                if (backwardQueue.Count > 0)
                {
                    await ExpandBfsLevelAsync(backwardQueue, backwardVisited, relationshipType, maxDepth, false);
                }
            }

            return false;
        }

        /// <summary>
        /// Expands one level of the BFS search
        /// </summary>
        private async Task ExpandBfsLevelAsync(
            Queue<Guid> queue,
            Dictionary<Guid, int> visited,
            string relationshipType,
            int maxDepth,
            bool isForward)
        {
            int levelSize = queue.Count;

            for (int i = 0; i < levelSize; i++)
            {
                Guid currentNode = queue.Dequeue();
                int currentDepth = visited[currentNode];

                // Don't expand beyond max depth
                if (currentDepth >= maxDepth)
                    continue;

                // Get related nodes in the correct direction
                var relatedNodes = await GetDirectlyRelatedChunksAsync(
                    currentNode,
                    isForward ? relationshipType : GetReverseRelationship(relationshipType));

                foreach (var relatedNode in relatedNodes)
                {
                    if (!visited.ContainsKey(relatedNode))
                    {
                        visited[relatedNode] = currentDepth + 1;
                        queue.Enqueue(relatedNode);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the reverse relationship type for bidirectional search
        /// </summary>
        private string GetReverseRelationship(string relationshipType)
        {
            // This would be expanded with a complete mapping of relationship types to their inverses
            switch (relationshipType)
            {
                case "IsA": return "HasInstance";
                case "HasInstance": return "IsA";
                case "HasPart": return "PartOf";
                case "PartOf": return "HasPart";
                case "MadeOf": return "MaterialFor";
                case "MaterialFor": return "MadeOf";
                case "CanDo": return "CanBeDoneBy";
                case "CanBeDoneBy": return "CanDo";
                default: return $"Inverse{relationshipType}";
            }
        }

        /// <summary>
        /// Periodically cleans up expired cache entries
        /// </summary>
        private void CleanupCacheIfNeeded()
        {
            var now = DateTime.Now;

            // Only clean up periodically
            if ((now - _lastCacheCleanup) < _transitiveCacheExpiration)
                return;

            // Clean up direct relationship cache (longer expiration)
            if ((now - _lastCacheCleanup) >= _directCacheExpiration)
            {
                _directRelationshipCache.Clear();
            }

            // Clean up transitive cache
            _transitiveRelationshipCache.Clear();

            _lastCacheCleanup = now;
        }

        /// <summary>
        /// Invalidates all cached entries related to a specific chunk
        /// Called when relationships for a chunk are modified
        /// </summary>
        public void InvalidateCacheForChunk(Guid chunkId)
        {
            var directKeys = _directRelationshipCache.Keys
                .Where(k => k.StartsWith($"{chunkId}:") || k.EndsWith($":{chunkId}"))
                .ToList();

            foreach (var key in directKeys)
            {
                _directRelationshipCache.Remove(key);
            }

            var transitiveKeys = _transitiveRelationshipCache.Keys
                .Where(k => k.StartsWith($"{chunkId}:") || k.EndsWith($":{chunkId}"))
                .ToList();

            foreach (var key in transitiveKeys)
            {
                _transitiveRelationshipCache.Remove(key);
            }
        }

        /// <summary>
        /// Completely clears all caches
        /// </summary>
        public void ClearAllCaches()
        {
            _directRelationshipCache.Clear();
            _transitiveRelationshipCache.Clear();
            _lastCacheCleanup = DateTime.Now;
        }
    }
}