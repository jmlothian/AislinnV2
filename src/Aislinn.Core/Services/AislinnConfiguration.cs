namespace Aislinn.Configuration
{
    /// <summary>
    /// Configuration settings for RAINA system
    /// </summary>
    public class AislinnConfiguration
    {
        // Storage Configuration
        public string ChunkDatabasePath { get; set; } = "chunks.db";
        public string AssociationDatabasePath { get; set; } = "associations.db";
        public string ChunkCollectionId { get; set; } = "default";
        public string AssociationCollectionId { get; set; } = "default";

        // Memory Configuration
        public int WorkingMemoryCapacity { get; set; } = 20;
        public double ActivationThreshold { get; set; } = 0.10;
        public double AssociativeThreshold { get; set; } = -0.3;

        // API Configuration
        public string OpenAIApiKey { get; set; } = "";





        // Validation
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(OpenAIApiKey))
                throw new InvalidOperationException("OpenAI API key is required");

            if (WorkingMemoryCapacity <= 0)
                throw new ArgumentException("Working memory capacity must be positive");

        }
    }
}