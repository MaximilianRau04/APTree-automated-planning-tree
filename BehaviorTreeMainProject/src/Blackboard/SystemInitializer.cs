





/// <summary>
/// This class is responsible for initializing the system by registering entity types, action types, and predicate types.
/// It also creates initial entities and adds them to the blackboard.
/// </summary>
public class SystemInitializer
{
    private  Blackboard<FastName> blackboard;
    private  EntityFactory entityFactory;
    private  ActionFactory actionFactory;
    private readonly PredicateFactory predicateFactory;

    public SystemInitializer(Blackboard<FastName> blackboard)
    {
        this.blackboard = blackboard; 
        this.entityFactory = EntityFactory.Instance;
        this.actionFactory = ActionFactory.Instance;
        this.predicateFactory = PredicateFactory.Instance;
    }

    public void InitializeTypesFromPDDL(string pddlFilePathentities, string pddlFilePathpredicates, string pddlFilePathactions)
    {
        var entityTypes = Parser.generateEntityTypesfromPDDLDomain(pddlFilePathentities);
        RegisterEntityTypes(entityTypes);
        var predicateTypes = Parser.generatePredicateTypesfromPDDLDomain(pddlFilePathpredicates);
        //registering types
        RegisterPredicateTypes(predicateTypes);
        var actionTypes = Parser.generateActionTypesfromPDDLDomain(pddlFilePathactions);
         RegisterActionTypes(actionTypes);

       // InitializeTypes(entityTypes, predicateTypes, actionTypes);
    }

     public void InitializeTypes(Dictionary<string, EntityTypeInfo> entityTypes,  Dictionary<string, PredicateTypeInfo> predicateTypes, Dictionary<string, ActionTypeInfo> actionTypes)
    {
        Console.WriteLine("Starting system initialization...");
        
        RegisterEntityTypes(entityTypes);
        
        RegisterPredicateTypes(predicateTypes);
        RegisterActionTypes(actionTypes);
      
        
        Console.WriteLine("System initialization completed.");
    }
/// <summary>
/// This method registers entity types with the entity factory and adds them to the blackboard.
/// </summary>
/// <param name="entityTypes">A dictionary of entity types to register.</param>
    public void RegisterEntityTypes( List<EntityTypeInfo> entityTypes)
    {
        Console.WriteLine("Registering entity types...");
        
        foreach (var info in entityTypes)
        {
            // Register type with factory
            var newType = entityFactory.RegisterEntityType(
                info.TypeName.ToString(), 
                info.BaseType, 
                info.Properties
            );

            // Add type to blackboard based on base type
            var key = new FastName(info.TypeName.ToString());
            if (info.BaseType == typeof(Element))
            {
                if (newType.IsAssignableTo(typeof(Element)))
                {
                    blackboard.SetElementType(key, newType);
                }
                else
                {
                    throw new InvalidCastException($"Type {info.TypeName.ToString()} was registered as Element but cannot be cast to Element");
                }
            }
            else if (info.BaseType == typeof(Agent))
            {
                if (newType.IsAssignableTo(typeof(Agent)))
                {
                    blackboard.SetAgentType(key, newType);
                }
                else
                {
                    throw new InvalidCastException($"Type {info.TypeName.ToString()} was registered as Agent but cannot be cast to Agent");
                }
            }
            else if (info.BaseType == typeof(Location))
            {
                if (newType.IsAssignableTo(typeof(Location)))
                {
                    blackboard.SetLocationType(key, newType);
                }
                else
                {
                    throw new InvalidCastException($"Type {info.TypeName.ToString()} was registered as Location but cannot be cast to Location");
                }
            }
            
            Console.WriteLine($"Registered and added {info.TypeName.ToString()} to blackboard");
        }
    }
/// <summary>
/// This method registers predicate types with the predicate factory and adds them to the blackboard.
/// </summary>
/// <param name="predicateTypes"></param>

    public void RegisterPredicateTypes(List< PredicateTypeInfo> predicateTypes)
    {
        Console.WriteLine("Registering predicate types...");
        
        foreach (var  info in predicateTypes)
        {
            // Define predicate type in factory
            predicateFactory.DefinePredicateType(info.PredicateName.ToString(), info.Parameters);

            // Add type to blackboard
            var predicateType = predicateFactory.GetPredicateType(info.PredicateName.ToString());
            blackboard.SetPredicateType(info.PredicateName, predicateType);
            
            Console.WriteLine($"Registered and added predicate {info.PredicateName.ToString()} to blackboard");
        }
    }
    /// <summary>
    /// This method registers action types with the action factory and adds them to the blackboard.
    /// </summary>
    /// <param name="actionTypes"></param>

     public void RegisterActionTypes(Dictionary<string, ActionTypeInfo> actionTypes)
    {
        Console.WriteLine("Registering action types...");
        
        foreach (var (actionName, info) in actionTypes)
        {
            actionFactory.RegisterActionType(actionName, info.Parameters, info.PreconditionTemplates, info.EffectTemplates, info.ActionLogic);
            var actionType = actionFactory.GetActionType(actionName);
            blackboard.SetActionType(new FastName(actionName), actionType);
            
            Console.WriteLine($"Registered and added action {actionName} to blackboard");
        }
    }
public void InitializeObjectsFromText(string pddlProblemFilePath)
{
    try
    {
        Console.WriteLine($"\nReading PDDL file: {pddlProblemFilePath}");
        string content = File.ReadAllText(pddlProblemFilePath);
        
        // Extract objects section
        int objectsStart = content.IndexOf("(:objects");
        if (objectsStart == -1)
        {
            throw new FormatException("Could not find (:objects section in PDDL file");
        }

        int objectsEnd = content.IndexOf(")", objectsStart);
        Console.WriteLine("Found objects section. Parsing...");
        
        // Debug output for registered types
        Console.WriteLine("\nRegistered entity types:");
        foreach (var type in entityFactory.registeredEntityTypes)
        {
            Console.WriteLine($"- {type.Key}");
        }

        string objectsSection = content.Substring(objectsStart, objectsEnd - objectsStart + 1);
        
        // Process each object definition with debug output
        string[] lines = objectsSection.Split('\n');
        Dictionary<string, List<string>> objectsByType = new Dictionary<string, List<string>>();

        foreach (string line in lines.Skip(1))
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(")")) continue;
            
            Console.WriteLine($"\nProcessing line: {trimmedLine}");
            string[] parts = trimmedLine.Split('-');
            if (parts.Length == 2)
            {
                string typeName = parts[1].Trim().ToLower();
                string[] objectNames = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                Console.WriteLine($"Type: {typeName}");
                Console.WriteLine($"Objects: {string.Join(", ", objectNames)}");
                
                if (!objectsByType.ContainsKey(typeName))
                {
                    objectsByType[typeName] = new List<string>();
                }
                objectsByType[typeName].AddRange(objectNames);
            }
        }

        // Create and register objects
        foreach (var (typeName, objects) in objectsByType)
        {
            // Check if type is registered
            var registeredType = entityFactory.registeredEntityTypes
                .FirstOrDefault(t => t.Key.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            if (registeredType.Key == null)
            {
                throw new ArgumentException($"Object type '{typeName}' is not registered");
            }

            foreach (string objectName in objects)
            {
                // Create object instance
                var instance = entityFactory.CreateInstance(typeName);
                
                // Add to appropriate blackboard collection based on base type
                if (instance is Element element)
                {
                    blackboard.SetElement(new FastName(objectName), element);
                    Console.WriteLine($"Created Element: {objectName} of type {typeName}");
                }
                else if (instance is Agent agent)
                {
                    blackboard.SetAgent(new FastName(objectName), agent);
                    Console.WriteLine($"Created Agent: {objectName} of type {typeName}");
                }
                else if (instance is Location location)
                {
                    blackboard.SetLocation(new FastName(objectName), location);
                    Console.WriteLine($"Created Location: {objectName} of type {typeName}");
                }
                else if (instance is Layer layer)
                {
                    blackboard.SetLayer(new FastName(objectName), layer);
                    Console.WriteLine($"Created Layer: {objectName} of type {typeName}");
                }
                else if (instance is Module module)
                {
                    blackboard.SetModule(new FastName(objectName), module);
                    Console.WriteLine($"Created Module: {objectName} of type {typeName}");
                }
                else
                {
                    throw new InvalidOperationException($"Unknown base type for object {objectName} of type {typeName}");
                }
            }
        }

        Console.WriteLine("Object initialization completed.");
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error in InitializeObjectsFromPDDL: {e.Message}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
        throw;
    }
}
/// <summary>
/// This method initializes the state from the PDDL file.
/// </summary>
/// <param name="pddlProblemFilePath"></param>
/// <param name="stateName"></param>
/// <param name="stateType"></param>
/// <exception cref="FormatException"></exception>
public async Task<List<Predicate>> InitializePredicatesFromtext(string pddlPredicatesFilePath)
{
    try
    {
        Console.WriteLine($"\nReading predicates file: {pddlPredicatesFilePath}");
        string[] lines = File.ReadAllLines(pddlPredicatesFilePath);
        var createdPredicates = new List<Predicate>();
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine)) continue;
            
            Console.WriteLine($"\nProcessing predicate: {trimmedLine}");
            string predicateContent = trimmedLine.Trim('(', ')');
            var parts = predicateContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length > 0)
            {
                string predicateName = parts[0];
                //get the parameters
                var parameters = parts.Skip(1).ToArray();

                // Create predicate instance using factory
                var paramDict = new Dictionary<string, object>();
                //get the predicate type
                var predicateType = predicateFactory.GetPredicateType(predicateName);

                Console.WriteLine($"Debug - Predicate type: {predicateType?.FullName ?? "null"}");
                if (predicateType == null)
                {
                    throw new ArgumentException($"Unknown predicate type: {predicateName}");
                }
//create the predicate instance
                Console.WriteLine($"Debug - Creating predicate {predicateName} with parameters:");

                // Log ALL properties before filtering
                var allProperties = predicateType.GetProperties();
                Console.WriteLine($"Debug - All properties: {string.Join(", ", allProperties.Select(p => p.Name))}");

                // Get only the parameter properties (excluding metadata properties)
                var properties = predicateType.GetProperties()
                    .Where(p => p.Name != "PredicateName" && p.Name != "PredicateType" && p.Name != "isNegated")
                    .ToArray();

                Console.WriteLine($"Debug - Parameter properties: {string.Join(", ", properties.Select(p => p.Name))}");
                Console.WriteLine($"Debug - PDDL parameters: {string.Join(", ", parameters)}");

                if (properties.Length != parameters.Length)
                {
                    throw new ArgumentException(
                        $"Mismatch for predicate {predicateName}: " +
                        $"Expected {properties.Length} parameters ({string.Join(", ", properties.Select(p => p.Name))}) " +
                        $"but got {parameters.Length} ({string.Join(", ", parameters)})"
                    );
                }

                for (int i = 0; i < properties.Length; i++)
                {
                    // Get the parameter value from the PDDL file (e.g. "lp1" or "fp1")
                    var paramValue = parameters[i];
                    
                    // Get the parameter name and type from the predicate definition
                    var propertyInfo = properties[i];  // Get property info by position
                    var paramName = propertyInfo.Name;  // This will be "obj", "p", etc.
                    var paramType = propertyInfo.PropertyType;  // This will be Element, Location, etc.
                    
                    Console.WriteLine($"Debug - Parameter {i}: Name={paramName}, Type={paramType.Name}, Value={paramValue}");
                    
                    // Get the object from the appropriate blackboard dictionary based on its type
                    object value;
                    if (paramType.IsAssignableTo(typeof(Element)))
                    {
                        value = blackboard.GetElement(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Location)))
                    {
                        value = blackboard.GetLocation(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Layer)))
                    {
                        value = blackboard.GetLayer(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Module)))
                    {
                        value = blackboard.GetModule(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Agent)))
                    {
                        value = blackboard.GetAgent(new FastName(paramValue));
                    }
                    else
                    {
                        throw new ArgumentException($"Unsupported parameter type: {paramType.Name}");
                    }
                    
                    paramDict.Add(paramName, value);
                    Console.WriteLine($"  Added to dictionary: {paramName}={paramValue}");
                }

                var predicate = predicateFactory.CreatePredicateInstance(predicateName, paramDict, blackboard);
                if (predicate == null)
                {
                    throw new InvalidOperationException($"Failed to create predicate instance for {predicateName}");
                }

                Console.WriteLine($"Debug - Created predicate: {predicate.PredicateName}");
                Console.WriteLine($"Debug - Predicate properties: {string.Join(", ", predicate.GetAllProperties().Select(p => $"{p.Key}={p.Value}"))}");

                await blackboard.SetPredicate(new FastName(predicateName), predicate);
                createdPredicates.Add(predicate);
                Console.WriteLine($"Created predicate: {predicateName} with parameters: {string.Join(", ", parameters)}");
            }
        }

        return createdPredicates;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error initializing predicates: {e.Message}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
        throw;
    }
}


private object GetObjectFromBlackboard(string paramValue, Type paramType)
{
    if (paramType == typeof(Element))
    {
        return blackboard.GetElement(new FastName(paramValue));
    }
    else if (paramType == typeof(Location))
    {
        return blackboard.GetLocation(new FastName(paramValue));
    }
    else if (paramType == typeof(Layer))
    {
        return blackboard.GetLayer(new FastName(paramValue));
    }
    else if (paramType == typeof(Module))
    {
        return blackboard.GetModule(new FastName(paramValue));
    }
    else
    {
        throw new ArgumentException($"Unsupported parameter type: {paramType}");
    }
}

public async Task<State> CreateStateFromPDDL(string pddlPredicatesFilePath, string stateName, StateType stateType)
{
    try
    {
        Console.WriteLine($"\nCreating {stateType} state from file: {pddlPredicatesFilePath}");
        
        // Create a new state
        var state = new State(stateType, new FastName(stateName));
        
        // Initialize predicates and get the list
        var predicates = await InitializePredicatesFromtext(pddlPredicatesFilePath);
        
        int count = 0;
        foreach (var predicate in predicates)
        {
            state.AddPredicate(new FastName(predicate.PredicateName), predicate);
            count++;
        }
        
        Console.WriteLine($"Successfully created {stateType} state with {count} predicates");
        return state;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error creating state from PDDL: {e.Message}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
        throw;
    }
}

public async Task<List<GenericBTAction>> InitializeActionsFromPDDL(string pddlActionsFilePath)
{
    var createdActions = new List<GenericBTAction>();
    try
    {
        Console.WriteLine($"\nReading actions file: {pddlActionsFilePath}");
        string[] lines = File.ReadAllLines(pddlActionsFilePath);
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine)) continue;
            
            Console.WriteLine($"\nProcessing action: {trimmedLine}");
            string actionContent = trimmedLine.Trim('(', ')');
            var parts = actionContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length > 0)
            {
                string actionName = parts[0];
                var parameters = parts.Skip(1).ToArray();

                var actionType = actionFactory.GetActionType(actionName);

                if (actionType == null)
                {
                    throw new ArgumentException($"Unknown action type: {actionName}");
                }

                var properties = actionType.GetProperties()
                    .Where(p => p.Name != "ActionName" 
                             && p.Name != "ActionType"
                             && p.Name != "DebugDisplayName"
                             && p.Name != "DisplayName"
                             && p.Name != "HasChildren"
                             && p.Name != "Parent"
                             && p.Name != "Children"
                             && p.Name != "OwningTree"
                             && p.Name != "LinkedBlackboard"
                             && p.Name != "LastStatus"
                             && p.Name != "HasFinished"
                             && !p.Name.StartsWith("Debug"))
                    .ToArray();

                Console.WriteLine($"\nAction {actionName} expects parameters in this order:");
                foreach (var prop in properties)
                {
                    Console.WriteLine($"- {prop.Name}: {prop.PropertyType.Name}");
                }

                // Create dictionary with default values first
                var paramDict = new Dictionary<string, IEntity>();
                foreach (var prop in properties)
                {
                    paramDict[prop.Name] = null; // Set default value
                }

                // Then update with provided parameters
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramValue = parameters[i];
                    var propertyInfo = properties[i];
                    var paramName = propertyInfo.Name;
                    var paramType = propertyInfo.PropertyType;
                    
                    Console.WriteLine($"\nTrying to map parameter {paramValue} to {paramName} ({paramType.Name})");
                    
                    IEntity value;
                    if (paramType.IsAssignableTo(typeof(Element)))
                    {
                        Console.WriteLine($"  Trying to get Element: {paramValue}");
                        value = blackboard.GetElement(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Location)))
                    {
                        Console.WriteLine($"  Trying to get Location: {paramValue}");
                        value = blackboard.GetLocation(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Layer)))
                    {
                        Console.WriteLine($"  Trying to get Layer: {paramValue}");
                        value = blackboard.GetLayer(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Module)))
                    {
                        Console.WriteLine($"  Trying to get Module: {paramValue}");
                        value = blackboard.GetModule(new FastName(paramValue));
                    }
                    else if (paramType.IsAssignableTo(typeof(Agent)))
                    {
                        Console.WriteLine($"  Trying to get Agent: {paramValue}");
                        value = blackboard.GetAgent(new FastName(paramValue));
                    }
                    else
                        throw new ArgumentException($"Parameter {paramName} must be an Element, Location, Layer, Module, or Agent");
                    
                    paramDict[paramName] = value;
                }

                var action = actionFactory.CreateActionInstance(actionName, blackboard, paramDict);
                if (action == null)
                {
                    throw new InvalidOperationException($"Failed to create action instance for {actionName}");
                }

                createdActions.Add(action as GenericBTAction);
                Console.WriteLine($"Created action: {actionName} with parameters: {string.Join(", ", parameters)}");
            }
        }

        return createdActions;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error initializing actions: {e.Message}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
        throw;
    }
}
}