using System;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Core.Cognitive;
using Aislinn.Core.Context;
using Aislinn.Core.Query;
using Aislinn.Core.Services;
using Aislinn.Configuration;

namespace Aislinn.Core
{
    /// <summary>
    /// Container for core Aislinn cognitive architecture services
    /// </summary>
    public class AislinnCoreServices
    {
        public IChunkStore ChunkStore { get; }
        public IAssociationStore AssociationStore { get; }
        public CognitiveMemorySystem MemorySystem { get; }
        public ChunkActivationService ActivationService { get; }
        public CognitiveTimeManager TimeManager { get; }
        public ChunkQueryService QueryService { get; }
        public ContextContainer ContextContainer { get; }

        // Collection IDs for convenience
        public string ChunkCollectionId { get; }
        public string AssociationCollectionId { get; }

        public AislinnCoreServices(
            IChunkStore chunkStore,
            IAssociationStore associationStore,
            CognitiveMemorySystem memorySystem,
            ChunkActivationService activationService,
            CognitiveTimeManager timeManager,
            ChunkQueryService queryService,
            ContextContainer contextContainer,
            AislinnConfiguration config)
        {
            ChunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            AssociationStore = associationStore ?? throw new ArgumentNullException(nameof(associationStore));
            MemorySystem = memorySystem ?? throw new ArgumentNullException(nameof(memorySystem));
            ActivationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
            TimeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
            QueryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            ContextContainer = contextContainer ?? throw new ArgumentNullException(nameof(contextContainer));

            ChunkCollectionId = config.ChunkCollectionId;
            AssociationCollectionId = config.AssociationCollectionId;
        }
    }
}

