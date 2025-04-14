using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aislinn.Core.Procedural
{
    /// <summary>
    /// Represents a condition that can be evaluated during procedure execution
    /// </summary>
    public class ProcedureCondition
    {
        /// <summary>
        /// Types of conditions
        /// </summary>
        public enum ConditionType
        {
            Comparison,     // Compare values (==, !=, >, <, etc.)
            Existence,      // Check if a value exists
            TypeCheck,      // Check type of a value
            Conjunction,    // AND of multiple conditions
            Disjunction,    // OR of multiple conditions
            Negation,       // NOT of a condition
            Custom          // Custom evaluated condition
        }

        /// <summary>
        /// Type of this condition
        /// </summary>
        public ConditionType Type { get; set; } = ConditionType.Comparison;

        /// <summary>
        /// For Comparison: left side of comparison
        /// For Existence/TypeCheck: target value reference
        /// For Custom: expression to evaluate
        /// </summary>
        public string LeftOperand { get; set; }

        /// <summary>
        /// For Comparison: comparison operator (==, !=, &gt;, &lt;, etc.)
        /// For TypeCheck: expected type name
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// For Comparison: right side of comparison
        /// </summary>
        public object RightOperand { get; set; }

        /// <summary>
        /// For Conjunction/Disjunction: subconditions
        /// </summary>
        public List<ProcedureCondition> Subconditions { get; set; } = new List<ProcedureCondition>();

        /// <summary>
        /// For Negation: the condition to negate
        /// </summary>
        public ProcedureCondition NegatableCondition { get; set; }

        /// <summary>
        /// Additional parameters for condition evaluation
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a simple comparison condition
        /// </summary>
        public static ProcedureCondition CreateComparison(string left, string op, object right)
        {
            return new ProcedureCondition
            {
                Type = ConditionType.Comparison,
                LeftOperand = left,
                Operator = op,
                RightOperand = right
            };
        }

        /// <summary>
        /// Creates an existence check condition
        /// </summary>
        public static ProcedureCondition CreateExistenceCheck(string target, bool shouldExist = true)
        {
            return new ProcedureCondition
            {
                Type = ConditionType.Existence,
                LeftOperand = target,
                Operator = shouldExist ? "exists" : "notexists"
            };
        }

        /// <summary>
        /// Creates a type check condition
        /// </summary>
        public static ProcedureCondition CreateTypeCheck(string target, string expectedType)
        {
            return new ProcedureCondition
            {
                Type = ConditionType.TypeCheck,
                LeftOperand = target,
                Operator = "typeof",
                RightOperand = expectedType
            };
        }

        /// <summary>
        /// Creates an AND condition of multiple subconditions
        /// </summary>
        public static ProcedureCondition CreateConjunction(params ProcedureCondition[] conditions)
        {
            return new ProcedureCondition
            {
                Type = ConditionType.Conjunction,
                Subconditions = conditions.ToList()
            };
        }

        /// <summary>
        /// Creates an OR condition of multiple subconditions
        /// </summary>
        public static ProcedureCondition CreateDisjunction(params ProcedureCondition[] conditions)
        {
            return new ProcedureCondition
            {
                Type = ConditionType.Disjunction,
                Subconditions = conditions.ToList()
            };
        }

        /// <summary>
        /// Creates a NOT condition
        /// </summary>
        public static ProcedureCondition CreateNegation(ProcedureCondition condition)
        {
            return new ProcedureCondition
            {
                Type = ConditionType.Negation,
                NegatableCondition = condition
            };
        }

        /// <summary>
        /// Evaluates this condition against the given context
        /// </summary>
        public bool Evaluate(Dictionary<string, object> context)
        {
            switch (Type)
            {
                case ConditionType.Comparison:
                    return EvaluateComparison(context);

                case ConditionType.Existence:
                    return EvaluateExistence(context);

                case ConditionType.TypeCheck:
                    return EvaluateTypeCheck(context);

                case ConditionType.Conjunction:
                    return EvaluateConjunction(context);

                case ConditionType.Disjunction:
                    return EvaluateDisjunction(context);

                case ConditionType.Negation:
                    return !NegatableCondition.Evaluate(context);

                case ConditionType.Custom:
                    return EvaluateCustom(context);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Evaluates a comparison condition
        /// </summary>
        private bool EvaluateComparison(Dictionary<string, object> context)
        {
            object leftValue = ResolveOperand(LeftOperand, context);
            object rightValue = ResolveOperand(RightOperand, context);

            switch (Operator)
            {
                case "==":
                    return ObjectEquals(leftValue, rightValue);

                case "!=":
                    return !ObjectEquals(leftValue, rightValue);

                case ">":
                    return CompareValues(leftValue, rightValue) > 0;

                case ">=":
                    return CompareValues(leftValue, rightValue) >= 0;

                case "<":
                    return CompareValues(leftValue, rightValue) < 0;

                case "<=":
                    return CompareValues(leftValue, rightValue) <= 0;

                case "contains":
                    return ContainsValue(leftValue, rightValue);

                case "startswith":
                    return StartsWithValue(leftValue, rightValue);

                case "endswith":
                    return EndsWithValue(leftValue, rightValue);

                case "matches":
                    return MatchesPattern(leftValue, rightValue);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Evaluates an existence check condition
        /// </summary>
        private bool EvaluateExistence(Dictionary<string, object> context)
        {
            bool checkExists = Operator == "exists";
            bool exists = context.ContainsKey(LeftOperand) && context[LeftOperand] != null;
            return checkExists ? exists : !exists;
        }

        /// <summary>
        /// Evaluates a type check condition
        /// </summary>
        private bool EvaluateTypeCheck(Dictionary<string, object> context)
        {
            if (!context.ContainsKey(LeftOperand) || context[LeftOperand] == null)
                return false;

            object value = context[LeftOperand];
            string expectedType = RightOperand?.ToString();

            if (string.IsNullOrEmpty(expectedType))
                return false;

            string actualType = value.GetType().Name;

            return string.Equals(actualType, expectedType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Evaluates an AND condition
        /// </summary>
        private bool EvaluateConjunction(Dictionary<string, object> context)
        {
            if (Subconditions == null || Subconditions.Count == 0)
                return true;

            foreach (var condition in Subconditions)
            {
                if (!condition.Evaluate(context))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Evaluates an OR condition
        /// </summary>
        private bool EvaluateDisjunction(Dictionary<string, object> context)
        {
            if (Subconditions == null || Subconditions.Count == 0)
                return false;

            foreach (var condition in Subconditions)
            {
                if (condition.Evaluate(context))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Evaluates a custom condition
        /// </summary>
        private bool EvaluateCustom(Dictionary<string, object> context)
        {
            // Custom evaluation logic would go here
            // For now, just return false
            return false;
        }

        /// <summary>
        /// Resolves an operand value from context
        /// </summary>
        private object ResolveOperand(object operand, Dictionary<string, object> context)
        {
            if (operand == null)
                return null;

            // If operand is a string and looks like a variable reference
            if (operand is string stringOperand && stringOperand.StartsWith("$"))
            {
                string variableName = stringOperand.Substring(1);
                return context.ContainsKey(variableName) ? context[variableName] : null;
            }

            return operand;
        }

        /// <summary>
        /// Compares two objects for equality
        /// </summary>
        private bool ObjectEquals(object a, object b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            // Handle numeric comparison with tolerance
            if (a is IConvertible && b is IConvertible)
            {
                try
                {
                    double aValue = Convert.ToDouble(a);
                    double bValue = Convert.ToDouble(b);
                    return Math.Abs(aValue - bValue) < 0.000001;
                }
                catch { }
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Compares two objects and returns -1, 0, or 1
        /// </summary>
        private int CompareValues(object a, object b)
        {
            if (a == null && b == null)
                return 0;

            if (a == null)
                return -1;

            if (b == null)
                return 1;

            // Compare numbers
            if (a is IConvertible && b is IConvertible)
            {
                try
                {
                    double aValue = Convert.ToDouble(a);
                    double bValue = Convert.ToDouble(b);
                    return aValue.CompareTo(bValue);
                }
                catch { }
            }

            // Compare strings
            if (a is string aString && b is string bString)
            {
                return string.Compare(aString, bString, StringComparison.OrdinalIgnoreCase);
            }

            // Try using IComparable if available
            if (a is IComparable comparable)
            {
                return comparable.CompareTo(b);
            }

            // Default to string representation comparison
            return a.ToString().CompareTo(b.ToString());
        }

        /// <summary>
        /// Checks if one value contains another
        /// </summary>
        private bool ContainsValue(object container, object item)
        {
            if (container == null || item == null)
                return false;

            // String contains
            if (container is string containerStr && item is string itemStr)
            {
                return containerStr.Contains(itemStr, StringComparison.OrdinalIgnoreCase);
            }

            // List/array contains
            if (container is System.Collections.IEnumerable enumerable)
            {
                foreach (var element in enumerable)
                {
                    if (ObjectEquals(element, item))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a string starts with a value
        /// </summary>
        private bool StartsWithValue(object value, object prefix)
        {
            if (value == null || prefix == null)
                return false;

            string valueStr = value.ToString();
            string prefixStr = prefix.ToString();

            return valueStr.StartsWith(prefixStr, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a string ends with a value
        /// </summary>
        private bool EndsWithValue(object value, object suffix)
        {
            if (value == null || suffix == null)
                return false;

            string valueStr = value.ToString();
            string suffixStr = suffix.ToString();

            return valueStr.EndsWith(suffixStr, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a string matches a regex pattern
        /// </summary>
        private bool MatchesPattern(object value, object pattern)
        {
            if (value == null || pattern == null)
                return false;

            string valueStr = value.ToString();
            string patternStr = pattern.ToString();

            try
            {
                return Regex.IsMatch(valueStr, patternStr);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a clone of this condition
        /// </summary>
        public ProcedureCondition Clone()
        {
            var clone = new ProcedureCondition
            {
                Type = Type,
                LeftOperand = LeftOperand,
                Operator = Operator,
                RightOperand = RightOperand,
                Parameters = new Dictionary<string, object>(Parameters)
            };

            // Deep copy subconditions for conjunction/disjunction
            if (Subconditions != null)
            {
                clone.Subconditions = new List<ProcedureCondition>();
                foreach (var subcondition in Subconditions)
                {
                    clone.Subconditions.Add(subcondition.Clone());
                }
            }

            // Deep copy negatable condition if present
            if (NegatableCondition != null)
            {
                clone.NegatableCondition = NegatableCondition.Clone();
            }

            return clone;
        }
    }

    /// <summary>
    /// Represents an effect that occurs when a procedure step is executed
    /// </summary>
    public class ProcedureEffect
    {
        /// <summary>
        /// Types of effects
        /// </summary>
        public enum EffectType
        {
            Assignment,     // Assign a value to a variable
            Modification,   // Modify an existing value
            Creation,       // Create a new entity
            Deletion,       // Delete an entity
            Custom          // Custom effect logic
        }

        /// <summary>
        /// Type of this effect
        /// </summary>
        public EffectType Type { get; set; } = EffectType.Assignment;

        /// <summary>
        /// Target of the effect (variable name, entity reference, etc.)
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Operation to perform (=, +=, -=, etc. for modifications)
        /// </summary>
        public string Operation { get; set; } = "=";

        /// <summary>
        /// Value to apply in the effect
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Additional parameters for effect application
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a simple assignment effect
        /// </summary>
        public static ProcedureEffect CreateAssignment(string target, object value)
        {
            return new ProcedureEffect
            {
                Type = EffectType.Assignment,
                Target = target,
                Operation = "=",
                Value = value
            };
        }

        /// <summary>
        /// Creates a modification effect
        /// </summary>
        public static ProcedureEffect CreateModification(string target, string operation, object value)
        {
            return new ProcedureEffect
            {
                Type = EffectType.Modification,
                Target = target,
                Operation = operation,
                Value = value
            };
        }

        /// <summary>
        /// Creates a creation effect
        /// </summary>
        public static ProcedureEffect CreateCreation(string entityType, string target, object properties)
        {
            return new ProcedureEffect
            {
                Type = EffectType.Creation,
                Target = target,
                Operation = "create",
                Value = properties,
                Parameters = new Dictionary<string, object> { { "EntityType", entityType } }
            };
        }

        /// <summary>
        /// Creates a deletion effect
        /// </summary>
        public static ProcedureEffect CreateDeletion(string target)
        {
            return new ProcedureEffect
            {
                Type = EffectType.Deletion,
                Target = target,
                Operation = "delete"
            };
        }

        /// <summary>
        /// Applies this effect to the given context
        /// </summary>
        public void Apply(Dictionary<string, object> context)
        {
            switch (Type)
            {
                case EffectType.Assignment:
                    ApplyAssignment(context);
                    break;

                case EffectType.Modification:
                    ApplyModification(context);
                    break;

                case EffectType.Creation:
                    ApplyCreation(context);
                    break;

                case EffectType.Deletion:
                    ApplyDeletion(context);
                    break;

                case EffectType.Custom:
                    ApplyCustom(context);
                    break;
            }
        }

        /// <summary>
        /// Applies an assignment effect
        /// </summary>
        private void ApplyAssignment(Dictionary<string, object> context)
        {
            object resolvedValue = ResolveValue(Value, context);
            context[Target] = resolvedValue;
        }

        /// <summary>
        /// Applies a modification effect
        /// </summary>
        private void ApplyModification(Dictionary<string, object> context)
        {
            if (!context.ContainsKey(Target))
                return;

            object currentValue = context[Target];
            object newValue = ResolveValue(Value, context);

            switch (Operation)
            {
                case "+=":
                    context[Target] = Add(currentValue, newValue);
                    break;

                case "-=":
                    context[Target] = Subtract(currentValue, newValue);
                    break;

                case "*=":
                    context[Target] = Multiply(currentValue, newValue);
                    break;

                case "/=":
                    context[Target] = Divide(currentValue, newValue);
                    break;

                default:
                    // Unknown operation, default to assignment
                    context[Target] = newValue;
                    break;
            }
        }

        /// <summary>
        /// Applies a creation effect
        /// </summary>
        private void ApplyCreation(Dictionary<string, object> context)
        {
            // In a real implementation, this would create a new entity
            // For now, just store the properties in the context
            string entityType = Parameters.ContainsKey("EntityType")
                ? Parameters["EntityType"].ToString()
                : "GenericEntity";

            Dictionary<string, object> properties;

            if (Value is Dictionary<string, object> valueDict)
            {
                properties = new Dictionary<string, object>(valueDict);
            }
            else
            {
                properties = new Dictionary<string, object>();
                if (Value != null)
                {
                    properties["Value"] = Value;
                }
            }

            // Add entity type
            properties["EntityType"] = entityType;

            // Store in context
            context[Target] = properties;
        }

        /// <summary>
        /// Applies a deletion effect
        /// </summary>
        private void ApplyDeletion(Dictionary<string, object> context)
        {
            if (context.ContainsKey(Target))
            {
                context.Remove(Target);
            }
        }

        /// <summary>
        /// Applies a custom effect
        /// </summary>
        private void ApplyCustom(Dictionary<string, object> context)
        {
            // Custom effect logic would go here
            // Not implemented in this example
        }

        /// <summary>
        /// Resolves a value from context if it's a variable reference
        /// </summary>
        private object ResolveValue(object value, Dictionary<string, object> context)
        {
            if (value == null)
                return null;

            // If value is a string and looks like a variable reference
            if (value is string stringValue && stringValue.StartsWith("$"))
            {
                string variableName = stringValue.Substring(1);
                return context.ContainsKey(variableName) ? context[variableName] : null;
            }

            return value;
        }

        /// <summary>
        /// Adds two values
        /// </summary>
        private object Add(object a, object b)
        {
            if (a == null) return b;
            if (b == null) return a;

            // Number addition
            if (a is IConvertible && b is IConvertible)
            {
                try
                {
                    double aValue = Convert.ToDouble(a);
                    double bValue = Convert.ToDouble(b);
                    return aValue + bValue;
                }
                catch { }
            }

            // String concatenation
            return a.ToString() + b.ToString();
        }

        /// <summary>
        /// Subtracts two values
        /// </summary>
        private object Subtract(object a, object b)
        {
            if (a == null || b == null)
                return a;

            // Number subtraction
            if (a is IConvertible && b is IConvertible)
            {
                try
                {
                    double aValue = Convert.ToDouble(a);
                    double bValue = Convert.ToDouble(b);
                    return aValue - bValue;
                }
                catch { }
            }

            // Default to original value
            return a;
        }

        /// <summary>
        /// Multiplies two values
        /// </summary>
        private object Multiply(object a, object b)
        {
            if (a == null || b == null)
                return a;

            // Number multiplication
            if (a is IConvertible && b is IConvertible)
            {
                try
                {
                    double aValue = Convert.ToDouble(a);
                    double bValue = Convert.ToDouble(b);
                    return aValue * bValue;
                }
                catch { }
            }

            // Default to original value
            return a;
        }

        /// <summary>
        /// Divides two values
        /// </summary>
        private object Divide(object a, object b)
        {
            if (a == null || b == null)
                return a;

            // Number division
            if (a is IConvertible && b is IConvertible)
            {
                try
                {
                    double aValue = Convert.ToDouble(a);
                    double bValue = Convert.ToDouble(b);

                    // Avoid division by zero
                    if (Math.Abs(bValue) < 0.000001)
                        return a;

                    return aValue / bValue;
                }
                catch { }
            }

            // Default to original value
            return a;
        }

        /// <summary>
        /// Creates a clone of this effect
        /// </summary>
        public ProcedureEffect Clone()
        {
            return new ProcedureEffect
            {
                Type = Type,
                Target = Target,
                Operation = Operation,
                Value = Value,
                Parameters = new Dictionary<string, object>(Parameters)
            };
        }
    }
}