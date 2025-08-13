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

    
    // Predicate templates for automatic instantiation
    protected abstract List<ActionPredicateTemplate> PreconditionTemplates { get; }
    protected abstract List<ActionPredicateTemplate> EffectTemplates { get; }

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

    // Helper method to instantiate predicates from templates
    private State InstantiatePredicates(List<ActionPredicateTemplate> templates, StateType stateType, string actionTypeName)
    {
        var state = new State(stateType, new FastName(actionTypeName));
        
        foreach (var template in templates)
        {
            try
            {
                var predicate = InstantiatePredicate(template);
                if (predicate != null)
                {
                    state.AddPredicate(new FastName(template.PredicateName), predicate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: Failed to instantiate predicate {template.PredicateName}: {e.Message}");
            }
        }
        
        return state;
    }

    // Helper method to instantiate a single predicate
    private Predicate InstantiatePredicate(ActionPredicateTemplate template)
    {
        var predicateParams = new Dictionary<string, string>();
        
        foreach (var mapping in template.ParameterMappings)
        {
            // Get the parameter value from the action's parameter properties
            object parameterValue = GetParameterValue(mapping.ActionParameterName);
            if (parameterValue == null)
            {
                throw new ArgumentException($"Action parameter '{mapping.ActionParameterName}' not found for predicate '{template.PredicateName}'");
            }
            
            predicateParams[mapping.PredicateParameterName] = parameterValue.ToString();
        }
        
        // Create predicate using PredicateFactory
        return FactoryPredicate.Instance.CreatePredicateInstance(template.PredicateName, ConvertToParameterMappingList(predicateParams), blackboard);
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
        // Use reflection to get the property value by name
        var property = this.GetType().GetProperty(parameterName);
        if (property != null)
        {
            return property.GetValue(this);
        }
        
        // If property not found, return null (will be handled by caller)
        return null;
    }
}
