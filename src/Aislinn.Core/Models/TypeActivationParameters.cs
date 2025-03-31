// New file: TypeActivationParameters.cs

namespace Aislinn.Models.Activation
{
    /// <summary>
    /// Parameters for type-specific activation dynamics
    /// </summary>
    public class TypeActivationParameters
    {
        /// <summary>
        /// The chunk type these parameters apply to
        /// </summary>
        public string ChunkType { get; set; }

        /// <summary>
        /// How quickly activation decays over time (0-1)
        /// </summary>
        public double DecayRate { get; set; } = 0.5;

        /// <summary>
        /// Initial activation level for new chunks of this type
        /// </summary>
        public double InitialActivation { get; set; } = 0.1;

        /// <summary>
        /// Factor for spreading activation to other chunks (0-1)
        /// </summary>
        public double SpreadingFactor { get; set; } = 0.5;

        /// <summary>
        /// Maximum activation level this type can reach
        /// </summary>
        public double ActivationCeiling { get; set; } = 1.0;

        /// <summary>
        /// Base level of activation added during explicit activation
        /// </summary>
        public double BaseActivationBoost { get; set; } = 1.0;

        /// <summary>
        /// How much activation noise to apply (0-1)
        /// </summary>
        public double ActivationNoise { get; set; } = 0.1;

        /// <summary>
        /// Priority for entering working memory (0-1)
        /// </summary>
        public double WorkingMemoryPriority { get; set; } = 0.5;

        public double AssociationStrengthIncrement { get; set; } = 0.1;

        /// <summary>
        /// Creates default activation parameters
        /// </summary>
        public TypeActivationParameters() { }



        /// <summary>
        /// Creates activation parameters for a specific chunk type
        /// </summary>
        public TypeActivationParameters(string chunkType)
        {
            ChunkType = chunkType;
        }
    }
}