using Aislinn.Core.Services;
using Aislinn.VectorStorage.Implementations;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Storage;

namespace RAINA.Services
{
    /// <summary>
    /// Container for higher-level RAINA application services
    /// </summary>
    public class RainaServices
    {
        public ConversationManager ConversationManager { get; }
        public EntityInstanceManager EntityManager { get; }
        public EntityRelationshipExtractionService EntityExtractionService { get; }
        public VectorStore VectorStore { get; }
        public IVectorCollection VectorCollection { get; }

        public RainaServices(
            ConversationManager conversationManager,
            EntityInstanceManager entityManager,
            EntityRelationshipExtractionService entityExtractionService,
            VectorStore vectorStore,
            IVectorCollection vectorCollection)
        {
            ConversationManager = conversationManager ?? throw new ArgumentNullException(nameof(conversationManager));
            EntityManager = entityManager ?? throw new ArgumentNullException(nameof(entityManager));
            EntityExtractionService = entityExtractionService ?? throw new ArgumentNullException(nameof(entityExtractionService));
            VectorStore = vectorStore;
            VectorCollection = vectorCollection;
        }
    }
}