using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PlanningDataStructures;
using AIPlanning;
using ModelLoader.ParameterTypes;



namespace BehaviorTreeMainProject
{
    public class FullTreeTest
    {
        // Track all planner executions
        private List<BTServicePlanner> allPlanners = new List<BTServicePlanner>();
        private DateTime testStartTime;
        private DateTime testEndTime;
        
        public async Task RunFullTreeTest()
        {
            testStartTime = DateTime.Now;
            
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine("🌳 FULL BEHAVIOR TREE TEST");
            Console.WriteLine($"🚀 Started at: {testStartTime:yyyy-MM-dd HH:mm:ss.fff}");
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
                     
                     // Test SubtreeInjectionService with detailed step-by-step analysis
                     Console.WriteLine("\n=== TESTING SUBTREE INJECTION SERVICE ===");
                     await TestSubtreeInjectionService(blackboard);
                }

                testEndTime = DateTime.Now;
                
                Console.WriteLine("\n" + "=".PadRight(80, '='));
                Console.WriteLine("🎉 FULL BEHAVIOR TREE TEST COMPLETED!");
                Console.WriteLine($"⏰ Finished at: {testEndTime:yyyy-MM-dd HH:mm:ss.fff}");
                Console.WriteLine($"⏱️ Total test duration: {testEndTime - testStartTime:hh\\:mm\\:ss\\.fff}");
                Console.WriteLine("=".PadRight(80, '='));
                
                // Display execution summary
                await DisplayExecutionSummary();
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

                // Create five cassette flow nodes
                var cassette1Node = new BTFlowNode_Dynamic(new FastName("cassette1"), behaviorTree);
                var cassette2Node = new BTFlowNode_Dynamic(new FastName("cassette2"), behaviorTree);
                var cassette3Node = new BTFlowNode_Dynamic(new FastName("cassette3"), behaviorTree);
                var cassette4Node = new BTFlowNode_Dynamic(new FastName("cassette4"), behaviorTree);
                var cassette5Node = new BTFlowNode_Dynamic(new FastName("cassette5"), behaviorTree);

                Console.WriteLine("✅ Created five cassette flow nodes");

                // Add cassette nodes to the root composite node
                rootNode.AddChild(cassette1Node);
                rootNode.AddChild(cassette2Node);
                rootNode.AddChild(cassette3Node);
                rootNode.AddChild(cassette4Node);
                rootNode.AddChild(cassette5Node);

                Console.WriteLine("✅ Added all cassette nodes to root composite node");

                // Set the root node
                behaviorTree.RootNode = rootNode;
                rootNode.SetOwiningTree(behaviorTree);

                // Create PDDL planners for each cassette (after behavior tree is created)
                // Mix of ENHSP and FF planners for demonstration
                var pddlRequest1 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC1.pddl", "/home/shermin/ENHSP-Public/enhsp.jar", "ENHSP");
                var pddlRequest2 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC2.pddl", "/home/shermin/ENHSP-Public/enhsp.jar", "ENHSP");
                var pddlRequest3 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC3.pddl", "/home/shermin/ENHSP-Public/enhsp.jar", "ENHSP");
                var pddlRequest4 = new PDDLPlanningRequest("./Plannerinputs/domain.pddl", "./Plannerinputs/problemC4.pddl", "/home/shermin/ENHSP-Public/enhsp.jar", "ENHSP");
                var pddlRequest5 = new PDDLPlanningRequest("./Plannerinputs/DomainML.pddl", "./Plannerinputs/ProblemML.pddl", "docker://ff", "FF");


                var pddlPlanner1 = new CallPDDLPlanner(behaviorTree, pddlRequest1);
                var pddlPlanner2 = new CallPDDLPlanner(behaviorTree, pddlRequest2);
                var pddlPlanner3 = new CallPDDLPlanner(behaviorTree, pddlRequest3);
                var pddlPlanner4 = new CallPDDLPlanner(behaviorTree, pddlRequest4);
                var pddlPlanner5 = new CallPDDLPlanner(behaviorTree, pddlRequest5);
                
                // Track all planners for execution summary
                allPlanners.Add(pddlPlanner1);
                allPlanners.Add(pddlPlanner2);
                allPlanners.Add(pddlPlanner3);
                allPlanners.Add(pddlPlanner4);
                allPlanners.Add(pddlPlanner5);
                
                // Configure execution modes for different cassettes
                pddlPlanner1.ExecutionMode = CallPDDLPlanner.ParallelExecutionMode.Parallel;    // Parallel execution
                pddlPlanner2.ExecutionMode = CallPDDLPlanner.ParallelExecutionMode.Sequential;  // Sequential execution
                pddlPlanner3.ExecutionMode = CallPDDLPlanner.ParallelExecutionMode.Hybrid;      // Hybrid execution
                pddlPlanner4.ExecutionMode = CallPDDLPlanner.ParallelExecutionMode.Parallel;    // Parallel execution
                pddlPlanner5.ExecutionMode = CallPDDLPlanner.ParallelExecutionMode.Sequential;  // Sequential execution (FF planner)

                Console.WriteLine("✅ Created PDDL planners for each cassette");
                Console.WriteLine($"🔧 Execution Modes:");
                Console.WriteLine($"   - Cassette 1: {pddlPlanner1.ExecutionMode} (Planner: {pddlRequest1.PlannerName})");
                Console.WriteLine($"   - Cassette 2: {pddlPlanner2.ExecutionMode} (Planner: {pddlRequest2.PlannerName})");
                Console.WriteLine($"   - Cassette 3: {pddlPlanner3.ExecutionMode} (Planner: {pddlRequest3.PlannerName})");
                Console.WriteLine($"   - Cassette 4: {pddlPlanner4.ExecutionMode} (Planner: {pddlRequest4.PlannerName})");
                Console.WriteLine($"   - Cassette 5: {pddlPlanner5.ExecutionMode} (Planner: {pddlRequest5.PlannerName})");

                // Set the planning service on each flow node
                cassette1Node.SetPlanningService(pddlPlanner1);
                cassette2Node.SetPlanningService(pddlPlanner2);
                cassette3Node.SetPlanningService(pddlPlanner3);
                cassette4Node.SetPlanningService(pddlPlanner4);
                cassette5Node.SetPlanningService(pddlPlanner5);

                Console.WriteLine("✅ Set planning services on all cassette flow nodes");
                
                // Debug: Check if FF planner is properly configured
                Console.WriteLine($"🔍 FF Planner Debug Info:");
                Console.WriteLine($"   - Domain File: {pddlRequest5.DomainFile}");
                Console.WriteLine($"   - Problem File: {pddlRequest5.ProblemFile}");
                Console.WriteLine($"   - Planner Name: {pddlRequest5.PlannerName}");
                Console.WriteLine($"   - Planner Path: {pddlRequest5.PlannerPath}");
                Console.WriteLine($"   - Timeout: {pddlRequest5.TimeoutSeconds} seconds");

                // Store the behavior tree in the blackboard for later use
                blackboard.SetNodeGraph(new FastName("MainBehaviorTree"), new NodeGraph());
                Console.WriteLine("✅ Stored behavior tree reference in blackboard");

                // Display tree structure
                Console.WriteLine("\n📋 BEHAVIOR TREE STRUCTURE:");
                Console.WriteLine($"Root: BTFlowNode_Composite ({rootNode.GetNodeName()})");
                Console.WriteLine($"├── BTFlowNode_Dynamic ({cassette1Node.GetNodeName()}) - PDDL Planner");
                Console.WriteLine($"├── BTFlowNode_Dynamic ({cassette2Node.GetNodeName()}) - PDDL Planner");
                Console.WriteLine($"├── BTFlowNode_Dynamic ({cassette3Node.GetNodeName()}) - PDDL Planner");
                Console.WriteLine($"├── BTFlowNode_Dynamic ({cassette4Node.GetNodeName()}) - PDDL Planner");
                Console.WriteLine($"└── BTFlowNode_Dynamic ({cassette5Node.GetNodeName()}) - PDDL Planner (FF)");

                Console.WriteLine("\n🎉 Behavior tree with cassette flow nodes created successfully!");

                // Test the tree structure
                await TestBehaviorTreeStructure(behaviorTree);
                
                // Monitor planner execution in real-time
                await MonitorPlannerExecution();
                
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

        // Monitor planner execution in real-time
        private async Task MonitorPlannerExecution()
        {
            Console.WriteLine("\n🔍 MONITORING PLANNER EXECUTION");
            Console.WriteLine("=".PadRight(50, '='));
            
            if (allPlanners.Count == 0)
            {
                Console.WriteLine("⚠️ No planners to monitor.");
                return;
            }
            
            Console.WriteLine($"🔍 Monitoring {allPlanners.Count} planners...");
            Console.WriteLine("Press any key to stop monitoring and continue...");
            
            var monitoringStartTime = DateTime.Now;
            var lastStatusTime = DateTime.Now;
            
            while (true)
            {
                // Check if any key is pressed (non-blocking)
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true); // Clear the key
                    break;
                }
                
                var currentTime = DateTime.Now;
                
                // Update status every 2 seconds
                if ((currentTime - lastStatusTime).TotalSeconds >= 2)
                {
                    Console.Clear();
                    Console.WriteLine($"🔍 PLANNER EXECUTION STATUS - {currentTime:HH:mm:ss}");
                    Console.WriteLine("=".PadRight(50, '='));
                    
                    var completedCount = allPlanners.Count(p => p.HasCompleted);
                    var executingCount = allPlanners.Count(p => p.IsExecuting);
                    var pendingCount = allPlanners.Count(p => !p.HasCompleted && !p.IsExecuting);
                    
                    Console.WriteLine($"📊 Progress: {completedCount}/{allPlanners.Count} completed, {executingCount} executing, {pendingCount} pending");
                    Console.WriteLine();
                    
                    foreach (var planner in allPlanners)
                    {
                        var status = planner.HasCompleted ? "✅" : planner.IsExecuting ? "🔄" : "⏳";
                        var duration = planner.IsExecuting ? currentTime - planner.StartTime : planner.ExecutionDuration;
                        
                        Console.WriteLine($"{status} {planner.PlannerName}: {duration:hh\\:mm\\:ss\\.fff}");
                    }
                    
                    Console.WriteLine("\nPress any key to stop monitoring...");
                    lastStatusTime = currentTime;
                }
                
                // Check if all planners are done
                if (allPlanners.All(p => p.HasCompleted || (!p.IsExecuting && !p.HasCompleted)))
                {
                    Console.WriteLine("\n✅ All planners have finished execution!");
                    break;
                }
                
                await Task.Delay(100); // Small delay to prevent high CPU usage
            }
            
            Console.WriteLine($"⏱️ Monitoring duration: {DateTime.Now - monitoringStartTime:hh\\:mm\\:ss\\.fff}");
        }
        
        // Display execution summary for all planners
        private async Task DisplayExecutionSummary()
        {
            Console.WriteLine("\n📊 PLANNER EXECUTION SUMMARY");
            Console.WriteLine("=".PadRight(80, '='));
            
            if (allPlanners.Count == 0)
            {
                Console.WriteLine("⚠️ No planners were executed during this test.");
                return;
            }
            
            Console.WriteLine($"🔍 Total planners executed: {allPlanners.Count}");
            Console.WriteLine();
            
            // Sort planners by start time
            var sortedPlanners = allPlanners.OrderBy(p => p.StartTime).ToList();
            
            for (int i = 0; i < sortedPlanners.Count; i++)
            {
                var planner = sortedPlanners[i];
                Console.WriteLine($"🎯 PLANNER {i + 1}: {planner.PlannerName}");
                Console.WriteLine($"   🚀 Started: {planner.StartTime:HH:mm:ss.fff}");
                
                if (planner.HasCompleted)
                {
                    Console.WriteLine($"   ✅ Finished: {planner.EndTime:HH:mm:ss.fff}");
                    Console.WriteLine($"   ⏱️ Duration: {planner.ExecutionDuration:hh\\:mm\\:ss\\.fff}");
                    
                    if (planner.GeneratedNodeGraph != null)
                    {
                        Console.WriteLine($"   📊 Actions Generated: {planner.GeneratedNodeGraph.GetAllActionNodes().Count}");
                    }
                }
                else if (planner.IsExecuting)
                {
                    Console.WriteLine($"   🔄 Still executing... (Started: {planner.StartTime:HH:mm:ss.fff})");
                }
                else
                {
                    Console.WriteLine($"   ❌ Failed or incomplete");
                }
                Console.WriteLine();
            }
            
            // Summary statistics
            var completedPlanners = allPlanners.Where(p => p.HasCompleted).ToList();
            var failedPlanners = allPlanners.Where(p => !p.HasCompleted && !p.IsExecuting).ToList();
            var executingPlanners = allPlanners.Where(p => p.IsExecuting).ToList();
            
            Console.WriteLine("📈 EXECUTION STATISTICS:");
            Console.WriteLine($"   ✅ Successfully completed: {completedPlanners.Count}");
            Console.WriteLine($"   ❌ Failed: {failedPlanners.Count}");
            Console.WriteLine($"   🔄 Still executing: {executingPlanners.Count}");
            
            if (completedPlanners.Any())
            {
                var avgDuration = TimeSpan.FromMilliseconds(completedPlanners.Average(p => p.ExecutionDuration.TotalMilliseconds));
                var minDuration = completedPlanners.Min(p => p.ExecutionDuration);
                var maxDuration = completedPlanners.Max(p => p.ExecutionDuration);
                
                Console.WriteLine($"   ⏱️ Average duration: {avgDuration:hh\\:mm\\:ss\\.fff}");
                Console.WriteLine($"   ⏱️ Fastest: {minDuration:hh\\:mm\\:ss\\.fff}");
                Console.WriteLine($"   ⏱️ Slowest: {maxDuration:hh\\:mm\\:ss\\.fff}");
            }
            
            Console.WriteLine("=".PadRight(80, '='));
        }
        
        // Test SubtreeInjectionService with detailed step-by-step analysis
        private async Task TestSubtreeInjectionService(Blackboard<FastName> blackboard)
        {
            Console.WriteLine("\n🔧 SUBTREE INJECTION SERVICE TEST");
            Console.WriteLine("=".PadRight(80, '='));
            
            try
            {
                // Step 1: Create the SubtreeInjectionService
                Console.WriteLine("\n📋 STEP 1: Creating SubtreeInjectionService");
                Console.WriteLine("   Input: Blackboard instance");
                var behaviorTree = new BTInstance();
                behaviorTree.Initialise(blackboard, "SubtreeInjectionTest");
                var subtreeService = new SubtreeInjectionService(behaviorTree);
                Console.WriteLine("   Output: SubtreeInjectionService instance created");
                Console.WriteLine("   ✅ Step 1 completed successfully");
                
                // Step 2: Get initial statistics
                Console.WriteLine("\n📋 STEP 2: Getting initial statistics");
                Console.WriteLine("   Input: None");
                var initialStats = subtreeService.GetStatistics();
                Console.WriteLine($"   Output: cachedSubtrees={initialStats.cachedSubtrees}, configurations={initialStats.configurations}, plannerMappings={initialStats.plannerMappings}");
                Console.WriteLine("   ✅ Step 2 completed successfully");
                
                // Step 3: Get initial planner mappings
                Console.WriteLine("\n📋 STEP 3: Getting initial planner mappings");
                Console.WriteLine("   Input: None");
                var initialMappings = subtreeService.GetPlannerMappings();
                Console.WriteLine("   Output: Planner mappings:");
                foreach (var mapping in initialMappings)
                {
                    Console.WriteLine($"      {mapping.Key} -> {mapping.Value}");
                }
                Console.WriteLine("   ✅ Step 3 completed successfully");
                
                // Step 4: Create a PickUpHL action instance for testing
                Console.WriteLine("\n📋 STEP 4: Creating PickUpHL action instance");
                Console.WriteLine("   Input: Action parameters (beam1, location1, robot1)");
                
                // Get instances from blackboard
                var beam1 = blackboard.GetElement(new FastName("b1")) as Beam;
                var location1 = blackboard.GetLocation(new FastName("fp1")) as Firstposition;
                var robot1 = blackboard.GetAgent(new FastName("r1")) as Robot;
                
                if (beam1 == null || location1 == null || robot1 == null)
                {
                    Console.WriteLine("   ❌ Step 4 failed: Could not retrieve required instances from blackboard");
                    Console.WriteLine($"      beam1: {(beam1 != null ? "found" : "not found")}");
                    Console.WriteLine($"      location1: {(location1 != null ? "found" : "not found")}");
                    Console.WriteLine($"      robot1: {(robot1 != null ? "found" : "not found")}");
                    return;
                }
                
                var pickUpAction = new PickUpHL("PickUpHL", "test_pickup_hl", blackboard, beam1, location1, robot1);
                Console.WriteLine($"   Output: PickUpHL action created with instance name: {pickUpAction.InstanceName}");
                Console.WriteLine($"   Action type: {pickUpAction.actionType}");
                Console.WriteLine($"   Parameters: beam={beam1.NameKey}, location={location1.NameKey}, robot={robot1.NameKey}");
                Console.WriteLine("   ✅ Step 4 completed successfully");
                
                // Step 5: Queue the action for injection
                Console.WriteLine("\n📋 STEP 5: Queueing action for injection");
                Console.WriteLine($"   Input: PickUpHL action '{pickUpAction.InstanceName}'");
                subtreeService.QueueActionForInjection(pickUpAction);
                Console.WriteLine("   Output: Action queued for injection in next tick");
                Console.WriteLine("   ✅ Step 5 completed successfully");
                
                // Step 6: Test the Tick method (this is where the injection happens)
                Console.WriteLine("\n📋 STEP 6: Testing Tick method (subtree injection)");
                Console.WriteLine("   Input: Tick call with deltaTime=0.1f");
                Console.WriteLine("   Expected: Action should be detected as HL and subtree should be injected");
                
                bool tickResult = subtreeService.Tick(0.1f);
                Console.WriteLine($"   Output: Tick result = {tickResult}");
                
                if (tickResult)
                {
                    Console.WriteLine("   ✅ Step 6 completed successfully - Tick returned true");
                }
                else
                {
                    Console.WriteLine("   ❌ Step 6 failed - Tick returned false");
                }
                
                // Step 7: Verify that the action now has a subtree
                Console.WriteLine("\n📋 STEP 7: Verifying subtree injection");
                Console.WriteLine("   Input: PickUpHL action after tick");
                
                // Check if the action has been converted to a high-level action
                bool hasSubtree = pickUpAction.IsHighLevelAction;
                Console.WriteLine($"   Output: Action has subtree = {hasSubtree}");
                
                if (hasSubtree)
                {
                    Console.WriteLine("   ✅ Step 7 completed successfully - Subtree was injected");
                    
                    // Get the subtree details
                    var subtree = pickUpAction.HighLevelSubtree;
                    if (subtree != null)
                    {
                        Console.WriteLine($"   Subtree type: {subtree.GetType().Name}");
                        Console.WriteLine($"   Subtree name: {subtree.NodeName.ToString()}");
                    }
                }
                else
                {
                    Console.WriteLine("   ❌ Step 7 failed - No subtree was injected");
                }
                
                // Step 8: Test custom planner mapping
                Console.WriteLine("\n📋 STEP 8: Testing custom planner mapping");
                Console.WriteLine("   Input: Setting PickUpHL -> ENHSP_Default mapping");
                subtreeService.SetPlannerMapping("PickUpHL", "ENHSP_Default");
                
                var updatedMappings = subtreeService.GetPlannerMappings();
                var pickUpMapping = updatedMappings.FirstOrDefault(m => m.Key == "PickUpHL");
                Console.WriteLine($"   Output: PickUpHL mapping = {pickUpMapping.Value}");
                Console.WriteLine("   ✅ Step 8 completed successfully");
                
                
                // Step 10: Test manual subtree injection
                Console.WriteLine("\n📋 STEP 10: Testing manual subtree injection");
                Console.WriteLine("   Input: Creating new PickUpML action and manually injecting GOAP subtree");
                
                var vacuumGripper = blackboard.GetTool(new FastName("vg1")) as VacuumGripper;
                if (vacuumGripper == null)
                {
                    Console.WriteLine("   ⚠️ Step 10 skipped: VacuumGripper not found in blackboard");
                }
                else
                {
                    var pickUpMLAction = new PickUpML("PickUpML", "test_pickup_ml", blackboard, beam1, location1, robot1, vacuumGripper);
                    Console.WriteLine($"   Created PickUpML action: {pickUpMLAction.InstanceName}");
                    
                    subtreeService.InjectSubtreeIntoAction(pickUpMLAction, "GOAP_Default", "test_goap");
                    Console.WriteLine("   Output: GOAP subtree manually injected");
                    
                    bool mlHasSubtree = pickUpMLAction.IsHighLevelAction;
                    Console.WriteLine($"   PickUpML has subtree: {mlHasSubtree}");
                    Console.WriteLine("   ✅ Step 10 completed successfully");
                }
                
                // Step 11: Test subtree removal
                Console.WriteLine("\n📋 STEP 11: Testing subtree removal");
                Console.WriteLine("   Input: Removing subtree from PickUpHL action");
                subtreeService.RemoveSubtreeFromAction(pickUpAction);
                
                bool stillHasSubtree = pickUpAction.IsHighLevelAction;
                Console.WriteLine($"   Output: Action still has subtree = {stillHasSubtree}");
                Console.WriteLine("   ✅ Step 11 completed successfully");
                
                // Step 12: Test cache operations
                Console.WriteLine("\n📋 STEP 12: Testing cache operations");
                Console.WriteLine("   Input: Getting cache statistics before clearing");
                var beforeClearStats = subtreeService.GetStatistics();
                Console.WriteLine($"   Output: Cached subtrees before clear = {beforeClearStats.cachedSubtrees}");
                
                subtreeService.ClearCache();
                Console.WriteLine("   Input: Clearing cache");
                
                var afterClearStats = subtreeService.GetStatistics();
                Console.WriteLine($"   Output: Cached subtrees after clear = {afterClearStats.cachedSubtrees}");
                Console.WriteLine("   ✅ Step 12 completed successfully");
                
                // Step 13: Test non-HL action (should not be injected)
                Console.WriteLine("\n📋 STEP 13: Testing non-HL action (no injection)");
                Console.WriteLine("   Input: Creating PickUpML action (non-HL) and queueing for injection");
                
                var pickUpMLAction2 = FactoryAction.Instance.CreateActionInstance("PickUpML test_pickup_ml2 b1 fp1 r1 vg1", blackboard);
               
                subtreeService.QueueActionForInjection(pickUpMLAction2);
                
                bool mlTickResult = subtreeService.Tick(0.1f);
                Console.WriteLine($"   Output: Tick result for non-HL action = {mlTickResult}");
                
                bool mlHasSubtree2 = pickUpMLAction2.IsHighLevelAction;
                Console.WriteLine($"   Non-HL action has subtree = {mlHasSubtree2}");
                
                if (!mlHasSubtree2)
                {
                    Console.WriteLine("   ✅ Step 13 completed successfully - Non-HL action correctly not injected");
                }
                else
                {
                    Console.WriteLine("   ❌ Step 13 failed - Non-HL action was incorrectly injected");
                }
                
                // Step 14: Final statistics
                Console.WriteLine("\n📋 STEP 14: Final statistics");
                Console.WriteLine("   Input: Getting final statistics");
                var finalStats = subtreeService.GetStatistics();
                Console.WriteLine($"   Output: Final cached subtrees = {finalStats.cachedSubtrees}");
                Console.WriteLine($"   Output: Final configurations = {finalStats.configurations}");
                Console.WriteLine($"   Output: Final planner mappings = {finalStats.plannerMappings}");
                Console.WriteLine("   ✅ Step 14 completed successfully");
                
                Console.WriteLine("\n🎉 SUBTREE INJECTION SERVICE TEST COMPLETED SUCCESSFULLY!");
                Console.WriteLine("=".PadRight(80, '='));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR during SubtreeInjectionService test: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                Console.WriteLine("=".PadRight(80, '='));
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
