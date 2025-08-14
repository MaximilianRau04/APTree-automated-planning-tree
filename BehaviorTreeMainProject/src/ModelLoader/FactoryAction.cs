using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


public class FactoryAction : Singleton<FactoryAction>
{
    /// <summary>
    /// Creates an action instance from an ActionInstance definition like:
    /// ActionInstance: pickUp(pickedObject : b1, rob : r1, loc : fp1, robTool : vg1)
    /// </summary>
    public GenericBTAction CreateActionInstance(
        string actionInstanceDefinition, 
        Blackboard<FastName> blackboard)
    {
        // Parse the action instance definition
        var (actionTypeName, instanceName, parameterValues) = ParseActionInstanceDefinition(actionInstanceDefinition);
        
        Console.WriteLine($"🔧 Creating action instance: {actionTypeName} with name: {instanceName}");
        
        // Dynamically find the action type
        Type actionType = FindActionType(actionTypeName);
        
        if (actionType == null)
        {
            throw new ArgumentException($"Unknown action type: {actionTypeName}");
        }
        
        Console.WriteLine($"✅ Found action type: {actionType.Name}");

        // Convert parameter values to actual instances from blackboard
        var constructorArgs = new List<object> { actionTypeName, instanceName, blackboard };
        
        foreach (var kvp in parameterValues)
        {
            Console.WriteLine($"  📋 Processing parameter: {kvp.Key} = {kvp.Value}");
            
            // Get the parameter instance from blackboard
            object parameterInstance = GetParameterInstanceFromBlackboard(blackboard, kvp.Value, actionType, kvp.Key);
            
            if (parameterInstance == null)
            {
                throw new ArgumentException($"Parameter instance '{kvp.Value}' not found in blackboard");
            }
            
            Console.WriteLine($"  ✅ Retrieved from blackboard: {kvp.Value} -> {parameterInstance.GetType().Name}");
            constructorArgs.Add(parameterInstance);
        }

        // Create the action instance
        Console.WriteLine($"🔍 Creating instance with constructor arguments...");
        GenericBTAction instance;
        try
        {
            instance = Activator.CreateInstance(actionType, constructorArgs.ToArray()) as GenericBTAction;
            
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create instance of type {actionTypeName}");
            }
            
            Console.WriteLine($"✅ Successfully created action instance: {instance.GetType().Name}");
            return instance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR during instance creation: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Parses an ActionInstance definition string like:
    /// "ActionInstance: pickUp(pickedObject : b1, rob : r1, loc : fp1, robTool : vg1)"
    /// </summary>
    private (string actionTypeName, string instanceName, Dictionary<string, string> parameters) ParseActionInstanceDefinition(string definition)
    {
        // Remove "ActionInstance: " prefix
        string content = definition.Replace("ActionInstance: ", "").Trim();
        
        // Find the opening parenthesis
        int openParen = content.IndexOf('(');
        int closeParen = content.LastIndexOf(')');
        
        if (openParen == -1 || closeParen == -1)
        {
            throw new ArgumentException($"Invalid ActionInstance format: {definition}");
        }
        
        // Extract action type name (use the part before the first parenthesis as instance name)
        string actionTypeName = content.Substring(0, openParen).Trim();
        string instanceName = actionTypeName; // Use action type name as instance name
        
        // Extract parameters
        string paramsString = content.Substring(openParen + 1, closeParen - openParen - 1);
        var parameters = new Dictionary<string, string>();
        
        if (!string.IsNullOrWhiteSpace(paramsString))
        {
            string[] paramPairs = paramsString.Split(',');
            foreach (string pair in paramPairs)
            {
                string trimmedPair = pair.Trim();
                if (trimmedPair.Contains(":"))
                {
                    string[] parts = trimmedPair.Split(':');
                    if (parts.Length == 2)
                    {
                        string paramName = parts[0].Trim();
                        string paramValue = parts[1].Trim();
                        parameters[paramName] = paramValue;
                    }
                }
            }
        }
        
        return (actionTypeName, instanceName, parameters);
    }
    
    /// <summary>
    /// Dynamically finds an action type by name
    /// </summary>
    private Type FindActionType(string actionTypeName)
    {
        Console.WriteLine($"🔍 Finding action type: '{actionTypeName}'");
        
        // Get the assembly containing GenericBTAction types
        var assembly = typeof(GenericBTAction).Assembly;
        
        // Search for types that inherit from GenericBTAction
        var actionTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(GenericBTAction)) && !t.IsAbstract)
            .ToList();
        
        Console.WriteLine($"📋 Found {actionTypes.Count} action types: {string.Join(", ", actionTypes.Select(t => t.Name))}");
        
        // Try exact match first (case-insensitive)
        var exactMatch = actionTypes.FirstOrDefault(t => 
            string.Equals(t.Name, actionTypeName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null)
        {
            Console.WriteLine($"✅ Found exact match: {exactMatch.Name}");
            return exactMatch;
        }
        
        // Try partial match (e.g., "pickup" matches "PickUp")
        var partialMatch = actionTypes.FirstOrDefault(t => 
            string.Equals(t.Name.Replace(" ", ""), actionTypeName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
        
        if (partialMatch != null)
        {
            Console.WriteLine($"✅ Found partial match: {partialMatch.Name}");
            return partialMatch;
        }
        
        Console.WriteLine($"❌ No match found for action name: {actionTypeName}");
        return null;
    }

    /// <summary>
    /// Retrieves a parameter instance from the blackboard
    /// </summary>
    private object GetParameterInstanceFromBlackboard(Blackboard<FastName> blackboard, string instanceName, Type actionType, string parameterName)
    {
        Console.WriteLine($"🔍 Getting parameter instance: '{instanceName}' (parameter '{parameterName}' in action '{actionType.Name}')");
        
        // Get the parameter type from the action class
        var parameterProperty = actionType.GetProperty(parameterName);
        if (parameterProperty == null)
        {
            throw new ArgumentException($"Parameter '{parameterName}' not found in action class '{actionType.Name}'");
        }
        
        Type parameterType = parameterProperty.PropertyType;
        Console.WriteLine($"📋 Parameter type: {parameterType.Name}");
        
        // Find the parent entity type
        Type parentType = GetParentEntityType(parameterType);
        Console.WriteLine($"📋 Parent entity type: {parentType?.Name ?? "null"}");
        
        if (parentType == null)
        {
            throw new ArgumentException($"Could not determine parent entity type for parameter type '{parameterType.Name}'");
        }
        
        // Get the instance from the correct blackboard dictionary
        var key = new FastName(instanceName);
        object result = parentType.Name switch
        {
            "Element" => blackboard.GetElement(key),
            "Agent" => blackboard.GetAgent(key),
            "Location" => blackboard.GetLocation(key),
            "Tool" => blackboard.GetTool(key),
            "Layer" => blackboard.GetLayer(key),
            "Module" => blackboard.GetModule(key),
            _ => throw new ArgumentException($"Unsupported parent entity type: {parentType.Name}")
        };
        
        if (result != null)
        {
            Console.WriteLine($"✅ Found instance '{instanceName}' as {parentType.Name}: {result.GetType().Name}");
        }
        else
        {
            Console.WriteLine($"❌ Instance '{instanceName}' not found in {parentType.Name} dictionary");
        }
        
        return result;
    }
    
    /// <summary>
    /// Determines the parent entity type for a given parameter type
    /// </summary>
    private Type GetParentEntityType(Type parameterType)
    {
        // Check if the parameter type directly inherits from one of our entity types
        var entityTypes = new[] { typeof(Element), typeof(Agent), typeof(Location), typeof(Tool), typeof(Layer), typeof(Module) };
        
        foreach (var entityType in entityTypes)
        {
            if (parameterType.IsSubclassOf(entityType) || parameterType == entityType)
            {
                return entityType;
            }
        }
        
        return null;
    }
}


