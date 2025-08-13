

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
    public Predicate CreatePredicateInstance(string predicateName, Dictionary<string, string> parameterMappings, Blackboard<FastName> blackboard)
    {
        // Dynamically find the predicate type
        Type predicateType = FindPredicateType(predicateName);
        
        if (predicateType == null)
        {
            throw new ArgumentException($"Unknown predicate type: {predicateName}");
        }

        // Create instance using empty constructor
        var instance = Activator.CreateInstance(predicateType) as Predicate;
        
        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to create instance of predicate type {predicateName}");
        }

        // Set the parameter values using reflection
        foreach (var kvp in parameterMappings)
        {
            var property = predicateType.GetProperty(kvp.Key);
            if (property != null)
            {
                // Get the actual entity from blackboard using the parameter name
                var key = new FastName(kvp.Value);
                object value = GetEntityFromBlackboard(blackboard, key, property.PropertyType);

                // Set the property value
                property.SetValue(instance, value);
                Console.WriteLine($"Set predicate property {kvp.Key} = {kvp.Value} (actual: {value})");
            }
            else
            {
                Console.WriteLine($"Warning: Property {kvp.Key} not found in predicate type {predicateName}");
            }
        }

        return instance;
    }

    // Dynamically find predicate type by name
    private Type FindPredicateType(string predicateName)
    {
        // Get the assembly containing Predicate types
        var assembly = typeof(Predicate).Assembly;
        
        // Search for types that inherit from Predicate
        var predicateTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Predicate)) && !t.IsAbstract)
            .ToList();
        
        // Try exact match first (case-insensitive)
        var exactMatch = predicateTypes.FirstOrDefault(t => 
            string.Equals(t.Name, predicateName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null)
        {
            return exactMatch;
        }
        
        // Try partial match (e.g., "isat" matches "IsAt")
        var partialMatch = predicateTypes.FirstOrDefault(t => 
            string.Equals(t.Name.Replace(" ", ""), predicateName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
        
        if (partialMatch != null)
        {
            return partialMatch;
        }
        
        // If no match found, return null
        return null;
    }

    // Get entity from blackboard based on type
    private object GetEntityFromBlackboard(Blackboard<FastName> blackboard, FastName key, Type entityType)
    {
        // Use a simple switch expression to map types to blackboard methods
        return entityType.Name switch
        {
            "Element" => blackboard.GetElement(key),
            "Agent" => blackboard.GetAgent(key),
            "Location" => blackboard.GetLocation(key),
            "Tool" => blackboard.GetTool(key),
            "Layer" => blackboard.GetLayer(key),
            "Module" => blackboard.GetModule(key),
            _ => throw new ArgumentException($"Unsupported entity type: {entityType.Name}")
        };
    }

    

   

  

}