

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

   

    // Second step: Create an instance of a predicate type with specific values
    // public Predicate CreatePredicateInstance(PredicateTemplate predicateTemplate, Blackboard blackboard)
    // {
       

       
    //     var instance = (Predicate)Activator.CreateInstance(predicateType);

    //     // Set the values using reflection
    //     foreach (var kvp in predicateTemplate.ParameterNames)
    //     {
    //         var property = predicateType.GetProperty(kvp.Key);
    //         if (property != null)
    //         {
    //             // Convert string ID to actual object
    //             var value = kvp.Value;
    //             if (value is string id)
    //             {
    //                 var key = new FastName(id);
    //                 if (property.PropertyType == typeof(Element))
    //                     value = blackboard.GetElement(key);
    //                 else if (property.PropertyType == typeof(Layer))
    //                     value = blackboard.GetLayer(key);
    //                 else if (property.PropertyType == typeof(Location))
    //                     value = blackboard.GetLocation(key);
    //                 else if (property.PropertyType == typeof(Agent))
    //                     value = blackboard.GetAgent(key);
    //                 else if (property.PropertyType == typeof(Tool))
    //                     value = blackboard.GetTool(key);
    //             }
                
    //             property.SetValue(instance, value);
    //             Console.WriteLine($"Setting property {kvp.Key} with value: {value}");
    //             if (value is IEntity thing)
    //             {
    //                 Console.WriteLine($"  NameKey: {thing.NameKey}");
    //             }
    //         }
    //     }

    //     return instance;
    // }

    // Overloaded method for creating predicate instances with direct parameters
    public Predicate CreatePredicateInstance(string predicateName, Dictionary<string, object> parameters, Blackboard<FastName> blackboard)
    {
       

        var instance = (Predicate)Activator.CreateInstance(typeof(Predicate));

                        // Set the values using reflection
                foreach (var kvp in parameters)
                {
                    var property = typeof(Predicate).GetProperty(kvp.Key);
                    if (property != null)
                    {
                        // Convert string ID to actual object
                        var value = kvp.Value;
                        if (value is string id)
                        {
                            var key = new FastName(id);
                            if (property.PropertyType == typeof(Element))
                                value = blackboard.GetElement(key);
                            else if (property.PropertyType == typeof(Layer))
                                value = blackboard.GetLayer(key);
                            else if (property.PropertyType == typeof(Location))
                                value = blackboard.GetLocation(key);
                            else if (property.PropertyType == typeof(Agent))
                                value = blackboard.GetAgent(key);
                            else if (property.PropertyType == typeof(Tool))
                                value = blackboard.GetTool(key);
                        }
                
                property.SetValue(instance, value);
                Console.WriteLine($"Setting property {kvp.Key} with value: {value}");
                if (value is Entity thing)
                {
                    Console.WriteLine($"  NameKey: {thing.NameKey}");
                }
            }
        }

        return instance;
    }

   

  

}