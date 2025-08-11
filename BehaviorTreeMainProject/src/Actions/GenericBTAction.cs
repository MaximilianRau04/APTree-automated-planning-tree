using System;
using System.Linq;
using System.Collections.Generic;


// Generic action class that will be created by the factory
public abstract class GenericBTAction : BTActionNodeBase
{
    private readonly FastName actionType;  
    private readonly object[] parameterValues;
    private readonly List<Parameter> parameters;
    private readonly State preconditions;
    private readonly State effects;
    private string debugDisplayName;
    
    // Predicate templates for automatic instantiation
    protected abstract List<ActionPredicateTemplate> PreconditionTemplates { get; }
    protected abstract List<ActionPredicateTemplate> EffectTemplates { get; }

    public override string DebugDisplayName 
    { 
        get => debugDisplayName;
        protected set => debugDisplayName = value;
    }

    public GenericBTAction(
        string actionType,
        string instanceName,
        Blackboard<FastName> blackboard,
        State preconditions,
        State effects,
        List<Parameter> parameters,
        object[] parameterValues
    ) : base(blackboard, instanceName)
    {
        this.parameters = parameters;
        this.parameterValues = parameterValues;
        this.preconditions = preconditions;
        this.effects = effects;

        // Set debug name based on action type and parameters
        var paramNames = parameterValues.Select(p => p.ToString()).ToList();
        this.DebugDisplayName = $"{instanceName} ({actionType} {string.Join(" ", paramNames)})";
    }

    // Constructor with automatic predicate instantiation
    public GenericBTAction(
        string actionType,
        string instanceName,
        Blackboard<FastName> blackboard,
        List<Parameter> parameters,
        object[] parameterValues
    ) : base(blackboard, instanceName)
    {
        this.parameters = parameters;
        this.parameterValues = parameterValues;
        
        // Automatically instantiate preconditions and effects
        this.preconditions = InstantiatePredicates(PreconditionTemplates, StateType.Precondition, actionType);
        this.effects = InstantiatePredicates(EffectTemplates, StateType.Effect, actionType);

        // Set debug name based on action type and parameters
        var paramNames = parameterValues.Select(p => p.ToString()).ToList();
        this.DebugDisplayName = $"{instanceName} ({actionType} {string.Join(" ", paramNames)})";
    }
    public void applyEffects()
    {
        // foreach (var effect in effects)
        // {
        //     blackboard.SetValue(effect.Key, effect.Value);
        // }
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
        var predicateParams = new Dictionary<string, object>();
        
        foreach (var mapping in template.ParameterMappings)
        {
            // Find the parameter value by name
            var paramIndex = parameters.FindIndex(p => p.Name.ToString() == mapping.ActionParameterName);
            if (paramIndex == -1)
            {
                throw new ArgumentException($"Action parameter '{mapping.ActionParameterName}' not found for predicate '{template.PredicateName}'");
            }
            
            predicateParams[mapping.PredicateParameterName] = parameterValues[paramIndex];
        }
        
        // Create predicate using PredicateFactory
        return FactoryPredicate.Instance.CreatePredicateInstance(template.PredicateName, predicateParams, blackboard);
    }
}
