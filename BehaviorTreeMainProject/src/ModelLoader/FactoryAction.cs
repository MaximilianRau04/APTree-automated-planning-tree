using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


public class FactoryAction : Singleton<FactoryAction>
{
    // Create an instance of a registered Monticore-generated action type
    public GenericBTAction CreateActionInstance(
        string actionTypeName, 
        Blackboard<FastName> blackboard,
        string instanceName,
        List<Parameter> parameters,
        object[] parameterValues)
    {
        // Dynamically find the action type
        Type actionType = FindActionType(actionTypeName);
        
        if (actionType == null)
        {
            throw new ArgumentException($"Unknown action type: {actionTypeName}");
        }

        // Build constructor arguments: actionType, instanceName, blackboard, then all parameter values
        var constructorArgs = new List<object> { actionTypeName, instanceName, blackboard };
        constructorArgs.AddRange(parameterValues);

        // Create instance using the new constructor with all parameters
        var instance = Activator.CreateInstance(actionType, constructorArgs.ToArray()) as GenericBTAction;
        
        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to create instance of type {actionTypeName}");
        }

        // Automatically store predicates in blackboard
        instance.StorePredicatesInBlackboard();

        return instance;
    }

    // Create an action instance with parameter values as dictionary
    public GenericBTAction CreateActionInstance(
        string actionTypeName, 
        Blackboard<FastName> blackboard,
        string instanceName,
        Dictionary<string, object> parameterValues)
    {
        // Dynamically find the action type
        Type actionType = FindActionType(actionTypeName);
        
        if (actionType == null)
        {
            throw new ArgumentException($"Unknown action type: {actionTypeName}");
        }

        // Convert dictionary to parameter list and values array
        var parameters = new List<Parameter>();
        var values = new List<object>();
        
        foreach (var kvp in parameterValues)
        {
            // Create parameter metadata
            var parameter = new Parameter(kvp.Key, kvp.Value.GetType());
            parameters.Add(parameter);
            values.Add(kvp.Value);
        }

        // Build constructor arguments: actionType, instanceName, blackboard, then all parameter values
        var constructorArgs = new List<object> { actionTypeName, instanceName, blackboard };
        constructorArgs.AddRange(values);

        // Create instance using the new constructor with all parameters
        var instance = Activator.CreateInstance(actionType, constructorArgs.ToArray()) as GenericBTAction;
        
        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to create instance of type {actionTypeName}");
        }

        // Automatically store predicates in blackboard
        instance.StorePredicatesInBlackboard();

        return instance;
    }

    // Create an action instance with simple parameter values (strings)
    public GenericBTAction CreateActionInstance(
        string actionTypeName, 
        Blackboard<FastName> blackboard,
        string instanceName,
        Dictionary<string, string> parameterValues)
    {
        // Dynamically find the action type
        Type actionType = FindActionType(actionTypeName);
        
        if (actionType == null)
        {
            throw new ArgumentException($"Unknown action type: {actionTypeName}");
        }

        // Convert string values to actual parameter objects using FactoryParameter
        var parameters = new List<Parameter>();
        var values = new List<object>();
        
        foreach (var kvp in parameterValues)
        {
            // Create parameter instance using FactoryParameter
            var parameterInstance = FactoryParameter.Instance.CreateParameter(kvp.Value, kvp.Value);
            
            // Create parameter metadata
            var parameter = new Parameter(kvp.Key, parameterInstance.GetType());
            parameters.Add(parameter);
            values.Add(parameterInstance);
        }

        // Build constructor arguments: actionType, instanceName, blackboard, then all parameter values
        var constructorArgs = new List<object> { actionTypeName, instanceName, blackboard };
        constructorArgs.AddRange(values);

        // Create instance using the new constructor with all parameters
        var instance = Activator.CreateInstance(actionType, constructorArgs.ToArray()) as GenericBTAction;
        
        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to create instance of type {actionTypeName}");
        }

        // Automatically store predicates in blackboard
        instance.StorePredicatesInBlackboard();

        return instance;
    }
    
    // Dynamically find action type by name
    private Type FindActionType(string actionTypeName)
    {
        // Get the assembly containing GenericBTAction types
        var assembly = typeof(GenericBTAction).Assembly;
        
        // Search for types that inherit from GenericBTAction
        var actionTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(GenericBTAction)) && !t.IsAbstract)
            .ToList();
        
        // Try exact match first (case-insensitive)
        var exactMatch = actionTypes.FirstOrDefault(t => 
            string.Equals(t.Name, actionTypeName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null)
        {
            return exactMatch;
        }
        
        // Try partial match (e.g., "pickup" matches "PickUp")
        var partialMatch = actionTypes.FirstOrDefault(t => 
            string.Equals(t.Name.Replace(" ", ""), actionTypeName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
        
        if (partialMatch != null)
        {
            return partialMatch;
        }
        
        // If no match found, return null
        return null;
    }


}

