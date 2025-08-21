using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using PlanningDataStructures;
using AIPlanning;

namespace BehaviorTreeMainProject
{
    public class FullTreeTest
    {
        public async Task RunFullTreeTest()
        {
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine("🌳 FULL BEHAVIOR TREE TEST");
            Console.WriteLine("=".PadRight(80, '='));

            try
            {
                // Create blackboard instance and test Neo4j connection
                using var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "12345678");
                
                // Test Neo4j connection
                Console.WriteLine("🔍 Testing Neo4j connection...");
                bool connectionSuccess = await TestNeo4jConnection(blackboard);

                if (connectionSuccess)
                {
                    // Create BlackboardWriter for type registration
                     var blackboardWriter = new BlackboardWriter(blackboard);

                     // Register all types
                    Console.WriteLine("\n=== REGISTERING ALL TYPES ===");
                    blackboardWriter.RegisterAllTypes();

                     // Register all instances from files
                     Console.WriteLine("\n=== REGISTERING ALL INSTANCES FROM FILES ===");
                     string actionInstancesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "ParameterInstances.txt");
                     blackboardWriter.RegisterAllInstances(actionInstancesFile);

                     // making the tree

                                         // Inspect blackboard contents
                     Console.WriteLine("\n=== INSPECTING BLACKBOARD CONTENTS ===");
                     await InspectBlackboard(blackboard);

                     // Create behavior tree with cassette flow nodes
                     Console.WriteLine("\n=== CREATING BEHAVIOR TREE WITH CASSETTE FLOW NODES ===");
                     await CreateCassetteBehaviorTree(blackboard);
                }

                Console.WriteLine("\n" + "=".PadRight(80, '='));
                Console.WriteLine("🎉 FULL BEHAVIOR TREE TEST COMPLETED!");
                Console.WriteLine("=".PadRight(80, '='));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR during full tree test: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }

        // Test Neo4j connection
        private async Task<bool> TestNeo4jConnection(Blackboard<FastName> blackboard)
        {
            try
            {
                // Try to connect to Neo4j
                bool connected = await blackboard.TestNeo4jConnection();
                if (connected)
                {
                    Console.WriteLine("✅ Successfully connected to Neo4j");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Neo4j connection test failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to connect to Neo4j: {ex.Message}");
                Console.WriteLine("   Make sure Neo4j is running and accessible at bolt://localhost:7687");
                Console.WriteLine("   Check your Neo4j credentials (neo4j/12345678)");
                return false;
            }
        }

        // Inspect blackboard contents
        private async Task InspectBlackboard(Blackboard<FastName> blackboard)
        {
            Console.WriteLine("\n📋 BLACKBOARD INSPECTION REPORT");
            Console.WriteLine("=".PadRight(50, '='));

            try
            {
                // 1. Entity Types
                var entityTypes = blackboard.GetAllEntityTypes();
                Console.WriteLine($"\n🏷️  ENTITY TYPES ({entityTypes.Count}):");
                foreach (var entityType in entityTypes)
                {
                    Console.WriteLine($"   - {entityType.ToString()}");
                }

                // 2. Predicate Types
                var predicateTypes = blackboard.GetAllPredicateTypes();
                Console.WriteLine($"\n🔍 PREDICATE TYPES ({predicateTypes.Count}):");
                foreach (var predicateType in predicateTypes)
                {
                    Console.WriteLine($"   - {predicateType.ToString()}");
                }

                // 3. Action Types
                var actionTypes = blackboard.GetAllActionTypes();
                Console.WriteLine($"\n⚡ ACTION TYPES ({actionTypes.Count}):");
                foreach (var actionType in actionTypes)
                {
                    Console.WriteLine($"   - {actionType.ToString()}");
                }

                // 4. Action Instances
                var actionInstances = blackboard.GetAllActionInstances();
                Console.WriteLine($"\n🎯 ACTION INSTANCES ({actionInstances.Count}):");
                foreach (var actionInstance in actionInstances)
                {
                    Console.WriteLine($"   - {actionInstance.InstanceName.ToString()} (Type: {actionInstance.actionType.ToString()})");
                }

                // 5. Built-in Values
                Console.WriteLine($"\n📝 BUILT-IN VALUES:");
                Console.WriteLine($"   - Int Values: {GetDictionaryCount(blackboard, "IntValues")}");
                if (GetDictionaryCount(blackboard, "IntValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "IntValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - Double Values: {GetDictionaryCount(blackboard, "DoubleValues")}");
                if (GetDictionaryCount(blackboard, "DoubleValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "DoubleValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - Bool Values: {GetDictionaryCount(blackboard, "BoolValues")}");
                if (GetDictionaryCount(blackboard, "BoolValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "BoolValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - String Values: {GetDictionaryCount(blackboard, "StringValues")}");
                if (GetDictionaryCount(blackboard, "StringValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "StringValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }

                // 6. Entity Values
                Console.WriteLine($"\n🏗️  ENTITY VALUES:");
                Console.WriteLine($"   - Element Values: {GetDictionaryCount(blackboard, "ElementValues")}");
                if (GetDictionaryCount(blackboard, "ElementValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "ElementValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - Location Values: {GetDictionaryCount(blackboard, "LocationValues")}");
                if (GetDictionaryCount(blackboard, "LocationValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "LocationValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - Agent Values: {GetDictionaryCount(blackboard, "AgentValues")}");
                if (GetDictionaryCount(blackboard, "AgentValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "AgentValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - Layer Values: {GetDictionaryCount(blackboard, "LayerValues")}");
                if (GetDictionaryCount(blackboard, "LayerValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "LayerValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - Module Values: {GetDictionaryCount(blackboard, "ModuleValues")}");
                if (GetDictionaryCount(blackboard, "ModuleValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "ModuleValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }
                
                Console.WriteLine($"   - Tool Values: {GetDictionaryCount(blackboard, "ToolValues")}");
                if (GetDictionaryCount(blackboard, "ToolValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "ToolValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }

                // 7. Predicate Values
                Console.WriteLine($"\n🔍 PREDICATE VALUES:");
                Console.WriteLine($"   - Predicate Values: {GetDictionaryCount(blackboard, "PredicateValues")}");
                if (GetDictionaryCount(blackboard, "PredicateValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "PredicateValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }

                // 8. Action Values
                Console.WriteLine($"\n⚡ ACTION VALUES:");
                Console.WriteLine($"   - Action Values: {GetDictionaryCount(blackboard, "ActionValues")}");
                if (GetDictionaryCount(blackboard, "ActionValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "ActionValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }

                // 9. State Values
                Console.WriteLine($"\n🌍 STATE VALUES:");
                Console.WriteLine($"   - State Values: {GetDictionaryCount(blackboard, "StateValues")}");
                if (GetDictionaryCount(blackboard, "StateValues") > 0)
                {
                    foreach (var item in GetDictionaryItems(blackboard, "StateValues"))
                    {
                        Console.WriteLine($"     • {item.Key}: {item.Value}");
                    }
                }

                // 8. NodeGraphs
                var nodeGraphs = blackboard.GetAllNodeGraphs();
                Console.WriteLine($"\n🌳 NODEGRAPHS ({nodeGraphs.Count}):");
                foreach (var nodeGraph in nodeGraphs)
                {
                    Console.WriteLine($"   - NodeGraph with {nodeGraph.GetAllActionNodes().Count} action nodes");
                }

                // 10. Summary
                Console.WriteLine($"\n📊 SUMMARY:");
                Console.WriteLine($"   - Entity Types: {entityTypes.Count}");
                Console.WriteLine($"   - Predicate Types: {predicateTypes.Count}");
                Console.WriteLine($"   - Action Types: {actionTypes.Count}");
                Console.WriteLine($"   - Action Instances: {actionInstances.Count}");
                Console.WriteLine($"   - NodeGraphs: {nodeGraphs.Count}");
                Console.WriteLine($"   - TOTAL ITEMS: {entityTypes.Count + predicateTypes.Count + actionTypes.Count + actionInstances.Count + nodeGraphs.Count}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during blackboard inspection: {ex.Message}");
            }
        }

        // Helper method to get dictionary count using reflection
        private int GetDictionaryCount(Blackboard<FastName> blackboard, string dictionaryName)
        {
            try
            {
                var field = typeof(Blackboard<FastName>).GetField(dictionaryName, 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var dictionary = field.GetValue(blackboard);
                    if (dictionary is System.Collections.ICollection collection)
                    {
                        return collection.Count;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // Helper method to get dictionary items using reflection
        private IEnumerable<KeyValuePair<FastName, object>> GetDictionaryItems(Blackboard<FastName> blackboard, string dictionaryName)
        {
            var result = new List<KeyValuePair<FastName, object>>();
            try
            {
                var field = typeof(Blackboard<FastName>).GetField(dictionaryName, 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var dictionary = field.GetValue(blackboard);
                    if (dictionary is System.Collections.IDictionary dict)
                    {
                        foreach (System.Collections.DictionaryEntry entry in dict)
                        {
                            result.Add(new KeyValuePair<FastName, object>((FastName)entry.Key, entry.Value));
                        }
                    }
                }
            }
            catch
            {
                // Return empty collection on error
            }
            return result;
        }

        // Create behavior tree with cassette flow nodes
        private async Task CreateCassetteBehaviorTree(Blackboard<FastName> blackboard)
        {
            try
            {
                Console.WriteLine("🌳 Creating behavior tree with cassette flow nodes...");

                // Create behavior tree instance first
                var behaviorTree = new BTInstance();
                behaviorTree.Initialise(blackboard, "CassetteBehaviorTree");
                Console.WriteLine("✅ Created behavior tree instance");

                // Create root composite flow node
                var rootNode = new BTFlowNode_Composite(new FastName("RootComposite"), behaviorTree);
                Console.WriteLine("✅ Created root composite flow node");

                // Create four cassette flow nodes
                var cassette1Node = new BTFlowNode_Dynamic(new FastName("cassette1"), behaviorTree);
                var cassette2Node = new BTFlowNode_Dynamic(new FastName("cassette2"), behaviorTree);
                var cassette3Node = new BTFlowNode_Dynamic(new FastName("cassette3"), behaviorTree);
                var cassette4Node = new BTFlowNode_Dynamic(new FastName("cassette4"), behaviorTree);

                Console.WriteLine("✅ Created four cassette flow nodes");

                // Add cassette nodes to the root composite node
                rootNode.AddChild(cassette1Node);
                rootNode.AddChild(cassette2Node);
                rootNode.AddChild(cassette3Node);
                rootNode.AddChild(cassette4Node);

                Console.WriteLine("✅ Added all cassette nodes to root composite node");

                // Set the root node
                behaviorTree.RootNode = rootNode;
                rootNode.SetOwiningTree(behaviorTree);

                // Create PDDL planners for each cassette (after behavior tree is created)
                var pddlRequest1 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC1.pddl", "/home/shermin/ENHSP-Public/enhsp.jar");
                var pddlRequest2 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC2.pddl", "/home/shermin/ENHSP-Public/enhsp.jar");
                var pddlRequest3 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC3.pddl", "/home/shermin/ENHSP-Public/enhsp.jar");
                var pddlRequest4 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC4.pddl", "/home/shermin/ENHSP-Public/enhsp.jar");

                var pddlPlanner1 = new CallPDDLPlanner(behaviorTree, pddlRequest1);
                var pddlPlanner2 = new CallPDDLPlanner(behaviorTree, pddlRequest2);
                var pddlPlanner3 = new CallPDDLPlanner(behaviorTree, pddlRequest3);
                var pddlPlanner4 = new CallPDDLPlanner(behaviorTree, pddlRequest4);

                Console.WriteLine("✅ Created PDDL planners for each cassette");

                // Set the planning service on each flow node
                cassette1Node.SetPlanningService(pddlPlanner1);
                cassette2Node.SetPlanningService(pddlPlanner2);
                cassette3Node.SetPlanningService(pddlPlanner3);
                cassette4Node.SetPlanningService(pddlPlanner4);

                Console.WriteLine("✅ Set planning services on all cassette flow nodes");

                // Store the behavior tree in the blackboard for later use
                blackboard.SetNodeGraph(new FastName("MainBehaviorTree"), new NodeGraph());
                Console.WriteLine("✅ Stored behavior tree reference in blackboard");

                // Display tree structure
                Console.WriteLine("\n📋 BEHAVIOR TREE STRUCTURE:");
                Console.WriteLine($"Root: BTFlowNode_Composite ({rootNode.GetNodeName()})");
                Console.WriteLine($"├── BTFlowNode_Dynamic ({cassette1Node.GetNodeName()}) - PDDL Planner");
                Console.WriteLine($"├── BTFlowNode_Dynamic ({cassette2Node.GetNodeName()}) - PDDL Planner");
                Console.WriteLine($"├── BTFlowNode_Dynamic ({cassette3Node.GetNodeName()}) - PDDL Planner");
                Console.WriteLine($"└── BTFlowNode_Dynamic ({cassette4Node.GetNodeName()}) - PDDL Planner");

                Console.WriteLine("\n🎉 Behavior tree with cassette flow nodes created successfully!");

                // Test the tree structure
                await TestBehaviorTreeStructure(behaviorTree);
                
                // Display NodeGraph status for each flow node
                await DisplayNodeGraphStatus(behaviorTree);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating behavior tree: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }

        // Test the behavior tree structure
        private async Task TestBehaviorTreeStructure(BTInstance behaviorTree)
        {
            try
            {
                Console.WriteLine("\n🧪 Testing behavior tree structure...");

                // Test initial tick
                var result = behaviorTree.Tick(0.0f);
                Console.WriteLine($"✅ Initial tree tick result: {result}");

                // Test individual cassette nodes
                var rootNode = behaviorTree.RootNode as BTFlowNode_Composite;
                if (rootNode != null)
                {
                    var children = rootNode.GetChildren();
                    Console.WriteLine($"✅ Root node has {children.Count} children");

                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];
                        if (child is BTFlowNodeBase flowNode)
                        {
                            Console.WriteLine($"   Child {i + 1}: {child.GetType().Name} - {flowNode.GetNodeName()}");
                        }
                        else
                        {
                            Console.WriteLine($"   Child {i + 1}: {child.GetType().Name} - {child.DebugDisplayName}");
                        }
                    }
                }

                Console.WriteLine("✅ Behavior tree structure test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error testing behavior tree structure: {ex.Message}");
            }
        }

        // Display NodeGraph status for each flow node
        private async Task DisplayNodeGraphStatus(BTInstance behaviorTree)
        {
            try
            {
                Console.WriteLine("\n📊 NODEGRAPH STATUS REPORT");
                Console.WriteLine("=".PadRight(50, '='));

                var rootNode = behaviorTree.RootNode as BTFlowNode_Composite;
                if (rootNode != null)
                {
                    var children = rootNode.GetChildren();
                    Console.WriteLine($"🔍 Checking {children.Count} flow nodes for NodeGraph status...\n");

                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];
                        if (child is BTFlowNode_Dynamic dynamicNode)
                        {
                            Console.WriteLine($"🎯 FLOW NODE {i + 1}: {dynamicNode.GetNodeName()}");
                            Console.WriteLine($"   📋 Node Type: {child.GetType().Name}");
                            
                            // Check if planning service is set
                            if (dynamicNode.PlanningService != null)
                            {
                                Console.WriteLine($"   🔧 Planning Service: {dynamicNode.PlanningService.GetType().Name}");
                                
                                // Check if it's a BTServicePlanner
                                if (dynamicNode.PlanningService is BTServicePlanner plannerService)
                                {
                                    Console.WriteLine($"   📊 Has Generated NodeGraph: {plannerService.HasGeneratedNodeGraph()}");
                                    
                                    if (plannerService.HasGeneratedNodeGraph())
                                    {
                                        var generatedGraph = plannerService.GetGeneratedNodeGraph();
                                        var actions = generatedGraph.GetAllActionNodes();
                                        Console.WriteLine($"   📈 Generated NodeGraph Actions: {actions.Count}");
                                        
                                        // List the actions
                                        for (int j = 0; j < actions.Count; j++)
                                        {
                                            Console.WriteLine($"      {j + 1}. {actions[j].InstanceName.ToString()}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"   ⚠️ No NodeGraph generated yet");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"   ❌ No planning service set");
                            }
                            
                            // Check the actionGraph
                            var actionGraph = dynamicNode.GetActionGraph();
                            var actionGraphNodes = actionGraph.GetAllActionNodes();
                            Console.WriteLine($"   📋 ActionGraph Nodes: {actionGraphNodes.Count}");
                            
                            if (actionGraphNodes.Count > 0)
                            {
                                for (int j = 0; j < actionGraphNodes.Count; j++)
                                {
                                    Console.WriteLine($"      {j + 1}. {actionGraphNodes[j].InstanceName.ToString()}");
                                }
                            }
                            
                            Console.WriteLine();
                        }
                    }
                }

                Console.WriteLine("✅ NodeGraph status report completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error displaying NodeGraph status: {ex.Message}");
            }
        }

        // Public method to run the test from Program.cs
        public static async Task RunTest()
        {
            var test = new FullTreeTest();
            await test.RunFullTreeTest();
        }
    }
}
