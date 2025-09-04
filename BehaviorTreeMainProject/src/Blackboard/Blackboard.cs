using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using System.Linq;
using System.Reflection;
using BehaviorTreeMainProject.Services;
using BehaviorTreeMainProject.Log.Services;


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
    private Dictionary<FastName, Predicate> PredicateValues = new();
    Dictionary<FastName, GenericBTAction> ActionValues = new();
    Dictionary<FastName, IBTNode> FlowNodeValues = new();
     Dictionary<FastName, State> StateValues = new();
    Dictionary<FastName, NodeGraph> NodeGraphValues = new();
    Dictionary<FastName, BTFlowNode_Dynamic> InjectedSubtreesValues = new();
   
    private readonly IDriver _driver;
    private readonly Neo4jService _graphService;

    /// <summary>
    /// Controls whether the system is in planning phase (true) or execution phase (false)
    /// During planning phase, HL actions only generate NodeGraphs without executing ML actions
    /// </summary>
    public bool PlanningPhase { get; set; } = true;
    public int LowestCost { get; set; } = 0;

    /// <summary>
    /// Array to track when each cassette has generated and inserted its subtree
    /// Index 0 = cassette1, Index 1 = cassette2, Index 2 = cassette3, Index 3 = cassette4
    /// </summary>
    public bool[] CassetteSubtreeCompleted { get; set; } = new bool[4] { false, false, false, false };

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

    public IBTNode GetFlowNode(FastName key)
    {
        if (!FlowNodeValues.ContainsKey(key))
        {
            throw new System.ArgumentException($"could not find a value for {key} this key");
        }
        return FlowNodeValues[key];
    }
    public List<IBTNode> GetAllFlowNodes()
    {
        return FlowNodeValues.Values.ToList();
    }
    public void SetFlowNodeInstance(FastName key, BTFlowNodeBase value)
    {
        if (!FlowNodeValues.ContainsKey(key))
        {
            FlowNodeValues[key] = value;
            // Log new flow node instance created
            BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Flow node instance: {value.GetType().Name}");
        }
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
        
        // Log new element instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Element instance: {value.GetType().Name}");
    }

    public void SetLocation(FastName key, Location value)
    {
        LocationValues[key] = value;
        value.NameKey = key;  // Set the instance ID
        
        // Log new location instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Location instance: {value.GetType().Name}");
    }

    public void SetAgent(FastName key, Agent value)
    {
        AgentValues[key] = value;
        value.NameKey = key;  // Set the instance ID
        
        // Log new agent instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Agent instance: {value.GetType().Name}");
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
        // Log new entity type added
        BlackboardTrackingLogger.LogNewType(key.ToString(), "Entity", $"Entity type: {elementType.GetType().Name}");
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
        // Log new entity type registered
        BlackboardTrackingLogger.LogNewType(typeName.ToString(), "Entity", "Registered entity type");
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
        // Log new predicate type registered
        BlackboardTrackingLogger.LogNewType(typeName.ToString(), "Predicate", "Registered predicate type");
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
        // Log new action type registered
        BlackboardTrackingLogger.LogNewType(typeName.ToString(), "Action", "Registered action type");
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

    // Check if this is a new predicate type (based on the actual type, not the key)
    var predicateTypeName = predicateType.GetType().Name;
    var typeKey = new FastName(predicateTypeName);
    
    if (!AvailablePredicateTypes.Contains(typeKey))
    {
        AvailablePredicateTypes.Add(typeKey);
        // Log new predicate type added (only for the actual type name, not instance key)
        BlackboardTrackingLogger.LogNewType(predicateTypeName, "Predicate", "Registered predicate type");
    }
}

    // Action type methods
    public void SetActionType(FastName key, GenericBTAction actionType)
    {
        if (!typeof(GenericBTAction).IsAssignableFrom(actionType.GetType()))
        {
            throw new ArgumentException($"Type {actionType.GetType().Name} is not an Action type");
        }

        // Check if this is a new action type (based on the actual type, not the key)
        var actionTypeName = actionType.GetType().Name;
        var typeKey = new FastName(actionTypeName);

        if (!AvailableActionTypes.Contains(typeKey))
        {
            AvailableActionTypes.Add(typeKey);
            // Log new action type added (only for the actual type name, not instance key)
            BlackboardTrackingLogger.LogNewType(actionTypeName, "Action", "Registered action type");
        }

        // // Store the action instance
        // ActionValues[key] = actionType;

        // // Log new action instance created
        // BlackboardTrackingLogger.LogNewInstance(key.ToString(), actionTypeName, "Blackboard", $"Action instance: {actionTypeName}");
    }

public void SetActionInstance(FastName key, GenericBTAction actionInstance)
{
    if (!ActionValues.ContainsKey(key))
    {
        ActionValues[key] = actionInstance;
        // Log new action instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), actionInstance.GetType().Name, "Blackboard", $"Action instance: {actionInstance.GetType().Name}");
    }
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
        LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: SetPredicateSecondary called with key: {key}");
        
        string newPredicateStr = BlackboardExtensions.FormatPredicate(predicate);
        LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: Formatted predicate string: {newPredicateStr}");
        
        LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: Current PredicateValues count: {PredicateValues.Count}");
        
        // Check if identical predicate exists
        if (!PredicateValues.ContainsKey(key))
        {
            LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: Key {key} not found in PredicateValues, adding new predicate");
            PredicateValues[key] = predicate;
            LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: PredicateValues count after adding: {PredicateValues.Count}");
            LoggingService.LogSuccess($"🔧 BLACKBOARD_SECONDARY: Successfully added predicate with key: {key}");
        }
        else if (BlackboardExtensions.FormatPredicate(PredicateValues[key]) != newPredicateStr)
        {
            LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: Key {key} exists but predicate content differs, updating predicate");
            LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: Old predicate: {BlackboardExtensions.FormatPredicate(PredicateValues[key])}");
            LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: New predicate: {newPredicateStr}");
            PredicateValues[key] = predicate;
            LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: PredicateValues count after updating: {PredicateValues.Count}");
            LoggingService.LogSuccess($"🔧 BLACKBOARD_SECONDARY: Successfully updated predicate with key: {key}");
        }
        else
        {
            LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: Key {key} exists and predicate content is identical, no update needed");
        }
        
        // Final verification
        var finalCount = PredicateValues.Count;
        var keyExists = PredicateValues.ContainsKey(key);
        LoggingService.LogInfo($"🔧 BLACKBOARD_SECONDARY: Final verification - Count: {finalCount}, Key exists: {keyExists}");
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
        LoggingService.LogInfo($"🔧 BLACKBOARD: SetPredicate called with key: {key}");
        LoggingService.LogInfo($"🔧 BLACKBOARD: Predicate type: {predicate.GetType().Name}");
        LoggingService.LogInfo($"🔧 BLACKBOARD: Predicate.PredicateName: {predicate.PredicateName}");
        LoggingService.LogInfo($"🔧 BLACKBOARD: Predicate.isNegated: {predicate.isNegated}");
        
        string newPredicateStr = BlackboardExtensions.FormatPredicate(predicate);
        LoggingService.LogInfo($"🔧 BLACKBOARD: Formatted predicate string: {newPredicateStr}");
        
        // Check for identical predicate
        if (PredicateValues.Values.Any(p => BlackboardExtensions.FormatPredicate(p) == newPredicateStr))
        {
            LoggingService.LogWarning($"🔧 BLACKBOARD: Identical predicate already exists: {newPredicateStr}");
            return;
        }

        LoggingService.LogInfo($"🔧 BLACKBOARD: Calling SetPredicateSecondary with key: {key}");
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

    public void SetPredicateSync(FastName key, Predicate predicate)
    {
        // NEW: Clear, prominent logging for predicate additions
        LoggingService.LogInfo("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        LoggingService.LogInfo($"➕ PREDICATE_ADDED: Adding predicate to blackboard");
        LoggingService.LogInfo($"   Key: {key}");
        LoggingService.LogInfo($"   Type: {predicate.GetType().Name}");
        LoggingService.LogInfo($"   PredicateName: {predicate.PredicateName}");
        LoggingService.LogInfo($"   isNegated: {predicate.isNegated}");
        LoggingService.LogInfo($"   Current total predicates: {PredicateValues.Count}");
        LoggingService.LogInfo("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        // NEW: Clean up conflicting atAgent predicates when updating location
        if (predicate.GetPredicateType() == "atAgent" && !predicate.isNegated)
        {
            CleanupConflictingAtAgentPredicates(predicate);
        }
        // Check if a predicate with the same key already exists
        if (PredicateValues.ContainsKey(key))
        {
            var existingPredicate = PredicateValues[key];
            LoggingService.LogWarning($"⚠️ PREDICATE_UPDATE: Key '{key}' already exists - updating negation");
            LoggingService.LogInfo($"   Old isNegated: {existingPredicate.isNegated} → New isNegated: {predicate.isNegated}");

            // Update the isNegated property of the existing predicate
            var oldNegationValue = existingPredicate.isNegated;
            existingPredicate.isNegated = predicate.isNegated;

            // Log predicate negation change
            BlackboardTrackingLogger.LogPredicateNegation(key.ToString(), oldNegationValue, predicate.isNegated, "Blackboard", "Updated existing predicate negation");

            LoggingService.LogSuccess($"✅ PREDICATE_UPDATE: Successfully updated negation for key: {key}");
            return;
        }

        // Check for identical predicate (different key but same content)
        string newPredicateStr = BlackboardExtensions.FormatPredicate(predicate);
        if (PredicateValues.Values.Any(p => BlackboardExtensions.FormatPredicate(p) == newPredicateStr))
        {
            LoggingService.LogWarning($"⚠️ PREDICATE_DUPLICATE: Identical predicate content already exists: {newPredicateStr}");
            return;
        }

        // Store the predicate in the dictionary
        PredicateValues[key] = predicate;
        
        // Log new predicate instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), predicate.GetType().Name, "Blackboard", $"Predicate instance: {predicate.PredicateName}");
        
        // Verify the predicate was actually added
        var foundInDict = PredicateValues.ContainsKey(key);
        if (!foundInDict)
        {
            LoggingService.LogError($"❌ PREDICATE_ERROR: Failed to add predicate with key {key}!");
        }
        else
        {
            LoggingService.LogSuccess($"✅ PREDICATE_ADDED: Successfully stored predicate with key: {key} (Total: {PredicateValues.Count})");
        }
    }



    // Implement IDisposable to properly close Neo4j connection
    public void Dispose()
    {
        _graphService?.Dispose();
        
        // Close the blackboard tracking logger
        BlackboardTrackingLogger.Close();
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
        
        // Log new layer instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Layer instance: {value.GetType().Name}");
    }

    public void SetModule(FastName key, Module value)
    {
        ModuleValues[key] = value;
        value.NameKey = key;  // Set the instance ID
        
        // Log new module instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Module instance: {value.GetType().Name}");
    }

    public void SetTool(FastName key, Tool value)
    {
        ToolValues[key] = value;
        value.NameKey = key;  // Set the instance ID
        
        // Log new tool instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Tool instance: {value.GetType().Name}");
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
            // Log new state instance created
            BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"State instance: {value.GetType().Name}");
        }
        StateValues[key] = value;
        Console.WriteLine($"Successfully added State to Blackboard with key: {key}");
    }

    // Get and Set methods for NodeGraphs
    public NodeGraph GetNodeGraph(FastName key)
    {
        if (!NodeGraphValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find NodeGraph for key: {key}");
        }
        return NodeGraphValues[key];
    }

    public void SetNodeGraph(FastName key, NodeGraph value)
    {
        NodeGraphValues[key] = value;
        Console.WriteLine($"Successfully added NodeGraph to Blackboard with key: {key}");
        
        // Log new node graph instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"NodeGraph instance: {value.GetType().Name}");
    }

    /// <summary>
    /// Gets all NodeGraph instances from the blackboard
    /// </summary>
    /// <returns>List of all NodeGraph instances</returns>
    public List<NodeGraph> GetAllNodeGraphs()
    {
        return NodeGraphValues.Values.ToList();
    }

    // Get and Set methods for Injected Subtrees
    public BTFlowNode_Dynamic GetInjectedSubtree(FastName key)
    {
        if (!InjectedSubtreesValues.ContainsKey(key))
        {
            throw new ArgumentException($"Could not find injected subtree for key: {key}");
        }
        return InjectedSubtreesValues[key];
    }

    public void SetInjectedSubtree(FastName key, BTFlowNode_Dynamic value)
    {
        InjectedSubtreesValues[key] = value;
        Console.WriteLine($"Successfully added injected subtree to Blackboard with key: {key}");
        
        // Log new injected subtree instance created
        BlackboardTrackingLogger.LogNewInstance(key.ToString(), value.GetType().Name, "Blackboard", $"Injected subtree instance: {value.GetType().Name}");
    }

    /// <summary>
    /// Gets all injected subtrees from the blackboard
    /// </summary>
    /// <returns>List of all injected subtrees</returns>
    public List<BTFlowNode_Dynamic> GetAllInjectedSubtrees()
    {
        return InjectedSubtreesValues.Values.ToList();
    }

    /// <summary>
    /// Clears all injected subtrees from the blackboard
    /// </summary>
    public void ClearInjectedSubtrees()
    {
        InjectedSubtreesValues.Clear();
        Console.WriteLine("Cleared all injected subtrees from Blackboard");
    }
     public List<Predicate> GetAllPredicates()
    {
        return PredicateValues.Values.ToList();
    }

    /// <summary>
    /// Gets all non-negated (positive) predicates from the blackboard
    /// </summary>
    /// <returns>List of all predicates where isNegated is false</returns>
    public List<Predicate> GetTruePredicates()
    {
        var truePredicates = new List<Predicate>();

        foreach (var predicate in PredicateValues.Values)
        {
            if (!predicate.isNegated)
            {
                truePredicates.Add(predicate);
            }
        }

        return truePredicates;
    }
    private void CleanupConflictingAtAgentPredicates(Predicate newLocationPredicate)
{
    try
    {
        // Extract robot and location from the new predicate
        var newLocationStr = newLocationPredicate.GetParameterValues();
        if (newLocationStr.Count < 2) return;

        string robotName = newLocationStr[0];
        string newLocation = newLocationStr[1];

        LoggingService.LogInfo($"🧹 CLEANUP: Cleaning up ALL atAgent predicates for robot {robotName}");

        // Find ALL blackboard keys that contain "atAgent" for this robot
        var keysToRemove = new List<FastName>();
        
        foreach (var kvp in PredicateValues)
        {
            string keyName = kvp.Key.ToString();
            
            // Check if this key contains "atAgent" and the robot name
            if (keyName.Contains("atAgent") && keyName.Contains(robotName))
            {
                keysToRemove.Add(kvp.Key);
                LoggingService.LogInfo($"   🗑️ Marking for removal: {keyName}");
            }
        }

        // Remove ALL conflicting atAgent predicates
        foreach (var key in keysToRemove)
        {
            PredicateValues.Remove(key);
            LoggingService.LogInfo($"   ✅ Removed: {key}");
        }

        LoggingService.LogSuccess($"�� CLEANUP: Removed {keysToRemove.Count} atAgent predicates for robot {robotName}");
        LoggingService.LogInfo($"   📍 New location will be: {robotName} at {newLocation}");
    }
    catch (Exception ex)
    {
        LoggingService.LogError($"❌ CLEANUP: Error during cleanup: {ex.Message}");
    }
}



  
}