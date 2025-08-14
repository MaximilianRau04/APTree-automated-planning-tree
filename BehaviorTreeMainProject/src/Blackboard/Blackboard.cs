using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using System.Linq;
using System.Reflection;


//we need to fix the query so that the parent types are also added to the graph
public class Blackboard<T> : IDisposable where T : class
{
    // built-in types
    Dictionary<FastName, int>           IntValues =            new ();
    Dictionary<FastName, double>        DoubleValues =         new ();
    Dictionary<FastName, bool>          BoolValues =           new (); 
    Dictionary<FastName, string>        StringValues =         new ();
 
    
    // registered types as lists
    List<FastName> AvailableEntityTypes = new();
    List<FastName> AvailablePredicateTypes = new();
    List<FastName> AvailableActionTypes = new(); 
     // registered instances
     Dictionary<FastName, Layer> LayerValues = new();
     Dictionary<FastName, Module> ModuleValues = new();
     Dictionary<FastName, Tool> ToolValues = new();
    Dictionary<FastName, Element>   ElementValues =    new ();
    Dictionary<FastName, Location>   LocationValues =    new ();
    Dictionary<FastName, Agent>   AgentValues =    new ();
    Dictionary<FastName, Predicate> PredicateValues = new();
    Dictionary<FastName, GenericBTAction> ActionValues = new();
     Dictionary<FastName, State> StateValues = new();
   
    private readonly IDriver _driver;
    private readonly Neo4jService _graphService;

    public Blackboard(string uri, string user, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        _graphService = new Neo4jService(uri, user, password);
    }

    public bool TryGet(FastName key, out int value, int defaultvalue = 0)
    {
        if(IntValues.ContainsKey(key))
        {
                value = IntValues[key];
                return true;
        }
            value = defaultvalue;
            return false;
    }
    public int GetInt(FastName key)
    {
        if( !IntValues.ContainsKey(key))
        {
                throw new System.ArgumentException($"could not find a value for {key} this key");
               
        }
         return IntValues[key];
    }
    public double GetDouble(FastName key)
    {
        if (!DoubleValues.ContainsKey(key))
        {
            throw new System.ArgumentException($"could not find a value for {key} this key");
        }
        return DoubleValues[key]; 
    }

    public string GetString(FastName key)
    {
        if (!StringValues.ContainsKey(key))
        {
            throw new System.ArgumentException($"could not find a value for {key} this key");
        }
        return StringValues[key];
    }

    public bool GetBool(FastName key)
    {
        if (!BoolValues.ContainsKey(key))
        {
            throw new System.ArgumentException($"could not find a value for {key} this key");
        }
        return BoolValues[key];
    }

   

    public Element GetElement(FastName key)
    {
        if (!ElementValues.ContainsKey(key))
        {
            throw new System.ArgumentException($"could not find a value for {key} this key");
        }
        return ElementValues[key];
    }

    public Location GetLocation(FastName key)
    {
        if (!LocationValues.ContainsKey(key))
        {
            throw new System.ArgumentException($"could not find a value for {key} this key");
        }
        return LocationValues[key];
    }

    public Agent GetAgent(FastName key)
    {
        if (!AgentValues.ContainsKey(key))
        {
            throw new System.ArgumentException($"could not find a value for {key} this key");
        }
        return AgentValues[key];
    }

    // Get methods for predicates
    public Predicate GetPredicate(FastName key)
    {
        if (!PredicateValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find predicate for {key}");
        }
        return PredicateValues[key];
    }

     // Update corresponding Get methods




// Get methods


    // Set methods for all types
    public void SetInt(FastName key, int value)
    {
        IntValues[key] = value;
    }

    public void SetDouble(FastName key, double value)
    {
        DoubleValues[key] = value;
    }

    public void SetBool(FastName key, bool value)
    {
        BoolValues[key] = value;
    }

    public void SetString(FastName key, string value)
    {
        StringValues[key] = value;
    }

    

    public void SetElement(FastName key, Element value)
    {
        // Store the element with its instance ID as the key
        ElementValues[key] = value;
        // Ensure the element's NameKey matches its instance ID
        value.NameKey = key;  // This ensures the element keeps its instance ID
        Console.WriteLine($"Successfully added {value.GetType().Name} to Blackboard with key: {key}");
    }

    public void SetLocation(FastName key, Location value)
    {
        LocationValues[key] = value;
        value.NameKey = key;  // Set the instance ID
    }

    public void SetAgent(FastName key, Agent value)
    {
        AgentValues[key] = value;
        value.NameKey = key;  // Set the instance ID
    }

    
/// <summary>
/// Sets the entity type for a given key
/// </summary>
/// <param name="key"></param>
/// <param name="elementType"></param>
/// <exception cref="ArgumentException"></exception>
   public void SetEntityType(FastName key, Entity elementType)
{
    if (!typeof(Entity).IsAssignableFrom(elementType.GetType()))
    {
        throw new ArgumentException($"Type {elementType.GetType().Name} is not an Entity type");
    }

    if (!AvailableEntityTypes.Contains(key))
    {
        AvailableEntityTypes.Add(key);
    }
    AvailableEntityTypes.Add(key);
}

/// <summary>
/// Registers an entity type
/// </summary>
/// <param name="typeName"></param>
public void RegisterEntityType(FastName typeName)
{
    if (!AvailableEntityTypes.Contains(typeName))
    {
        AvailableEntityTypes.Add(typeName);
    }
}

/// <summary>
/// Checks if an entity type is available
/// </summary>
/// <param name="typeName"></param>
/// <returns></returns>
public bool HasEntityType(FastName typeName)
{
    return AvailableEntityTypes.Contains(typeName);
}

/// <summary>
/// Gets all available entity types
/// </summary>
/// <returns></returns>
public List<FastName> GetAllEntityTypes()
{
    return AvailableEntityTypes.ToList();
}

/// <summary>
/// Registers a predicate type
/// </summary>
/// <param name="typeName"></param>
public void RegisterPredicateType(FastName typeName)
{
    if (!AvailablePredicateTypes.Contains(typeName))
    {
        AvailablePredicateTypes.Add(typeName);
    }
}

/// <summary>
/// Checks if a predicate type is available
/// </summary>
/// <param name="typeName"></param>
/// <returns></returns>
public bool HasPredicateType(FastName typeName)
{
    return AvailablePredicateTypes.Contains(typeName);
}

/// <summary>
/// Gets all available predicate types
/// </summary>
/// <returns></returns>
public List<FastName> GetAllPredicateTypes()
{
    return AvailablePredicateTypes.ToList();
}

/// <summary>
/// Registers an action type
/// </summary>
/// <param name="typeName"></param>
public void RegisterActionType(FastName typeName)
{
    if (!AvailableActionTypes.Contains(typeName))
    {
        AvailableActionTypes.Add(typeName);
    }
}

/// <summary>
/// Checks if an action type is available
/// </summary>
/// <param name="typeName"></param>
/// <returns></returns>
public bool HasActionType(FastName typeName)
{
    return AvailableActionTypes.Contains(typeName);
}

/// <summary>
/// Gets all available action types
/// </summary>
/// <returns></returns>
public List<FastName> GetAllActionTypes()
{
    return AvailableActionTypes.ToList();
}


// Predicate type methods
public void SetPredicateType(FastName key, Predicate predicateType)
{
    if (!typeof(Predicate).IsAssignableFrom(predicateType.GetType()))
    {
        throw new ArgumentException($"Type {predicateType.GetType().Name} is not a Predicate type");
    }

    if (!AvailablePredicateTypes.Contains(key))
    {
        AvailablePredicateTypes.Add(key);
    }
    AvailablePredicateTypes.Add(key);
}

// Action type methods
public void SetActionType(FastName key, GenericBTAction actionType)
{
    if (!typeof(GenericBTAction).IsAssignableFrom(actionType.GetType()))
    {
        throw new ArgumentException($"Type {actionType.GetType().Name} is not an Action type");
    }

    if (!AvailableActionTypes.Contains(key))
    {
        AvailableActionTypes.Add(key);
    }
    AvailableActionTypes.Add(key);
    
    // Store the action instance
    ActionValues[key] = actionType;
}

public BTActionNodeBase GetAction(FastName key)
{
    if (!ActionValues.ContainsKey(key))
    {
        throw new ArgumentException($"Could not find action for {key}");
    }
    return ActionValues[key];
}

/// <summary>
/// Gets all action instances from the blackboard
/// </summary>
/// <returns>List of all action instances</returns>
public List<GenericBTAction> GetAllActionInstances()
{
    return ActionValues.Values.ToList();
}

    // Set methods for predicates
    private void SetPredicateSecondary(FastName key, Predicate predicate)
    {
        string newPredicateStr = BlackboardExtensions.FormatPredicate(predicate);
        
        // Check if identical predicate exists
        if (!PredicateValues.ContainsKey(key) || BlackboardExtensions.FormatPredicate(PredicateValues[key]) != newPredicateStr)
        {
            PredicateValues[key] = predicate;
        }
    }
    

    public bool HasSimilarPredicate(Predicate newPredicate)
    {
        foreach (var existingPredicate in PredicateValues.Values)
        {
            // First check if predicates have the same name
            if (existingPredicate.PredicateName == newPredicate.PredicateName)
            {
                Console.WriteLine($"\nComparing predicates:");
                Console.WriteLine($"New: {newPredicate.PredicateName}");
                Console.WriteLine($"Existing: {existingPredicate.PredicateName}");
                
                // Get properties of both predicates
                var existingParams = existingPredicate.GetAllProperties();
                var newParams = newPredicate.GetAllProperties();
                
                // Check if all parameter names and values match exactly
                bool allParamsMatch = true;
                foreach (var param in newParams)
                {
                    // Skip metadata properties
                    if (param.Key == "PredicateName" || param.Key == "PredicateType" || param.Key == "isNegated")
                        continue;

                    // If parameter doesn't exist in existing predicate
                    if (!existingParams.ContainsKey(param.Key))
                    {
                        Console.WriteLine($"Parameter {param.Key} not found in existing predicate");
                        allParamsMatch = false;
                        break;
                    }

                    // Compare the actual instance names
                    var existingValue = existingParams[param.Key];
                    var newValue = param.Value;

                    Console.WriteLine($"\nComparing parameter {param.Key}:");
                    Console.WriteLine($"Existing value: {existingValue}");
                    Console.WriteLine($"New value: {newValue}");

                    // Get the instance identifiers for comparison
                    var existingId = existingValue?.GetType().GetProperty("InstanceId")?.GetValue(existingValue)?.ToString()
                        ?? existingValue?.ToString();
                    var newId = newValue?.GetType().GetProperty("InstanceId")?.GetValue(newValue)?.ToString()
                        ?? newValue?.ToString();

                    Console.WriteLine($"Existing ID: {existingId}");
                    Console.WriteLine($"New ID: {newId}");

                    if (existingId != newId)
                    {
                        Console.WriteLine("IDs don't match");
                        allParamsMatch = false;
                        break;
                    }
                }
                
                if (allParamsMatch)
                {
                    Console.WriteLine("Found similar predicate!");
                    return true;
                }
            }
        }
        return false;
    }
/// <summary>
/// Adds predicate to the blackboard and the
/// </summary>
/// <param name="key"></param>
/// <param name="predicate"></param>
/// <returns></returns>
/// <exception cref="InvalidOperationException"></exception>
    // Use it before adding new predicates
    public async Task SetPredicate(FastName key, Predicate predicate)
    {
        string newPredicateStr = BlackboardExtensions.FormatPredicate(predicate);
        
        // Check for identical predicate
        if (PredicateValues.Values.Any(p => BlackboardExtensions.FormatPredicate(p) == newPredicateStr))
        {
            Console.WriteLine($"Identical predicate already exists: {newPredicateStr}");
            return;
        }

        SetPredicateSecondary(key, predicate);
        
        if (_driver == null)
        {
            throw new InvalidOperationException("Neo4j driver not initialized");
        }

        var parameters = predicate.GetAllProperties();
        
        using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            var paramList = parameters
                .Where(p => p.Key != "PredicateName" && p.Key != "PredicateType" && p.Key != "isNegated")
                .ToList();

            string query;
            var queryParams = new Dictionary<string, object>();

            if (paramList.Count == 1)
            {
                var value = paramList[0].Value as IEntity;
                query = $@"
                    MERGE (p0:{paramList[0].Value.GetType().Name} {{name: $firstParamName}})
                    SET p0:{predicate.PredicateName}
                    RETURN p0";

                queryParams.Add("firstParamName", (value as Entity)?.NameKey.ToString() ?? paramList[0].Value.ToString());
            }
            else if (paramList.Count == 2)
            {
                var value1 = paramList[0].Value as IEntity;
                var value2 = paramList[1].Value as IEntity;
                query = $@"
                    MERGE (p0:{paramList[0].Value.GetType().Name} {{name: $firstParamName}})
                    MERGE (p1:{paramList[1].Value.GetType().Name} {{name: $secondParamName}})
                    MERGE (p0)-[r:{predicate.PredicateName}]->(p1)
                    RETURN p0, p1";

                queryParams.Add("firstParamName", (value1 as Entity)?.NameKey.ToString() ?? paramList[0].Value.ToString());
                queryParams.Add("secondParamName", (value2 as Entity)?.NameKey.ToString() ?? paramList[1].Value.ToString());
            }
            else
            {
                throw new ArgumentException($"Unsupported number of parameters: {paramList.Count}");
            }

            await tx.RunAsync(query, queryParams);
        });
    }

    // Implement IDisposable to properly close Neo4j connection
    public void Dispose()
    {
        _graphService?.Dispose();
    }
    
    public async Task<bool> TestNeo4jConnection()
    {
        try
        {
            return await _graphService.TestConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Neo4j connection test failed: {ex.Message}");
            return false;
        }
    }

    // Set methods
   

    public void SetLayer(FastName key, Layer value)
    {
        LayerValues[key] = value;
        value.NameKey = key;  // Set the instance ID
    }

    public void SetModule(FastName key, Module value)
    {
        ModuleValues[key] = value;
        value.NameKey = key;  // Set the instance ID
    }

    public void SetTool(FastName key, Tool value)
    {
        ToolValues[key] = value;
        value.NameKey = key;  // Set the instance ID
    }

    public Layer GetLayer(FastName key)
    {
        if (!LayerValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find a value for {key} this key");
        }
        return LayerValues[key];
    }

    public Module GetModule(FastName key)
    {
        if (!ModuleValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find a value for {key} this key");
        }
        return ModuleValues[key];
    }

    public Tool GetTool(FastName key)
    {
        if (!ToolValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find a value for {key} this key");
        }
        return ToolValues[key];
    }

    // Get and Set methods for States
    public State GetState(FastName key)
    {
        if (!StateValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find state for key: {key}");
        }
        return StateValues[key];
    }

    public void SetState(FastName key, State value)
    {
        if (!StateValues.ContainsKey(key))
        {
            StateValues[key] = value;
        }
        StateValues[key] = value;
        Console.WriteLine($"Successfully added State to Blackboard with key: {key}");
    }

    public IDriver GetDriver()
    {
        if (_driver == null)
            throw new InvalidOperationException("Neo4j driver not initialized");
        return _driver;
    }
}