

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
        // Map predicate names to actual predicate types
        Type predicateType = predicateName.ToLower() switch
        {
            "isat" => typeof(IsAt),
            "hastool" => typeof(HasTool),
            _ => throw new ArgumentException($"Unknown predicate type: {predicateName}")
        };

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
                object value = null;

                // Map property types to blackboard getter methods
                if (property.PropertyType == typeof(Element))
                    value = blackboard.GetElement(key);
                else if (property.PropertyType == typeof(Agent))
                    value = blackboard.GetAgent(key);
                else if (property.PropertyType == typeof(Location))
                    value = blackboard.GetLocation(key);
                else if (property.PropertyType == typeof(Tool))
                    value = blackboard.GetTool(key);
                else if (property.PropertyType == typeof(Layer))
                    value = blackboard.GetLayer(key);
                else if (property.PropertyType == typeof(Module))
                    value = blackboard.GetModule(key);
                else
                    throw new ArgumentException($"Unsupported property type: {property.PropertyType.Name} for property {kvp.Key}");

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

    

   

  

}