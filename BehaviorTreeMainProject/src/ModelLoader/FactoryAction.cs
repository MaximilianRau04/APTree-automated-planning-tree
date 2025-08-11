using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using ModelLoader.ActionTypes;

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
       

        // Create the action instance using the new constructor with automatic predicate instantiation
        return null;
    }

   


    // Auto-register all Monticore-generated action types
  
}

