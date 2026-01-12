using Aislinn.Core.Models;
using RAINA.Modules.Implementations;
using Microsoft.Extensions.Logging;
using Aislinn.Core.Cognitive;
using System.Threading.Tasks;
namespace RAINA.Modules.Implementations
{
    public class MemoryChunkConverter
    {
        private readonly ILogger<MemoryChunkConverter> _logger;
        private readonly CognitiveMemorySystem _memorySystem;
        // Mapping from extraction categories to ontology paths
        private static readonly Dictionary<string, string> CategoryToSemanticType = new()
        {
            ["people"] = "essence.entity.abstract.person",
            ["places"] = "essence.space.location",
            ["activities"] = "essence.entity.event.action",
            ["objects"] = "essence.entity.physical.artificial",
            ["events"] = "essence.entity.event.occurrence",
            ["relationships"] = "essence.relation.social",
            ["likes"] = "essence.entity.abstract.concept", // preference concepts
            ["dislikes"] = "essence.entity.abstract.concept",
            ["loves"] = "essence.entity.abstract.concept",
            ["hates"] = "essence.entity.abstract.concept",
            ["constraints"] = "essence.state",
            ["facts"] = "essence.entity.abstract.information"
        };

        public MemoryChunkConverter(
            ILogger<MemoryChunkConverter> logger,
            CognitiveMemorySystem memorySystem)
        {
            _logger = logger;
            _memorySystem = memorySystem;
        }

        public async Task<(List<Chunk>, List<ChunkAssociation>)> ConvertMemoryExtractionToChunks(MemoryExtractionResponse extraction)
        {
            _logger.LogInformation("Starting memory extraction conversion");

            // Log what was extracted
            LogExtractionSummary(extraction);

            var chunks = new List<Chunk>();
            var associations = new List<ChunkAssociation>();
            var entityChunks = new Dictionary<string, Chunk>(); // Track entity chunks to avoid duplicates

            // Process people and their facts
            await ProcessEntityCategory(extraction.People, "people", chunks, associations, entityChunks);
            await ProcessEntityCategory(extraction.Places, "places", chunks, associations, entityChunks);
            await ProcessEntityCategory(extraction.Activities, "activities", chunks, associations, entityChunks);
            await ProcessEntityCategory(extraction.Objects, "objects", chunks, associations, entityChunks);
            await ProcessEntityCategory(extraction.Events, "events", chunks, associations, entityChunks);
            await ProcessEntityCategory(extraction.Constraints, "constraints", chunks, associations, entityChunks);

            // Process preferences with specific relationship types
            await ProcessPreferences(extraction.Likes, "likes", chunks, associations, entityChunks);
            await ProcessPreferences(extraction.Dislikes, "dislikes", chunks, associations, entityChunks);
            await ProcessPreferences(extraction.Loves, "loves", chunks, associations, entityChunks);
            await ProcessPreferences(extraction.Hates, "hates", chunks, associations, entityChunks);

            // Process relationships between people
            await ProcessRelationships(extraction.Relationships, chunks, associations, entityChunks);

            // Process inferences as knowledge chunks
            ProcessInferences(extraction.Inferences, chunks, associations, entityChunks);

            // Log final assembly summary
            _logger.LogInformation("Conversion complete: {ChunkCount} chunks, {AssociationCount} associations created",
                chunks.Count, associations.Count);

            return (chunks, associations);
        }

        private void LogExtractionSummary(MemoryExtractionResponse extraction)
        {
            _logger.LogInformation("Extracted entities: {PeopleCount} people, {PlacesCount} places, {ActivitiesCount} activities, {ObjectsCount} objects, {EventsCount} events",
                extraction.People.Count, extraction.Places.Count, extraction.Activities.Count, extraction.Objects.Count, extraction.Events.Count);

            if (extraction.People.Any())
                _logger.LogInformation("People found: {People}", string.Join(", ", extraction.People.Keys));

            if (extraction.Places.Any())
                _logger.LogInformation("Places found: {Places}", string.Join(", ", extraction.Places.Keys));

            var preferenceSubjects = extraction.Likes.Keys
                .Concat(extraction.Dislikes.Keys)
                .Concat(extraction.Loves.Keys)
                .Concat(extraction.Hates.Keys)
                .Distinct();

            if (preferenceSubjects.Any())
                _logger.LogInformation("Preferences extracted for: {PreferenceSubjects}", string.Join(", ", preferenceSubjects));

            if (extraction.Relationships.Any())
                _logger.LogInformation("Relationships found: {Relationships}", string.Join(", ", extraction.Relationships.Keys));

            if (extraction.Inferences.Any())
                _logger.LogInformation("Inferences made: {InferenceCount} items", extraction.Inferences.Length);
        }

        private async Task ProcessEntityCategory(Dictionary<string, string[]> entityData, string category,
            List<Chunk> chunks, List<ChunkAssociation> associations, Dictionary<string, Chunk> entityChunks)
        {
            if (!entityData.Any()) return;

            _logger.LogInformation("Processing {Category}: {EntityCount} entities with facts", category, entityData.Count);

            foreach (var entity in entityData)
            {
                // Get or create entity chunk
                var entityChunk = await GetOrCreateEntityChunkAsync(entity.Key, category, chunks, entityChunks);

                _logger.LogInformation("Entity '{EntityName}' ({Category}) has {FactCount} facts",
                    entity.Key, category, entity.Value.Length);

                // Create fact chunks and associate them
                foreach (var fact in entity.Value)
                {
                    var factChunk = CreateFactChunk(fact);
                    chunks.Add(factChunk);

                    associations.Add(new ChunkAssociation
                    {
                        ChunkAId = entityChunk.ID,
                        ChunkBId = factChunk.ID,
                        RelationAtoB = "HasFact", // Entity has this fact
                        RelationBtoA = "FactAbout", // Fact is about entity
                        SubTypeRelationshipAtoB = $"{category}_information",
                        SubTypeRelationshipBtoA = $"about_{category}",
                        WeightAtoB = 1.0,
                        WeightBtoA = 1.0
                    });
                }
            }
        }

        private async Task ProcessPreferences(Dictionary<string, string[]> preferenceData, string preferenceType,
            List<Chunk> chunks, List<ChunkAssociation> associations, Dictionary<string, Chunk> entityChunks)
        {
            if (!preferenceData.Any()) return;

            _logger.LogInformation("Processing {PreferenceType} preferences for {PersonCount} people",
                preferenceType, preferenceData.Count);

            foreach (var person in preferenceData)
            {
                var personChunk = await GetOrCreateEntityChunkAsync(person.Key, "people", chunks, entityChunks);

                _logger.LogInformation("Person '{PersonName}' {PreferenceType}: {PreferenceItems}",
                    person.Key, preferenceType, string.Join(", ", person.Value));

                foreach (var preference in person.Value)
                {
                    var preferenceChunk = await GetOrCreateEntityChunkAsync(preference, preferenceType, chunks, entityChunks);

                    // Map preference types to relationship types
                    var (relationAtoB, relationBtoA, weight) = preferenceType switch
                    {
                        "likes" => ("Enjoys", "EnjoyedBy", 0.6),
                        "dislikes" => ("Avoids", "AvoidedBy", 0.4),
                        "loves" => ("Enjoys", "EnjoyedBy", 0.9),
                        "hates" => ("Avoids", "AvoidedBy", 0.8),
                        _ => ("RelatedTo", "RelatedTo", 0.5)
                    };

                    associations.Add(new ChunkAssociation
                    {
                        ChunkAId = personChunk.ID,
                        ChunkBId = preferenceChunk.ID,
                        RelationAtoB = relationAtoB,
                        RelationBtoA = relationBtoA,
                        SubTypeRelationshipAtoB = $"{preferenceType}_preference",
                        SubTypeRelationshipBtoA = $"{preferenceType}_by_person",
                        WeightAtoB = weight,
                        WeightBtoA = weight
                    });
                }
            }
        }

        private async Task ProcessRelationships(Dictionary<string, string[]> relationships,
            List<Chunk> chunks, List<ChunkAssociation> associations, Dictionary<string, Chunk> entityChunks)
        {
            if (!relationships.Any()) return;

            _logger.LogInformation("Processing {RelationshipCount} relationships", relationships.Count);

            foreach (var relationship in relationships)
            {
                var parts = relationship.Key.Split('-');
                if (parts.Length == 2)
                {
                    var personA = await GetOrCreateEntityChunkAsync(parts[0], "people", chunks, entityChunks);
                    var personB = await GetOrCreateEntityChunkAsync(parts[1], "people", chunks, entityChunks);

                    _logger.LogInformation("Relationship between '{PersonA}' and '{PersonB}': {FactCount} facts",
                        parts[0], parts[1], relationship.Value.Length);

                    // Create relationship facts and associate with both people
                    foreach (var fact in relationship.Value)
                    {
                        var relationshipChunk = CreateFactChunk(fact);
                        chunks.Add(relationshipChunk);

                        // Associate relationship with both people
                        associations.Add(new ChunkAssociation
                        {
                            ChunkAId = personA.ID,
                            ChunkBId = relationshipChunk.ID,
                            RelationAtoB = "InvolvedIn",
                            RelationBtoA = "Involves",
                            SubTypeRelationshipAtoB = "relationship_participant",
                            WeightAtoB = 0.8,
                            WeightBtoA = 0.8
                        });

                        associations.Add(new ChunkAssociation
                        {
                            ChunkAId = personB.ID,
                            ChunkBId = relationshipChunk.ID,
                            RelationAtoB = "InvolvedIn",
                            RelationBtoA = "Involves",
                            SubTypeRelationshipAtoB = "relationship_participant",
                            WeightAtoB = 0.8,
                            WeightBtoA = 0.8
                        });

                        // Direct association between the two people
                        associations.Add(new ChunkAssociation
                        {
                            ChunkAId = personA.ID,
                            ChunkBId = personB.ID,
                            RelationAtoB = "RelatedTo",
                            RelationBtoA = "RelatedTo",
                            SubTypeRelationshipAtoB = "personal_relationship",
                            SubTypeRelationshipBtoA = "personal_relationship",
                            WeightAtoB = 0.7,
                            WeightBtoA = 0.7
                        });
                    }
                }
                else
                {
                    _logger.LogWarning("Malformed relationship key: '{RelationshipKey}' - expected format 'PersonA-PersonB'",
                        relationship.Key);
                }
            }
        }

        private void ProcessInferences(string[] inferences, List<Chunk> chunks,
            List<ChunkAssociation> associations, Dictionary<string, Chunk> entityChunks)
        {
            if (!inferences.Any()) return;

            _logger.LogInformation("Processing {InferenceCount} inferences", inferences.Length);

            foreach (var inference in inferences)
            {
                var inferenceChunk = CreateInferenceChunk(inference);
                chunks.Add(inferenceChunk);

                _logger.LogInformation("Created inference: '{InferenceText}'", inference);

                // Try to associate inference with mentioned entities
                var associatedEntities = new List<string>();
                foreach (var entityName in entityChunks.Keys)
                {
                    if (inference.Contains(entityName, StringComparison.OrdinalIgnoreCase))
                    {
                        associations.Add(new ChunkAssociation
                        {
                            ChunkAId = entityChunks[entityName].ID,
                            ChunkBId = inferenceChunk.ID,
                            RelationAtoB = "InferredAbout",
                            RelationBtoA = "BasisForInference",
                            SubTypeRelationshipAtoB = "behavioral_inference",
                            SubTypeRelationshipBtoA = "inferred_about",
                            WeightAtoB = 0.6,
                            WeightBtoA = 0.6
                        });

                        associatedEntities.Add(entityName);
                    }
                }

                if (associatedEntities.Any())
                {
                    _logger.LogInformation("Inference associated with entities: {AssociatedEntities}",
                        string.Join(", ", associatedEntities));
                }
            }
        }
        private async Task<Chunk> GetOrCreateFactChunkAsync(string factText, List<Chunk> chunks)
        {
            // Search for existing fact using the fact text as the name
            var existingFact = await _memorySystem.FindChunkBySemanticTypeAndName(
                "essence.entity.abstract.information", factText);

            if (existingFact != null)
            {
                _logger.LogInformation("Found existing fact, reusing: '{FactText}'", factText);
                return existingFact;
            }

            // Create new fact chunk with factText as the name
            var factChunk = new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "$Memory",
                SemanticType = "essence.entity.abstract.information",
                Name = factText, // Use actual fact text as name instead of UUID
                ActivationLevel = 1.0,
                Slots = new Dictionary<string, ModelSlot>
                {
                    ["fact_text"] = new ModelSlot { Name = "fact_text", Value = factText },
                    ["fact_type"] = new ModelSlot { Name = "fact_type", Value = "observed" }
                }
            };

            chunks.Add(factChunk);
            _logger.LogInformation("Created new fact chunk: '{FactText}'", factText);
            return factChunk;
        }

        private async Task<Chunk> GetOrCreateInferenceChunkAsync(string inference, List<Chunk> chunks)
        {
            var existingInference = await _memorySystem.FindChunkBySemanticTypeAndName(
                "essence.entity.abstract.knowledge", inference);

            if (existingInference != null)
            {
                _logger.LogInformation("Found existing inference, reusing: '{Inference}'", inference);
                return existingInference;
            }

            return new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "$Memory",
                SemanticType = "essence.entity.abstract.knowledge",
                Name = inference, // Use actual inference text as name
                ActivationLevel = 0.8,
                Slots = new Dictionary<string, ModelSlot>
                {
                    ["inference_text"] = new ModelSlot { Name = "inference_text", Value = inference },
                    ["fact_type"] = new ModelSlot { Name = "fact_type", Value = "inferred" }
                }
            };
        }
        private async Task<Chunk> GetOrCreateEntityChunkAsync(string entityName, string category,
            List<Chunk> chunks, Dictionary<string, Chunk> entityChunks)
        {
            // First check local cache from this conversion
            if (entityChunks.TryGetValue(entityName, out var localChunk))
            {
                _logger.LogInformation("Reusing local entity chunk for '{EntityName}'", entityName);
                return localChunk;
            }

            // Check if entity already exists in memory system
            var semanticType = CategoryToSemanticType[category];
            var existingChunk = await _memorySystem.FindChunkBySemanticTypeAndName(semanticType, entityName);

            if (existingChunk != null)
            {
                _logger.LogInformation("Found existing entity chunk for '{EntityName}' in memory", entityName);
                entityChunks[entityName] = existingChunk; // Cache locally
                return existingChunk;
            }

            // Create new entity chunk
            var chunk = new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "$Memory",
                SemanticType = semanticType,
                Name = entityName,
                ActivationLevel = 1.0
            };

            chunk.Slots["entity_name"] = new ModelSlot { Name = "entity_name", Value = entityName };
            chunk.Slots["category"] = new ModelSlot { Name = "category", Value = category };

            chunks.Add(chunk);
            entityChunks[entityName] = chunk;

            _logger.LogInformation("Created new entity chunk for '{EntityName}' ({Category})", entityName, category);
            return chunk;
        }

        private static Chunk CreateFactChunk(string fact)
        {
            return new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "$Memory",
                SemanticType = "essence.entity.abstract.information",
                Name = $"fact_{Guid.NewGuid().ToString("N")[..8]}",
                ActivationLevel = 1.0,
                Slots = new Dictionary<string, ModelSlot>
                {
                    ["fact_text"] = new ModelSlot { Name = "fact_text", Value = fact },
                    ["fact_type"] = new ModelSlot { Name = "fact_type", Value = "observed" }
                }
            };
        }

        private static Chunk CreateInferenceChunk(string inference)
        {
            return new Chunk
            {
                ChunkType = "Declarative",
                CognitiveCategory = "$Memory",
                SemanticType = "essence.entity.abstract.knowledge",
                Name = $"inference_{Guid.NewGuid().ToString("N")[..8]}",
                ActivationLevel = 0.8, // Lower activation for derived knowledge
                Slots = new Dictionary<string, ModelSlot>
                {
                    ["inference_text"] = new ModelSlot { Name = "inference_text", Value = inference },
                    ["fact_type"] = new ModelSlot { Name = "fact_type", Value = "inferred" }
                }
            };
        }
    }
}