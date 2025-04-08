using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RAINA.Modules;

namespace RAINA.Modules.Implementations
{
    /// <summary>
    /// module for handling information retrieval queries
    /// </summary>
    public class QueryIntentModule : IIntentModule
    {
        private readonly QueryEngine _queryEngine;
        private readonly ConversationManager _conversationManager;

        public QueryIntentModule(QueryEngine queryEngine, ConversationManager conversationManager)
        {
            _queryEngine = queryEngine ?? throw new ArgumentNullException(nameof(queryEngine));
            _conversationManager = conversationManager ?? throw new ArgumentNullException(nameof(conversationManager));
        }

        public string GetIntentType()
        {
            return "Query";
        }

        public string GetPromptDescription()
        {
            return "Query: User is asking RAINA to recall information, search through memory, or provide stored knowledge";
        }

        public string[] GetPromptExamples()
        {
            return new[]
            {
                "What did I say about the Johnson project last week?",
                "When was my last meeting with Sarah?",
                "Do you remember the website I bookmarked about machine learning?",
                "What was that restaurant we talked about for the team dinner?"
            };
        }

        public string[] GetExpectedEntities()
        {
            return new[]
            {
                "topic",
                "person",
                "timeframe",
                "location"
            };
        }

        public string[] GetExpectedParameters()
        {
            return new[]
            {
                "recency",
                "specificity",
                "importance"
            };
        }

        public async Task<Response> HandleAsync(string userInput, Intent intent, UserContext context)
        {
            // Extract query parameters from intent
            var queryParameters = new QueryParameters
            {
                Keywords = ExtractKeywords(intent),
                RelevanceThreshold = 0.7,
                MaxResults = 5
            };

            // Process query using QueryEngine
            var queryResults = await _queryEngine.SearchAsync(queryParameters, context);

            // Generate response using conversation manager
            return await _conversationManager.GenerateQueryResponseAsync(userInput, intent, queryResults, context);
        }

        private List<string> ExtractKeywords(Intent intent)
        {
            var keywords = new List<string>();
            foreach (var entity in intent.Entities)
            {
                keywords.Add(entity.Value);
            }
            return keywords;
        }
    }
}