using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
 
 

public class ActionFactory : Singleton<ActionFactory>
{
    private Dictionary<string, Type> registeredActionTypes = new();
    private Dictionary<string, ActionTypeInfo> actionTypeDefinitions = new();

  

    // Modify registration method to accept action logic
    public void RegisterActionType(
        string actionName,
        List<Parameter> parameters,
        List<PredicateTemplate> preconditions,
        List<PredicateTemplate> effects,
        Func<Dictionary<string, object>, float, bool> actionLogic)
    {
        var definition = new ActionTypeInfo(
           
            parameters,
            preconditions,
            effects,
            actionLogic
        );

        actionTypeDefinitions[actionName] = definition;
        registeredActionTypes[actionName] = typeof(DynamicBTAction);
        
        Console.WriteLine($"Registered action type: {actionName}");
    }

    // STEP 2: Create an instance of a registered type
    public GenericBTAction CreateActionInstance(
        string actionTypeName, 
        Blackboard<FastName> blackboard,
        Dictionary<string, IEntity> parameterValues)
    {
        if (!actionTypeDefinitions.TryGetValue(actionTypeName, out var definition))
        {
            throw new ArgumentException($"Action type {actionTypeName} is not registered");
        }

        // Create preconditions state
        var preconditions = new State(StateType.Precondition, new FastName(actionTypeName));
        foreach (var template in definition.PreconditionTemplates)
        {
            var predicateParams = new Dictionary<string, object>();
            foreach (var paramName in template.ParameterNames)
            {
                predicateParams[paramName] = parameterValues[paramName];
            }
            
            var predicate = PredicateFactory.Instance.CreatePredicateInstance(
                template.PredicateName,
                predicateParams,
                blackboard
            );
            preconditions.AddPredicate(new FastName(template.PredicateName),predicate);
        }

        // Create effects state
        var effects = new State(StateType.Effect, new FastName(actionTypeName));
        foreach (var template in definition.EffectTemplates)
        {
            var predicateParams = new Dictionary<string, object>();
            foreach (var paramName in template.ParameterNames)
            {
                predicateParams[paramName] = parameterValues[paramName];
            }
            
            var predicate = PredicateFactory.Instance.CreatePredicateInstance(
                template.PredicateName,
                predicateParams,
                blackboard
            );
            effects.AddPredicate(new FastName(template.PredicateName),predicate);
        }

        return new DynamicBTAction(
            actionTypeName,
            blackboard,
            preconditions,
            effects,
            definition.Parameters,
            parameterValues.Values.ToArray(),
            definition.ActionLogic
        );
    }

    public bool IsActionTypeRegistered(string actionTypeName)
    {
        return registeredActionTypes.ContainsKey(actionTypeName);
    }

    public Type GetActionType(string actionTypeName)
    {
        if (!registeredActionTypes.TryGetValue(actionTypeName, out Type type))
        {
            throw new ArgumentException($"Action type {actionTypeName} is not registered");
        }
        return type;
    }
}

