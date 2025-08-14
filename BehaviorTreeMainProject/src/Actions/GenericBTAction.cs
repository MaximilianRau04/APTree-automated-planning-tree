using System;
using System.Linq;
using System.Collections.Generic;


// Generic action class that will be created by the factory
public abstract class GenericBTAction : BTActionNodeBase
{
    private readonly FastName actionType;  
    private State preconditions;
    private State effects;
    private readonly Blackboard<FastName> blackboard;

    
    // Predicate templates for automatic instantiation (as strings)
    protected abstract List<string> PreconditionTemplates { get; }
    protected abstract List<string> EffectTemplates { get; }

    public override string DebugDisplayName 
    { 
        get => debugDisplayName;
        protected set => debugDisplayName = value;
    }

    public GenericBTAction(
        string instanceName,
        Blackboard<FastName> blackboard,
        State preconditions,
        State effects
    ) : base(blackboard, instanceName)
    {
        this.blackboard = blackboard;
        this.preconditions = preconditions;
        this.effects = effects;
    }

    // Constructor with automatic predicate instantiation
    public GenericBTAction(
        string actionType,
        string instanceName,
        Blackboard<FastName> blackboard
    ) : base(blackboard, instanceName)
    {
        this.actionType = new FastName(actionType);
        this.blackboard = blackboard;
        
        // Note: Predicates will be instantiated after properties are set
        // This is done in StorePredicatesInBlackboard() called by FactoryAction
    }


    public void applyEffects()
    {
        // foreach (var effect in effects)
        // {
        //     blackboard.SetValue(effect.Key, effect.Value);
        // }
    }

    // Store all predicates (preconditions and effects) in the blackboard
    public void StorePredicatesInBlackboard()
    {
        try
        {
            // Instantiate predicates if they haven't been instantiated yet
            if (preconditions == null)
            {
                this.preconditions = InstantiatePredicates(PreconditionTemplates, StateType.Precondition, actionType.ToString());
            }
            if (effects == null)
            {
                this.effects = InstantiatePredicates(EffectTemplates, StateType.Effect, actionType.ToString());
            }

            // Store precondition predicates
            if (preconditions != null)
            {
                foreach (var objectKey in preconditions.GetAllObjects())
                {
                    var predicates = preconditions.GetPredicates(objectKey);
                    foreach (var predicate in predicates)
                    {
                        blackboard.SetPredicate(predicate.PredicateName, predicate);
                        Console.WriteLine($"Stored precondition predicate: {predicate.PredicateName}");
                    }
                }
            }

            // Store effect predicates
            if (effects != null)
            {
                foreach (var objectKey in effects.GetAllObjects())
                {
                    var predicates = effects.GetPredicates(objectKey);
                    foreach (var predicate in predicates)
                    {
                        blackboard.SetPredicate(predicate.PredicateName, predicate);
                        Console.WriteLine($"Stored effect predicate: {predicate.PredicateName}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Warning: Failed to store predicates in blackboard: {e.Message}");
        }
    }


    protected abstract override bool OnTick_NodeLogic(float InDeltaTime);

    // Helper method to instantiate predicates from string templates
    private State InstantiatePredicates(List<string> templates, StateType stateType, string actionTypeName)
    {
        var state = new State(stateType, new FastName(actionTypeName));
        
        Console.WriteLine($"Instantiating predicates for {stateType} state with {templates.Count} templates");
        
        foreach (var templateString in templates)
        {
            try
            {
                Console.WriteLine($"Processing template: {templateString}");
                var predicate = InstantiatePredicateFromString(templateString);
                if (predicate != null)
                {
                    // Create a unique key for the predicate based on its name and parameters
                    var predicateKey = CreatePredicateKey(predicate, templateString);
                    state.AddPredicate(predicateKey, predicate);
                    Console.WriteLine($"Successfully added predicate: {predicateKey}");
                }
                else
                {
                    Console.WriteLine($"Failed to instantiate predicate from template: {templateString}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: Failed to instantiate predicate from template '{templateString}': {e.Message}");
                Console.WriteLine($"Stack trace: {e.StackTrace}");
            }
        }
        
        return state;
    }
    
    // Helper method to create a unique key for a predicate
    private FastName CreatePredicateKey(Predicate predicate, string templateString)
    {
        // Extract predicate name from template
        var predicateName = ExtractPredicateName(templateString);
        
        // Create a unique key that includes the predicate name and some identifier
        var uniqueKey = $"{predicateName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        return new FastName(uniqueKey);
    }

    // Helper method to instantiate a single predicate from string template
    private Predicate InstantiatePredicateFromString(string templateString)
    {
        // Parse the template string: "PredicateInstance: predicateName(arg1 = value1, arg2 = value2, isNegated = false)"
        var predicateParams = ParsePredicateTemplateString(templateString);
        
        // Extract predicate name
        var predicateName = ExtractPredicateName(templateString);
        
        // Substitute parameter values with actual property values
        var substitutedParams = SubstituteParameterValues(predicateParams);
        
        // Create predicate using FactoryPredicate
        return FactoryPredicate.Instance.CreatePredicateInstance(predicateName, ConvertToParameterMappingList(substitutedParams), blackboard);
    }
    
    // Helper method to substitute parameter values with actual property values
    private Dictionary<string, string> SubstituteParameterValues(Dictionary<string, string> predicateParams)
    {
        var substitutedParams = new Dictionary<string, string>();
        
        Console.WriteLine($"Substituting parameters: {string.Join(", ", predicateParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        
        foreach (var kvp in predicateParams)
        {
            var paramName = kvp.Key;
            var paramValue = kvp.Value;
            
            // Check if the parameter value is a property name that should be substituted
            var propertyValue = GetParameterValue(paramValue);
            if (propertyValue != null)
            {
                // Use the actual property value
                substitutedParams[paramName] = propertyValue.ToString();
                Console.WriteLine($"Substituted parameter {paramName}: {paramValue} -> {propertyValue}");
            }
            else
            {
                // Keep the original value (might be a direct value like "true", "false", etc.)
                substitutedParams[paramName] = paramValue;
                Console.WriteLine($"Kept parameter {paramName}: {paramValue}");
            }
        }
        
        Console.WriteLine($"Final substituted parameters: {string.Join(", ", substitutedParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        return substitutedParams;
    }
    
    // Helper method to parse predicate template string
    private Dictionary<string, string> ParsePredicateTemplateString(string templateString)
    {
        var predicateParams = new Dictionary<string, string>();
        
        try
        {
            // Extract the part between parentheses
            var startIndex = templateString.IndexOf('(');
            var endIndex = templateString.LastIndexOf(')');
            
            if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
            {
                var argsString = templateString.Substring(startIndex + 1, endIndex - startIndex - 1);
                var args = argsString.Split(',');
                
                foreach (var arg in args)
                {
                    var trimmedArg = arg.Trim();
                    if (trimmedArg.Contains("="))
                    {
                        var parts = trimmedArg.Split('=');
                        if (parts.Length == 2)
                        {
                            var paramName = parts[0].Trim();
                            var paramValue = parts[1].Trim();
                            
                            // Skip isNegated parameter as it's handled separately
                            if (paramName != "isNegated")
                            {
                                predicateParams[paramName] = paramValue;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Warning: Failed to parse predicate template string '{templateString}': {e.Message}");
        }
        
        return predicateParams;
    }
    
    // Helper method to extract predicate name from template string
    private string ExtractPredicateName(string templateString)
    {
        try
        {
            // Format: "PredicateInstance: predicateName(...)"
            var colonIndex = templateString.IndexOf(':');
            var parenIndex = templateString.IndexOf('(');
            
            if (colonIndex >= 0 && parenIndex >= 0 && parenIndex > colonIndex)
            {
                return templateString.Substring(colonIndex + 1, parenIndex - colonIndex - 1).Trim();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Warning: Failed to extract predicate name from '{templateString}': {e.Message}");
        }
        
        return "unknown";
    }
    
    /// <summary>
    /// Converts a Dictionary<string, string> to a List<ParameterMapping>
    /// </summary>
    /// <param name="parameters">The parameters dictionary to convert</param>
    /// <returns>List of ParameterMapping</returns>
    private List<ParameterMapping> ConvertToParameterMappingList(Dictionary<string, string> parameters)
    {
        var parameterMappings = new List<ParameterMapping>();
        foreach (var kvp in parameters)
        {
            parameterMappings.Add(new ParameterMapping(kvp.Key, kvp.Value));
        }
        return parameterMappings;
    }
    
    // Helper method to get parameter value by name using reflection
    private object GetParameterValue(string parameterName)
    {
        try
        {
            // Use reflection to get the property value by name
            var property = this.GetType().GetProperty(parameterName);
            if (property != null)
            {
                var value = property.GetValue(this);
                Console.WriteLine($"Found property {parameterName}: {value}");
                return value;
            }
            
            // Also check for fields (in case parameters are stored as fields)
            var field = this.GetType().GetField(parameterName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                var value = field.GetValue(this);
                Console.WriteLine($"Found field {parameterName}: {value}");
                return value;
            }
            
            Console.WriteLine($"Property/field not found: {parameterName}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting parameter value for {parameterName}: {ex.Message}");
            return null;
        }
    }
}
