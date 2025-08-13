

using System;
using System.Collections.Generic;
using ModelLoader.PredicateTypes;

public class FactoryPredicate
{
    private static FactoryPredicate instance;
   

    public static FactoryPredicate Instance
    {
        get
        {
            return instance ??= new FactoryPredicate();
        }
    }

   

    // Create a predicate instance by predicate name and parameter mappings
    public Predicate CreatePredicateInstance(string predicateName, List<ParameterMapping> parameterMappings, Blackboard<FastName> blackboard)
    {
        Console.WriteLine($"\n🔍 DEBUG: Creating predicate instance for '{predicateName}'");
        Console.WriteLine($"📋 Parameter mappings: {string.Join(", ", parameterMappings.Select(pm => $"{pm.ParameterName}={pm.ParameterValue}"))}");
        
        // Dynamically find the predicate type
        Type predicateType = FindPredicateType(predicateName);
        
        if (predicateType == null)
        {
            Console.WriteLine($"❌ ERROR: Unknown predicate type: {predicateName}");
            throw new ArgumentException($"Unknown predicate type: {predicateName}");
        }
        
        Console.WriteLine($"✅ Found predicate type: {predicateType.Name}");

        // Get the actual parameter values from blackboard
        var parameterValues = new List<object>();
        var parameterTypes = new List<Type>();
        
        // Get constructor parameters in order
        var constructors = predicateType.GetConstructors();
        if (constructors.Length == 0)
        {
            Console.WriteLine($"❌ ERROR: No constructors found for predicate type {predicateName}");
            throw new InvalidOperationException($"No constructors found for predicate type {predicateName}");
        }
        
        Console.WriteLine($"🔧 Found {constructors.Length} constructor(s)");
        
        // Use the first constructor (assuming it's the one with parameters)
        var constructor = constructors[0];
        var constructorParams = constructor.GetParameters();
        
        Console.WriteLine($"🔧 Constructor parameters: {string.Join(", ", constructorParams.Select(p => $"{p.Name}:{p.ParameterType.Name}"))}");
        
        // Map parameter mappings to constructor parameters
        foreach (var param in constructorParams)
        {
            Console.WriteLine($"\n🔍 Looking for constructor parameter: {param.Name} (type: {param.ParameterType.Name})");
            
            var mapping = parameterMappings.FirstOrDefault(m => 
                string.Equals(m.ParameterName, param.Name, StringComparison.OrdinalIgnoreCase));
            
            if (mapping != null)
            {
                Console.WriteLine($"✅ Found mapping: {mapping.ParameterName} = {mapping.ParameterValue}");
                
                // Get the actual entity from blackboard
                var key = new FastName(mapping.ParameterValue);
                Console.WriteLine($"🔍 Looking up entity in blackboard with key: {key}");
                
                try
                {
                    object value = GetEntityFromBlackboard(blackboard, key, param.ParameterType);
                    Console.WriteLine($"✅ Retrieved entity: {value} (type: {value?.GetType().Name})");
                    parameterValues.Add(value);
                    parameterTypes.Add(param.ParameterType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ ERROR retrieving entity: {ex.Message}");
                    throw;
                }
            }
            else
            {
                Console.WriteLine($"❌ ERROR: Constructor parameter '{param.Name}' not found in parameter mappings");
                Console.WriteLine($"Available mappings: {string.Join(", ", parameterMappings.Select(m => m.ParameterName))}");
                throw new ArgumentException($"Constructor parameter '{param.Name}' not found in parameter mappings for predicate {predicateName}");
            }
        }

        Console.WriteLine($"\n🔧 Creating instance with parameters: {string.Join(", ", parameterValues.Select(v => $"{v}"))}");
        
        // Create instance using constructor with parameters
        var instance = Activator.CreateInstance(predicateType, parameterValues.ToArray()) as Predicate;
        
        if (instance == null)
        {
            Console.WriteLine($"❌ ERROR: Failed to create instance of predicate type {predicateName}");
            throw new InvalidOperationException($"Failed to create instance of predicate type {predicateName}");
        }
        
        Console.WriteLine($"✅ Successfully created predicate instance: {instance.GetType().Name}");

        // Set any additional properties that might not be in constructor
        foreach (var mapping in parameterMappings)
        {
            var property = predicateType.GetProperty(mapping.ParameterName);
            if (property != null && !parameterTypes.Contains(property.PropertyType))
            {
                Console.WriteLine($"🔧 Setting additional property: {mapping.ParameterName} = {mapping.ParameterValue}");
                
                // Get the actual entity from blackboard using the parameter name
                var key = new FastName(mapping.ParameterValue);
                object value = GetEntityFromBlackboard(blackboard, key, property.PropertyType);

                // Set the property value
                property.SetValue(instance, value);
                Console.WriteLine($"✅ Set predicate property {mapping.ParameterName} = {mapping.ParameterValue} (actual: {value})");
            }
        }
        
        Console.WriteLine($"🔧 Registering predicate in blackboard with key: {instance.PredicateName}");
        
        // Create a unique key for this predicate instance based on its name and parameters
        var uniqueKey = CreateUniquePredicateKey(instance, parameterMappings);
        Console.WriteLine($"🔧 Using unique key: {uniqueKey}");
        
        blackboard.SetPredicate(uniqueKey, instance);
        Console.WriteLine($"✅ Successfully registered predicate: {uniqueKey}");
        
        return instance;
    }

    /// <summary>
    /// Dynamically find predicate type by name
    /// </summary>
    /// <param name="predicateName"></param>
    /// <returns></returns>
    private Type FindPredicateType(string predicateName)
    {
        Console.WriteLine($"🔍 FindPredicateType: searching for '{predicateName}'");
        
        // Get the assembly containing Predicate types
        var assembly = typeof(Predicate).Assembly;
        
        // Search for types that inherit from Predicate
        var predicateTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Predicate)) && !t.IsAbstract)
            .ToList();
        
        Console.WriteLine($"📋 Found {predicateTypes.Count} predicate types: {string.Join(", ", predicateTypes.Select(t => t.Name))}");
        
        // Try exact match first (case-insensitive)
        var exactMatch = predicateTypes.FirstOrDefault(t => 
            string.Equals(t.Name, predicateName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null)
        {
            Console.WriteLine($"✅ Found exact match: {exactMatch.Name}");
            return exactMatch;
        }
        
        // Try partial match (e.g., "isat" matches "IsAt")
        var partialMatch = predicateTypes.FirstOrDefault(t => 
            string.Equals(t.Name.Replace(" ", ""), predicateName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
        
        if (partialMatch != null)
        {
            Console.WriteLine($"✅ Found partial match: {partialMatch.Name}");
            return partialMatch;
        }
        
        Console.WriteLine($"❌ No match found for predicate name: {predicateName}");
        // If no match found, return null
        return null;
    }

    /// <summary>
    /// Creates a unique key for a predicate instance based on its name and parameters
    /// </summary>
    /// <param name="predicate">The predicate instance</param>
    /// <param name="parameterMappings">The parameter mappings used to create the predicate</param>
    /// <returns>A unique FastName key</returns>
    private FastName CreateUniquePredicateKey(Predicate predicate, List<ParameterMapping> parameterMappings)
    {
        // Create a unique identifier by combining predicate name with parameter values
        var parameterValues = parameterMappings
            .Where(pm => pm.ParameterName.ToLower() != "isnegated") // Exclude isNegated from the key
            .OrderBy(pm => pm.ParameterName) // Sort for consistent ordering
            .Select(pm => pm.ParameterValue);
        
        string uniqueKeyString = $"{predicate.PredicateName}_{string.Join("_", parameterValues)}";
        return new FastName(uniqueKeyString);
    }

    /// <summary>
    /// Get entity from blackboard based on type
    /// </summary>
    /// <param name="blackboard"></param>
    /// <param name="key"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private object GetEntityFromBlackboard(Blackboard<FastName> blackboard, FastName key, Type entityType)
    {
        Console.WriteLine($"🔍 GetEntityFromBlackboard: key={key}, expectedType={entityType.Name}");
        
        // Handle primitive types first
        if (entityType == typeof(bool))
        {
            // For boolean values, parse the string value directly
            bool boolValue = bool.Parse(key.ToString());
            Console.WriteLine($"✅ GetEntityFromBlackboard result: {boolValue} (type: Boolean)");
            return boolValue;
        }
        else if (entityType == typeof(int))
        {
            // For integer values, parse the string value directly
            int intValue = int.Parse(key.ToString());
            Console.WriteLine($"✅ GetEntityFromBlackboard result: {intValue} (type: Int32)");
            return intValue;
        }
        else if (entityType == typeof(double))
        {
            // For double values, parse the string value directly
            double doubleValue = double.Parse(key.ToString());
            Console.WriteLine($"✅ GetEntityFromBlackboard result: {doubleValue} (type: Double)");
            return doubleValue;
        }
        else if (entityType == typeof(string))
        {
            // For string values, return the key as string
            string stringValue = key.ToString();
            Console.WriteLine($"✅ GetEntityFromBlackboard result: {stringValue} (type: String)");
            return stringValue;
        }
        
        // Use a simple switch expression to map entity types to blackboard methods
        object result = entityType.Name switch
        {
            "Element" => blackboard.GetElement(key),
            "Agent" => blackboard.GetAgent(key),
            "Location" => blackboard.GetLocation(key),
            "Tool" => blackboard.GetTool(key),
            "Layer" => blackboard.GetLayer(key),
            "Module" => blackboard.GetModule(key),
            _ => throw new ArgumentException($"Unsupported entity type: {entityType.Name}")
        };
        
        Console.WriteLine($"✅ GetEntityFromBlackboard result: {result} (type: {result?.GetType().Name})");
        return result;
    }

    

   

  

}