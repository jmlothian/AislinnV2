namespace Aislinn.Core.Models
{
    public class ChunkAssociation
    {
        public Guid ChunkAId { get; set; }
        public Guid ChunkBId { get; set; }
        public string RelationAtoB { get; set; } // "IsA", "CreatedBy", etc.
        public string RelationBtoA { get; set; } // "HasInstance", "Created", etc.
        public double WeightAtoB { get; set; } = 0.0;
        public double WeightBtoA { get; set; } = 0.0;
        public DateTime LastActivated { get; set; } = DateTime.Now;
        public List<ActivationHistoryItem> ActivationHistory { get; set; } = new List<ActivationHistoryItem>();

        public void Strengthen(bool directionAtoB, double amount = 0.1)
        {
            if (directionAtoB)
                WeightAtoB += amount;
            else
                WeightBtoA += amount;

            LastActivated = DateTime.Now;
        }
    }
    public enum RelationshipType
    {
        IsA,
        HasA,
        PartOf,
        RelatedTo,
        Causes,
        Enables,
        Impedes,
        Association,
        SimilarTo,
        OppositeOf,
        InstanceOf,
        Temporal, //feel like spreading activations should do this automagically based on activation dates
                  // Add more relationship types as needed
    }
}