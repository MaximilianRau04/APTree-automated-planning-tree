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
        Console.WriteLine($"\n🔧 DEBUG: Creating action instance for '{actionTypeName}'");
        Console.WriteLine($"📋 Instance name: {instanceName}");
        Console.WriteLine($"📋 Parameter values: {string.Join(", ", parameterValues.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        
        // Dynamically find the action type
        Console.WriteLine($"🔍 Step 1: Finding action type '{actionTypeName}'...");
        Type actionType = FindActionType(actionTypeName);
        
        if (actionType == null)
        {
            Console.WriteLine($"❌ ERROR: Unknown action type: {actionTypeName}");
            throw new ArgumentException($"Unknown action type: {actionTypeName}");
        }
        
        Console.WriteLine($"✅ Found action type: {actionType.Name}");

        // Convert dictionary to parameter list and values array
        Console.WriteLine($"🔍 Step 2: Processing parameters...");
        var parameters = new List<Parameter>();
        var values = new List<object>();
        
        foreach (var kvp in parameterValues)
        {
            Console.WriteLine($"  📋 Processing parameter: {kvp.Key} = {kvp.Value} (type: {kvp.Value?.GetType().Name ?? "null"})");
            
            // Create parameter metadata
            var parameter = new Parameter(kvp.Key, kvp.Value.GetType());
            parameters.Add(parameter);
            values.Add(kvp.Value);
            
            Console.WriteLine($"  ✅ Added parameter: {kvp.Key} -> {kvp.Value?.GetType().Name ?? "null"}");
        }

        // Build constructor arguments: actionType, instanceName, blackboard, then all parameter values
        Console.WriteLine($"🔍 Step 3: Building constructor arguments...");
        var constructorArgs = new List<object> { actionTypeName, instanceName, blackboard };
        constructorArgs.AddRange(values);
        
        Console.WriteLine($"📋 Constructor arguments: {string.Join(", ", constructorArgs.Select(arg => $"{arg} ({arg?.GetType().Name ?? "null"})"))}");

        // Find the constructor
        Console.WriteLine($"🔍 Step 4: Finding constructor...");
        var constructors = actionType.GetConstructors();
        Console.WriteLine($"📋 Found {constructors.Length} constructor(s)");
        
        foreach (var constructor in constructors)
        {
            var constructorParams = constructor.GetParameters();
            Console.WriteLine($"  📋 Constructor: {constructorParams.Length} parameters");
            for (int i = 0; i < constructorParams.Length; i++)
            {
                Console.WriteLine($"    {i}: {constructorParams[i].Name} ({constructorParams[i].ParameterType.Name})");
            }
        }

        // Create instance using the new constructor with all parameters
        Console.WriteLine($"🔍 Step 5: Creating instance...");
        GenericBTAction instance;
        try
        {
            instance = Activator.CreateInstance(actionType, constructorArgs.ToArray()) as GenericBTAction;
            
            if (instance == null)
            {
                Console.WriteLine($"❌ ERROR: Failed to create instance of type {actionTypeName}");
                throw new InvalidOperationException($"Failed to create instance of type {actionTypeName}");
            }
            
            Console.WriteLine($"✅ Successfully created instance: {instance.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR during instance creation: {ex.Message}");
            Console.WriteLine($"📋 Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"📋 Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }

        // Automatically store predicates in blackboard
        Console.WriteLine($"🔍 Step 6: Storing predicates in blackboard...");
        try
        {
            instance.StorePredicatesInBlackboard();
            Console.WriteLine($"✅ Successfully stored predicates");
            
            Console.WriteLine($"🎉 Action instance creation completed successfully!");
            return instance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR during predicate storage: {ex.Message}");
            throw;
        }
    }

    // Create an action instance with simple parameter values (strings)
    public GenericBTAction CreateActionInstance(
        string actionTypeName, 
        Blackboard<FastName> blackboard,
        string instanceName,
        Dictionary<string, string> parameterValues)
    {
        Console.WriteLine($"\n🔧 DEBUG: Creating action instance for '{actionTypeName}' (string parameters)");
        Console.WriteLine($"📋 Instance name: {instanceName}");
        Console.WriteLine($"📋 Parameter values: {string.Join(", ", parameterValues.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        
        // Dynamically find the action type
        Console.WriteLine($"🔍 Step 1: Finding action type '{actionTypeName}'...");
        Type actionType = FindActionType(actionTypeName);
        
        if (actionType == null)
        {
            Console.WriteLine($"❌ ERROR: Unknown action type: {actionTypeName}");
            throw new ArgumentException($"Unknown action type: {actionTypeName}");
        }
        
        Console.WriteLine($"✅ Found action type: {actionType.Name}");

        // Convert string values to actual parameter objects by retrieving from blackboard
        Console.WriteLine($"🔍 Step 2: Converting string parameters to blackboard instances...");
        var parameters = new List<Parameter>();
        var values = new List<object>();
        
        foreach (var kvp in parameterValues)
        {
            Console.WriteLine($"  📋 Processing parameter: {kvp.Key} = {kvp.Value}");
            
            // Get the parameter instance from blackboard instead of creating it
            object parameterInstance = GetParameterInstanceFromBlackboard(blackboard, kvp.Value, actionType, kvp.Key);
            
            if (parameterInstance == null)
            {
                Console.WriteLine($"❌ ERROR: Could not find parameter instance '{kvp.Value}' in blackboard");
                throw new ArgumentException($"Parameter instance '{kvp.Value}' not found in blackboard");
            }
            
            Console.WriteLine($"  ✅ Retrieved from blackboard: {kvp.Value} -> {parameterInstance.GetType().Name}");
            
            // Create parameter metadata
            var parameter = new Parameter(kvp.Key, parameterInstance.GetType());
            parameters.Add(parameter);
            values.Add(parameterInstance);
        }

        // Build constructor arguments: actionType, instanceName, blackboard, then all parameter values
        Console.WriteLine($"🔍 Step 3: Building constructor arguments...");
        var constructorArgs = new List<object> { actionTypeName, instanceName, blackboard };
        constructorArgs.AddRange(values);
        
        Console.WriteLine($"📋 Constructor arguments: {string.Join(", ", constructorArgs.Select(arg => $"{arg} ({arg?.GetType().Name ?? "null"})"))}");

        // Find the constructor
        Console.WriteLine($"🔍 Step 4: Finding constructor...");
        var constructors = actionType.GetConstructors();
        Console.WriteLine($"📋 Found {constructors.Length} constructor(s)");
        
        foreach (var constructor in constructors)
        {
            var constructorParams = constructor.GetParameters();
            Console.WriteLine($"  📋 Constructor: {constructorParams.Length} parameters");
            for (int i = 0; i < constructorParams.Length; i++)
            {
                Console.WriteLine($"    {i}: {constructorParams[i].Name} ({constructorParams[i].ParameterType.Name})");
            }
        }

        // Create instance using the new constructor with all parameters
        Console.WriteLine($"🔍 Step 5: Creating instance...");
        GenericBTAction instance;
        try
        {
            instance = Activator.CreateInstance(actionType, constructorArgs.ToArray()) as GenericBTAction;
            
            if (instance == null)
            {
                Console.WriteLine($"❌ ERROR: Failed to create instance of type {actionTypeName}");
                throw new InvalidOperationException($"Failed to create instance of type {actionTypeName}");
            }
            
            Console.WriteLine($"✅ Successfully created instance: {instance.GetType().Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR during instance creation: {ex.Message}");
            Console.WriteLine($"📋 Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"📋 Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }

        // Automatically store predicates in blackboard
        Console.WriteLine($"🔍 Step 6: Storing predicates in blackboard...");
        try
        {
            instance.StorePredicatesInBlackboard();
            Console.WriteLine($"✅ Successfully stored predicates");
            
            Console.WriteLine($"🎉 Action instance creation completed successfully!");
            return instance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR during predicate storage: {ex.Message}");
            throw;
        }
    }
    
    // Dynamically find action type by name
    private Type FindActionType(string actionTypeName)
    {
        Console.WriteLine($"🔍 FindActionType: searching for '{actionTypeName}'");
        
        // Get the assembly containing GenericBTAction types
        var assembly = typeof(GenericBTAction).Assembly;
        Console.WriteLine($"📋 Assembly: {assembly.FullName}");
        
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
        // If no match found, return null
        return null;
    }

    /// <summary>
    /// Retrieves a parameter instance from the blackboard by checking the action's parameter type definition
    /// </summary>
    /// <param name="blackboard">The blackboard to search in</param>
    /// <param name="instanceName">The name of the instance to find</param>
    /// <param name="actionType">The action class type</param>
    /// <param name="parameterName">The name of the parameter in the action</param>
    /// <returns>The found entity or null if not found</returns>
    private object GetParameterInstanceFromBlackboard(Blackboard<FastName> blackboard, string instanceName, Type actionType, string parameterName)
    {
        Console.WriteLine($"🔍 GetParameterInstanceFromBlackboard: searching for '{instanceName}' (parameter '{parameterName}' in action '{actionType.Name}')");
        
        // Get the parameter type from the action class
        var parameterProperty = actionType.GetProperty(parameterName);
        if (parameterProperty == null)
        {
            Console.WriteLine($"❌ ERROR: Parameter '{parameterName}' not found in action class '{actionType.Name}'");
            throw new ArgumentException($"Parameter '{parameterName}' not found in action class '{actionType.Name}'");
        }
        
        Type parameterType = parameterProperty.PropertyType;
        Console.WriteLine($"📋 Parameter type: {parameterType.Name}");
        
        // Find the parent class (e.g., Beam -> Element, Robot -> Agent, etc.)
        Type parentType = GetParentEntityType(parameterType);
        Console.WriteLine($"📋 Parent entity type: {parentType?.Name ?? "null"}");
        
        if (parentType == null)
        {
            Console.WriteLine($"❌ ERROR: Could not determine parent entity type for parameter type '{parameterType.Name}'");
            throw new ArgumentException($"Could not determine parent entity type for parameter type '{parameterType.Name}'");
        }
        
        // Get the instance from the correct blackboard dictionary based on parent type
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
    /// <param name="parameterType">The parameter type to check</param>
    /// <returns>The parent entity type or null if not found</returns>
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
        
        // If not found, return null
        return null;
    }


}


