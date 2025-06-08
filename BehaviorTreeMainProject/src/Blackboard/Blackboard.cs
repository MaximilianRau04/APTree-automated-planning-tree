using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using System.Linq;
using System.Reflection;


//we need to fix the query so that the parent types are also added to the graph
public class Blackboard<FastName> : IDisposable
{
    Dictionary<FastName, int>           IntValues =            new ();
    Dictionary<FastName, double>        DoubleValues =         new ();
    Dictionary<FastName, bool>          BoolValues =           new (); 
    Dictionary<FastName, string>        StringValues =         new ();
    Dictionary<FastName, Element>   ElementValues =    new ();
    Dictionary<FastName, Location>   LocationValues =    new ();
    Dictionary<FastName, Agent>   AgentValues =    new ();
    Dictionary<FastName, HashSet<Predicate>> PredicateValues = new();
    Dictionary<FastName, HashSet<Action>> ActionValues = new();
    Dictionary<FastName, HashSet<Type>> RegisteredActionTypeValues = new();
    Dictionary<FastName, HashSet<Type>> RegisteredElementTypeValues = new();
     Dictionary<FastName, HashSet<Type>> RegisteredLocationTypeValues = new(); // Add this new dictionary
    Dictionary<FastName, HashSet<Type>> RegisteredAgentTypeValues = new();
     Dictionary<FastName, HashSet<Type>> RegisteredPredicateTypeValues = new(); // Add this new dictionary
     Dictionary<FastName, HashSet<Type>> RegisteredLayerTypeValues = new();
     Dictionary<FastName, HashSet<Type>> RegisteredModuleTypeValues = new();
     Dictionary<FastName, Layer> LayerValues = new();
     Dictionary<FastName, Module> ModuleValues = new();
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
    public HashSet<Predicate> GetPredicates(FastName key)
    {
        if (!PredicateValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find predicates for {key}");
        }
        return PredicateValues[key];
    }

     // Update corresponding Get methods
public HashSet<Type> GetElementTypes(FastName key)
{
    if (!RegisteredElementTypeValues.ContainsKey(key))
    {
        throw new ArgumentException($"Could not find element types for {key}");
    }
    return RegisteredElementTypeValues[key];
}

public HashSet<Type> GetLocationTypes(FastName key)
{
    if (!RegisteredLocationTypeValues.ContainsKey(key))
    {
        throw new ArgumentException($"Could not find location types for {key}");
    }
    return RegisteredLocationTypeValues[key];
}

public HashSet<Type> GetPredicateTypes(FastName key)
{
    if (!RegisteredPredicateTypeValues.ContainsKey(key))
    {
        throw new ArgumentException($"Could not find predicate types for {key}");
    }
    return RegisteredPredicateTypeValues[key];
}

public HashSet<Type> GetActionTypes(FastName key)
{
    if (!RegisteredActionTypeValues.ContainsKey(key))
    {
        throw new ArgumentException($"Could not find action types for {key}");
    }
    return RegisteredActionTypeValues[key];
}

// Get methods
public HashSet<Type> GetLayerTypes(FastName key)
{
    if (!RegisteredLayerTypeValues.ContainsKey(key))
    {
        throw new ArgumentException($"Could not find layer types for {key}");
    }
    return RegisteredLayerTypeValues[key];
}

public HashSet<Type> GetModuleTypes(FastName key)
{
    if (!RegisteredModuleTypeValues.ContainsKey(key))
    {
        throw new ArgumentException($"Could not find module types for {key}");
    }
    return RegisteredModuleTypeValues[key];
}

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

    public void SetAgentType(FastName key, Type agentType)
    {
        if (!typeof(Agent).IsAssignableFrom(agentType))
        {
            throw new ArgumentException($"Type {agentType.Name} is not an Agent type");
        }

        if (!RegisteredAgentTypeValues.ContainsKey(key))
        {
            RegisteredAgentTypeValues[key] = new HashSet<Type>();
        }
        RegisteredAgentTypeValues[key].Add(agentType);
    }

   public void SetElementType(FastName key, Type elementType)
{
    if (!typeof(Element).IsAssignableFrom(elementType))
    {
        throw new ArgumentException($"Type {elementType.Name} is not an Element type");
    }

    if (!RegisteredElementTypeValues.ContainsKey(key))
    {
        RegisteredElementTypeValues[key] = new HashSet<Type>();
    }
    RegisteredElementTypeValues[key].Add(elementType);
}

// Location type methods
public void SetLocationType(FastName key, Type locationType)
{
    if (!typeof(Location).IsAssignableFrom(locationType))
    {
        throw new ArgumentException($"Type {locationType.Name} is not a Location type");
    }

    if (!RegisteredLocationTypeValues.ContainsKey(key))
    {
        RegisteredLocationTypeValues[key] = new HashSet<Type>();
    }
    RegisteredLocationTypeValues[key].Add(locationType);
}

// Predicate type methods
public void SetPredicateType(FastName key, Type predicateType)
{
    if (!typeof(Predicate).IsAssignableFrom(predicateType))
    {
        throw new ArgumentException($"Type {predicateType.Name} is not a Predicate type");
    }

    if (!RegisteredPredicateTypeValues.ContainsKey(key))
    {
        RegisteredPredicateTypeValues[key] = new HashSet<Type>();
    }
    RegisteredPredicateTypeValues[key].Add(predicateType);
}

// Action type methods
public void SetActionType(FastName key, Type actionType)
{
    if (!typeof(GenericBTAction).IsAssignableFrom(actionType))
    {
        throw new ArgumentException($"Type {actionType.Name} is not an Action type");
    }

    if (!RegisteredActionTypeValues.ContainsKey(key))
    {
        RegisteredActionTypeValues[key] = new HashSet<Type>();
    }
    RegisteredActionTypeValues[key].Add(actionType);
}

    // Set methods for predicates
    private void SetPredicateSecondary(FastName key, Predicate predicate)
    {
        string newPredicateStr = FormatPredicate(predicate);
        
        if (!PredicateValues.ContainsKey(key))
        {
            PredicateValues[key] = new HashSet<Predicate>();
        }
        
        // Check if identical predicate exists
        if (!PredicateValues[key].Any(p => FormatPredicate(p) == newPredicateStr))
        {
            PredicateValues[key].Add(predicate);
        }
    }
    

    public bool HasSimilarPredicate(Predicate newPredicate)
    {
        foreach (var predicateSet in PredicateValues.Values)
        {
            foreach (var existingPredicate in predicateSet)
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
        string newPredicateStr = FormatPredicate(predicate);
        
        // Check for identical predicate
        if (PredicateValues.Values.Any(set => set.Any(p => FormatPredicate(p) == newPredicateStr)))
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

                queryParams.Add("firstParamName", value?.NameKey.ToString() ?? paramList[0].Value.ToString());
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

                queryParams.Add("firstParamName", value1?.NameKey.ToString() ?? paramList[0].Value.ToString());
                queryParams.Add("secondParamName", value2?.NameKey.ToString() ?? paramList[1].Value.ToString());
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
    public void SetLayerType(FastName key, Type layerType)
    {
        if (!typeof(Layer).IsAssignableFrom(layerType))
        {
            throw new ArgumentException($"Type {layerType.Name} is not a Layer type");
        }

        if (!RegisteredLayerTypeValues.ContainsKey(key))
        {
            RegisteredLayerTypeValues[key] = new HashSet<Type>();
        }
        RegisteredLayerTypeValues[key].Add(layerType);
    }

    public void SetModuleType(FastName key, Type moduleType)
    {
        if (!typeof(Module).IsAssignableFrom(moduleType))
        {
            throw new ArgumentException($"Type {moduleType.Name} is not a Module type");
        }

        if (!RegisteredModuleTypeValues.ContainsKey(key))
        {
            RegisteredModuleTypeValues[key] = new HashSet<Type>();
        }
        RegisteredModuleTypeValues[key].Add(moduleType);
    }

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

    private string FormatPredicate(Predicate predicate)
    {
        var parameters = predicate.GetAllProperties()
            .Where(p => p.Key != "PredicateName" && p.Key != "PredicateType" && p.Key != "isNegated")
            .Select(p => {
                var value = p.Value;
                if (value is Element element)
                {
                    return element.NameKey.ToString();
                }
                else if (value is Layer layer)
                {
                    return layer.NameKey.ToString();
                }
                else if (value is Module module)
                {
                    return module.NameKey.ToString();
                }
                else if (value is Location location)
                {
                    return location.NameKey.ToString();
                }
                return value?.ToString() ?? "";
            })
            .ToList();

        var result = $"{predicate.PredicateName}({string.Join(",", parameters)})";
        Console.WriteLine($"Formatted predicate: {result}");
        return result;
    }
}