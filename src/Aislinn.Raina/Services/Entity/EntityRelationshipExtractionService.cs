using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using Aislinn.Configuration;

namespace Aislinn.Core.Services
{
    public class EntityRelationshipExtractionService
    {
        private static readonly string[] STATIC_RELATIONSHIPS = {
            "AboutTopic", "AccessedBy", "AccessibleBy", "AchievedThrough", "AcquiredBy", "Adjacent",
            "AffectsEntity", "AffectsEnvironment", "After", "AimedAt", "AppliedIn", "AppliedTo",
            "AppliesTo", "Association", "AuthoredBy", "BasedOn", "Before", "BehindAction", "Between",
            "BordersWith", "BornAt", "BornIn", "CarriesPassengers", "CausedBy", "Causes", "ChangesOver",
            "ChangesUnder", "CharacterizedBy", "CommunicatedBy", "ComposedOf", "ConflictsWith",
            "ConnectedBy", "ConnectedTo", "Connects", "ConsumedBy", "ContainedIn", "Contains",
            "ContainsBuildings", "ContainsComponents", "ContainsEquipment", "ContainsNutrients",
            "ControlledBy", "ConvertedTo", "CreatedBy", "DefinedBy", "DependsOn", "DesignedFor",
            "DevelopedBy", "DevelopedThrough", "Dies", "DirectionTo", "DistanceFrom", "DrivenBy",
            "During", "DuringTime", "EatsFood", "EmbodiedBy", "Enables", "EvaluatedAs", "Evolved",
            "ExcludesFrom", "ExperiencedBy", "ExplainedBy", "ExpressedAs", "FollowedBy", "FoundIn",
            "GovernedBy", "GrowsIn", "GuidesAction", "HasBody", "HasBrand", "HasCapital", "HasCurrency",
            "HasDNA", "HasDeadline", "HasDirection", "HasEmployees", "HasIdentity", "HasLanguage",
            "HasLocation", "HasMayor", "HasName", "HasOffspring", "HasPersonality", "HasPopulation",
            "HasPostalCode", "HasProperty", "HasPurpose", "HasResponsibilities", "HasRole", "HasSize",
            "HasStages", "HasState", "HasStrength", "HasSubtype", "HasWeight", "HeldBy", "Impedes",
            "InCity", "InContext", "InfluencedBy", "InspiredBy", "InstanceOf", "IntegratesWith",
            "Involves", "InvolvesMaterials", "IsA", "IsTransitive", "JudgedBy", "KnownFor",
            "KnowsPerson", "LeadBy", "LeadsTo", "LivesIn", "LocatedIn", "LocatesBuilding",
            "LongerThan", "MadeOf", "ManagedBy", "ManufacturedBy", "MeasuredBy", "MeasuredIn",
            "MeasuredWith", "Measures", "Mediated", "NearTo", "ObservedBy", "ObservedIn", "OccursAt",
            "OccursOn", "OnStreet", "OpenedWith", "OperatedBy", "OppositeOf", "PartOf", "PartnersWith",
            "PerformedBy", "PossessedBy", "PoweredBy", "PreparedBy", "ProcessedBy", "ProducesResult",
            "ProposedBy", "PursuitBy", "RecognizedBy", "ReferencesOther", "RegulatedBy", "RelatedTo",
            "RequiredFor", "RequiresActions", "RequiresAttention", "RequiresSkill", "RequiresSkills",
            "RequiresTime", "Results", "RunsOn", "ScheduledAt", "ScheduledFor", "Sequences",
            "SharedVia", "SharedWith", "ShorterThan", "SignedBy", "SimilarTo", "SoldBy", "SolvedBy",
            "SpecifiesLocation", "StoredIn", "StoredOn", "SubcategoryOf", "SupportedBy", "Temporary",
            "TestedBy", "TransformsTo", "TransitionsTo", "TransmittedBy", "TravelsOn", "TriggeredBy",
            "UnderstoodBy", "Unpredictable", "UpdatedBy", "UsedBy", "UsedFor", "UsesTools",
            "VerifiedBy", "ViolatedBy"
        };

        private static readonly string[] DEFAULT_ONTOLOGY_CATEGORIES = {
            "entity.abstract.belief", "entity.abstract.category", "entity.abstract.concept",
            "entity.abstract.emotion", "entity.abstract.goal", "entity.abstract.idea",
            "entity.abstract.information.document", "entity.abstract.information.file",
            "entity.abstract.intent", "entity.abstract.issue", "entity.abstract.knowledge",
            "entity.abstract.opinion", "entity.abstract.organization", "entity.abstract.person",
            "entity.abstract.principle", "entity.abstract.role", "entity.abstract.skill",
            "entity.abstract.software", "entity.abstract.system", "entity.abstract.theory",
            "entity.artificial.device", "entity.artificial.product", "entity.artificial.tool",
            "entity.artificial.vehicle", "entity.event.action", "entity.event.interaction",
            "entity.event.occurrence", "entity.event.process", "entity.physical.artificial",
            "entity.physical.living.human", "entity.physical.natural", "entity.physical.substance",
            "entity.substance.food", "property.physical", "property.quality", "relation",
            "space.location.address", "space.location.city", "space.location.coordinate",
            "space.location.country", "space.location.facility", "space.location.region",
            "space.location.state", "state", "time.clock", "time.date", "time.duration", "time.period"
        };

        private readonly HttpClient _httpClient;
        private readonly string _openAIApiKey;

        // Cache storage
        private Dictionary<string, string> _relationshipMappings;
        private HashSet<string> _approvedNovelRelations;

        // Ontology management
        private List<string> _ontologyCategories;
        private string _ontologyCategoriesString;

        public EntityRelationshipExtractionService(AislinnConfiguration config)
        {
            _openAIApiKey = config.OpenAIApiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAIApiKey}");

            // Initialize collections
            _relationshipMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _approvedNovelRelations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _ontologyCategories = new List<string>(DEFAULT_ONTOLOGY_CATEGORIES);

            UpdateOntologyCategoriesString();
        }

        /// <summary>
        /// Main entry point for entity and relationship extraction
        /// </summary>
        public async Task<ExtractionResult> ExtractEntitiesAndRelationshipsAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ExtractionResult();

            // First prompt: extract entities and relationships
            var result = await ExtractEntitiesAndRawRelationshipsAsync(text);

            // Check for unknown relationships
            var unknownRelations = FindUnknownRelations(result.Relationships);

            // If we have unknown relations, run second prompt to map them
            if (unknownRelations.Any())
            {
                var mappingResult = await MapUnknownRelationshipsAsync(text, unknownRelations);

                if (mappingResult != null)
                {
                    // Update caches
                    foreach (var mapping in mappingResult.Mappings)
                    {
                        _relationshipMappings[mapping.Key] = mapping.Value;
                    }

                    foreach (var novel in mappingResult.KeepOriginal)
                    {
                        _approvedNovelRelations.Add(novel);
                    }

                    // Update relationships in result
                    foreach (var relationship in result.Relationships)
                    {
                        if (_relationshipMappings.TryGetValue(relationship.Type, out var mappedType))
                        {
                            relationship.Type = mappedType;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// First prompt: extract entities and relationships with example relations
        /// </summary>
        private async Task<ExtractionResult> ExtractEntitiesAndRawRelationshipsAsync(string text)
        {
            var exampleRelations = "IsA, CreatedBy, LocatedIn, PartOf, UsedFor, CausedBy, Contains, Association";

            var prompt = $@"Extract entities and relationships from the text. Entity names should use spaces if multiple words. If the word is slang or colloquial, include a ""formal"" field.  Use CamelCase with no spaces or special characters for relationships. 

Ontology categories: {_ontologyCategoriesString}

Example relationships: {exampleRelations}

Text: ""{text}""

Output JSON:
{{
  ""entities"": [
    {{""name"": ""entity name"", ""type"": ""ontology.category"", ""formal"":""entity name"",}}
  ],
  ""relationships"": [
    {{""entity1"": ""name1"", ""entity2"": ""name2"", ""relation"": ""RelationType""}}
  ]
}}";

            try
            {
                var response = await CallOpenAIAsync(prompt, jsonMode: true);
                var jsonResponse = response.Choices[0].Message.Content;

                return JsonSerializer.Deserialize<ExtractionResult>(jsonResponse) ?? new ExtractionResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in first extraction prompt: {ex.Message}");
                return new ExtractionResult();
            }
        }

        /// <summary>
        /// Second prompt: map unknown relationships to known types
        /// </summary>
        private async Task<MappingResult> MapUnknownRelationshipsAsync(string text, List<string> unknownRelations)
        {
            var availableRelations = BuildAvailableRelationsList();
            var unknownRelationsStr = string.Join(", ", unknownRelations);

            var prompt = $@"Reclassify these relationships using our standard types, or keep original if no good match exists.

Original text: ""{text}""
Unknown relations: [{unknownRelationsStr}]

Primary relations: {string.Join(" ", STATIC_RELATIONSHIPS)}
Additional learned relations: {string.Join(" ", _approvedNovelRelations)}

Output JSON:
{{
  ""mappings"": {{
    ""unknown_relation"": ""StandardRelation""
  }},
  ""keep_original"": [""relation_to_keep""]
}}";

            try
            {
                var response = await CallOpenAIAsync(prompt, jsonMode: true);
                var jsonResponse = response.Choices[0].Message.Content;

                return JsonSerializer.Deserialize<MappingResult>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in relationship mapping prompt: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find relationships that are not in our known lists
        /// </summary>
        private List<string> FindUnknownRelations(List<Relationship> relationships)
        {
            var knownRelations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Add static relationships
            foreach (var rel in STATIC_RELATIONSHIPS)
                knownRelations.Add(rel);

            // Add approved novel relationships
            foreach (var rel in _approvedNovelRelations)
                knownRelations.Add(rel);

            // Add cached mappings (keys)
            foreach (var mapping in _relationshipMappings.Keys)
                knownRelations.Add(mapping);

            return relationships
                .Where(r => !knownRelations.Contains(r.Type))
                .Select(r => r.Type)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Build the complete list of available relationships for prompts
        /// </summary>
        private string BuildAvailableRelationsList()
        {
            var allRelations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rel in STATIC_RELATIONSHIPS)
                allRelations.Add(rel);

            foreach (var rel in _approvedNovelRelations)
                allRelations.Add(rel);

            return string.Join(" ", allRelations.OrderBy(r => r));
        }

        /// <summary>
        /// Update the space-separated ontology categories string for prompts
        /// </summary>
        private void UpdateOntologyCategoriesString()
        {
            _ontologyCategoriesString = string.Join(" ", _ontologyCategories);
        }

        /// <summary>
        /// Add a new ontology category
        /// </summary>
        public void AddOntologyCategory(string category)
        {
            if (!string.IsNullOrWhiteSpace(category) && !_ontologyCategories.Contains(category))
            {
                _ontologyCategories.Add(category);
                UpdateOntologyCategoriesString();
            }
        }

        /// <summary>
        /// Remove an ontology category
        /// </summary>
        public void RemoveOntologyCategory(string category)
        {
            if (_ontologyCategories.Remove(category))
            {
                UpdateOntologyCategoriesString();
            }
        }

        /// <summary>
        /// Save caches to file
        /// </summary>
        public async Task SaveCacheAsync(string filePath)
        {
            try
            {
                var cacheData = new CacheData
                {
                    RelationshipMappings = _relationshipMappings,
                    ApprovedNovelRelations = _approvedNovelRelations.ToList()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(cacheData, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Load caches from file
        /// </summary>
        public async Task LoadCacheAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;

                var json = await File.ReadAllTextAsync(filePath);
                var cacheData = JsonSerializer.Deserialize<CacheData>(json);

                if (cacheData != null)
                {
                    _relationshipMappings = new Dictionary<string, string>(
                        cacheData.RelationshipMappings ?? new Dictionary<string, string>(),
                        StringComparer.OrdinalIgnoreCase);

                    _approvedNovelRelations = new HashSet<string>(
                        cacheData.ApprovedNovelRelations ?? new List<string>(),
                        StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Call OpenAI API
        /// </summary>
        private async Task<OpenAIResponse> CallOpenAIAsync(string prompt, double temperature = 0.1, bool jsonMode = false)
        {
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant specialized in entity and relationship extraction." },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = 2000,
                response_format = jsonMode ? new { type = "json_object" } : null
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OpenAIResponse>(responseString);
        }

        public bool IsKnownRelation(string relationType)
        {
            if (string.IsNullOrWhiteSpace(relationType))
                return false;

            // Check static relationships
            if (STATIC_RELATIONSHIPS.Contains(relationType, StringComparer.OrdinalIgnoreCase))
                return true;

            // Check approved novel relationships
            if (_approvedNovelRelations.Contains(relationType))
                return true;

            // Check cached mappings (both keys and values)
            if (_relationshipMappings.ContainsKey(relationType) ||
                _relationshipMappings.Values.Contains(relationType, StringComparer.OrdinalIgnoreCase))
                return true;

            return false;
        }

        // Then update the ExtractionResult method:
        public bool HasUnmappedRelations(List<Relationship> relationships)
        {
            return relationships.Any(r => !this.IsKnownRelation(r.Type));
        }
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Data models
    public class ExtractionResult
    {
        [JsonPropertyName("entities")]
        public List<Entity> Entities { get; set; } = new List<Entity>();

        [JsonPropertyName("relationships")]
        public List<Relationship> Relationships { get; set; } = new List<Relationship>();

        // Add this method to EntityRelationshipExtractionService
    }



    public class Relationship
    {
        [JsonPropertyName("entity1")]
        public string Entity1 { get; set; }

        [JsonPropertyName("entity2")]
        public string Entity2 { get; set; }

        [JsonPropertyName("relation")]
        public string Type { get; set; }
    }

    public class MappingResult
    {
        [JsonPropertyName("mappings")]
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("keep_original")]
        public List<string> KeepOriginal { get; set; } = new List<string>();
    }

    public class CacheData
    {
        public Dictionary<string, string> RelationshipMappings { get; set; } = new Dictionary<string, string>();
        public List<string> ApprovedNovelRelations { get; set; } = new List<string>();
    }

    // OpenAI response models (reusing from existing code)
    public class OpenAIResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new List<Choice>();
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}