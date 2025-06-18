using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aislinn.Core.Models;
using Aislinn.Core.Services;
using Aislinn.ChunkStorage.Interfaces;
using Aislinn.Configuration;
using Aislinn.Core;
using Aislinn.VectorStorage.Interfaces;
using Aislinn.VectorStorage.Models;

namespace RAINA.Services
{
    /// <summary>
    /// Manages entity instances, ontology lookups, and relationship creation
    /// </summary>
    public class EntityInstanceManager
    {
        private readonly IChunkStore _chunkStore;
        private readonly IAssociationStore _associationStore;
        private readonly string _chunkCollectionId;
        private readonly string _associationCollectionId;
        private readonly ChunkActivationService _activationService;
        private readonly CognitiveTimeManager _cognitiveTimeManager;
        public IVectorCollection _vectorCollection { get; }


        public EntityInstanceManager(
            AislinnCoreServices coreServices,
            AislinnConfiguration config,
            IVectorCollection vectorCollection)
        {
            _chunkStore = coreServices.ChunkStore;
            _associationStore = coreServices.AssociationStore;
            _activationService = coreServices.ActivationService;

            _chunkCollectionId = config.ChunkCollectionId;
            _associationCollectionId = config.AssociationCollectionId;
            _cognitiveTimeManager = coreServices.TimeManager;
            _vectorCollection = vectorCollection;

        }

        /// <summary>
        /// Find or create an entity instance and link it to ontology
        /// </summary>
        public async Task<Chunk> FindOrCreateEntityInstanceAsync(string entityName, string ontologyType)
        {
            if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(ontologyType))
                return null;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // First, try to find existing entity instance
            var existingEntity = await FindExistingEntityInstanceAsync(entityName, ontologyType);
            if (existingEntity != null)
                return existingEntity;

            // Find the ontology concept chunk
            var ontologyConcept = await FindOntologyConceptAsync(ontologyType);
            if (ontologyConcept == null)
            {
                // Create ontology concept if it doesn't exist
                ontologyConcept = await CreateOntologyConceptAsync(ontologyType);
            }

            // Create new entity instance
            var entityInstance = new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "Instance",
                SemanticType = ontologyType,
                Name = entityName,
                Slots = new Dictionary<string, ModelSlot>
                {
                    { "EntityName", new ModelSlot { Name = "EntityName", Value = entityName } },
                    { "OntologyType", new ModelSlot { Name = "OntologyType", Value = ontologyType } },
                    { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                }
            };
            var vectorText = $"{ontologyType} : {entityName}";
            var vectorMeta = new Dictionary<string, string>()
            {
                {"DataType", "EntityInstance"},
                {"Entity", entityName },
                {"OntologyType", ontologyType },
            };
            entityInstance.Vector = (await _vectorCollection.AddVectorAsync(entityInstance.ID.ToString(), vectorText, vectorMeta)).Vector;
            // Save the entity instance
            entityInstance = await chunkCollection.AddChunkAsync(entityInstance);

            // Create InstanceOf association with ontology concept
            if (ontologyConcept != null)
            {
                await CreateInstanceOfAssociationAsync(entityInstance.ID, ontologyConcept.ID);
            }
            if (entityInstance != null)
            {
                await _activationService.ActivateChunkAsync(entityInstance.ID, "entity_extraction", "EntityInstanceManager.FindOrCreateEntityInstanceAsync", null, 0.1);

                // Also activate the ontology concept
                if (ontologyConcept != null)
                {
                    await _activationService.ActivateChunkAsync(ontologyConcept.ID, "entity_extraction", "EntityInstanceManager.FindOrCreateEntityInstanceAsync.ontology", null, 0.1);
                }
            }
            return entityInstance;
        }

        /// <summary>
        /// Find existing entity instance by name and type
        /// </summary>
        public async Task<Chunk> FindExistingEntityInstanceAsync(string entityName, string ontologyType)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null) return null;

            var allChunks = await chunkCollection.GetAllChunksAsync();

            return allChunks.FirstOrDefault(c =>
                c.ChunkType == "Declarative" &&
                c.CognitiveCategory == "Instance" &&
                c.SemanticType == ontologyType &&
                string.Equals(c.Name, entityName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Find ontology concept chunk by semantic type
        /// </summary>
        public async Task<Chunk> FindOntologyConceptAsync(string ontologyType)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null) return null;

            var allChunks = await chunkCollection.GetAllChunksAsync();

            return allChunks.FirstOrDefault(c =>
                c.ChunkType == "Declarative" &&
                c.CognitiveCategory == "Ontology" &&
                c.SemanticType == ontologyType);
        }
        /// <summary>
        /// Batch create ontology concepts for multiple entity types
        /// </summary>
        private async Task<Dictionary<string, Chunk>> BatchCreateOntologyConceptsAsync(IEnumerable<string> entityTypes)
        {
            Console.WriteLine("[ENTITY] BatchCreateOntology");
            var uniqueTypes = entityTypes.Distinct().ToList();
            var results = new Dictionary<string, Chunk>();

            if (!uniqueTypes.Any())
                return results;

            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                return results;

            // Check which ones already exist
            var existingConcepts = new List<Chunk>();
            var newTypes = new List<string>();

            foreach (var type in uniqueTypes)
            {
                var existing = await FindOntologyConceptAsync(type);
                if (existing != null)
                {
                    results[type] = existing;
                }
                else
                {
                    newTypes.Add(type);
                }
            }

            if (!newTypes.Any())
                return results;

            // Batch vectorize new ontology concepts
            var vectorTexts = newTypes.Select(type => type).ToList();
            var vectorMetadata = newTypes.Select(type =>
            {
                var parts = type.Split('.');
                int level = parts.Length - 1;
                return new Dictionary<string, string>()
                {
                    {"DataType", "OntologyType"},
                    {"Level", level.ToString()},
                    {"OntologyType", type},
                    {"ParentID", Guid.Empty.ToString()},
                };
            }).ToList();
            Console.WriteLine("[ENTITY] Calling AddVectorssss");
            var vectorResults = await _vectorCollection.AddVectorsAsync(vectorTexts, vectorMetadata);

            // Create all new ontology concepts
            for (int i = 0; i < newTypes.Count; i++)
            {
                var type = newTypes[i];
                var vector = vectorResults[i].Vector;

                var parts = type.Split('.');
                int level = parts.Length - 1;
                string parentType = parts.Length > 1 ? string.Join(".", parts.Take(parts.Length - 1)) : null;

                var ontologyConcept = new Chunk
                {
                    ChunkType = "Declarative",
                    CognitiveCategory = "Ontology",
                    SemanticType = type,
                    Name = parts.Last(),
                    Vector = vector,
                    Slots = new Dictionary<string, ModelSlot>
                    {
                        { "Level", new ModelSlot { Name = "Level", Value = level } },
                        { "FullPath", new ModelSlot { Name = "FullPath", Value = type } },
                        { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } },
                        { "AutoGenerated", new ModelSlot { Name = "AutoGenerated", Value = true } }
                    }
                };

                // Add parent reference if applicable
                if (!string.IsNullOrEmpty(parentType) && results.ContainsKey(parentType))
                {
                    ontologyConcept.Slots["Parent"] = new ModelSlot { Name = "Parent", Value = results[parentType].ID };
                }

                var savedConcept = await chunkCollection.AddChunkAsync(ontologyConcept);
                results[type] = savedConcept;
            }

            return results;
        }
        /// <summary>
        /// Create a new ontology concept if it doesn't exist
        /// </summary>
        private async Task<Chunk> CreateOntologyConceptAsync(string ontologyType)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null) return null;

            // Parse ontology type to determine level and parent
            var parts = ontologyType.Split('.');
            int level = parts.Length - 1;
            string parentType = parts.Length > 1 ? string.Join(".", parts.Take(parts.Length - 1)) : null;

            var ontologyConcept = new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "Ontology",
                SemanticType = ontologyType,
                Name = parts.Last(),
                Slots = new Dictionary<string, ModelSlot>
                {
                    { "Level", new ModelSlot { Name = "Level", Value = level } },
                    { "FullPath", new ModelSlot { Name = "FullPath", Value = ontologyType } },
                    { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } },
                    { "AutoGenerated", new ModelSlot { Name = "AutoGenerated", Value = true } }
                }
            };
            var vectorText = $"{ontologyType}";
            var vectorMeta = new Dictionary<string, string>()
            {
                {"DataType", "OntologyType"},
                {"Level", level.ToString() },
                {"OntologyType", ontologyType },
                {"ParentID", Guid.Empty.ToString() },
            };
            // Add parent reference if applicable
            if (!string.IsNullOrEmpty(parentType))
            {
                var parentConcept = await FindOntologyConceptAsync(parentType);
                if (parentConcept != null)
                {
                    ontologyConcept.Slots["Parent"] = new ModelSlot { Name = "Parent", Value = parentConcept.ID };
                    vectorMeta["ParentID"] = parentConcept.ID.ToString();
                }
            }
            ontologyConcept.Vector = (await _vectorCollection.AddVectorAsync(ontologyConcept.ID.ToString(), vectorText, vectorMeta)).Vector;

            return await chunkCollection.AddChunkAsync(ontologyConcept);
        }

        /// <summary>
        /// Create InstanceOf association between entity and ontology concept
        /// </summary>
        private async Task<ChunkAssociation> CreateInstanceOfAssociationAsync(Guid entityInstanceId, Guid ontologyConceptId)
        {
            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null) return null;

            var association = new ChunkAssociation
            {
                ChunkAId = entityInstanceId,
                ChunkBId = ontologyConceptId,
                RelationAtoB = "InstanceOf",
                RelationBtoA = "HasInstance",
                WeightAtoB = 1.0,
                WeightBtoA = 0.8,
                LastActivated = _cognitiveTimeManager.GetCognitiveSteps()
            };

            return await associationCollection.AddAssociationAsync(association);
        }

        /// <summary>
        /// Attach entity instances to utterance slots
        /// </summary>
        public async Task AttachEntitiesToUtteranceAsync(Chunk utteranceChunk, List<Entity> entities)
        {
            if (utteranceChunk == null || entities == null || !entities.Any())
                return;

            var entityChunks = new List<Chunk>();

            foreach (var entity in entities)
            {
                var entityChunk = await this.FindOrCreateEntityInstanceAsync(entity.Name, entity.Type);
                if (entityChunk != null)
                {
                    entityChunks.Add(entityChunk);
                }
            }

            // Add entities to utterance slots
            if (entityChunks.Any())
            {
                utteranceChunk.Slots["ExtractedEntities"] = new ModelSlot
                {
                    Name = "ExtractedEntities",
                    Value = entityChunks.Select(e => e.ID).ToList()
                };

                // Update the utterance chunk
                var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                await chunkCollection.UpdateChunkAsync(utteranceChunk);
            }
        }

        /// <summary>
        /// Create associations based on extracted relationships
        /// </summary>
        public async Task CreateRelationshipAssociationsAsync(List<Relationship> relationships, List<Entity> entities)
        {
            if (relationships == null || !relationships.Any() || entities == null || !entities.Any())
                return;

            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
            if (associationCollection == null) return;

            // Create lookup for entity chunks by name
            var entityLookup = new Dictionary<string, Chunk>(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in entities)
            {
                var entityChunk = await FindExistingEntityInstanceAsync(entity.Name, entity.Type);
                if (entityChunk != null)
                {
                    entityLookup[entity.Name] = entityChunk;
                }
            }

            // Create associations for each relationship
            foreach (var relationship in relationships)
            {
                if (entityLookup.TryGetValue(relationship.Entity1, out var entity1Chunk) &&
                    entityLookup.TryGetValue(relationship.Entity2, out var entity2Chunk))
                {
                    var reversal = GetReverseRelationType(relationship.Type);
                    // Check if association already exists
                    var existingAssociation = await associationCollection.GetAssociationAsync(entity1Chunk.ID, entity2Chunk.ID, relationship.Type, reversal);
                    if (existingAssociation == null)
                    {
                        var association = new ChunkAssociation
                        {
                            ChunkAId = entity1Chunk.ID,
                            ChunkBId = entity2Chunk.ID,
                            RelationAtoB = relationship.Type,
                            RelationBtoA = reversal,
                            WeightAtoB = 0.7,
                            WeightBtoA = 0.7,
                            LastActivated = _cognitiveTimeManager.GetCognitiveSteps()
                        };

                        await associationCollection.AddAssociationAsync(association);
                    }
                }
            }
        }

        /// <summary>
        // /// Handle special processing for person entities (speaker/listener slots)
        // /// </summary>
        // public async Task ProcessPersonEntitiesAsync(Chunk utteranceChunk, List<Entity> entities)
        // {
        //     var personEntities = entities.Where(e => e.Type.Contains("person")).ToList();
        //     if (!personEntities.Any()) return;

        //     foreach (var personEntity in personEntities)
        //     {
        //         var personChunk = await FindOrCreateEntityInstanceAsync(personEntity.Name, personEntity.Type);
        //         if (personChunk == null) continue;

        //         // Check if this person matches the speaker
        //         if (utteranceChunk.Slots.TryGetValue("SpeakerName", out var speakerSlot) &&
        //             string.Equals(speakerSlot.Value?.ToString(), personEntity.Name, StringComparison.OrdinalIgnoreCase))
        //         {
        //             utteranceChunk.Slots["Speaker"] = new ModelSlot { Name = "Speaker", Value = personChunk };
        //         }

        //         // Check if this person matches the listener
        //         if (utteranceChunk.Slots.TryGetValue("ListenerName", out var listenerSlot) &&
        //             string.Equals(listenerSlot.Value?.ToString(), personEntity.Name, StringComparison.OrdinalIgnoreCase))
        //         {
        //             utteranceChunk.Slots["Listener"] = new ModelSlot { Name = "Listener", Value = personChunk };
        //         }
        //     }

        //     // Update the utterance chunk
        //     var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
        //     await chunkCollection.UpdateChunkAsync(utteranceChunk);
        // }

        /// <summary>
        /// Process all entities for an utterance - creates entity instances, attaches to utterance, and handles special person processing
        /// </summary>
        public async Task ProcessEntitiesForUtteranceAsync(Chunk utteranceChunk, List<Entity> entities)
        {
            if (utteranceChunk == null || entities == null || !entities.Any())
                return;
            Console.WriteLine("[PROCESS ENTITY] ProcessEntitiesForUtteranceAsync");

            // First, batch check for existing entities (check both name and formal if present)
            var entityChunks = new List<Chunk>();
            var newEntityData = new List<(Entity entity, string name, bool isFormal)>();

            foreach (var entity in entities)
            {
                // Check original name
                var existingOriginal = await FindExistingEntityInstanceAsync(entity.Name, entity.Type);
                if (existingOriginal != null)
                {
                    entityChunks.Add(existingOriginal);
                    if (entity.Type.Contains("person"))
                    {
                        await ProcessPersonEntityForUtterance(utteranceChunk, entity, existingOriginal);
                    }
                }
                else
                {
                    newEntityData.Add((entity, entity.Name, false));
                }

                // If formal name exists and is different, check that too
                if (!string.IsNullOrEmpty(entity.Formal) && !string.Equals(entity.Name, entity.Formal, StringComparison.OrdinalIgnoreCase))
                {
                    var existingFormal = await FindExistingEntityInstanceAsync(entity.Formal, entity.Type);
                    if (existingFormal != null)
                    {
                        if (!entityChunks.Any(c => c.ID == existingFormal.ID)) // Avoid duplicates
                        {
                            entityChunks.Add(existingFormal);
                        }
                    }
                    else
                    {
                        newEntityData.Add((entity, entity.Formal, true));
                    }
                }
            }

            // Only vectorize and create entities that don't exist
            if (newEntityData.Any())
            {
                var entityTypes = newEntityData.Select(ned => ned.entity.Type).Distinct().ToList();
                var ontologyConcepts = await BatchCreateOntologyConceptsAsync(entityTypes);                // Group by entity to batch vectorize (use formal name for vectorization when available)
                var vectorTexts = new List<string>();
                var vectorMetadata = new List<Dictionary<string, string>>();

                foreach (var (entity, name, isFormal) in newEntityData)
                {
                    // Always use formal name for vectorization if available, otherwise use original name
                    string vectorName = !string.IsNullOrEmpty(entity.Formal) ? entity.Formal : entity.Name;
                    string vectorText = $"{entity.Type} : {vectorName}";

                    vectorTexts.Add(vectorText);
                    vectorMetadata.Add(new Dictionary<string, string>()
                    {
                        {"DataType", "EntityInstance"},
                        {"Entity", name },
                        {"OntologyType", entity.Type },
                        {"IsFormal", isFormal.ToString()},
                        {"CanonicalName", vectorName}
                    });
                }
                Console.WriteLine("[PROCESS ENTITY] Calling AddVectorsssss");
                // Batch vectorize all new entities using formal names
                var vectorResults = await _vectorCollection.AddVectorsAsync(vectorTexts, vectorMetadata);

                // Create new entity chunks and track relationships
                var createdChunks = new Dictionary<string, List<Chunk>>(); // entity.Name -> chunks created for this entity

                for (int i = 0; i < newEntityData.Count; i++)
                {
                    var (entity, name, isFormal) = newEntityData[i];
                    var vector = vectorResults[i].Vector;
                    var ontologyConcept = ontologyConcepts.GetValueOrDefault(entity.Type);
                    var entityChunk = await CreateEntityInstanceWithVector(entity, vector, ontologyConcept);

                    if (entityChunk != null)
                    {
                        entityChunks.Add(entityChunk);

                        // Track chunks by original entity name for relationship creation
                        if (!createdChunks.ContainsKey(entity.Name))
                            createdChunks[entity.Name] = new List<Chunk>();
                        createdChunks[entity.Name].Add(entityChunk);

                        // Handle person processing
                        if (entity.Type.Contains("person"))
                        {
                            await ProcessPersonEntityForUtterance(utteranceChunk, entity, entityChunk);
                        }

                        // Activate the new entity
                        await _activationService.ActivateChunkAsync(entityChunk.ID, "entity_extraction", "EntityInstanceManager.ProcessEntitiesForUtteranceAsync.new", null, 0.1);
                    }
                }

                // Create canonical/variant relationships between original and formal names
                foreach (var entity in entities.Where(e => !string.IsNullOrEmpty(e.Formal) && !string.Equals(e.Name, e.Formal, StringComparison.OrdinalIgnoreCase)))
                {
                    if (createdChunks.ContainsKey(entity.Name) && createdChunks[entity.Name].Count == 2)
                    {
                        var chunks = createdChunks[entity.Name];
                        var originalChunk = chunks.FirstOrDefault(c => c.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase));
                        var formalChunk = chunks.FirstOrDefault(c => c.Name.Equals(entity.Formal, StringComparison.OrdinalIgnoreCase));

                        if (originalChunk != null && formalChunk != null)
                        {
                            // Create canonical/variant relationships
                            var associationCollection = await _associationStore.GetCollectionAsync(_associationCollectionId);
                            var association = new ChunkAssociation
                            {
                                ChunkAId = originalChunk.ID,
                                ChunkBId = formalChunk.ID,
                                RelationAtoB = "CanonicalForm",
                                RelationBtoA = "Variant",
                                WeightAtoB = 1.0,
                                WeightBtoA = 1.0,
                                LastActivated = _cognitiveTimeManager.GetCognitiveSteps()
                            };
                            await associationCollection.AddAssociationAsync(association);
                        }
                    }
                }
            }

            // Activate existing entities too
            var existingCount = entityChunks.Count - newEntityData.Count;
            for (int i = 0; i < existingCount; i++)
            {
                await _activationService.ActivateChunkAsync(entityChunks[i].ID, "entity_extraction", "EntityInstanceManager.ProcessEntitiesForUtteranceAsync.existing", null, 0.05);
            }

            // Add all entities to utterance slots
            if (entityChunks.Any())
            {
                utteranceChunk.Slots["ExtractedEntities"] = new ModelSlot
                {
                    Name = "ExtractedEntities",
                    Value = entityChunks.Select(e => e.ID).ToList()
                };

                var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
                await chunkCollection.UpdateChunkAsync(utteranceChunk);
            }


        }

        /// <summary>
        /// Create entity instance with specific name and pre-generated vector
        /// </summary>
        private async Task<Chunk> CreateEntityInstanceWithName(string entityType, string entityName, double[] vector)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // Find or create ontology concept
            var ontologyConcept = await FindOntologyConceptAsync(entityType);
            if (ontologyConcept == null)
            {
                ontologyConcept = await CreateOntologyConceptAsync(entityType);
            }

            // Create new entity instance with provided vector
            var entityInstance = new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "Instance",
                SemanticType = entityType,
                Name = entityName,
                Vector = vector,
                Slots = new Dictionary<string, ModelSlot>
                {
                    { "EntityName", new ModelSlot { Name = "EntityName", Value = entityName } },
                    { "OntologyType", new ModelSlot { Name = "OntologyType", Value = entityType } },
                    { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                }
            };

            // Save the entity instance
            entityInstance = await chunkCollection.AddChunkAsync(entityInstance);

            // Create InstanceOf association with ontology concept
            if (ontologyConcept != null)
            {
                await CreateInstanceOfAssociationAsync(entityInstance.ID, ontologyConcept.ID);
            }

            return entityInstance;
        }
        /// <summary>
        /// Create entity instance with pre-generated vector
        /// </summary>
        private async Task<Chunk> CreateEntityInstanceWithVector(Entity entity, double[] vector, Chunk ontologyConcept = null)
        {
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            if (ontologyConcept == null)
            {
                // Find or create ontology concept
                ontologyConcept = await FindOntologyConceptAsync(entity.Type);
                if (ontologyConcept == null)
                {
                    Console.WriteLine("[PROCESS ENTITY] Calling CreateOntologyConcept");

                    ontologyConcept = await CreateOntologyConceptAsync(entity.Type);
                }
            }

            // Create new entity instance with provided vector
            var entityInstance = new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "Instance",
                SemanticType = entity.Type,
                Name = entity.Name,
                Vector = vector,
                Slots = new Dictionary<string, ModelSlot>
                {
                    { "EntityName", new ModelSlot { Name = "EntityName", Value = entity.Name } },
                    { "OntologyType", new ModelSlot { Name = "OntologyType", Value = entity.Type } },
                    { "CreatedTimestamp", new ModelSlot { Name = "CreatedTimestamp", Value = DateTime.Now } }
                }
            };

            // Save the entity instance
            entityInstance = await chunkCollection.AddChunkAsync(entityInstance);

            // Create InstanceOf association with ontology concept
            if (ontologyConcept != null)
            {
                await CreateInstanceOfAssociationAsync(entityInstance.ID, ontologyConcept.ID);
            }

            return entityInstance;
        }

        /// <summary>
        /// Handle person entity speaker/listener matching
        /// </summary>
        private async Task ProcessPersonEntityForUtterance(Chunk utteranceChunk, Entity personEntity, Chunk personChunk)
        {
            // Check if this person matches the speaker
            if (utteranceChunk.Slots.TryGetValue("SpeakerName", out var speakerSlot) &&
                string.Equals(speakerSlot.Value?.ToString(), personEntity.Name, StringComparison.OrdinalIgnoreCase))
            {
                utteranceChunk.Slots["Speaker"] = new ModelSlot { Name = "Speaker", Value = personChunk };
            }

            // Check if this person matches the listener
            if (utteranceChunk.Slots.TryGetValue("ListenerName", out var listenerSlot) &&
                string.Equals(listenerSlot.Value?.ToString(), personEntity.Name, StringComparison.OrdinalIgnoreCase))
            {
                utteranceChunk.Slots["Listener"] = new ModelSlot { Name = "Listener", Value = personChunk };
            }
        }
        // /// <summary>
        /// Get reverse relation type for bidirectional associations
        /// </summary>
        private string GetReverseRelationType(string relationType)
        {
            // Common relationship reversals
            var reversals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "AboutTopic", "TopicOf" },
                { "AccessedBy", "Accesses" },
                { "AccessibleBy", "CanAccess" },
                { "AchievedThrough", "Achieves" },
                { "AcquiredBy", "Acquires" },
                { "Adjacent", "Adjacent" },
                { "AffectsEntity", "AffectedBy" },
                { "AffectsEnvironment", "EnvironmentAffectedBy" },
                { "After", "Before" },
                { "AimedAt", "TargetOf" },
                { "AppliedIn", "ApplicationOf" },
                { "AppliedTo", "ApplicationOf" },
                { "AppliesTo", "ApplicationOf" },
                { "Association", "Association" },
                { "AuthoredBy", "Authors" },
                { "BasedOn", "BasisFor" },
                { "Before", "After" },
                { "BehindAction", "ActionOf" },
                { "Between", "Between" },
                { "BordersWith", "BordersWith" },
                { "BornAt", "BirthplaceOf" },
                { "BornIn", "BirthplaceOf" },
                { "CarriesPassengers", "CarriedBy" },
                { "CausedBy", "Causes" },
                { "Causes", "CausedBy" },
                { "ChangesOver", "ChangedBy" },
                { "ChangesUnder", "CausesChangeIn" },
                { "CharacterizedBy", "Characterizes" },
                { "CommunicatedBy", "Communicates" },
                { "ComposedOf", "ComponentOf" },
                { "ConflictsWith", "ConflictsWith" },
                { "ConnectedBy", "Connects" },
                { "ConnectedTo", "ConnectedTo" },
                { "Connects", "ConnectedBy" },
                { "ConsumedBy", "Consumes" },
                { "ContainedIn", "Contains" },
                { "Contains", "ContainedIn" },
                { "ContainsBuildings", "BuildingIn" },
                { "ContainsComponents", "ComponentOf" },
                { "ContainsEquipment", "EquipmentIn" },
                { "ContainsNutrients", "NutrientIn" },
                { "ControlledBy", "Controls" },
                { "ConvertedTo", "ConvertedFrom" },
                { "CreatedBy", "Creates" },
                { "DefinedBy", "Defines" },
                { "DependsOn", "RequiredBy" },
                { "DesignedFor", "DesignOf" },
                { "DevelopedBy", "Develops" },
                { "DevelopedThrough", "DevelopmentMethodFor" },
                { "Dies", "DeathOf" },
                { "DirectionTo", "DirectionFrom" },
                { "DistanceFrom", "DistanceTo" },
                { "DrivenBy", "Drives" },
                { "During", "TimeOf" },
                { "DuringTime", "EventDuring" },
                { "EatsFood", "EatenBy" },
                { "EmbodiedBy", "Embodies" },
                { "Enables", "EnabledBy" },
                { "EvaluatedAs", "EvaluationOf" },
                { "Evolved", "EvolutionOf" },
                { "ExcludesFrom", "ExcludedFrom" },
                { "ExperiencedBy", "Experiences" },
                { "ExplainedBy", "Explains" },
                { "ExpressedAs", "ExpressionOf" },
                { "FollowedBy", "Follows" },
                { "FoundIn", "LocationOf" },
                { "GovernedBy", "Governs" },
                { "GrowsIn", "GrowthLocationOf" },
                { "GuidesAction", "GuidedBy" },
                { "HasBody", "BodyOf" },
                { "HasBrand", "BrandOf" },
                { "HasCapital", "CapitalOf" },
                { "HasCurrency", "CurrencyOf" },
                { "HasDNA", "DNAOf" },
                { "HasDeadline", "DeadlineFor" },
                { "HasDirection", "DirectionOf" },
                { "HasEmployees", "EmployeeOf" },
                { "HasIdentity", "IdentityOf" },
                { "HasLanguage", "LanguageOf" },
                { "HasLocation", "LocationOf" },
                { "HasMayor", "MayorOf" },
                { "HasName", "NameOf" },
                { "HasOffspring", "OffspringOf" },
                { "HasPersonality", "PersonalityOf" },
                { "HasPopulation", "PopulationOf" },
                { "HasPostalCode", "PostalCodeOf" },
                { "HasProperty", "PropertyOf" },
                { "HasPurpose", "PurposeOf" },
                { "HasResponsibilities", "ResponsibilityOf" },
                { "HasRole", "RoleOf" },
                { "HasSize", "SizeOf" },
                { "HasStages", "StageOf" },
                { "HasState", "StateOf" },
                { "HasStrength", "StrengthOf" },
                { "HasSubtype", "SubtypeOf" },
                { "HasWeight", "WeightOf" },
                { "HeldBy", "Holds" },
                { "Impedes", "ImpededBy" },
                { "InCity", "CityOf" },
                { "InContext", "ContextOf" },
                { "InfluencedBy", "Influences" },
                { "InspiredBy", "Inspires" },
                { "InstanceOf", "HasInstance" },
                { "IntegratesWith", "IntegratesWith" },
                { "Involves", "InvolvedIn" },
                { "InvolvesMaterials", "MaterialFor" },
                { "IsA", "HasInstance" },
                { "IsTransitive", "TransitiveProperty" },
                { "JudgedBy", "Judges" },
                { "KnownFor", "FamousAspectOf" },
                { "KnowsPerson", "KnownBy" },
                { "LeadBy", "Leads" },
                { "LeadsTo", "ResultOf" },
                { "LivesIn", "ResidentOf" },
                { "LocatedIn", "LocationOf" },
                { "LocatesBuilding", "BuildingLocatedBy" },
                { "LongerThan", "ShorterThan" },
                { "MadeOf", "MaterialFor" },
                { "ManagedBy", "Manages" },
                { "ManufacturedBy", "Manufactures" },
                { "MeasuredBy", "Measures" },
                { "MeasuredIn", "UnitFor" },
                { "MeasuredWith", "MeasurementToolFor" },
                { "Measures", "MeasuredBy" },
                { "Mediated", "Mediator" },
                { "NearTo", "NearTo" },
                { "ObservedBy", "Observes" },
                { "ObservedIn", "ObservationLocationOf" },
                { "OccursAt", "LocationOf" },
                { "OccursOn", "EventDate" },
                { "OnStreet", "StreetOf" },
                { "OpenedWith", "OpeningToolFor" },
                { "OperatedBy", "Operates" },
                { "OppositeOf", "OppositeOf" },
                { "PartOf", "HasPart" },
                { "PartnersWith", "PartnersWith" },
                { "PerformedBy", "Performs" },
                { "PossessedBy", "Possesses" },
                { "PoweredBy", "Powers" },
                { "PreparedBy", "Prepares" },
                { "ProcessedBy", "Processes" },
                { "ProducesResult", "ResultOf" },
                { "ProposedBy", "Proposes" },
                { "PursuitBy", "Pursues" },
                { "RecognizedBy", "Recognizes" },
                { "ReferencesOther", "ReferencedBy" },
                { "RegulatedBy", "Regulates" },
                { "RelatedTo", "RelatedTo" },
                { "RequiredFor", "Requires" },
                { "RequiresActions", "ActionRequiredBy" },
                { "RequiresAttention", "AttentionRequiredBy" },
                { "RequiresSkill", "SkillRequiredBy" },
                { "RequiresSkills", "SkillsRequiredBy" },
                { "RequiresTime", "TimeRequiredBy" },
                { "Results", "CausedBy" },
                { "RunsOn", "PlatformFor" },
                { "ScheduledAt", "ScheduleFor" },
                { "ScheduledFor", "ScheduledAt" },
                { "Sequences", "SequencedBy" },
                { "SharedVia", "SharingMediumFor" },
                { "SharedWith", "SharesFrom" },
                { "ShorterThan", "LongerThan" },
                { "SignedBy", "Signs" },
                { "SimilarTo", "SimilarTo" },
                { "SoldBy", "Sells" },
                { "SolvedBy", "Solves" },
                { "SpecifiesLocation", "LocationSpecifiedBy" },
                { "StoredIn", "StorageFor" },
                { "StoredOn", "StorageMediumFor" },
                { "SubcategoryOf", "HasSubcategory" },
                { "SupportedBy", "Supports" },
                { "Temporary", "TemporaryProperty" },
                { "TestedBy", "Tests" },
                { "TransformsTo", "TransformsFrom" },
                { "TransitionsTo", "TransitionsFrom" },
                { "TransmittedBy", "Transmits" },
                { "TravelsOn", "RouteFor" },
                { "TriggeredBy", "Triggers" },
                { "UnderstoodBy", "Understands" },
                { "Unpredictable", "UnpredictableProperty" },
                { "UpdatedBy", "Updates" },
                { "UsedBy", "Uses" },
                { "UsedFor", "UseOf" },
                { "UsesTools", "ToolUsedBy" },
                { "VerifiedBy", "Verifies" },
                { "ViolatedBy", "Violates" }
            };


            return reversals.TryGetValue(relationType, out var reverse) ? reverse : "Association";
        }
    }
}