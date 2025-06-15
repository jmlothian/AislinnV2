
using Aislinn.Configuration;

public class RainaConfiguration : AislinnConfiguration
{
    // File Paths
    public string RelationshipCachePath { get; set; } = "relationship_cache.json";
    public string BasicKnowledgeChunksPath { get; set; } = "./basic_knowledge/chunks";
    public string BasicKnowledgeAssociationsPath { get; set; } = "./basic_knowledge/associations";

    // Agent Configuration
    public string AgentName { get; set; } = "Raina";

    public string RainaVectorCollection { get; set; } = "Raina";
    // Vector Search Configuration
    public double VectorSearchUtteranceBoost { get; set; } = 0.4;
    public double VectorSearchEntityBoost { get; set; } = 0.3;
    public double VectorSearchContextBoost { get; set; } = 0.3;
    public double VectorSearchRecentHistoryBoost { get; set; } = 0.2;
    public double VectorSearchBackgroundBoost { get; set; } = 0.3;
    public int VectorSearchMaxResults { get; set; } = 5;
    public double VectorSearchMinSimilarity { get; set; } = 0.6;
    public int VectorSearchTopActivatedChunks { get; set; } = 10;
    public int VectorSearchChunksToPush { get; set; } = 5;
    public double[] VectorSearchDiminishingReturns { get; set; } = new[] { 1.0, 0.1, 0.05, 0.02 };
}