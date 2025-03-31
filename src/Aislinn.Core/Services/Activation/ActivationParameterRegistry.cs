// New file: ActivationParametersRegistry.cs

using System;
using System.Collections.Generic;
using Aislinn.Core.Models;
using Aislinn.Models.Activation;

namespace Aislinn.Core.Activation
{
    /// <summary>
    /// Registry for managing type-specific activation parameters
    /// </summary>
    public class ActivationParametersRegistry
    {
        // Default parameters used when type-specific ones aren't available
        private readonly TypeActivationParameters _defaultParameters = new TypeActivationParameters
        {
            DecayRate = 0.5,
            InitialActivation = 0.1,
            SpreadingFactor = 0.5,
            ActivationCeiling = 1.0,
            BaseActivationBoost = 1.0,
            ActivationNoise = 0.1,
            WorkingMemoryPriority = 0.5,
            AssociationStrengthIncrement = 0.1
        };

        // Map of chunk types to specific parameters
        private readonly Dictionary<string, TypeActivationParameters> _typeParameters =
            new Dictionary<string, TypeActivationParameters>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Creates a new activation parameters registry with default settings
        /// </summary>
        public ActivationParametersRegistry()
        {
            // Initialize with default parameters for common types
            RegisterTypeParameters("Goal", new TypeActivationParameters
            {
                DecayRate = 0.3,
                InitialActivation = 0.3,
                SpreadingFactor = 0.7,
                ActivationCeiling = 1.0,
                BaseActivationBoost = 1.2,
                WorkingMemoryPriority = 0.8,
                AssociationStrengthIncrement = 0.1

            });

            RegisterTypeParameters("Procedure", new TypeActivationParameters
            {
                DecayRate = 0.4,
                InitialActivation = 0.2,
                SpreadingFactor = 0.6,
                ActivationCeiling = 1.0,
                BaseActivationBoost = 1.0,
                WorkingMemoryPriority = 0.7,
                AssociationStrengthIncrement = 0.1

            });

            // Add more default type parameters as needed
        }

        /// <summary>
        /// Registers parameters for a specific chunk type
        /// </summary>
        public void RegisterTypeParameters(string chunkType, TypeActivationParameters parameters)
        {
            if (string.IsNullOrEmpty(chunkType))
                throw new ArgumentException("Chunk type cannot be null or empty", nameof(chunkType));

            parameters.ChunkType = chunkType;
            _typeParameters[chunkType] = parameters;
        }

        /// <summary>
        /// Gets activation parameters for a specific chunk
        /// </summary>
        public TypeActivationParameters GetParameters(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            // Try to get type-specific parameters
            if (!string.IsNullOrEmpty(chunk.ChunkType) &&
                _typeParameters.TryGetValue(chunk.ChunkType, out var parameters))
            {
                return parameters;
            }

            // Fall back to default parameters
            return _defaultParameters;
        }

        /// <summary>
        /// Gets activation parameters for a specific chunk type
        /// </summary>
        public TypeActivationParameters GetParametersForType(string chunkType)
        {
            if (string.IsNullOrEmpty(chunkType))
                throw new ArgumentException("Chunk type cannot be null or empty", nameof(chunkType));

            // Try to get type-specific parameters
            if (_typeParameters.TryGetValue(chunkType, out var parameters))
            {
                return parameters;
            }

            // Fall back to default parameters
            return _defaultParameters;
        }

        /// <summary>
        /// Gets the default parameters used when no type-specific ones exist
        /// </summary>
        public TypeActivationParameters GetDefaultParameters()
        {
            return _defaultParameters;
        }

        /// <summary>
        /// Updates the default parameters
        /// </summary>
        public void SetDefaultParameters(TypeActivationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            // Copy properties to avoid reference issues
            _defaultParameters.DecayRate = parameters.DecayRate;
            _defaultParameters.InitialActivation = parameters.InitialActivation;
            _defaultParameters.SpreadingFactor = parameters.SpreadingFactor;
            _defaultParameters.ActivationCeiling = parameters.ActivationCeiling;
            _defaultParameters.BaseActivationBoost = parameters.BaseActivationBoost;
            _defaultParameters.ActivationNoise = parameters.ActivationNoise;
            _defaultParameters.WorkingMemoryPriority = parameters.WorkingMemoryPriority;
            _defaultParameters.AssociationStrengthIncrement = parameters.AssociationStrengthIncrement;
        }
    }
}