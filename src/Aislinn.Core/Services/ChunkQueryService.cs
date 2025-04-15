using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aislinn.Core.Models;
using Aislinn.ChunkStorage.Interfaces;

namespace Aislinn.Core.Query
{
    /// <summary>
    /// Handles querying for chunks based on various criteria beyond simple ID lookups.
    /// Supports both exact and similarity-based matching of chunk properties and slots.
    /// </summary>
    public class ChunkQueryService
    {
        private readonly IChunkStore _chunkStore;
        private readonly string _chunkCollectionId;

        /// <summary>
        /// Creates a new chunk query service
        /// </summary>
        public ChunkQueryService(IChunkStore chunkStore, string chunkCollectionId = "default")
        {
            _chunkStore = chunkStore ?? throw new ArgumentNullException(nameof(chunkStore));
            _chunkCollectionId = chunkCollectionId;
        }

        /// <summary>
        /// Executes a chunk query against the chunk store
        /// </summary>
        public async Task<List<ChunkQueryResult>> ExecuteQueryAsync(ChunkQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            // Validate query
            query.Validate();

            // Get chunk collection
            var chunkCollection = await _chunkStore.GetCollectionAsync(_chunkCollectionId);
            if (chunkCollection == null)
                throw new InvalidOperationException($"Chunk collection '{_chunkCollectionId}' not found");

            // First, get all chunks of the specified type (efficient filtering)
            var allChunks = await chunkCollection.GetAllChunksAsync();
            var filteredChunks = allChunks.Where(c =>
                c.ChunkType == query.ChunkType &&
                (string.IsNullOrEmpty(query.SemanticType) || IsSemanticTypeMatch(c, query.SemanticType)) &&
                (string.IsNullOrEmpty(query.CognitiveCategory) || IsCognitiveCategoryMatch(c, query.CognitiveCategory))).ToList();

            // Filter by name if specified
            if (!string.IsNullOrEmpty(query.Name) && query.NameHandling != NameMatchType.Ignore)
            {
                filteredChunks = filteredChunks.Where(c => IsNameMatch(c, query.Name, query.NameHandling)).ToList();
            }

            // Now evaluate the compiled query expression on the remaining chunks
            var compiledQuery = CompileQueryExpression(query.SlotQueryExpression);

            // Apply parameter values if available
            if (query.Parameters != null && query.Parameters.Count > 0)
            {
                compiledQuery = ApplyParametersToQuery(compiledQuery, query.Parameters);
            }

            // Evaluate each chunk against the query
            var results = new List<ChunkQueryResult>();

            foreach (var chunk in filteredChunks)
            {
                var matchResult = EvaluateChunk(chunk, compiledQuery, query.ExtraSlotsHandling);

                // Check if it meets the minimum threshold
                if (matchResult.SimilarityScore >= query.MinimumThreshold)
                {
                    results.Add(matchResult);
                }
            }

            // Sort results based on the ranking parameter
            if (query.RankingStrategy == RankingStrategy.ExactMatchFirst)
            {
                results = results.OrderByDescending(r => r.ExactMatch)
                                 .ThenByDescending(r => r.SimilarityScore)
                                 .ToList();
            }
            else
            {
                results = results.OrderByDescending(r => r.SimilarityScore).ToList();
            }

            return results;
        }

        /// <summary>
        /// Compiles a query expression string into a structured query object
        /// </summary>
        public CompiledQuery CompileQueryExpression(string queryExpression)
        {
            if (string.IsNullOrWhiteSpace(queryExpression))
                return new CompiledQuery { SlotQueries = new List<SlotQuery>() };

            var compiledQuery = new CompiledQuery();
            compiledQuery.OriginalExpression = queryExpression;
            compiledQuery.SlotQueries = new List<SlotQuery>();

            // Parse the query expression
            var parser = new QueryExpressionParser();
            compiledQuery = parser.Parse(queryExpression);

            return compiledQuery;
        }

        /// <summary>
        /// Applies parameter values to a compiled query
        /// </summary>
        public CompiledQuery ApplyParametersToQuery(CompiledQuery query, Dictionary<string, object> parameters)
        {
            if (query == null || parameters == null || !parameters.Any())
                return query;

            // Create a new query to avoid modifying the original
            var newQuery = new CompiledQuery
            {
                OriginalExpression = query.OriginalExpression,
                LogicalOperator = query.LogicalOperator,
                SlotQueries = new List<SlotQuery>(),
                SubQueries = query.SubQueries != null ? new List<CompiledQuery>() : null
            };

            // Replace parameters in slot queries
            foreach (var slotQuery in query.SlotQueries)
            {
                var newSlotQuery = new SlotQuery
                {
                    SlotName = slotQuery.SlotName,
                    ComparisonType = slotQuery.ComparisonType,
                    Required = slotQuery.Required,
                    Weight = slotQuery.Weight,
                    CustomComparer = slotQuery.CustomComparer
                };

                // Replace parameter values in Value and Value2
                if (slotQuery.Value is string valueStr && valueStr.StartsWith("@") &&
                    parameters.TryGetValue(valueStr.Substring(1), out var value1))
                {
                    newSlotQuery.Value = value1;
                }
                else
                {
                    newSlotQuery.Value = slotQuery.Value;
                }

                if (slotQuery.Value2 is string value2Str && value2Str.StartsWith("@") &&
                    parameters.TryGetValue(value2Str.Substring(1), out var value2))
                {
                    newSlotQuery.Value2 = value2;
                }
                else
                {
                    newSlotQuery.Value2 = slotQuery.Value2;
                }

                newQuery.SlotQueries.Add(newSlotQuery);
            }

            // Recursively apply parameters to sub-queries
            if (query.SubQueries != null)
            {
                foreach (var subQuery in query.SubQueries)
                {
                    newQuery.SubQueries.Add(ApplyParametersToQuery(subQuery, parameters));
                }
            }

            return newQuery;
        }

        /// <summary>
        /// Evaluates a chunk against a compiled query
        /// </summary>
        private ChunkQueryResult EvaluateChunk(Chunk chunk, CompiledQuery query, ExtraSlotsHandling extraSlotsHandling)
        {
            var result = new ChunkQueryResult
            {
                Chunk = chunk,
                ExactMatch = true,
                SlotMatches = new Dictionary<string, SlotMatchResult>()
            };

            // If no query criteria, return perfect match
            if (query.SlotQueries.Count == 0 && (query.SubQueries == null || query.SubQueries.Count == 0))
            {
                result.SimilarityScore = 1.0;
                return result;
            }

            // Check for extra slots if needed
            if (extraSlotsHandling == ExtraSlotsHandling.NoExtraAllowed)
            {
                var querySlotNames = query.SlotQueries.Select(q => q.SlotName).ToHashSet();
                var extraSlots = chunk.Slots.Keys.Where(k => !querySlotNames.Contains(k)).ToList();

                // Exclude standard properties that aren't considered slots
                extraSlots.RemoveAll(s => s == "ID" || s == "ChunkType" || s == "Name" || s == "Vector" || s == "ActivationLevel");

                if (extraSlots.Any())
                {
                    result.ExactMatch = false;
                    result.SimilarityScore = 0;
                    return result;
                }
            }

            double totalScore = 0;
            double totalWeight = 0;
            bool allRequiredMatched = true;

            // Evaluate each slot query
            foreach (var slotQuery in query.SlotQueries)
            {
                var slotMatchResult = EvaluateSlotQuery(chunk, slotQuery);
                result.SlotMatches[slotQuery.SlotName] = slotMatchResult;

                if (slotQuery.Required && slotMatchResult.SimilarityScore < 0.001)
                {
                    allRequiredMatched = false;
                }

                if (slotMatchResult.SimilarityScore < 1.0)
                {
                    result.ExactMatch = false;
                }

                totalScore += slotMatchResult.SimilarityScore * slotQuery.Weight;
                totalWeight += slotQuery.Weight;
            }

            // If any required slots didn't match, score is 0
            if (!allRequiredMatched)
            {
                result.SimilarityScore = 0;
                return result;
            }

            // Calculate the weighted average similarity score
            result.SimilarityScore = totalWeight > 0 ? totalScore / totalWeight : 1.0;

            // Apply penalty for extra slots if configured
            if (extraSlotsHandling == ExtraSlotsHandling.Penalty)
            {
                var querySlotNames = query.SlotQueries.Select(q => q.SlotName).ToHashSet();
                var extraSlots = chunk.Slots.Keys.Where(k => !querySlotNames.Contains(k) &&
                                                            k != "ID" && k != "ChunkType" &&
                                                            k != "Name" && k != "Vector" &&
                                                            k != "ActivationLevel").ToList();

                if (extraSlots.Any())
                {
                    // Apply a penalty proportional to the number of extra slots
                    double penalty = Math.Min(0.5, extraSlots.Count * 0.1);
                    result.SimilarityScore *= (1.0 - penalty);
                    result.ExactMatch = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Evaluates a single slot query against a chunk
        /// </summary>
        private SlotMatchResult EvaluateSlotQuery(Chunk chunk, SlotQuery query)
        {
            var result = new SlotMatchResult
            {
                SlotName = query.SlotName,
                SlotExists = chunk.Slots.ContainsKey(query.SlotName)
            };

            // If the slot doesn't exist
            if (!result.SlotExists)
            {
                result.SimilarityScore = 0;
                return result;
            }

            // Get the slot value
            var slot = chunk.Slots[query.SlotName];
            var slotValue = slot.Value;
            result.ActualValue = slotValue;

            // If slot value is null
            if (slotValue == null)
            {
                result.SimilarityScore = query.Value == null ? 1.0 : 0;
                return result;
            }

            // Check if custom comparer is provided
            if (query.CustomComparer != null)
            {
                result.SimilarityScore = query.CustomComparer(slotValue, query.Value);
                return result;
            }

            // Default comparison based on ComparisonType
            switch (query.ComparisonType)
            {
                case ComparisonType.ExactMatch:
                    result.SimilarityScore = CompareExact(slotValue, query.Value);
                    break;

                case ComparisonType.Range:
                    result.SimilarityScore = CompareRange(slotValue, query.Value, query.Value2);
                    break;

                case ComparisonType.Contains:
                    result.SimilarityScore = CompareContains(slotValue, query.Value);
                    break;

                case ComparisonType.StringDistance:
                    result.SimilarityScore = CompareStringDistance(slotValue, query.Value);
                    break;

                case ComparisonType.GreaterThan:
                    result.SimilarityScore = CompareGreaterThan(slotValue, query.Value);
                    break;

                case ComparisonType.LessThan:
                    result.SimilarityScore = CompareLessThan(slotValue, query.Value);
                    break;

                case ComparisonType.NumericSimilarity:
                    result.SimilarityScore = CompareNumericSimilarity(slotValue, query.Value);
                    break;

                default:
                    result.SimilarityScore = 0;
                    break;
            }

            return result;
        }

        #region Comparison Methods

        private double CompareExact(object actual, object expected)
        {
            if (actual == null && expected == null)
                return 1.0;

            if (actual == null || expected == null)
                return 0;

            // Handle numeric comparison with small tolerance for floating point
            if (IsNumeric(actual) && IsNumeric(expected))
            {
                double actualValue = Convert.ToDouble(actual);
                double expectedValue = Convert.ToDouble(expected);
                return Math.Abs(actualValue - expectedValue) < 0.000001 ? 1.0 : 0;
            }

            // Boolean comparison
            if (actual is bool actualBool && expected is bool expectedBool)
            {
                return actualBool == expectedBool ? 1.0 : 0;
            }

            // String comparison
            if (actual is string actualStr && expected is string expectedStr)
            {
                return string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0;
            }

            // Default to ToString comparison
            return actual.ToString() == expected.ToString() ? 1.0 : 0;
        }

        private double CompareRange(object actual, object min, object max)
        {
            if (actual == null || min == null || max == null)
                return 0;

            if (!IsNumeric(actual) || !IsNumeric(min) || !IsNumeric(max))
                return 0;

            double actualValue = Convert.ToDouble(actual);
            double minValue = Convert.ToDouble(min);
            double maxValue = Convert.ToDouble(max);

            // Swap if min > max
            if (minValue > maxValue)
            {
                var temp = minValue;
                minValue = maxValue;
                maxValue = temp;
            }

            // Check if value is in range
            if (actualValue >= minValue && actualValue <= maxValue)
                return 1.0;

            // Calculate distance from range
            double distance;
            if (actualValue < minValue)
                distance = minValue - actualValue;
            else
                distance = actualValue - maxValue;

            // Calculate similarity based on how far outside the range
            double rangeSize = maxValue - minValue;
            if (rangeSize < 0.000001) // Avoid division by zero
                return actualValue >= minValue - 0.000001 && actualValue <= maxValue + 0.000001 ? 1.0 : 0;

            // Similarity decreases as distance increases
            return Math.Max(0, 1.0 - (distance / rangeSize));
        }

        private double CompareContains(object actual, object substring)
        {
            if (actual == null || substring == null)
                return 0;

            string actualStr = actual.ToString();
            string substringStr = substring.ToString();

            if (string.IsNullOrEmpty(substringStr))
                return 1.0;

            bool contains = actualStr.IndexOf(substringStr, StringComparison.OrdinalIgnoreCase) >= 0;
            return contains ? 1.0 : 0;
        }

        private double CompareStringDistance(object actual, object expected)
        {
            if (actual == null || expected == null)
                return 0;

            string actualStr = actual.ToString();
            string expectedStr = expected.ToString();

            if (string.IsNullOrEmpty(expectedStr) && string.IsNullOrEmpty(actualStr))
                return 1.0;

            if (string.IsNullOrEmpty(expectedStr) || string.IsNullOrEmpty(actualStr))
                return 0;

            // Calculate Levenshtein distance
            int distance = LevenshteinDistance(actualStr, expectedStr);
            int maxLength = Math.Max(actualStr.Length, expectedStr.Length);

            return 1.0 - ((double)distance / maxLength);
        }

        private double CompareGreaterThan(object actual, object threshold)
        {
            if (actual == null || threshold == null)
                return 0;

            if (!IsNumeric(actual) || !IsNumeric(threshold))
                return 0;

            double actualValue = Convert.ToDouble(actual);
            double thresholdValue = Convert.ToDouble(threshold);

            return actualValue > thresholdValue ? 1.0 : 0;
        }

        private double CompareLessThan(object actual, object threshold)
        {
            if (actual == null || threshold == null)
                return 0;

            if (!IsNumeric(actual) || !IsNumeric(threshold))
                return 0;

            double actualValue = Convert.ToDouble(actual);
            double thresholdValue = Convert.ToDouble(threshold);

            return actualValue < thresholdValue ? 1.0 : 0;
        }

        private double CompareNumericSimilarity(object actual, object expected)
        {
            if (actual == null || expected == null)
                return 0;

            if (!IsNumeric(actual) || !IsNumeric(expected))
                return 0;

            double actualValue = Convert.ToDouble(actual);
            double expectedValue = Convert.ToDouble(expected);

            if (Math.Abs(expectedValue) < 0.000001) // Avoid division by zero
                return Math.Abs(actualValue) < 0.000001 ? 1.0 : 0;

            // Calculate similarity as ratio (bounded to avoid extreme values)
            double ratio;
            if (actualValue * expectedValue >= 0) // Same sign
            {
                double maxValue = Math.Max(Math.Abs(actualValue), Math.Abs(expectedValue));
                double minValue = Math.Min(Math.Abs(actualValue), Math.Abs(expectedValue));
                ratio = minValue / maxValue;
            }
            else // Different signs
            {
                ratio = 0; // Different signs are completely dissimilar
            }

            return ratio;
        }

        private int LevenshteinDistance(string s, string t)
        {
            // Levenshtein distance implementation
            int sLen = s.Length;
            int tLen = t.Length;

            int[,] d = new int[sLen + 1, tLen + 1];

            for (int i = 0; i <= sLen; i++)
                d[i, 0] = i;

            for (int j = 0; j <= tLen; j++)
                d[0, j] = j;

            for (int j = 1; j <= tLen; j++)
            {
                for (int i = 1; i <= sLen; i++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[sLen, tLen];
        }

        private bool IsNumeric(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort ||
                   value is int || value is uint || value is long || value is ulong ||
                   value is float || value is double || value is decimal;
        }

        #endregion

        #region Helper Methods

        private bool IsSemanticTypeMatch(Chunk chunk, string semanticType)
        {
            // Check if the SemanticType slot exists and matches
            return chunk.Slots.TryGetValue("SemanticType", out var semanticTypeSlot) &&
                   semanticTypeSlot.Value?.ToString() == semanticType;
        }

        private bool IsCognitiveCategoryMatch(Chunk chunk, string cognitiveCategory)
        {
            // Check if the CognitiveCategory slot exists and matches
            return chunk.Slots.TryGetValue("CognitiveCategory", out var categorySlot) &&
                   categorySlot.Value?.ToString() == cognitiveCategory;
        }

        private bool IsNameMatch(Chunk chunk, string name, NameMatchType matchType)
        {
            if (string.IsNullOrEmpty(chunk.Name) && string.IsNullOrEmpty(name))
                return true;

            if (string.IsNullOrEmpty(chunk.Name) || string.IsNullOrEmpty(name))
                return false;

            switch (matchType)
            {
                case NameMatchType.ExactMatch:
                    return string.Equals(chunk.Name, name, StringComparison.OrdinalIgnoreCase);

                case NameMatchType.Contains:
                    return chunk.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;

                case NameMatchType.StringDistance:
                    double similarity = 1.0 - ((double)LevenshteinDistance(chunk.Name, name) / Math.Max(chunk.Name.Length, name.Length));
                    return similarity >= 0.8; // 80% similarity threshold

                default:
                    return true; // Ignore = always match
            }
        }

        #endregion
    }

    /// <summary>
    /// Parser for the query expression language
    /// </summary>
    public class QueryExpressionParser
    {
        public CompiledQuery Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return new CompiledQuery { SlotQueries = new List<SlotQuery>() };

            var query = new CompiledQuery
            {
                OriginalExpression = expression,
                SlotQueries = new List<SlotQuery>()
            };

            // Process AND operator first (top level)
            if (expression.Contains(" AND "))
            {
                query.LogicalOperator = LogicalOperator.AND;
                query.SubQueries = new List<CompiledQuery>();

                var subExpressions = SplitWithoutBreakingParentheses(expression, " AND ");
                foreach (var subExpression in subExpressions)
                {
                    query.SubQueries.Add(Parse(subExpression));
                }

                return query;
            }

            // Process OR operator
            if (expression.Contains(" OR "))
            {
                query.LogicalOperator = LogicalOperator.OR;
                query.SubQueries = new List<CompiledQuery>();

                var subExpressions = SplitWithoutBreakingParentheses(expression, " OR ");
                foreach (var subExpression in subExpressions)
                {
                    query.SubQueries.Add(Parse(subExpression));
                }

                return query;
            }

            // Process NOT operator
            if (expression.StartsWith("NOT "))
            {
                query.LogicalOperator = LogicalOperator.NOT;
                query.SubQueries = new List<CompiledQuery>
                {
                    Parse(expression.Substring(4))
                };

                return query;
            }

            // Process parentheses
            if (expression.StartsWith("(") && expression.EndsWith(")"))
            {
                // Remove outer parentheses and parse the inner expression
                return Parse(expression.Substring(1, expression.Length - 2));
            }

            // Process single slot query
            try
            {
                var slotQuery = ParseSlotQuery(expression);
                if (slotQuery != null)
                {
                    query.SlotQueries.Add(slotQuery);
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"Error parsing slot query: {expression}", ex);
            }

            return query;
        }

        private SlotQuery ParseSlotQuery(string expression)
        {
            // Define regex patterns for different types of slot queries
            var exactMatchPattern = @"^(\w+)\s+==\s+(.+)$";
            var rangePattern = @"^(\w+)\s+BETWEEN\s+(.+)\s+AND\s+(.+)$";
            var containsPattern = @"^(\w+)\s+CONTAINS\s+(.+)$";
            var greaterThanPattern = @"^(\w+)\s+>\s+(.+)$";
            var lessThanPattern = @"^(\w+)\s+<\s+(.+)$";
            var stringDistancePattern = @"^(\w+)\s+LIKE\s+(.+)$";
            var numericSimilarityPattern = @"^(\w+)\s+SIMILAR\s+TO\s+(.+)$";

            // Check for REQUIRED and WEIGHT modifiers
            bool isRequired = false;
            double weight = 1.0;

            var requiredMatch = Regex.Match(expression, @"REQUIRED\s+");
            if (requiredMatch.Success)
            {
                isRequired = true;
                expression = expression.Replace(requiredMatch.Value, "");
            }

            var weightMatch = Regex.Match(expression, @"WEIGHT\s+([0-9.]+)");
            if (weightMatch.Success)
            {
                if (double.TryParse(weightMatch.Groups[1].Value, out double parsedWeight))
                {
                    weight = parsedWeight;
                }
                expression = expression.Replace(weightMatch.Value, "");
            }

            // Try to match each pattern
            var query = new SlotQuery
            {
                Required = isRequired,
                Weight = weight
            };

            // Parse ExactMatch
            var exactMatch = Regex.Match(expression, exactMatchPattern);
            if (exactMatch.Success)
            {
                query.SlotName = exactMatch.Groups[1].Value;
                query.ComparisonType = ComparisonType.ExactMatch;
                query.Value = ParseValue(exactMatch.Groups[2].Value);
                return query;
            }

            // Parse Range
            var rangeMatch = Regex.Match(expression, rangePattern);
            if (rangeMatch.Success)
            {
                query.SlotName = rangeMatch.Groups[1].Value;
                query.ComparisonType = ComparisonType.Range;
                query.Value = ParseValue(rangeMatch.Groups[2].Value);
                query.Value2 = ParseValue(rangeMatch.Groups[3].Value);
                return query;
            }

            // Parse Contains
            var containsMatch = Regex.Match(expression, containsPattern);
            if (containsMatch.Success)
            {
                query.SlotName = containsMatch.Groups[1].Value;
                query.ComparisonType = ComparisonType.Contains;
                query.Value = ParseValue(containsMatch.Groups[2].Value);
                return query;
            }

            // Parse GreaterThan
            var greaterThanMatch = Regex.Match(expression, greaterThanPattern);
            if (greaterThanMatch.Success)
            {
                query.SlotName = greaterThanMatch.Groups[1].Value;
                query.ComparisonType = ComparisonType.GreaterThan;
                query.Value = ParseValue(greaterThanMatch.Groups[2].Value);
                return query;
            }

            // Parse LessThan
            var lessThanMatch = Regex.Match(expression, lessThanPattern);
            if (lessThanMatch.Success)
            {
                query.SlotName = lessThanMatch.Groups[1].Value;
                query.ComparisonType = ComparisonType.LessThan;
                query.Value = ParseValue(lessThanMatch.Groups[2].Value);
                return query;
            }

            // Parse StringDistance
            var stringDistanceMatch = Regex.Match(expression, stringDistancePattern);
            if (stringDistanceMatch.Success)
            {
                query.SlotName = stringDistanceMatch.Groups[1].Value;
                query.ComparisonType = ComparisonType.StringDistance;
                query.Value = ParseValue(stringDistanceMatch.Groups[2].Value);
                return query;
            }

            // Parse NumericSimilarity
            var numericSimilarityMatch = Regex.Match(expression, numericSimilarityPattern);
            if (numericSimilarityMatch.Success)
            {
                query.SlotName = numericSimilarityMatch.Groups[1].Value;
                query.ComparisonType = ComparisonType.NumericSimilarity;
                query.Value = ParseValue(numericSimilarityMatch.Groups[2].Value);
                return query;
            }

            throw new FormatException($"Could not parse slot query: {expression}");
        }

        private object ParseValue(string value)
        {
            value = value.Trim();

            // Check if it's a parameter
            if (value.StartsWith("@"))
            {
                return value;
            }

            // Check if it's a quoted string
            if ((value.StartsWith("'") && value.EndsWith("'")) ||
                (value.StartsWith("\"") && value.EndsWith("\"")))
            {
                return value.Substring(1, value.Length - 2);
            }

            // Check if it's a number
            if (double.TryParse(value, out double number))
            {
                return number;
            }

            // Check if it's a boolean
            if (bool.TryParse(value, out bool boolean))
            {
                return boolean;
            }

            // Default to string
            return value;
        }

        private string[] SplitWithoutBreakingParentheses(string input, string delimiter)
        {
            var result = new List<string>();
            int nestingLevel = 0;
            int startIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                // Check for opening parenthesis
                if (input[i] == '(')
                {
                    nestingLevel++;
                }
                // Check for closing parenthesis
                else if (input[i] == ')')
                {
                    nestingLevel--;
                }

                // Check for delimiter at current position when not inside parentheses
                if (nestingLevel == 0 && i + delimiter.Length <= input.Length)
                {
                    string potentialDelimiter = input.Substring(i, delimiter.Length);
                    if (potentialDelimiter == delimiter)
                    {
                        // Add segment before delimiter
                        result.Add(input.Substring(startIndex, i - startIndex).Trim());

                        // Move startIndex to after the delimiter
                        startIndex = i + delimiter.Length;

                        // Skip ahead to avoid double-counting
                        i += delimiter.Length - 1;
                    }
                }
            }

            // Add the last segment
            if (startIndex < input.Length)
            {
                result.Add(input.Substring(startIndex).Trim());
            }

            return result.ToArray();
        }
    }

    /// <summary>
    /// Represents a compiled query ready for execution
    /// </summary>
    public class CompiledQuery
    {
        /// <summary>
        /// The original query expression string
        /// </summary>
        public string OriginalExpression { get; set; }

        /// <summary>
        /// The logical operator for combining slot queries or sub-queries
        /// </summary>
        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.AND;

        /// <summary>
        /// The individual slot queries
        /// </summary>
        public List<SlotQuery> SlotQueries { get; set; }

        /// <summary>
        /// Sub-queries for compound expressions
        /// </summary>
        public List<CompiledQuery> SubQueries { get; set; }
    }

    /// <summary>
    /// Represents a query to find chunks with specific criteria
    /// </summary>
    public class ChunkQuery
    {
        /// <summary>
        /// ChunkType must match exactly (required)
        /// </summary>
        public string ChunkType { get; set; }

        /// <summary>
        /// SemanticType must match exactly (required)
        /// </summary>
        public string SemanticType { get; set; }

        /// <summary>
        /// CognitiveCategory must match exactly (required)
        /// </summary>
        public string CognitiveCategory { get; set; }

        /// <summary>
        /// Optional Name to match
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// How to handle Name matching
        /// </summary>
        public NameMatchType NameHandling { get; set; } = NameMatchType.Ignore;

        /// <summary>
        /// Expression string for additional slot criteria
        /// </summary>
        public string SlotQueryExpression { get; set; }

        /// <summary>
        /// Parameter values for the query
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// How to handle extra slots in the chunk
        /// </summary>
        public ExtraSlotsHandling ExtraSlotsHandling { get; set; } = ExtraSlotsHandling.Ignore;

        /// <summary>
        /// Minimum similarity score required for a match (0.0-1.0)
        /// </summary>
        public double MinimumThreshold { get; set; } = 0.6;

        /// <summary>
        /// How to rank matching results
        /// </summary>
        public RankingStrategy RankingStrategy { get; set; } = RankingStrategy.SimilarityScore;

        /// <summary>
        /// Validates the query
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(ChunkType))
                throw new ArgumentException("ChunkType is required");

            // SemanticType and CognitiveCategory might be optional depending on implementation

            if (MinimumThreshold < 0 || MinimumThreshold > 1)
                throw new ArgumentException("MinimumThreshold must be between 0.0 and 1.0");
        }
    }

    /// <summary>
    /// Represents a query for a specific slot
    /// </summary>
    public class SlotQuery
    {
        /// <summary>
        /// Name of the slot to query
        /// </summary>
        public string SlotName { get; set; }

        /// <summary>
        /// Type of comparison to perform
        /// </summary>
        public ComparisonType ComparisonType { get; set; } = ComparisonType.ExactMatch;

        /// <summary>
        /// Primary value for comparison
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Secondary value for comparison (e.g., max value for range)
        /// </summary>
        public object Value2 { get; set; }

        /// <summary>
        /// Whether this slot is required for a match
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Weight of this slot in the overall similarity calculation (0.0-1.0)
        /// </summary>
        public double Weight { get; set; } = 1.0;

        /// <summary>
        /// Optional custom comparison function
        /// </summary>
        public Func<object, object, double> CustomComparer { get; set; }
    }

    /// <summary>
    /// Result of a chunk query
    /// </summary>
    public class ChunkQueryResult
    {
        /// <summary>
        /// The matching chunk
        /// </summary>
        public Chunk Chunk { get; set; }

        /// <summary>
        /// Overall similarity score (0.0-1.0)
        /// </summary>
        public double SimilarityScore { get; set; }

        /// <summary>
        /// Whether all slots matched exactly
        /// </summary>
        public bool ExactMatch { get; set; }

        /// <summary>
        /// Individual slot match results
        /// </summary>
        public Dictionary<string, SlotMatchResult> SlotMatches { get; set; }
    }

    /// <summary>
    /// Result of matching a single slot
    /// </summary>
    public class SlotMatchResult
    {
        /// <summary>
        /// Name of the slot
        /// </summary>
        public string SlotName { get; set; }

        /// <summary>
        /// Whether the slot exists in the chunk
        /// </summary>
        public bool SlotExists { get; set; }

        /// <summary>
        /// The actual value in the chunk
        /// </summary>
        public object ActualValue { get; set; }

        /// <summary>
        /// Similarity score for this slot (0.0-1.0)
        /// </summary>
        public double SimilarityScore { get; set; }
    }

    /// <summary>
    /// How to handle the Name property in queries
    /// </summary>
    public enum NameMatchType
    {
        /// <summary>
        /// Ignore the Name property
        /// </summary>
        Ignore,

        /// <summary>
        /// Name must match exactly
        /// </summary>
        ExactMatch,

        /// <summary>
        /// Name must contain the query text
        /// </summary>
        Contains,

        /// <summary>
        /// Name is compared using string distance algorithm
        /// </summary>
        StringDistance
    }

    /// <summary>
    /// How to handle extra slots in the chunk
    /// </summary>
    public enum ExtraSlotsHandling
    {
        /// <summary>
        /// Ignore extra slots
        /// </summary>
        Ignore,

        /// <summary>
        /// No extra slots allowed
        /// </summary>
        NoExtraAllowed,

        /// <summary>
        /// Apply a penalty for extra slots
        /// </summary>
        Penalty
    }

    /// <summary>
    /// Types of slot value comparison
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// Values must match exactly
        /// </summary>
        ExactMatch,

        /// <summary>
        /// Value must be in the specified range
        /// </summary>
        Range,

        /// <summary>
        /// String must contain the specified substring
        /// </summary>
        Contains,

        /// <summary>
        /// Compare strings using string distance algorithm
        /// </summary>
        StringDistance,

        /// <summary>
        /// Value must be greater than the specified value
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Value must be less than the specified value
        /// </summary>
        LessThan,

        /// <summary>
        /// Compare numeric values for similarity
        /// </summary>
        NumericSimilarity
    }

    /// <summary>
    /// Logical operators for compound queries
    /// </summary>
    public enum LogicalOperator
    {
        /// <summary>
        /// All parts must match
        /// </summary>
        AND,

        /// <summary>
        /// At least one part must match
        /// </summary>
        OR,

        /// <summary>
        /// Negate the match
        /// </summary>
        NOT
    }

    /// <summary>
    /// Strategies for ranking query results
    /// </summary>
    public enum RankingStrategy
    {
        /// <summary>
        /// Rank by similarity score only
        /// </summary>
        SimilarityScore,

        /// <summary>
        /// Rank exact matches first, then by similarity score
        /// </summary>
        ExactMatchFirst
    }
}