using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

public class FactoryParameter : Singleton<FactoryParameter>
{
    // Create a parameter instance by type name and instance name only
    public Entity CreateParameter(string typeName, string instanceName)
    {
        // Map type names to actual types
        Type parameterType = typeName.ToLower() switch
        {
            "element" => typeof(Element),
            "agent" => typeof(Agent),
            "location" => typeof(Location),
            "tool" => typeof(Tool),
            "layer" => typeof(Layer),
            "module" => typeof(Module),
            "beam" => typeof(Beam),
            "plate" => typeof(Plate),
            "robot" => typeof(Robot),
            "firstlocation" => typeof(FirstLocation),
            "positiononrail" => typeof(PositionOnRail),
            _ => throw new ArgumentException($"Unknown parameter type: {typeName}")
        };

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

    // Create a Parameter metadata object (for action definitions)
    public Parameter CreateParameterMetadata(string name, string typeName)
    {
        Type parameterType = typeName.ToLower() switch
        {
            "element" => typeof(Element),
            "agent" => typeof(Agent),
            "location" => typeof(Location),
            "tool" => typeof(Tool),
            "layer" => typeof(Layer),
            "module" => typeof(Module),
            "beam" => typeof(Beam),
            "plate" => typeof(Plate),
            "robot" => typeof(Robot),
            "firstlocation" => typeof(FirstLocation),
            "positiononrail" => typeof(PositionOnRail),
            _ => throw new ArgumentException($"Unknown parameter type: {typeName}")
        };

        return new Parameter(name, parameterType);
    }
}


