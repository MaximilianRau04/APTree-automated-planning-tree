using Neo4j.Driver;



// // objects
// //var r1 = new Agent("r1");
// //var lp1 = new BigPlate("lp1");
// var lp2 = new BigPlate("lp2");
// //var b1 = new Beam("b1");
// var b2 = new Beam("b2");
// //var loc3 = new OnRail("loc3");
// //var loc1 = new CurrentPosition("loc1");
// //var loc2 = new CurrentPosition("loc2");
// var loc9 = new CurrentPosition("loc9");
// var loc10 = new CurrentPosition("loc10");
// //var loc4 = new FinalPosition("loc4");
// // second cassette
// var lp3 = new BigPlate("lp3");
// var lp4 = new BigPlate("lp4");
// var b3 = new Beam("b3");
// var b4 = new Beam("b4");
// var loc5 = new CurrentPosition("loc5");
// var loc6 = new CurrentPosition("loc6");
// var loc11 = new CurrentPosition("loc11");
// var loc12 = new CurrentPosition("loc12");
// var loc7 = new OnRail("loc7");
// var loc8 = new FinalPosition("loc8");

/////
///


    // Initialize blackboard
 

    // Create entity type definitions
    // var entityTypes = new Dictionary<string, EntityTypeInfo>
    // {
    //     ["LowPlate"] = new EntityTypeInfo 
    //     { 
    //         BaseType = typeof(Element),
    //         Properties = new Dictionary<string, Type>
    //         {
    //             ["Length"] = typeof(float),
    //             ["Width"] = typeof(float),
    //             ["Thickness"] = typeof(float)
    //         }
    //     },
    //     ["Beam"] = new EntityTypeInfo
    //     {
    //         BaseType = typeof(Element),
    //         Properties = new Dictionary<string, Type>
    //         {
    //             ["Length"] = typeof(float),
    //             ["Height"] = typeof(float),
    //             ["Material"] = typeof(string)
    //         }
    //     },
    //     ["Robot"] = new EntityTypeInfo
    //     {
    //         BaseType = typeof(Agent),
    //         Properties = new Dictionary<string, Type>
    //         {
    //             ["Speed"] = typeof(float),
    //             ["Battery"] = typeof(int),
    //             ["MaxLoad"] = typeof(float)
    //         }
    //     },
    //     ["StackLocation"] = new EntityTypeInfo
    //     {
    //         BaseType = typeof(Location),
    //         Properties = new Dictionary<string, Type>
    //         {
    //             ["MaxHeight"] = typeof(float),
    //             ["CurrentCapacity"] = typeof(int),
    //             ["IsEmpty"] = typeof(bool)
    //         }
    //     },
    //     ["RailLocation"] = new EntityTypeInfo
    //     {
    //         BaseType = typeof(Location),
    //         Properties = new Dictionary<string, Type>
    //         {
    //             ["Position"] = typeof(float),
    //             ["IsOccupied"] = typeof(bool),
    //             ["RailNumber"] = typeof(int)
    //         }
    //     }
    // };


       // Define predicate types
    // var predicateTypes = new Dictionary<string, PredicateTypeInfo>
    // {
    //     ["AtLocation"] = new PredicateTypeInfo 
    //     { 
    //         Parameters = new Dictionary<string, Type>
    //         {
    //             ["element"] = typeof(Element),
    //             ["location"] = typeof(Location)
    //         }
    //     },
    //     ["AtPlace"] = new PredicateTypeInfo 
    //     { 
    //         Parameters = new Dictionary<string, Type>
    //         {
    //             ["robot"] = typeof(Agent),
    //             ["loc"] = typeof(Location)
    //         }
    //     },
    //      ["Holding"] = new PredicateTypeInfo 
    //     { 
    //         Parameters = new Dictionary<string, Type>
    //         {
    //             ["robot"] = typeof(Agent),
    //             ["element"] = typeof(Element)
    //         }
    //     }
    // };

    // // Register the predicate types
    // systemInitializer.RegisterPredicateTypes(predicateTypes);

    // // Define action types
    // var actionTypes = new Dictionary<string, ActionTypeInfo>
    // {
    //     ["PickUp"] = new ActionTypeInfo 
    //     { 
    //         Parameters = new List<Parameter> 
    //         {
    //             new Parameter("agent", typeof(Agent)),
    //             new Parameter("element", typeof(Element)),
    //             new Parameter("location", typeof(Location))
    //         },
    //         PreconditionTemplates = new List<PredicateTemplate>
    //         {
    //             new PredicateTemplate { PredicateName = "AtLocation", ParameterNames = new[] { "agent", "location" } }
    //         },
    //         EffectTemplates = new List<PredicateTemplate>
    //         {
    //             new PredicateTemplate { PredicateName = "Holding", ParameterNames = new[] { "agent", "element" } }
    //         }
    //     }
    // };

//     // Initialize system with types
//     systemInitializer.InitializeTypes(entityTypes, predicateTypes, actionTypes);

//     // Create some instances to test
//     var entityFactory = EntityFactory.Instance;
//   var predicateFactory = PredicateFactory.Instance; 
//   var actionFactory = ActionFactory.Instance;
//     // Create and add entities to blackboard
//     var plate1 = EntityFactory.CreateInstance("LowPlate", new Dictionary<string, object>
//     {
//         { "Length", 20.0f },
//         { "Width", 10.0f },
//         { "Thickness", 0.5f }
//     });
//     blackboard.SetElement(new FastName("Plate1"), plate1 as Element);

//     var beam1 = EntityFactory.CreateInstance("Beam", new Dictionary<string, object>
//     {
//         { "Length", 30.0f },
//         { "Height", 5.0f },
//         { "Material", "Steel" }
//     });
//     blackboard.SetElement(new FastName("Beam1"), beam1 as Element);

//     var robot1 = EntityFactory.CreateInstance("Robot", new Dictionary<string, object>
//     {
//         { "Speed", 5.0f },
//         { "Battery", 100 },
//         { "MaxLoad", 50.0f }
//     });
//     blackboard.SetAgent(new FastName("Robot1"), robot1 as Agent);

//     var stackLoc = EntityFactory.CreateInstance("StackLocation", new Dictionary<string, object>
//     {
//         { "MaxHeight", 100.0f },
//         { "CurrentCapacity", 0 },
//         { "IsEmpty", true }
//     });
//     blackboard.SetLocation(new FastName("Stack1"), stackLoc as Location);

//     var railLoc = EntityFactory.CreateInstance("RailLocation", new Dictionary<string, object>
//     {
//         { "Position", 0.0f },
//         { "IsOccupied", false },
//         { "RailNumber", 1 }
//     });
//     blackboard.SetLocation(new FastName("Rail1"), railLoc as Location);

//   // Create predicate instances
//     var beamAtLocation = predicateFactory.CreatePredicateInstance("AtLocation", new Dictionary<string, object>
//     {
//         ["element"] = beam1,
//         ["loc"] = stackLoc
//     });

//     var robotAtPlace = predicateFactory.CreatePredicateInstance("AtPlace", new Dictionary<string, object>
//     {
//         ["robot"] = robot1,
//         ["loc"] = railLoc
//     });

//     // Add predicates to blackboard
//     blackboard.AddPredicateWithGraph(new FastName("AtLocation"), beamAtLocation);
//     blackboard.AddPredicateWithGraph(new FastName("AtPlace"), robotAtPlace);


//     Console.WriteLine("Created entities:");
//     Console.WriteLine($"Plate1: {plate1}");
//     Console.WriteLine($"Beam1: {beam1}");
//     Console.WriteLine($"Robot1: {robot1}");
//     Console.WriteLine($"Stack1: {stackLoc}");
//     Console.WriteLine($"Rail1: {railLoc}");

 

//     // Register PickUp action type
// ActionFactory.Instance.RegisterActionType(
//     "PickUp",
//     // Parameters
//     new List<Parameter> 
//     {
//         new Parameter("agent", typeof(Agent)),
//         new Parameter("element", typeof(Element)),
//         new Parameter("location", typeof(Location))
//     },
//     // Preconditions
//     new List<PredicateTemplate> 
//     {
//         new PredicateTemplate 
//         { 
//             PredicateName = "AtLocation",
//             ParameterNames = new[] { "agent", "location" }
//         },
//         new PredicateTemplate 
//         { 
//             PredicateName = "AtLocation",
//             ParameterNames = new[] { "element", "location" }
//         }
//     },
//     // Effects
//     new List<PredicateTemplate> 
//     {
//         new PredicateTemplate 
//         { 
//             PredicateName = "Holding",
//             ParameterNames = new[] { "agent", "element" }
//         }
//     },
//     // Action Logic
//     (parameters, deltaTime) => 
//     {
//         var agent = (Agent)parameters["agent"];
//         var element = (Element)parameters["element"];
//         Console.WriteLine($"{agent} picking up {element}");
//         return true;
//     }
// );

// // Create an instance of PickUp action
// try 
// {
//     var pickupAction = ActionFactory.Instance.CreateActionInstance(
//         "PickUp",
//         blackboard,
//         new Dictionary<string, IThings>
//         {
//             ["agent"] = robot1,
//             ["element"] = beam1,
//             ["location"] = stackLoc
//         }
//     );
    
//     Console.WriteLine("Successfully created PickUp action instance");
//      float deltaTime = 0.1f; // Example delta time
//     var result = pickupAction.Tick(deltaTime);
//     Console.WriteLine($"PickUp action execution result: {result}");
//     Console.WriteLine($"Action status: {pickupAction.LastStatus}");
// } 
// catch (Exception e) 
// {
//     Console.WriteLine($"Failed to create PickUp action: {e.Message}");
// }



////////////////////////////////////////////////
///  Console.WriteLine("Testing Dynamic Flow Node\n");

//       Console.WriteLine("Testing Dynamic Flow Node System\n");

//     // Create blackboard and behavior tree
//    // var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "password");
//     var behaviorTree = new BTInstance();
//     behaviorTree.Initialise(blackboard, "TestTree");

//     // Create dynamic flow node and set it as root
//     var dynamicFlow = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.All);
//     behaviorTree.RootNode = dynamicFlow;  // Set as root instead of adding as child
//     dynamicFlow.SetOwiningTree(behaviorTree);

//     // Create test actions
//     var pickupAction = new DynamicBTAction(
//         "Pickup",
//         blackboard,
//         new State(),
//         new State(),
//         new List<Parameter> { new Parameter("object", typeof(string)) },
//         new object[] { "Box" },
//         (parameters, deltaTime) => {
//             Console.WriteLine($"Picking up {parameters["object"]}");
//             return true;
//         }
//     );

//     var moveAction = new DynamicBTAction(
//         "Move",
//         blackboard,
//         new State(),
//         new State(),
//         new List<Parameter> { new Parameter("location", typeof(string)) },
//         new object[] { "Location A" },
//         (parameters, deltaTime) => {
//             Console.WriteLine($"Moving to {parameters["location"]}");
//             return true;
//         }
//     );

//     // Add actions to dynamic flow node
//     dynamicFlow.AddChild(pickupAction);
//     dynamicFlow.AddChild(moveAction);
//     dynamicFlow.CreateOrderConstraint(pickupAction, moveAction, OrderType.Total);

//     // Run simulation
//     Console.WriteLine("\nStarting simulation:");
//     float deltaTime = 0.016f;
    
//     for (int tick = 0; tick < 8; tick++)
//     {
//         Console.WriteLine($"\n=== Tick {tick} ===");
//         behaviorTree.Tick(deltaTime);

//         Console.WriteLine($"Pickup Status: {pickupAction.LastStatus}");
//         Console.WriteLine($"Move Status: {moveAction.LastStatus}");
        
//         if (dynamicFlow.LastStatus == EBTNodeResult.Succeeded)
//         {
//             Console.WriteLine("\nAll actions completed successfully!");
//             break;
//         }
//     }



    /////////////////////////////////////////////////////////////////
    ///
    // Console.WriteLine("Testing PDDL Parser\n");

   



    // try
    // {
    //     // Use absolute path
    //    // string pddlPath = @"C:\Users\sherk\Documents\PDDLfiles\objecttypes.pddl.txt";
        
        


    //   //  var entityTypes = Parser.generateEntityTypesfromPDDLDomain(pddlPath);

    //     // Print results
    //     Console.WriteLine("\nGenerated Entity Types:");
    //     foreach (var (typeName, typeInfo) in entityTypes)
    //     {
    //         Console.WriteLine($"Type: {typeName}");
    //         Console.WriteLine($"  Base Type: {typeInfo.BaseType.Name}");
    //         Console.WriteLine($"  Properties: {typeInfo.Properties.Count}");
    //     }
    // }
    // catch (Exception e)
    // {
    //     Console.WriteLine($"Error: {e.Message}");
    //     Console.WriteLine($"Stack trace: {e.StackTrace}");
    // }
   

    // Console.WriteLine("\nPress any key to exit...");
    // Console.ReadKey();

///////////////////////////////////////////////////////////////
///
// Console.WriteLine("Testing PDDL predicate Parser\n");
//    string pddlPath = @"C:\Users\sherk\Documents\PDDLfiles\objecttypes.pddl.txt";
//     string pddlPath2 = @"C:\Users\sherk\Documents\PDDLfiles\predicatetypes.pddl.txt";
    
//     try
//     {
//         // First register entity types
//         Console.WriteLine("=== Testing Entity Types ===");
//         var entityTypes = Parser.generateEntityTypesfromPDDLDomain(pddlPath);
//         foreach (var (typeName, typeInfo) in entityTypes)
//         {
//             EntityFactory.Instance.RegisterEntityType(typeName, typeInfo.BaseType, typeInfo.Properties);
//             Console.WriteLine($"Registered Entity Type: {typeName}, Base: {typeInfo.BaseType.Name}");
//         }

//         // Then test predicate types
//         Console.WriteLine("\n=== Testing Predicate Types ===");
//         var predicateTypes = Parser.generatePredicateTypesfromPDDLDomain(pddlPath2);
//         foreach (var (predName, predInfo) in predicateTypes)
//         {
//             Console.WriteLine($"\nPredicate: {predName}");
//             Console.WriteLine("Parameters:");
//             foreach (var param in predInfo.Parameters)
//             {
//                 Console.WriteLine($"  {param.Key}: {param.Value.Name}");
//             }
//         }
//     }
//     catch (Exception e)
//     {
//         Console.WriteLine($"Error: {e.Message}");
//         Console.WriteLine($"Stack trace: {e.StackTrace}");
//     }

//     Console.WriteLine("\nPress any key to exit...");
//     Console.ReadKey();


///////////////////////////////////////////////////////////////
///
Console.WriteLine("Testing PDDL Parser\n");
  var blackboard = new Blackboard<FastName>(
    "bolt://localhost:7687", 
    "neo4j",           // username
    "asd24819zxc"          // password - make sure this matches your Neo4j password
);

try 
{
    using var session = blackboard.GetDriver().AsyncSession();
    await session.RunAsync("RETURN 1");
    Console.WriteLine("Successfully connected to Neo4j!");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to connect to Neo4j: {ex.Message}");
    return; // Exit if connection fails
}

    var systemInitializer = new SystemInitializer(blackboard);

string pddlPath3 = @"C:\Users\sherk\Documents\PDDLfiles\actiontypes.pddl.txt";
string pddlPath = @"C:\Users\sherk\Documents\PDDLfiles\objecttypes.pddl.txt";
string pddlPath2 = @"C:\Users\sherk\Documents\PDDLfiles\predicatetypes.pddl.txt";
string pddlPath4 = @"C:\Users\sherk\Documents\PDDLfiles\problemtypes.txt";
string pddlPath5 = @"C:\Users\sherk\Documents\PDDLfiles\probleminitstate.txt";
string pddlPath6 = @"C:\Users\sherk\Documents\PDDLfiles\dummyplan.txt";

   systemInitializer.InitializeTypesFromPDDL(pddlPath, pddlPath2, pddlPath3);

   systemInitializer.InitializeObjectsFromText(pddlPath4);

   await systemInitializer.InitializePredicatesFromtext(pddlPath5);
  var actions = await systemInitializer.InitializeActionsFromPDDL(pddlPath6);
