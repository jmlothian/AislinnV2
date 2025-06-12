using Aislinn;
public class VoyageConfiguration
{
    public string VoyageApiKey { get; set; }
    public string VoyageModel { get; set; } = "voyage-3-large";
    public string VoyageBaseUrl { get; set; } = "https://api.voyageai.com/v1/embeddings";
    public int VoyageTimeoutSeconds { get; set; } = 30;
    public int VoyageMaxRetries { get; set; } = 3;
    public string VoyageInputType { get; set; } = null; // null, "query", or "document"
}