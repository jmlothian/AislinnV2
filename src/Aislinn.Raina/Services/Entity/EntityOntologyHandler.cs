
namespace Aislinn.Raina
{
    public static class EntityOntologyHandler
    {
        private static readonly Dictionary<string, string> EntityOntologyMap = new()
{
    // People & Roles
    { "person", "entity.person.instance" },
    { "organization", "entity.organization.instance" },
    { "role", "entity.person.role" },

    // Locations & Geography
    { "location", "entity.location.generic" },
    { "country", "entity.location.country" },
    { "city", "entity.location.city" },
    { "region", "entity.location.region" },
    { "state", "entity.location.state" },
    { "address", "entity.location.address" },
    { "coordinate", "entity.location.coordinate" },
    { "facility", "entity.location.facility" },

    // Time & Date
    { "date", "entity.time.date" },
    { "time", "entity.time.clock" },
    { "duration", "entity.time.duration" },
    { "time_period", "entity.time.period" },

    // Objects & Items
    { "product", "entity.object.product" },
    { "tool", "entity.object.tool" },
    { "device", "entity.object.device" },
    { "software", "entity.object.software" },
    { "vehicle", "entity.object.vehicle" },
    { "food", "entity.object.food" },
    { "document", "entity.object.document" },
    { "file", "entity.object.file" },

    // Concepts & Abstract Things
    { "idea", "entity.abstract.idea" },
    { "emotion", "entity.abstract.emotion" },
    { "belief", "entity.abstract.belief" },
    { "opinion", "entity.abstract.opinion" },
    { "goal", "entity.abstract.goal" },
    { "intent", "entity.abstract.intent" },
    { "issue", "entity.abstract.issue" },
    { "skill", "entity.abstract.skill" },
    { "knowledge", "entity.abstract.knowledge" },
    { "concept", "entity.abstract.concept" },
    { "theory", "entity.abstract.theory" },
    { "principle", "entity.abstract.principle" }
    };

        /// <summary>
        /// Retrieves the ontology for a given entity type.
        /// </summary>
        /// <param name="entityType">The entity type to look up.</param>
        /// <returns>The corresponding ontology string, or null if not found.</returns>
        public static string GetEntityOntology(string entityType)
        {
            if (EntityOntologyMap.TryGetValue(entityType.ToLower(), out var ontology))
            {
                return ontology;
            }
            return null; // or throw an exception if you prefer
        }
    }
}
