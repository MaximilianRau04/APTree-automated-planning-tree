using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

public class FactoryParameter : Singleton<FactoryParameter>
{
    // Create a parameter instance by type name and instance name only
    public Entity CreateParameter(string typeName, string instanceName)
    {
        // Dynamically find the parameter type
        Type parameterType = FindParameterType(typeName);
        
        if (parameterType == null)
        {
            throw new ArgumentException($"Unknown parameter type: {typeName}");
        }

        // Create instance using empty constructor
        var instance = Activator.CreateInstance(parameterType) as Entity;
        
        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to create instance of type {typeName}");
        }

        // Set the name using the NameKey property
        instance.NameKey = new FastName(instanceName);
        instance.ID = instanceName;

        return instance;
    }

    // Create a parameter instance with parameter values
    public Entity CreateParameter(string typeName, string instanceName, Dictionary<string, object> parameters)
    {
        // Create the base instance
        var instance = CreateParameter(typeName, instanceName);
        
        // Set the parameter values using the abstract method
        instance.SetParameters(parameters);
        
        return instance;
    }
    
    // Dynamically find parameter type by name
    private Type FindParameterType(string typeName)
    {
        // Get the assembly containing Entity types
        var assembly = typeof(Entity).Assembly;
        
        // Search for types that inherit from Entity
        var entityTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Entity)) && !t.IsAbstract)
            .ToList();
        
        // Try exact match first (case-insensitive)
        var exactMatch = entityTypes.FirstOrDefault(t => 
            string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null)
        {
            return exactMatch;
        }
        
        // Try partial match (e.g., "firstlocation" matches "FirstLocation")
        var partialMatch = entityTypes.FirstOrDefault(t => 
            string.Equals(t.Name.Replace(" ", ""), typeName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));
        
        if (partialMatch != null)
        {
            return partialMatch;
        }
        
        // If no match found, return null
        return null;
    }

    // Create a Parameter metadata object (for action definitions)
    public Parameter CreateParameterMetadata(string name, string typeName)
    {
        // Dynamically find the parameter type
        Type parameterType = FindParameterType(typeName);
        
        if (parameterType == null)
        {
            throw new ArgumentException($"Unknown parameter type: {typeName}");
        }

        return new Parameter(name, parameterType);
    }
}


