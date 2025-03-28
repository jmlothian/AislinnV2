using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aislinn.Core.Models;
using Aislinn.Core.Relationships;

namespace Aislinn.Core.Goals
{
    // Extension methods for GoalManagementService to integrate relationship matching
    public static class GoalManagementServiceExtensions
    {
        // Regular expression for matching relationship patterns in expressions
        private static readonly Regex RelationshipPatternRegex = new Regex(
            @"(\w+)\s+(\w+)\s+(\w+)",
            RegexOptions.Compiled);

        /// <summary>
        /// Evaluates a criteria expression that may contain relationship patterns
        /// </summary>
        public static async Task<bool> EvaluateExpressionWithRelationshipsAsync(
            this GoalManagementService goalService,
            string expression,
            Chunk goal,
            GoalRelationshipMatcher relationshipMatcher)
        {
            // Handle OR expressions first
            if (expression.Contains(" OR "))
            {
                var orExpressions = expression.Split(new[] { " OR " }, StringSplitOptions.None);

                foreach (var orExpression in orExpressions)
                {
                    if (await goalService.EvaluateExpressionWithRelationshipsAsync(
                        orExpression.Trim(), goal, relationshipMatcher))
                    {
                        return true;
                    }
                }

                return false;
            }

            // Process AND expressions
            var conditions = expression.Split(new[] { " AND " }, StringSplitOptions.None);

            foreach (var condition in conditions)
            {
                string trimmedCondition = condition.Trim();

                // Check if this is a relationship pattern
                var match = RelationshipPatternRegex.Match(trimmedCondition);
                if (match.Success)
                {
                    // Evaluate the relationship pattern
                    bool relationshipResult = await relationshipMatcher.EvaluateRelationshipPatternAsync(
                        trimmedCondition, goal);

                    if (!relationshipResult)
                        return false;
                }
                else
                {
                    // Handle standard condition using the original expression evaluator
                    // This assumes access to the original EvaluateExpressionCriteriaAsync logic
                    bool standardResult = await EvaluateStandardConditionAsync(goalService, trimmedCondition, goal);

                    if (!standardResult)
                        return false;
                }
            }

            return true; // All conditions passed
        }

        /// <summary>
        /// Example wrapper method for evaluating standard conditions
        /// This would need to call into your existing expression evaluation logic
        /// </summary>
        private static async Task<bool> EvaluateStandardConditionAsync(
            GoalManagementService goalService,
            string condition,
            Chunk goal)
        {
            // This would call into your existing implementation for standard expressions
            // For demonstration purposes, we're showing a simple implementation

            // Handle different comparison operators
            if (condition.Contains(">="))
            {
                var parts = condition.Split(new[] { ">=" }, StringSplitOptions.None);
                if (parts.Length != 2) return false;

                string slotName = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!goal.Slots.TryGetValue(slotName, out var slot) || slot.Value == null)
                    return false;

                // Try to convert both to double for comparison
                if (!TryConvertToDouble(slot.Value, out double slotValue) ||
                    !TryConvertToDouble(valueStr, out double comparisonValue) ||
                    !(slotValue >= comparisonValue))
                {
                    return false;
                }
            }
            else if (condition.Contains("<="))
            {
                // Implement other operators as needed
                // Similar to existing implementation
                return false;
            }
            else if (condition.Contains("=="))
            {
                var parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
                if (parts.Length != 2) return false;

                string slotName = parts[0].Trim();
                string valueStr = parts[1].Trim();

                if (!goal.Slots.TryGetValue(slotName, out var slot) || slot.Value == null)
                    return false;

                // Handle string comparison with quoted strings
                if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
                {
                    string stringValue = valueStr.Substring(1, valueStr.Length - 2); // Remove quotes
                    return slot.Value.ToString() == stringValue;
                }
                // Handle boolean
                else if (valueStr.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         valueStr.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    bool boolValue = valueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
                    return slot.Value is bool slotBool && slotBool == boolValue;
                }
                // Handle numeric comparison
                else if (TryConvertToDouble(valueStr, out double numericValue) &&
                         TryConvertToDouble(slot.Value, out double slotNumericValue))
                {
                    return Math.Abs(slotNumericValue - numericValue) < 0.00001;
                }
                else
                {
                    // Default string comparison
                    return slot.Value.ToString() == valueStr;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper to try converting an object to a double
        /// </summary>
        private static bool TryConvertToDouble(object value, out double result)
        {
            try
            {
                if (value is double doubleValue)
                {
                    result = doubleValue;
                    return true;
                }

                if (value is int intValue)
                {
                    result = intValue;
                    return true;
                }

                if (value is string stringValue)
                {
                    return double.TryParse(stringValue, out result);
                }

                result = 0;
                return false;
            }
            catch
            {
                result = 0;
                return false;
            }
        }
    }
}