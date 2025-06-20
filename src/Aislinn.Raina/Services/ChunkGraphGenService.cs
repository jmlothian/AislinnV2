using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Configuration;
using Aislinn.Core.Models;
using Aislinn.Storage.AssociationStore;

public class SigmaGraphDiff
{
    [JsonPropertyName("nodes")]
    public List<SigmaGraphNode> Nodes { get; set; } = new List<SigmaGraphNode>();

    [JsonPropertyName("edges")]
    public List<SigmaGraphEdge> Edges { get; set; } = new List<SigmaGraphEdge>();

    [JsonPropertyName("removedNodeIds")]
    public List<string> RemovedNodeIds { get; set; } = new List<string>();

    [JsonPropertyName("removedEdgeIds")]
    public List<string> RemovedEdgeIds { get; set; } = new List<string>();

    // Add missing properties that were referenced in CalculateGraphDiff method
    [JsonPropertyName("nodesToAdd")]
    public List<SigmaGraphNode> NodesToAdd { get; set; } = new List<SigmaGraphNode>();

    [JsonPropertyName("nodesToRemove")]
    public List<string> NodesToRemove { get; set; } = new List<string>();

    [JsonPropertyName("nodesToUpdate")]
    public List<SigmaGraphNode> NodesToUpdate { get; set; } = new List<SigmaGraphNode>();

    [JsonPropertyName("edgesToAdd")]
    public List<SigmaGraphEdge> EdgesToAdd { get; set; } = new List<SigmaGraphEdge>();

    [JsonPropertyName("edgesToRemove")]
    public List<string> EdgesToRemove { get; set; } = new List<string>();

    [JsonPropertyName("edgesToUpdate")]
    public List<SigmaGraphEdge> EdgesToUpdate { get; set; } = new List<SigmaGraphEdge>();
}

public class ChunkGraphGenService
{
    // Add missing fields that are referenced in the code
    private readonly IAssociationStore _associationStore;
    private readonly string _associationCollectionId;
    private readonly IChunkStore _chunkStore;
    private readonly string _chunkCollectionId;
    public ChunkGraphGenService(IChunkStore chunkStore, IAssociationStore associationStore,
            AislinnConfiguration config)
    {
        _associationCollectionId = config.AssociationCollectionId;
        _associationStore = associationStore;
        _chunkStore = chunkStore;
        _chunkCollectionId = config.ChunkCollectionId;
    }

    public async Task<string> GenerateSigmaGraphDiffAsync(Chunk[] oldChunks, Chunk[] newChunks)
    {
        var diff = new SigmaGraphDiff();

        var oldChunkIds = oldChunks.Select(c => c.ID).ToHashSet();
        var newChunkIds = newChunks.Select(c => c.ID).ToHashSet();

        // Find removed chunks
        diff.RemovedNodeIds = oldChunkIds.Except(newChunkIds).Select(id => id.ToString()).ToList();

        // Find added or changed chunks (by activation level)
        var oldChunkDict = oldChunks.ToDictionary(c => c.ID, c => c);

        foreach (var newChunk in newChunks)
        {
            bool shouldInclude = false;

            // New chunk
            if (!oldChunkDict.ContainsKey(newChunk.ID))
            {
                shouldInclude = true;
            }
            // Existing chunk with activation change
            else if (oldChunkDict[newChunk.ID].ActivationLevel != newChunk.ActivationLevel)
            {
                shouldInclude = true;
            }

            if (shouldInclude)
            {
                // Reuse the existing node creation logic
                var associationCount = await GetAssociationCountForChunk(newChunk.ID, newChunkIds);
                var maxAssociations = await GetMaxAssociationCount(newChunks);
                var maxActivation = newChunks.Any() ? newChunks.Max(c => c.ActivationLevel) : 1.0;

                var node = CreateSigmaNode(newChunk, associationCount, maxAssociations, maxActivation);
                diff.Nodes.Add(node);
            }
        }

        // Handle associations
        var oldAssociations = await GetAssociationsForChunks(oldChunkIds);
        var newAssociations = await GetAssociationsForChunks(newChunkIds);

        var oldAssociationKeys = oldAssociations.Select(a => $"{a.ChunkAId}-{a.ChunkBId}").ToHashSet();
        var newAssociationKeys = newAssociations.Select(a => $"{a.ChunkAId}-{a.ChunkBId}").ToHashSet();

        // Find removed associations
        diff.RemovedEdgeIds = oldAssociationKeys.Except(newAssociationKeys).ToList();

        // Find added or changed associations
        var oldAssociationDict = oldAssociations.ToDictionary(a => $"{a.ChunkAId}-{a.ChunkBId}", a => a);

        foreach (var newAssociation in newAssociations)
        {
            var key = $"{newAssociation.ChunkAId}-{newAssociation.ChunkBId}";
            bool shouldInclude = false;

            // New association
            if (!oldAssociationDict.ContainsKey(key))
            {
                shouldInclude = true;
            }
            // Existing association with weight change
            else
            {
                var oldAssoc = oldAssociationDict[key];
                if (oldAssoc.WeightAtoB != newAssociation.WeightAtoB ||
                    oldAssoc.WeightBtoA != newAssociation.WeightBtoA)
                {
                    shouldInclude = true;
                }
            }

            if (shouldInclude)
            {
                var edge = CreateSigmaEdge(newAssociation);
                diff.Edges.Add(edge);
            }
        }

        // Use System.Text.Json instead of JsonConvert
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Serialize(diff, options);
    }

    private async Task<int> GetAssociationCountForChunk(Guid chunkId, HashSet<Guid> validChunkIds)
    {
        var associations = await _associationStore.GetCollectionAsync(_associationCollectionId)
            .ContinueWith(async collection =>
            {
                if (collection.Result != null)
                    return await collection.Result.GetAssociationsForChunkAsync(chunkId);
                return new List<ChunkAssociation>();
            }).Unwrap();

        return associations.Count(a => validChunkIds.Contains(a.ChunkAId) && validChunkIds.Contains(a.ChunkBId));
    }

    private async Task<int> GetMaxAssociationCount(Chunk[] chunks)
    {
        var chunkIds = chunks.Select(c => c.ID).ToHashSet();
        var maxCount = 0;

        foreach (var chunk in chunks)
        {
            var count = await GetAssociationCountForChunk(chunk.ID, chunkIds);
            maxCount = Math.Max(maxCount, count);
        }

        return maxCount;
    }

    private async Task<List<ChunkAssociation>> GetAssociationsForChunks(HashSet<Guid> chunkIds)
    {
        var allAssociations = new List<ChunkAssociation>();

        foreach (var chunkId in chunkIds)
        {
            var associations = await _associationStore.GetCollectionAsync(_associationCollectionId)
                .ContinueWith(async collection =>
                {
                    if (collection.Result != null)
                        return await collection.Result.GetAssociationsForChunkAsync(chunkId);
                    return new List<ChunkAssociation>();
                }).Unwrap();

            var filteredAssociations = associations.Where(a =>
                chunkIds.Contains(a.ChunkAId) && chunkIds.Contains(a.ChunkBId));

            allAssociations.AddRange(filteredAssociations);
        }

        return allAssociations
            .GroupBy(a => new { a.ChunkAId, a.ChunkBId })
            .Select(g => g.First())
            .ToList();
    }

    private SigmaGraphNode CreateSigmaNode(Chunk chunk, int associationCount, int maxAssociations, double maxActivation)
    {
        var random = new Random();
        return new SigmaGraphNode
        {
            Id = chunk.ID.ToString(),
            Label = chunk.Name ?? chunk.ID.ToString(),
            X = random.NextDouble() * 1000,
            Y = random.NextDouble() * 1000,
            Size = CalculateNodeSize(associationCount, maxAssociations),
            Color = GetColorByActivationLevel(chunk.ActivationLevel, maxActivation),
            ChunkType = chunk.ChunkType,
            CognitiveCategory = chunk.CognitiveCategory,
            SemanticType = chunk.SemanticType,
            ActivationLevel = chunk.ActivationLevel,
            ActivationHistory = chunk.ActivationHistory?.Select(h => new SigmaGraphActivationHistory
            {
                PreviousValue = h.PreviousValue,
                NewValue = h.NewValue,
                Change = h.Change,
                SequenceNumber = h.SequenceNumber,
                ActivationDate = h.ActivationDate,
                EmotionName = h.EmotionName,
                ActivatedByChunk = h.ActivatedByChunk.ToString(),
                ActivatedBy = h.ActivatedBy?.Select(id => id.ToString()).ToList() ?? new List<string>(),
                FormattedDate = FormatCognitiveTime(h.ActivationDate),
                ActivationReason = h.ActivationReason,
                ActivationSource = h.ActivationSource
            }).ToList() ?? new List<SigmaGraphActivationHistory>(),
            Slots = chunk.Slots?.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)new
                {
                    Name = kvp.Value.Name,
                    SlotType = kvp.Value.SlotType,
                    Value = kvp.Value.Value?.ToString()
                }
            ) ?? new Dictionary<string, object>()
        };
    }

    private SigmaGraphEdge CreateSigmaEdge(ChunkAssociation association)
    {
        return new SigmaGraphEdge
        {
            Id = $"{association.ChunkAId}-{association.ChunkBId}",
            Source = association.ChunkAId.ToString(),
            Target = association.ChunkBId.ToString(),
            Label = $"{association.RelationAtoB} / {association.RelationBtoA}",
            Size = Math.Max(1, (association.WeightAtoB + association.WeightBtoA) * 5),
            Color = GetColorByRelationType(association.RelationAtoB),
            RelationAtoB = association.RelationAtoB,
            RelationBtoA = association.RelationBtoA,
            WeightAtoB = association.WeightAtoB,
            WeightBtoA = association.WeightBtoA,
            LastActivated = association.LastActivated
        };
    }

    public SigmaGraphDiff CalculateGraphDiff(SigmaGraph oldGraph, SigmaGraph newGraph, double threshold = 0.001)
    {
        var diff = new SigmaGraphDiff();

        // Create lookup dictionaries for efficient comparison
        var oldNodes = oldGraph.Nodes.ToDictionary(n => n.Id, n => n);
        var newNodes = newGraph.Nodes.ToDictionary(n => n.Id, n => n);
        var oldEdges = oldGraph.Edges.ToDictionary(e => e.Id, e => e);
        var newEdges = newGraph.Edges.ToDictionary(e => e.Id, e => e);

        // Find nodes to add
        foreach (var newNode in newGraph.Nodes)
        {
            if (!oldNodes.ContainsKey(newNode.Id))
            {
                diff.NodesToAdd.Add(newNode);
            }
        }

        // Find nodes to remove
        foreach (var oldNode in oldGraph.Nodes)
        {
            if (!newNodes.ContainsKey(oldNode.Id))
            {
                diff.NodesToRemove.Add(oldNode.Id);
            }
        }

        // Find nodes to update
        foreach (var newNode in newGraph.Nodes)
        {
            if (oldNodes.TryGetValue(newNode.Id, out var oldNode))
            {
                if (HasNodeChanged(oldNode, newNode, threshold))
                {
                    diff.NodesToUpdate.Add(newNode);
                }
            }
        }

        // Find edges to add
        foreach (var newEdge in newGraph.Edges)
        {
            if (!oldEdges.ContainsKey(newEdge.Id))
            {
                diff.EdgesToAdd.Add(newEdge);
            }
        }

        // Find edges to remove
        foreach (var oldEdge in oldGraph.Edges)
        {
            if (!newEdges.ContainsKey(oldEdge.Id))
            {
                diff.EdgesToRemove.Add(oldEdge.Id);
            }
        }

        // Find edges to update
        foreach (var newEdge in newGraph.Edges)
        {
            if (oldEdges.TryGetValue(newEdge.Id, out var oldEdge))
            {
                if (HasEdgeChanged(oldEdge, newEdge, threshold))
                {
                    diff.EdgesToUpdate.Add(newEdge);
                }
            }
        }

        return diff;
    }

    private bool HasNodeChanged(SigmaGraphNode oldNode, SigmaGraphNode newNode, double threshold)
    {
        // Check numerical properties with threshold
        if (Math.Abs(oldNode.X - newNode.X) > threshold) return true;
        if (Math.Abs(oldNode.Y - newNode.Y) > threshold) return true;
        if (Math.Abs(oldNode.Size - newNode.Size) > threshold) return true;
        if (Math.Abs(oldNode.ActivationLevel - newNode.ActivationLevel) > threshold) return true;

        // Check string properties
        if (oldNode.Label != newNode.Label) return true;
        if (oldNode.Color != newNode.Color) return true;
        if (oldNode.ChunkType != newNode.ChunkType) return true;
        if (oldNode.CognitiveCategory != newNode.CognitiveCategory) return true;
        if (oldNode.SemanticType != newNode.SemanticType) return true;

        // Check activation history changes (compare counts as a simple check)
        if (oldNode.ActivationHistory.Count != newNode.ActivationHistory.Count) return true;

        // Optionally check if latest activation history item is different
        if (oldNode.ActivationHistory.Any() && newNode.ActivationHistory.Any())
        {
            var oldLatest = oldNode.ActivationHistory.First();
            var newLatest = newNode.ActivationHistory.First();
            if (oldLatest.SequenceNumber != newLatest.SequenceNumber) return true;
        }

        return false;
    }

    private bool HasEdgeChanged(SigmaGraphEdge oldEdge, SigmaGraphEdge newEdge, double threshold)
    {
        // Check numerical properties
        if (Math.Abs(oldEdge.Size - newEdge.Size) > threshold) return true;
        if (Math.Abs(oldEdge.WeightAtoB - newEdge.WeightAtoB) > threshold) return true;
        if (Math.Abs(oldEdge.WeightBtoA - newEdge.WeightBtoA) > threshold) return true;

        // Check string properties
        if (oldEdge.Label != newEdge.Label) return true;
        if (oldEdge.Color != newEdge.Color) return true;
        if (oldEdge.Source != newEdge.Source) return true;
        if (oldEdge.Target != newEdge.Target) return true;
        if (oldEdge.RelationAtoB != newEdge.RelationAtoB) return true;
        if (oldEdge.RelationBtoA != newEdge.RelationBtoA) return true;

        // Check timestamp
        if (oldEdge.LastActivated != newEdge.LastActivated) return true;

        return false;
    }

    public async Task<string> GenerateSigmaGraphAsync(Chunk[] chunks)
    {
        var graph = new SigmaGraph();
        var random = new Random();


        // Get all working memory associations first to calculate node sizes
        var allAssociations = new List<ChunkAssociation>();
        var chunkIds = chunks.Select(c => c.ID).ToHashSet();




        // Collect additional chunk IDs to load
        var additionalChunkIds = new HashSet<Guid>();

        // 1. Get chunks referenced by associations
        foreach (var association in allAssociations)
        {
            additionalChunkIds.Add(association.ChunkAId);
            additionalChunkIds.Add(association.ChunkBId);
        }

        // 2. Get chunks referenced in activation history
        foreach (var chunk in chunks)
        {
            if (chunk.ActivationHistory != null)
            {
                foreach (var historyItem in chunk.ActivationHistory)
                {
                    if (historyItem.ActivatedBy != null)
                    {
                        additionalChunkIds.UnionWith(historyItem.ActivatedBy);
                    }
                }
            }
        }

        // Remove chunks we already have
        additionalChunkIds.ExceptWith(chunkIds);

        // Load the additional chunks
        var additionalChunks = new List<Chunk>();
        if (additionalChunkIds.Any())
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection != null)
            {
                foreach (var chunkId in additionalChunkIds)
                {
                    try
                    {
                        var chunk = await chunkCollection.GetChunkAsync(chunkId);
                        if (chunk != null)
                        {
                            additionalChunks.Add(chunk);
                        }
                    }
                    catch
                    {
                        // Skip chunks that can't be loaded
                    }
                }
            }
        }
        var allChunks = chunks.Concat(additionalChunks).ToArray();
        var allChunkIds = allChunks.Select(c => c.ID).ToHashSet();

        foreach (var chunkId in allChunkIds)
        {
            var chunkAssociations = await _associationStore.GetCollectionAsync(_associationCollectionId)
                .ContinueWith(async collection =>
                {
                    if (collection.Result != null)
                        return await collection.Result.GetAssociationsForChunkAsync(chunkId);
                    return new List<ChunkAssociation>();
                }).Unwrap();

            // Only include associations where both chunks are in our set
            var filteredAssociations = chunkAssociations.Where(a =>
                allChunkIds.Contains(a.ChunkAId) && allChunkIds.Contains(a.ChunkBId));

            allAssociations.AddRange(filteredAssociations);
        }
        // Remove duplicates
        allAssociations = allAssociations
            .GroupBy(a => new { a.ChunkAId, a.ChunkBId, a.RelationAtoB, a.RelationBtoA })
            .Select(g => g.First())
            .ToList();

        // Count associations per chunk
        var associationCounts = new Dictionary<Guid, int>();
        foreach (var chunk in allChunks)
        {
            var count = allAssociations.Count(a => a.ChunkAId == chunk.ID || a.ChunkBId == chunk.ID);
            associationCounts[chunk.ID] = count;
        }

        // Find max values for normalization
        var maxAssociations = associationCounts.Values.Any() ? associationCounts.Values.Max() : 1;
        var maxActivation = allChunks.Any() ? allChunks.Max(c => c.ActivationLevel) : 1.0;

        // Create nodes from all chunks
        var originalChunkIds = chunks.Select(c => c.ID).ToHashSet();
        foreach (var chunk in allChunks)
        {
            var associationCount = associationCounts.GetValueOrDefault(chunk.ID, 0);

            var node = new SigmaGraphNode
            {
                Id = chunk.ID.ToString(),
                Label = chunk.Name ?? chunk.ID.ToString(),
                X = random.NextDouble() * 1000, // Random positioning
                Y = random.NextDouble() * 1000,
                Size = CalculateNodeSize(associationCount, maxAssociations),
                Color = GetColorByActivationLevel(chunk.ActivationLevel, maxActivation),
                ChunkType = chunk.ChunkType,
                CognitiveCategory = chunk.CognitiveCategory,
                SemanticType = chunk.SemanticType,
                ActivationLevel = chunk.ActivationLevel,
                IsWorkingMemory = originalChunkIds.Contains(chunk.ID),
                ActivationHistory = chunk.ActivationHistory?.Select(h => new SigmaGraphActivationHistory
                {
                    PreviousValue = h.PreviousValue,
                    NewValue = h.NewValue,
                    Change = h.Change,
                    SequenceNumber = h.SequenceNumber,
                    ActivationDate = h.ActivationDate,
                    EmotionName = h.EmotionName,
                    ActivatedByChunk = h.ActivatedByChunk.ToString(),
                    ActivatedBy = h.ActivatedBy?.Select(id => id.ToString()).ToList() ?? new List<string>(),
                    FormattedDate = FormatCognitiveTime(h.ActivationDate),
                    ActivationReason = h.ActivationReason,
                    ActivationSource = h.ActivationSource
                }).ToList() ?? new List<SigmaGraphActivationHistory>(),
                Slots = chunk.Slots?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)new
                    {
                        Name = kvp.Value.Name,
                        SlotType = kvp.Value.SlotType,
                        Value = kvp.Value.Value?.ToString()
                    }
                ) ?? new Dictionary<string, object>()
            };

            graph.Nodes.Add(node);
        }

        // Get all associations for the chunks
        var associations = allAssociations;

        // Create edges from associations
        foreach (var association in associations)
        {
            var edge = new SigmaGraphEdge
            {
                Id = $"{association.ChunkAId}-{association.ChunkBId}",
                Source = association.ChunkAId.ToString(),
                Target = association.ChunkBId.ToString(),
                Label = $"{association.RelationAtoB} / {association.RelationBtoA}",
                Size = Math.Max(1, (association.WeightAtoB + association.WeightBtoA) * 5),
                Color = GetColorByRelationType(association.RelationAtoB),
                RelationAtoB = association.RelationAtoB,
                RelationBtoA = association.RelationBtoA,
                WeightAtoB = association.WeightAtoB,
                WeightBtoA = association.WeightBtoA,
                LastActivated = association.LastActivated,
            };

            graph.Edges.Add(edge);
        }

        // Use System.Text.Json instead of JsonConvert
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Serialize(graph, options);
    }

    private double CalculateNodeSize(int associationCount, int maxAssociations)
    {
        // Minimum size of 8, maximum size of 40
        const double minSize = 8.0;
        const double maxSize = 40.0;

        if (maxAssociations == 0) return minSize;

        // Normalize association count to 0-1 range
        double normalized = (double)associationCount / maxAssociations;

        // Apply square root to make size differences more visible for smaller counts
        normalized = Math.Sqrt(normalized);

        return minSize + (normalized * (maxSize - minSize));
    }

    private string GetColorByActivationLevel(double activation, double maxActivation)
    {
        // Normalize activation to 0-1 range
        double normalized = maxActivation > 0 ? Math.Min(1.0, activation / maxActivation) : 0;

        // Create a gradient from dark blue (low activation) to bright yellow (high activation)
        // Through purple and red in between

        if (normalized <= 0.25)
        {
            // Dark blue to purple
            double t = normalized / 0.25;
            int r = (int)(20 + t * (80 - 20));    // 20 -> 80
            int g = (int)(20 + t * (20 - 20));    // 20 -> 20  
            int b = (int)(60 + t * (120 - 60));   // 60 -> 120
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        else if (normalized <= 0.5)
        {
            // Purple to red
            double t = (normalized - 0.25) / 0.25;
            int r = (int)(80 + t * (160 - 80));   // 80 -> 160
            int g = (int)(20 + t * (20 - 20));    // 20 -> 20
            int b = (int)(120 + t * (40 - 120));  // 120 -> 40
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        else if (normalized <= 0.75)
        {
            // Red to orange
            double t = (normalized - 0.5) / 0.25;
            int r = (int)(160 + t * (255 - 160)); // 160 -> 255
            int g = (int)(20 + t * (140 - 20));   // 20 -> 140
            int b = (int)(40 + t * (0 - 40));     // 40 -> 0
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        else
        {
            // Orange to bright yellow
            double t = (normalized - 0.75) / 0.25;
            int r = (int)(255 + t * (255 - 255)); // 255 -> 255
            int g = (int)(140 + t * (255 - 140)); // 140 -> 255
            int b = (int)(0 + t * (100 - 0));     // 0 -> 100 (slight yellow tint)
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }

    private string FormatCognitiveTime(long cognitiveSteps)
    {
        // You can customize this based on how you want to display cognitive time
        // This is a simple example - you might want to use your CognitiveTimeManager
        if (cognitiveSteps == 0) return "Unknown";

        // Simple formatting - you can make this more sophisticated
        return $"Step {cognitiveSteps}";
    }

    private string GetColorByChunkType(string chunkType)
    {
        // Keep this method for backward compatibility or edge coloring
        return chunkType switch
        {
            "Memory" => "#FF6B6B",
            "Procedure" => "#4ECDC4",
            "Goal" => "#45B7D1",
            "Concept" => "#96CEB4",
            "Event" => "#FFEAA7",
            _ => "#DDA0DD"
        };
    }

    private string GetColorByRelationType(string relationType)
    {
        return relationType switch
        {
            "IsA" => "#FF6B6B",
            "HasA" => "#4ECDC4",
            "PartOf" => "#45B7D1",
            "RelatedTo" => "#96CEB4",
            "Causes" => "#FFEAA7",
            "Enables" => "#DDA0DD",
            _ => "#B0B0B0"
        };
    }
}

public class SigmaGraphActivationHistory
{
    [JsonPropertyName("previousValue")]
    public double PreviousValue { get; set; }

    [JsonPropertyName("newValue")]
    public double NewValue { get; set; }

    [JsonPropertyName("change")]
    public double Change { get; set; }

    [JsonPropertyName("sequenceNumber")]
    public ulong SequenceNumber { get; set; }

    [JsonPropertyName("activationDate")]
    public long ActivationDate { get; set; }

    [JsonPropertyName("emotionName")]
    public string EmotionName { get; set; }

    [JsonPropertyName("activatedByChunk")]
    public string ActivatedByChunk { get; set; }

    [JsonPropertyName("activatedBy")]
    public List<string> ActivatedBy { get; set; } = new List<string>();

    [JsonPropertyName("formattedDate")]
    public string FormattedDate { get; set; }
    [JsonPropertyName("activationReason")]
    public string ActivationReason { get; set; }
    [JsonPropertyName("activationSource")]
    public string ActivationSource { get; set; }
}

public class SigmaGraphNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("size")]
    public double Size { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    // Additional chunk properties
    [JsonPropertyName("chunkType")]
    public string ChunkType { get; set; }

    [JsonPropertyName("cognitiveCategory")]
    public string CognitiveCategory { get; set; }

    [JsonPropertyName("semanticType")]
    public string SemanticType { get; set; }

    [JsonPropertyName("activationLevel")]
    public double ActivationLevel { get; set; }

    [JsonPropertyName("activationHistory")]
    public List<SigmaGraphActivationHistory> ActivationHistory { get; set; } = new List<SigmaGraphActivationHistory>();

    [JsonPropertyName("slots")]
    public Dictionary<string, object> Slots { get; set; } = new Dictionary<string, object>();

    [JsonPropertyName("isWorkingMemory")]
    public bool IsWorkingMemory { get; set; } = true;

}

public class SigmaGraphEdge
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("size")]
    public double Size { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }

    // Additional association properties
    [JsonPropertyName("relationAtoB")]
    public string RelationAtoB { get; set; }

    [JsonPropertyName("relationBtoA")]
    public string RelationBtoA { get; set; }

    [JsonPropertyName("weightAtoB")]
    public double WeightAtoB { get; set; }

    [JsonPropertyName("weightBtoA")]
    public double WeightBtoA { get; set; }

    [JsonPropertyName("lastActivated")]
    public long LastActivated { get; set; }


}

public class SigmaGraph
{
    [JsonPropertyName("nodes")]
    public List<SigmaGraphNode> Nodes { get; set; } = new List<SigmaGraphNode>();

    [JsonPropertyName("edges")]
    public List<SigmaGraphEdge> Edges { get; set; } = new List<SigmaGraphEdge>();
}