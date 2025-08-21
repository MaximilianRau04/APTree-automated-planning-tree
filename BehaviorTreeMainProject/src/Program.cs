using Neo4j.Driver;
using ModelLoader;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;
using BehaviorTreeMainProject;
using AIPlanning;
using PlanningDataStructures;
using BehaviorTreeMainProject.Services.GOAPPlanning;


public class Program
{
    public static async Task Main(string[] args)
    {
        
        // test the full tree
                Console.WriteLine("\n=== TESTING FULL BEHAVIOR TREE ===");
                await FullTreeTest.RunTest();
        // try
        // {
        // Console.WriteLine("Setting up Blackboard...");

        // // Create blackboard instance (skipping Neo4j for now)
        // using var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "12345678");

        // // Skip Neo4j connection test for now
        // Console.WriteLine("⚠️  Skipping Neo4j connection test for now...");
        // bool connectionSuccess = true; // Assume success to continue with PDDL testing

        // if (connectionSuccess)
        // {
        //     Console.WriteLine("✅ Continuing without Neo4j...");

        // // Create BlackboardWriter for type registration
        // var blackboardWriter = new BlackboardWriter(blackboard);

        // // Register all types
        // Console.WriteLine("\n=== REGISTERING ALL TYPES ===");
        // blackboardWriter.RegisterAllTypes();

        // // Register all instances from files
        // Console.WriteLine("\n=== REGISTERING ALL INSTANCES FROM FILES ===");
        // string actionInstancesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "ActionInstances.txt");
        // blackboardWriter.RegisterAllInstances(actionInstancesFile);

        // Console.WriteLine("✅ All operations completed successfully!");

        // // Test PDDL Planning Pipeline Step by Step
        // Console.WriteLine("\n=== TESTING PDDL PLANNING PIPELINE STEP BY STEP ===");
        // TestPDDLPlanningPipeline(blackboard);



        // Test GOAP Example (commented out for now)
        // Console.WriteLine("\n=== TESTING GOAP EXAMPLE ===");
        // RobotGOAPExample.Test();
        // }
        //     else
        //     {
        //         Console.WriteLine("❌ Setup failed!");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"❌ Error: {ex.Message}");
        //     Console.WriteLine("Please check your setup.");
        // }
    }
    
    /// <summary>
    /// Comprehensive test function to demonstrate flow node logic and graph execution
    /// This function shows exactly what happens when a flow node is created and ticked
    /// </summary>
    // private static async Task TestFlowNodeLogic(Blackboard<FastName> blackboard)
    // {
    //     Console.WriteLine("\n🔧 STEP 1: Creating Behavior Tree Instance");
    //     Console.WriteLine("==========================================");
        
    //     // Create behavior tree instance
    //     var behaviorTree = new BTInstance();
    //     behaviorTree.Initialise(blackboard, "TestFlowTree");
    //     Console.WriteLine($"✅ Created behavior tree: {behaviorTree.DebugDisplayName}");
        
    //     Console.WriteLine("\n🔧 STEP 2: Creating Dynamic Flow Node");
    //     Console.WriteLine("=====================================");
        
    //     // Create a dynamic flow node with different success criteria for testing
    //     var flowNode = new BTFlowNode_Dynamic(
    //         behaviorTree, 
    //         SuccessCriteria.ANY,  // At least one child must succeed
    //         0.0f
    //     );
    //     Console.WriteLine($"✅ Created flow node: {flowNode.DebugDisplayName}");
    //     Console.WriteLine($"   Success Criteria: {flowNode.successCriteria}");
    //     Console.WriteLine($"   Success Threshold: 0.0f (configured for ANY criteria)");
        
    //     Console.WriteLine("\n🔧 STEP 3: Getting Registered Action Instances from Blackboard");
    //     Console.WriteLine("=============================================================");
        
        
        
    //     // Get all action instances directly from blackboard
    //     var allActionInstances = blackboard.GetAllActionInstances();
    //     Console.WriteLine($"📋 Available action instances: {allActionInstances.Count}");
        
    //     if (allActionInstances.Count > 0)
    //     {
    //         // Use all available action instances for testing
          
            
    //                      foreach (var actionInstance in allActionInstances)
    //          {
    //              Console.WriteLine($"✅ Retrieved action: {actionInstance.InstanceName.ToString()} (Type: {actionInstance.actionType.ToString()})");
    //          }
    //     }
    //     else
    //     {
    //         Console.WriteLine("❌ No action instances found in blackboard!");
    //         Console.WriteLine("Make sure the InputInstances folder contains action definitions.");
    //         return; // Exit the test if no actions are available
    //     }
        
    //     Console.WriteLine($"📊 Total action nodes created: {allActionInstances.Count}");
        
    //     Console.WriteLine("\n🔧 STEP 4: Creating NodeGraph from Actions");
    //     Console.WriteLine("===========================================");
        
    //     // Create a NodeGraph from the action instances with default relations
    //     var nodeGraph = flowNode.CreateNodeGraphFromActions(allActionInstances);
        
    //     // Replace the flow node's action graph with our new one
    //     // Note: This would require making actionGraph accessible or adding a method to set it
    //     // For now, we'll add actions individually and show the graph structure
        
    //     Console.WriteLine($"📊 Created NodeGraph with {nodeGraph.GetAllActionNodes().Count} nodes");
    //     Console.WriteLine($"📊 Execution order: {string.Join(" → ", nodeGraph.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
        
    //     Console.WriteLine("\n🔧 STEP 5: Using NodeGraph for Flow Node");
    //     Console.WriteLine("=========================================");
        
    //     // Use the NodeGraph we created instead of adding actions individually
    //     flowNode.SetActionGraph(nodeGraph);
    //     Console.WriteLine($"✅ Set NodeGraph with {nodeGraph.GetAllActionNodes().Count} nodes as flow node's action graph");
    //     Console.WriteLine($"📊 NodeGraph execution order: {string.Join(" → ", nodeGraph.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
        
    //     // Debug: Show what's in the NodeGraph
    //     Console.WriteLine("\n🔍 DEBUG: NodeGraph Contents:");
    //     var allNodes = nodeGraph.GetAllActionNodes();
    //     for (int i = 0; i < allNodes.Count; i++)
    //     {
    //         Console.WriteLine($"   Node {i}: {allNodes[i].InstanceName.ToString()}");
    //     }
        
    //     // Note: We don't need to add actions individually anymore since the NodeGraph contains them
        
    //     Console.WriteLine("\n🔧 STEP 6: Examining Flow Node Structure");
    //     Console.WriteLine("=========================================");
        
    //     // Examine the flow node structure through public interface
    //     Console.WriteLine($"📊 Flow node debug name: {flowNode.DebugDisplayName}");
    //     Console.WriteLine($"📊 Flow node has children: {flowNode.HasChildren}");
    //     Console.WriteLine($"📊 Flow node last status: {flowNode.LastStatus}");
    //     Console.WriteLine($"📊 Flow node has finished: {flowNode.HasFinished}");
        
    //     Console.WriteLine("\n🔧 STEP 7: Testing Flow Node Tick Logic");
    //     Console.WriteLine("======================================");
        
    //     // Test the tick logic step by step
    //     float deltaTime = 0.016f; // 60 FPS simulation
        
    //     for (int tick = 1; tick <= 5; tick++)
    //     {
    //         Console.WriteLine($"\n🔄 TICK {tick}:");
    //         Console.WriteLine("   " + new string('-', 40));
            
    //         // Show flow node status before tick
    //         Console.WriteLine($"   📊 Flow node status before tick: {flowNode.LastStatus}");
    //         Console.WriteLine($"   🎯 Flow node finished: {flowNode.HasFinished}");
            
    //         // Execute the tick
    //         Console.WriteLine($"   ⚡ Executing flow node tick...");
    //         var tickResult = flowNode.Tick(deltaTime);
    //         Console.WriteLine($"   📊 Tick result: {tickResult}");
            
    //         // Check flow node status after tick
    //         Console.WriteLine($"   📊 Flow node status after tick: {flowNode.LastStatus}");
    //         Console.WriteLine($"   🎯 Flow node finished: {flowNode.HasFinished}");
            
    //         // If flow node is finished, break
    //         if (flowNode.HasFinished)
    //         {
    //             Console.WriteLine($"   🏁 Flow node completed on tick {tick}");
    //             break;
    //         }
            
    //         // Small delay to simulate real-time execution
    //         await Task.Delay(100);
    //     }
        
    //     Console.WriteLine("\n🔧 STEP 8: Final Results");
    //     Console.WriteLine("=======================");
        
    //     Console.WriteLine($"📊 Final flow node status: {flowNode.LastStatus}");
    //     Console.WriteLine($"🎯 Flow node finished: {flowNode.HasFinished}");
    //     Console.WriteLine($"✅ Success criteria evaluation completed");
        
    //     Console.WriteLine("\n📋 Test completed successfully!");
        
    //     Console.WriteLine("\n✅ Flow Node Logic Test Completed!");
    //     Console.WriteLine("This test demonstrates:");
    //     Console.WriteLine("1. Flow node creation and configuration");
    //     Console.WriteLine("2. Action node addition to the graph");
    //     Console.WriteLine("3. Flow node structure examination");
    //     Console.WriteLine("4. Step-by-step tick execution");
    //     Console.WriteLine("5. Success criteria evaluation");
    //     Console.WriteLine("6. Node status tracking");
        
    //     // Test different success criteria
    //     Console.WriteLine("\n🔧 STEP 9: Testing Different Success Criteria");
    //     Console.WriteLine("============================================");
        
    //     await TestDifferentSuccessCriteria(blackboard);
    // }
    
        /// <summary>
    /// Test different success criteria to show how they affect flow node behavior
    /// </summary>
    // private static async Task TestDifferentSuccessCriteria(Blackboard<FastName> blackboard)
    // {
    //     // Reset all action instances before testing different success criteria
    //     Console.WriteLine("🔄 Resetting all action instances for clean test...");
    //     var actionInstancesToReset = blackboard.GetAllActionInstances();
    //     foreach (var actionInstance in actionInstancesToReset)
    //     {
    //         actionInstance.Reset();
    //     }
    //     Console.WriteLine($"✅ Reset {actionInstancesToReset.Count} action instances");
        
    //     Console.WriteLine("\n🔧 Testing SuccessCriteria.ALL (all children must succeed)");
    //     Console.WriteLine("========================================================");
        
    //     var behaviorTree = new BTInstance();
    //     behaviorTree.Initialise(blackboard, "TestALL");
        
    //     var flowNodeALL = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL, 1.0f);
        
    //     // Get real actions from blackboard and create a proper NodeGraph
    //     var allActionInstances = blackboard.GetAllActionInstances();
    //     if (allActionInstances.Count >= 3)
    //     {
    //         // Take first 3 actions and create a NodeGraph with proper relations
    //         var testActions = allActionInstances.Take(3).Cast<GenericBTAction>().ToList();
    //         var nodeGraphALL = flowNodeALL.CreateNodeGraphFromActions(testActions);
    //         flowNodeALL.SetActionGraph(nodeGraphALL);
            
    //         Console.WriteLine($"   Created NodeGraph with {nodeGraphALL.GetAllActionNodes().Count} nodes");
    //         Console.WriteLine($"   Execution order: {string.Join(" → ", nodeGraphALL.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
    //     }
    //     else
    //     {
    //         Console.WriteLine("   ⚠️  Not enough actions available for ALL test");
    //         return;
    //     }
        
    //     // Tick until completion
    //     for (int tick = 1; tick <= 5; tick++)
    //     {
    //         Console.WriteLine($"   Tick {tick}: Status = {flowNodeALL.LastStatus}, Finished = {flowNodeALL.HasFinished}");
    //         flowNodeALL.Tick(0.016f);
    //         if (flowNodeALL.HasFinished) break;
    //         await Task.Delay(50);
    //     }
        
    //     Console.WriteLine($"   Final Result: {flowNodeALL.LastStatus}");
        
    //     Console.WriteLine("\n🔧 Testing SuccessCriteria.ANY (at least one child must succeed)");
    //     Console.WriteLine("=============================================================");
        
    //     var flowNodeANY = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ANY, 0.0f);
        
    //     // Get real actions from blackboard and create a proper NodeGraph
    //     if (allActionInstances.Count >= 3)
    //     {
    //         // Take first 3 actions and create a NodeGraph with proper relations
    //         var testActions = allActionInstances.Take(3).Cast<GenericBTAction>().ToList();
    //         var nodeGraphANY = flowNodeANY.CreateNodeGraphFromActions(testActions);
    //         flowNodeANY.SetActionGraph(nodeGraphANY);
            
    //         Console.WriteLine($"   Created NodeGraph with {nodeGraphANY.GetAllActionNodes().Count} nodes");
    //         Console.WriteLine($"   Execution order: {string.Join(" → ", nodeGraphANY.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
    //     }
    //     else
    //     {
    //         Console.WriteLine("   ⚠️  Not enough actions available for ANY test");
    //         return;
    //     }
        
    //     // Tick until completion
    //     for (int tick = 1; tick <= 5; tick++)
    //     {
    //         Console.WriteLine($"   Tick {tick}: Status = {flowNodeANY.LastStatus}, Finished = {flowNodeANY.HasFinished}");
    //         flowNodeANY.Tick(0.016f);
    //         if (flowNodeANY.HasFinished) break;
    //         await Task.Delay(50);
    //     }
        
    //     Console.WriteLine($"   Final Result: {flowNodeANY.LastStatus}");
        
    //     Console.WriteLine("\n✅ Success Criteria Tests Completed!");
    // }

    // /// <summary>
    // /// Test NodeGraph parsing from file and storage in blackboard
    // /// </summary>
    // private static async Task TestNodeGraphParsing(Blackboard<FastName> blackboard)
    // {
    //     Console.WriteLine("\n🔧 STEP 10: Testing NodeGraph Parsing from File");
    //     Console.WriteLine("=============================================");
        
    //     try
    //     {
    //         // Create BlackboardWriter for NodeGraph parsing
    //         var blackboardWriter = new BlackboardWriter(blackboard);
            
    //         // Path to the NodeGraph file
    //         string nodeGraphFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "NodeGraphGenerated.txt");
            
    //         Console.WriteLine($"📁 NodeGraph file path: {nodeGraphFile}");
            
    //         // Parse and register the NodeGraph
    //         var nodeGraph = blackboardWriter.ParseAndRegisterNodeGraph(nodeGraphFile);
            
    //         Console.WriteLine($"✅ Successfully parsed NodeGraph from file");
    //         Console.WriteLine($"📊 NodeGraph contains {nodeGraph.GetAllActionNodes().Count} action nodes");
            
    //         // Test retrieving the NodeGraph from blackboard
    //         Console.WriteLine("\n🔧 STEP 11: Testing NodeGraph Retrieval from Blackboard");
    //         Console.WriteLine("=======================================================");
            
    //         var allNodeGraphs = blackboard.GetAllNodeGraphs();
    //         Console.WriteLine($"📊 Total NodeGraphs in blackboard: {allNodeGraphs.Count}");
            
    //         foreach (var graph in allNodeGraphs)
    //         {
    //             Console.WriteLine($"   📋 NodeGraph with {graph.GetAllActionNodes().Count} actions");
    //             Console.WriteLine($"   📋 Execution order: {string.Join(" → ", graph.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
    //         }
            
    //         // Test getting a specific NodeGraph by name
    //         try
    //         {
    //             var retrievedNodeGraph = blackboard.GetNodeGraph(new FastName("Cassette1"));
    //             Console.WriteLine($"✅ Successfully retrieved NodeGraph 'Cassette1' from blackboard");
    //             Console.WriteLine($"📊 Retrieved NodeGraph has {retrievedNodeGraph.GetAllActionNodes().Count} actions");
    //         }
    //         catch (ArgumentException ex)
    //         {
    //             Console.WriteLine($"❌ Error retrieving NodeGraph: {ex.Message}");
    //         }
            
    //         // Test NodeGraph execution simulation
    //         Console.WriteLine("\n🔧 STEP 12: Testing NodeGraph Execution Simulation");
    //         Console.WriteLine("=================================================");
            
    //         // Reset the NodeGraph for testing
    //         nodeGraph.Reset();
    //         Console.WriteLine("🔄 Reset NodeGraph state for testing");
            
    //         // Simulate execution for a few ticks
    //         float deltaTime = 0.016f;
    //         for (int tick = 1; tick <= 10; tick++)
    //         {
    //             Console.WriteLine($"\n🔄 TICK {tick}:");
    //             Console.WriteLine("   " + new string('-', 30));
                
    //             var executableNodes = nodeGraph.GetExecutableNodes(deltaTime);
    //             Console.WriteLine($"   📊 Executable nodes: {executableNodes.Count}");
                
    //             foreach (var executableNode in executableNodes)
    //             {
    //                 Console.WriteLine($"   ✅ Can execute: {executableNode.InstanceName.ToString()}");
    //                 // Mark the node as completed for simulation
    //                 nodeGraph.MarkNodeCompleted(executableNode);
    //             }
                
    //             // Check if all nodes are completed
    //             var allNodes = nodeGraph.GetAllActionNodes();
    //             var completedNodes = allNodes.Count(node => 
    //             {
    //                 // This is a simplified check - in real implementation you'd check the actual completion status
    //                 return true; // For simulation purposes
    //             });
                
    //             Console.WriteLine($"   📊 Progress: {completedNodes}/{allNodes.Count} nodes completed");
                
    //             if (completedNodes >= allNodes.Count)
    //             {
    //                 Console.WriteLine($"   🏁 All nodes completed on tick {tick}");
    //                 break;
    //             }
                
    //             await Task.Delay(100);
    //         }
            
    //         Console.WriteLine("\n✅ NodeGraph Parsing Test Completed!");
    //         Console.WriteLine("This test demonstrates:");
    //         Console.WriteLine("1. NodeGraph parsing from text file");
    //         Console.WriteLine("2. NodeGraph storage in blackboard");
    //         Console.WriteLine("3. NodeGraph retrieval from blackboard");
    //         Console.WriteLine("4. NodeGraph execution simulation");
    //         Console.WriteLine("5. Action dependency resolution");
            
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"❌ Error in NodeGraph parsing test: {ex.Message}");
    //         Console.WriteLine($"Stack trace: {ex.StackTrace}");
    //     }
    // }

    // /// <summary>
    // /// Test Flow Node with NodeGraph execution
    // /// </summary>
    // private static async Task TestFlowNodeWithNodeGraph(Blackboard<FastName> blackboard)
    // {
    //     Console.WriteLine("\n🔧 STEP 13: Testing Flow Node with NodeGraph");
    //     Console.WriteLine("===========================================");
        
    //     try
    //     {
    //         // First, ensure we have a NodeGraph in the blackboard
    //         Console.WriteLine("📋 Step 1: Ensuring NodeGraph is available in blackboard");
            
    //         NodeGraph nodeGraph;
    //         try
    //         {
    //             nodeGraph = blackboard.GetNodeGraph(new FastName("Cassette1"));
    //             Console.WriteLine($"✅ Found existing NodeGraph 'Cassette1' with {nodeGraph.GetAllActionNodes().Count} actions");
    //         }
    //         catch (ArgumentException)
    //         {
    //             Console.WriteLine("⚠️ NodeGraph not found, creating one from file...");
    //             var blackboardWriter = new BlackboardWriter(blackboard);
    //             string nodeGraphFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "NodeGraphGenerated.txt");
    //             nodeGraph = blackboardWriter.ParseAndRegisterNodeGraph(nodeGraphFile);
    //             Console.WriteLine($"✅ Created NodeGraph 'Cassette1' with {nodeGraph.GetAllActionNodes().Count} actions");
    //         }
            
    //         // Create a behavior tree instance
    //         Console.WriteLine("\n📋 Step 2: Creating behavior tree instance");
    //         var behaviorTree = new BTInstance();
    //         behaviorTree.Initialise(blackboard, "TestTree");
    //         Console.WriteLine($"✅ Created behavior tree: {behaviorTree.DebugDisplayName}");
            
    //         // Create a flow node with ALL success criteria
    //         Console.WriteLine("\n📋 Step 3: Creating flow node with ALL success criteria");
    //         var flowNode = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL);
    //         flowNode.SetOwiningTree(behaviorTree);
    //         Console.WriteLine($"✅ Created flow node with success criteria: {flowNode.successCriteria}");
            
    //         // Set the NodeGraph as the action graph for the flow node
    //         Console.WriteLine("\n📋 Step 4: Setting NodeGraph as action graph for flow node");
    //         flowNode.SetActionGraph(nodeGraph);
    //         Console.WriteLine($"✅ Set NodeGraph with {nodeGraph.GetAllActionNodes().Count} actions as flow node's action graph");
            
    //         // Reset the NodeGraph for testing
    //         Console.WriteLine("\n📋 Step 5: Resetting NodeGraph for testing");
    //         nodeGraph.Reset();
    //         Console.WriteLine("🔄 Reset NodeGraph state");
            
    //         // Test the flow node execution
    //         Console.WriteLine("\n📋 Step 6: Testing flow node execution");
    //         Console.WriteLine("=====================================");
            
    //         float deltaTime = 0.016f;
    //         int maxTicks = 50; // Limit to prevent infinite loops
            
    //         for (int tick = 1; tick <= maxTicks; tick++)
    //         {
    //             Console.WriteLine($"\n🔄 TICK {tick}:");
    //             Console.WriteLine("   " + new string('-', 40));
                
    //             // Tick the flow node
    //             var result = flowNode.Tick(deltaTime);
    //             Console.WriteLine($"   📊 Flow node result: {result}");
    //             Console.WriteLine($"   📊 Flow node status: {flowNode.LastStatus}");
                
    //             // Check if the flow node has finished
    //             if (flowNode.HasFinished)
    //             {
    //                 Console.WriteLine($"   🏁 Flow node finished on tick {tick} with status: {flowNode.LastStatus}");
    //                 break;
    //             }
                
    //             // Show progress of action nodes
    //             var allActionNodes = nodeGraph.GetAllActionNodes();
    //             var completedNodes = allActionNodes.Count(node => 
    //                 node.LastStatus == EBTNodeResult.Succeeded || node.LastStatus == EBTNodeResult.failed);
                
    //             Console.WriteLine($"   📊 Action progress: {completedNodes}/{allActionNodes.Count} completed");
                
    //             // Show status of each action node
    //             foreach (var actionNode in allActionNodes)
    //             {
    //                 Console.WriteLine($"      {actionNode.InstanceName.ToString()}: {actionNode.LastStatus}");
    //             }
                
    //             // Small delay to make output readable
    //             await Task.Delay(200);
    //         }
            
    //         // Final summary
    //         Console.WriteLine("\n📋 Step 7: Final Summary");
    //         Console.WriteLine("=======================");
            
    //         var finalActionNodes = nodeGraph.GetAllActionNodes();
    //         var succeededNodes = finalActionNodes.Count(node => node.LastStatus == EBTNodeResult.Succeeded);
    //         var failedNodes = finalActionNodes.Count(node => node.LastStatus == EBTNodeResult.failed);
    //         var inProgressNodes = finalActionNodes.Count(node => node.LastStatus == EBTNodeResult.InProgress);
            
    //         Console.WriteLine($"📊 Final Flow Node Status: {flowNode.LastStatus}");
    //         Console.WriteLine($"📊 Total Actions: {finalActionNodes.Count}");
    //         Console.WriteLine($"📊 Succeeded: {succeededNodes}");
    //         Console.WriteLine($"📊 Failed: {failedNodes}");
    //         Console.WriteLine($"📊 In Progress: {inProgressNodes}");
            
    //         if (flowNode.LastStatus == EBTNodeResult.Succeeded)
    //         {
    //             Console.WriteLine("🎉 SUCCESS: Flow node completed successfully with ALL success criteria!");
    //         }
    //         else if (flowNode.LastStatus == EBTNodeResult.failed)
    //         {
    //             Console.WriteLine("❌ FAILED: Flow node failed to meet ALL success criteria");
    //         }
    //         else
    //         {
    //             Console.WriteLine("⏳ IN PROGRESS: Flow node is still executing");
    //         }
            
    //         Console.WriteLine("\n✅ Flow Node with NodeGraph Test Completed!");
    //         Console.WriteLine("This test demonstrates:");
    //         Console.WriteLine("1. Flow node creation with ALL success criteria");
    //         Console.WriteLine("2. Setting NodeGraph as flow node's action graph");
    //         Console.WriteLine("3. Flow node execution with NodeGraph");
    //         Console.WriteLine("4. Success criteria evaluation (ALL actions must succeed)");
    //         Console.WriteLine("5. Action execution logging and status tracking");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"❌ Error in Flow Node with NodeGraph test: {ex.Message}");
    //         Console.WriteLine($"Stack trace: {ex.StackTrace}");
    //     }
    // }

    // /// <summary>
    // /// Test a behavior tree with two flow nodes, each having their own NodeGraph instance
    // /// First flow node: ALL success criteria
    // /// Second flow node: 50% success criteria
    // /// </summary>
    // private static async Task TestTwoFlowNodesWithNodeGraphs(Blackboard<FastName> blackboard)
    // {
    //     Console.WriteLine("\n🔧 TESTING TWO FLOW NODES WITH NODEGRAPHS");
    //     Console.WriteLine("=========================================");
        
    //     try
    //     {
    //         // Create BlackboardWriter for parsing NodeGraphs
    //         var blackboardWriter = new BlackboardWriter(blackboard);
            
    //         // Parse and register both NodeGraphs
    //         Console.WriteLine("\n📋 Step 1: Parsing and registering NodeGraphs");
    //         Console.WriteLine("=============================================");
            
    //         string nodeGraphFile1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "NodeGraphGenerated.txt");
    //         string nodeGraphFile2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "NodeGraphGenerated2.txt");
            
    //         NodeGraph nodeGraph1 = blackboardWriter.ParseAndRegisterNodeGraph(nodeGraphFile1);
    //         NodeGraph nodeGraph2 = blackboardWriter.ParseAndRegisterNodeGraph(nodeGraphFile2);
            
    //         Console.WriteLine($"✅ NodeGraph1 'Cassette1': {nodeGraph1.GetAllActionNodes().Count} actions");
    //         Console.WriteLine($"✅ NodeGraph2 'Cassette2': {nodeGraph2.GetAllActionNodes().Count} actions");
            
    //         // Create behavior tree instance
    //         Console.WriteLine("\n📋 Step 2: Creating behavior tree instance");
    //         var behaviorTree = new BTInstance();
    //         behaviorTree.Initialise(blackboard, "TwoFlowNodesTree");
    //         Console.WriteLine($"✅ Created behavior tree: {behaviorTree.DebugDisplayName}");
            
    //         // Create first flow node with ALL success criteria
    //         Console.WriteLine("\n📋 Step 3: Creating first flow node (ALL success criteria)");
    //         var flowNode1 = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL);
    //         flowNode1.SetActionGraph(nodeGraph1);
    //         Console.WriteLine($"✅ Created flow node 1 with success criteria: {flowNode1.successCriteria}");
            
    //         // Create second flow node with 50% success criteria
    //         Console.WriteLine("\n📋 Step 4: Creating second flow node (50% success criteria)");
    //         var flowNode2 = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.PERCENTAGE, 0.5f);
    //         flowNode2.SetActionGraph(nodeGraph2);
    //         Console.WriteLine($"✅ Created flow node 2 with success criteria: {flowNode2.successCriteria} (50%)");
            
    //         // Add both flow nodes as children to the root node
    //         Console.WriteLine("\n📋 Step 5: Adding flow nodes as children to root node");
    //         behaviorTree.AddChildToRootNode<BTFlowNode_Dynamic>(flowNode1);
    //         behaviorTree.AddChildToRootNode<BTFlowNode_Dynamic>(flowNode2);
    //         Console.WriteLine($"✅ Added flow node 1 as child to root node");
    //         Console.WriteLine($"✅ Added flow node 2 as child to root node");
            
    //         // Get the composite root node to show child count
    //         var compositeRoot = behaviorTree.RootNode as BTFlowNode_Composite;
    //         Console.WriteLine($"📊 Root node has {compositeRoot?.ChildCount ?? 0} children");
            
    //         // Reset both NodeGraphs for testing
    //         Console.WriteLine("\n📋 Step 5: Resetting NodeGraphs for testing");
    //         nodeGraph1.Reset();
    //         nodeGraph2.Reset();
    //         Console.WriteLine("🔄 Reset both NodeGraphs");
            
    //         // Test execution of the behavior tree with both flow nodes
    //         Console.WriteLine("\n📋 Step 6: Testing execution of behavior tree with both flow nodes");
    //         Console.WriteLine("===============================================================");
            
    //         float deltaTime = 0.016f;
    //         int maxTicks = 100; // Increased limit for two flow nodes
            
    //         for (int tick = 1; tick <= maxTicks; tick++)
    //         {
    //             Console.WriteLine($"\n🔄 TICK {tick}:");
    //             Console.WriteLine("   " + new string('-', 50));
                
    //             // Tick the behavior tree (which will tick the root node, which will tick both flow nodes)
    //             var treeResult = behaviorTree.Tick(deltaTime);
                
    //             Console.WriteLine($"   📊 Behavior Tree Result: {treeResult}");
    //             Console.WriteLine($"   📊 Root Node Status: {behaviorTree.RootNode.LastStatus}");
    //             Console.WriteLine($"   📊 Flow Node 1 (ALL): Status: {flowNode1.LastStatus}");
    //             Console.WriteLine($"   📊 Flow Node 2 (50%): Status: {flowNode2.LastStatus}");
                
    //             // Check if the behavior tree has finished
    //             if (behaviorTree.HasFinished())
    //             {
    //                 Console.WriteLine($"   🏁 Behavior tree finished on tick {tick}");
    //                 break;
    //             }
                
    //             // Show progress of action nodes for both NodeGraphs
    //             var allActionNodes1 = nodeGraph1.GetAllActionNodes();
    //             var allActionNodes2 = nodeGraph2.GetAllActionNodes();
                
    //             var completedNodes1 = allActionNodes1.Count(node => 
    //                 node.LastStatus == EBTNodeResult.Succeeded || node.LastStatus == EBTNodeResult.failed);
    //             var completedNodes2 = allActionNodes2.Count(node => 
    //                 node.LastStatus == EBTNodeResult.Succeeded || node.LastStatus == EBTNodeResult.failed);
                
    //             Console.WriteLine($"   📊 NodeGraph1 Progress: {completedNodes1}/{allActionNodes1.Count} completed");
    //             Console.WriteLine($"   📊 NodeGraph2 Progress: {completedNodes2}/{allActionNodes2.Count} completed");
                
    //             // Show status of each action node (only show first few for readability)
    //             Console.WriteLine("   📋 NodeGraph1 Actions:");
    //             foreach (var actionNode in allActionNodes1.Take(3))
    //             {
    //                 Console.WriteLine($"      {actionNode.InstanceName.ToString()}: {actionNode.LastStatus}");
    //             }
    //             if (allActionNodes1.Count > 3)
    //             {
    //                 Console.WriteLine($"      ... and {allActionNodes1.Count - 3} more actions");
    //             }
                
    //             Console.WriteLine("   📋 NodeGraph2 Actions:");
    //             foreach (var actionNode in allActionNodes2.Take(3))
    //             {
    //                 Console.WriteLine($"      {actionNode.InstanceName.ToString()}: {actionNode.LastStatus}");
    //             }
    //             if (allActionNodes2.Count > 3)
    //             {
    //                 Console.WriteLine($"      ... and {allActionNodes2.Count - 3} more actions");
    //             }
                
    //             // Small delay to make output readable
    //             await Task.Delay(300);
    //         }
            
    //         // Final summary
    //         Console.WriteLine("\n📋 Step 7: Final Summary");
    //         Console.WriteLine("=======================");
            
    //         var finalActionNodes1 = nodeGraph1.GetAllActionNodes();
    //         var finalActionNodes2 = nodeGraph2.GetAllActionNodes();
            
    //         var succeededNodes1 = finalActionNodes1.Count(node => node.LastStatus == EBTNodeResult.Succeeded);
    //         var failedNodes1 = finalActionNodes1.Count(node => node.LastStatus == EBTNodeResult.failed);
    //         var succeededNodes2 = finalActionNodes2.Count(node => node.LastStatus == EBTNodeResult.Succeeded);
    //         var failedNodes2 = finalActionNodes2.Count(node => node.LastStatus == EBTNodeResult.failed);
            
    //         Console.WriteLine($"📊 Behavior Tree Final Status: {behaviorTree.RootNode.LastStatus}");
    //         Console.WriteLine($"📊 Root Node Status: {behaviorTree.RootNode.LastStatus}");
            
    //         Console.WriteLine($"📊 Flow Node 1 (ALL) Final Status: {flowNode1.LastStatus}");
    //         Console.WriteLine($"   Total Actions: {finalActionNodes1.Count}");
    //         Console.WriteLine($"   Succeeded: {succeededNodes1}");
    //         Console.WriteLine($"   Failed: {failedNodes1}");
            
    //         Console.WriteLine($"📊 Flow Node 2 (50%) Final Status: {flowNode2.LastStatus}");
    //         Console.WriteLine($"   Total Actions: {finalActionNodes2.Count}");
    //         Console.WriteLine($"   Succeeded: {succeededNodes2}");
    //         Console.WriteLine($"   Failed: {failedNodes2}");
    //         Console.WriteLine($"   Success Rate: {(double)succeededNodes2 / finalActionNodes2.Count:P1}");
            
    //         // Success criteria evaluation
    //         Console.WriteLine("\n🎯 Success Criteria Evaluation:");
    //         if (flowNode1.LastStatus == EBTNodeResult.Succeeded)
    //         {
    //             Console.WriteLine("✅ Flow Node 1 (ALL): SUCCESS - All actions completed successfully");
    //         }
    //         else
    //         {
    //             Console.WriteLine("❌ Flow Node 1 (ALL): FAILED - Not all actions completed successfully");
    //         }
            
    //         if (flowNode2.LastStatus == EBTNodeResult.Succeeded)
    //         {
    //             Console.WriteLine("✅ Flow Node 2 (50%): SUCCESS - At least 50% of actions completed successfully");
    //         }
    //         else
    //         {
    //             Console.WriteLine("❌ Flow Node 2 (50%): FAILED - Less than 50% of actions completed successfully");
    //         }
            
    //         // Root node success criteria evaluation (ALL children must succeed)
    //         if (behaviorTree.RootNode.LastStatus == EBTNodeResult.Succeeded)
    //         {
    //             Console.WriteLine("✅ Root Node (ALL): SUCCESS - All child flow nodes completed successfully");
    //         }
    //         else
    //         {
    //             Console.WriteLine("❌ Root Node (ALL): FAILED - Not all child flow nodes completed successfully");
    //         }
            
    //         Console.WriteLine("\n✅ Two Flow Nodes with NodeGraphs Test Completed!");
    //         Console.WriteLine("This test demonstrates:");
    //         Console.WriteLine("1. Multiple NodeGraphs in the same behavior tree");
    //         Console.WriteLine("2. Different success criteria for different flow nodes");
    //         Console.WriteLine("3. Parallel execution of multiple flow nodes");
    //         Console.WriteLine("4. Independent NodeGraph execution and status tracking");
    //         Console.WriteLine("5. Success criteria evaluation for different thresholds");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"❌ Error in Two Flow Nodes with NodeGraphs test: {ex.Message}");
    //         Console.WriteLine($"Stack trace: {ex.StackTrace}");
    //     }
    // }

    // /// <summary>
    // /// Test high-level action system with modified PickUp action
    // /// Demonstrates hierarchical planning and execution
    // /// </summary>
    // private static async Task TestHighLevelActionSystem(Blackboard<FastName> blackboard)
    // {
    //     Console.WriteLine("\n🔧 TESTING HIGH-LEVEL ACTION SYSTEM");
    //     Console.WriteLine("===================================");
        
    //     try
    //     {
    //         // Create BlackboardWriter for parsing NodeGraphs
    //         var blackboardWriter = new BlackboardWriter(blackboard);
            
    //         // Register all instances to ensure we have actions available
    //         Console.WriteLine("\n📋 Step 1: Registering all instances");
    //         Console.WriteLine("====================================");
            
    //         string actionInstancesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "ActionInstances.txt");
    //         blackboardWriter.RegisterAllInstances(actionInstancesFile);
            
    //         Console.WriteLine($"✅ Registered all action instances");
            
    //         // Create behavior tree instance
    //         Console.WriteLine("\n📋 Step 2: Creating behavior tree instance");
    //         Console.WriteLine("==========================================");
            
    //         var behaviorTree = new BTInstance();
    //         behaviorTree.Initialise(blackboard, "HighLevelActionTree");
    //         Console.WriteLine($"✅ Created behavior tree: {behaviorTree.DebugDisplayName}");
            
    //         // Create a regular PickUp action
    //         Console.WriteLine("\n📋 Step 3: Creating regular PickUp action");
    //         Console.WriteLine("=========================================");
            
    //         var regularPickUpAction = blackboard.GetAction(new FastName("pickUpHL_lp1_r1_fp1")) as PickUpHL;
    //         if (regularPickUpAction == null)
    //         {
    //             Console.WriteLine("❌ Could not find PickUp action instance");
    //             return;
    //         }
            
    //         regularPickUpAction.SetOwiningTree(behaviorTree);
    //         Console.WriteLine($"✅ Created regular PickUp action: {regularPickUpAction.InstanceName.ToString()}");
    //         Console.WriteLine($"📊 IsHighLevelAction: {regularPickUpAction.IsHighLevelAction}");
            
    //         // Create a high-level PickUp action (use a different instance)
    //         Console.WriteLine("\n📋 Step 4: Creating high-level PickUp action");
    //         Console.WriteLine("============================================");
            
    //         var highLevelPickUpAction = blackboard.GetAction(new FastName("pickUpHL_b1_r1_fp2")) as PickUpHL;
    //         if (highLevelPickUpAction == null)
    //         {
    //             Console.WriteLine("❌ Could not find PickUp action instance for high-level");
    //             return;
    //         }
            
    //         highLevelPickUpAction.SetOwiningTree(behaviorTree);
            
    //         // Create planning service
    //         var planningService = new CallPDDLPlanner(behaviorTree);
            
    //         // Create subtree with planning service
    //         var subtree = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL);
    //         subtree.SetPlanningService(planningService);
            
    //         // Generate actions for subtree (simulate PDDL planning)
    //         var allActions = blackboard.GetAllActionInstances();
    //         var relevantActions = allActions.Where(action => 
    //             action.actionType.ToString().Contains("place") ||
    //             action.actionType.ToString().Contains("stack")
    //         ).Take(3).ToList();
            
    //         if (relevantActions.Count > 0)
    //         {
    //             var nodeGraph = subtree.CreateNodeGraphFromActions(relevantActions);
    //             subtree.SetActionGraph(nodeGraph);
    //             Console.WriteLine($"🔧 Created subtree with {nodeGraph.GetAllActionNodes().Count} actions");
    //         }
            
    //         // Set as high-level action
    //         highLevelPickUpAction.SetAsHighLevelAction(subtree, planningService);
    //         Console.WriteLine($"✅ Created high-level PickUp action: {highLevelPickUpAction.InstanceName.ToString()}");
    //         Console.WriteLine($"📊 IsHighLevelAction: {highLevelPickUpAction.IsHighLevelAction}");
            
    //         // Create flow nodes for both actions
    //         Console.WriteLine("\n📋 Step 5: Creating flow nodes for both actions");
    //         Console.WriteLine("==============================================");
            
    //         var regularFlowNode = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL);
    //         var regularNodeGraph = new NodeGraph();
    //         regularNodeGraph.AddNode(regularPickUpAction);
    //         regularFlowNode.SetActionGraph(regularNodeGraph);
            
    //         var highLevelFlowNode = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL);
    //         var highLevelNodeGraph = new NodeGraph();
    //         highLevelNodeGraph.AddNode(highLevelPickUpAction);
    //         highLevelFlowNode.SetActionGraph(highLevelNodeGraph);
            
    //         Console.WriteLine($"✅ Created flow nodes for both actions");
            
    //         // Add both flow nodes to the root node
    //         Console.WriteLine("\n📋 Step 6: Adding flow nodes to root node");
    //         Console.WriteLine("=========================================");
            
    //         behaviorTree.AddChildToRootNode<BTFlowNode_Dynamic>(regularFlowNode);
    //         behaviorTree.AddChildToRootNode<BTFlowNode_Dynamic>(highLevelFlowNode);
    //         Console.WriteLine($"✅ Added both flow nodes to root node");
            
    //         // Test execution of both action types
    //         Console.WriteLine("\n📋 Step 7: Testing execution of both action types");
    //         Console.WriteLine("================================================");
            
    //         float deltaTime = 0.016f;
    //         int maxTicks = 30;
            
    //         for (int tick = 1; tick <= maxTicks; tick++)
    //         {
    //             Console.WriteLine($"\n🔄 TICK {tick}:");
    //             Console.WriteLine("   " + new string('-', 50));
                
    //             // Tick the behavior tree
    //             var treeResult = behaviorTree.Tick(deltaTime);
                
    //             Console.WriteLine($"   📊 Behavior Tree Result: {treeResult}");
    //             Console.WriteLine($"   📊 Root Node Status: {behaviorTree.RootNode.LastStatus}");
    //             Console.WriteLine($"   📊 Regular Flow Node Status: {regularFlowNode.LastStatus}");
    //             Console.WriteLine($"   📊 High-Level Flow Node Status: {highLevelFlowNode.LastStatus}");
    //             Console.WriteLine($"   📊 Regular PickUp Status: {regularPickUpAction.LastStatus}");
    //             Console.WriteLine($"   📊 High-Level PickUp Status: {highLevelPickUpAction.LastStatus}");
                
    //             // Check if the behavior tree has finished
    //             if (behaviorTree.HasFinished())
    //             {
    //                 Console.WriteLine($"   🏁 Behavior tree finished on tick {tick}");
    //                 break;
    //             }
                
    //             // Show subtree information for high-level action
    //             if (highLevelPickUpAction.HighLevelSubtree != null)
    //             {
    //                 var subtreeActionGraph = highLevelPickUpAction.HighLevelSubtree.GetActionGraph();
    //                 if (subtreeActionGraph != null)
    //                 {
    //                     var allSubtreeActions = subtreeActionGraph.GetAllActionNodes();
    //                     var completedSubtreeActions = allSubtreeActions.Count(node => 
    //                         node.LastStatus == EBTNodeResult.Succeeded || node.LastStatus == EBTNodeResult.failed);
                        
    //                     Console.WriteLine($"   📊 High-Level Subtree Progress: {completedSubtreeActions}/{allSubtreeActions.Count} completed");
                        
    //                     // Show status of subtree actions
    //                     Console.WriteLine("   📋 High-Level Subtree Actions:");
    //                     foreach (var action in allSubtreeActions)
    //                     {
    //                         Console.WriteLine($"      {action.InstanceName.ToString()}: {action.LastStatus}");
    //                     }
    //                 }
    //             }
                
    //             // Small delay to make output readable
    //             await Task.Delay(300);
    //         }
            
    //         // Final summary
    //         Console.WriteLine("\n📋 Step 8: Final Summary");
    //         Console.WriteLine("=======================");
            
    //         Console.WriteLine($"📊 Behavior Tree Final Status: {behaviorTree.RootNode.LastStatus}");
    //         Console.WriteLine($"📊 Regular PickUp Final Status: {regularPickUpAction.LastStatus}");
    //         Console.WriteLine($"📊 High-Level PickUp Final Status: {highLevelPickUpAction.LastStatus}");
            
    //         // Compare execution results
    //         Console.WriteLine("\n🎯 Comparison Results:");
    //         if (regularPickUpAction.LastStatus == EBTNodeResult.Succeeded)
    //         {
    //             Console.WriteLine("✅ Regular PickUp: SUCCESS - Direct execution completed");
    //         }
    //         else
    //         {
    //             Console.WriteLine("❌ Regular PickUp: FAILED - Direct execution failed");
    //         }
            
    //         if (highLevelPickUpAction.LastStatus == EBTNodeResult.Succeeded)
    //         {
    //             Console.WriteLine("✅ High-Level PickUp: SUCCESS - Subtree execution completed");
    //         }
    //         else
    //         {
    //             Console.WriteLine("❌ High-Level PickUp: FAILED - Subtree execution failed");
    //         }
            
    //         Console.WriteLine("\n✅ High-Level Action System Test Completed!");
    //         Console.WriteLine("This test demonstrates:");
    //         Console.WriteLine("1. Regular actions with direct execution");
    //         Console.WriteLine("2. High-level actions with subtree delegation");
    //         Console.WriteLine("3. Planning service integration");
    //         Console.WriteLine("4. Dynamic subtree creation and execution");
    //         Console.WriteLine("5. Hierarchical behavior tree execution");
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"❌ Error in High-Level Action System test: {ex.Message}");
    //         Console.WriteLine($"Stack trace: {ex.StackTrace}");
    //     }
    // }



    // static void TestPDDLPlanningPipeline(Blackboard<FastName> blackboard)
    // {
    //     Console.WriteLine("\n" + "=".PadRight(80, '='));
    //     Console.WriteLine("🧪 TESTING PDDL PLANNING PIPELINE STEP BY STEP");
    //     Console.WriteLine("=".PadRight(80, '='));
        
    //     try
    //     {
    //         // Step 1: Create the PDDL Planner
    //         Console.WriteLine("\n📋 STEP 1: Creating PDDL Planner");
    //         Console.WriteLine("-".PadRight(40, '-'));
            
    //         var behaviorTree = new BTInstance();
    //         behaviorTree.Initialise(blackboard);
            
    //         var pddlPlanner = new CallPDDLPlanner(behaviorTree);
    //         Console.WriteLine($"✅ PDDL Planner created successfully");
    //         Console.WriteLine($"   Planner type: {pddlPlanner.GetType().Name}");
    //         Console.WriteLine($"   Has generated NodeGraph: {pddlPlanner.HasGeneratedNodeGraph()}");
            
    //         // Step 2: Check if REST API is available
    //         Console.WriteLine("\n📋 STEP 2: Checking REST API Availability");
    //         Console.WriteLine("-".PadRight(40, '-'));
            
    //          var communicator = new RestPlannerCommunicator("http://localhost:5000");
    //          bool isAvailable = communicator.IsAvailable();
    //          Console.WriteLine($"✅ REST API availability check: {isAvailable}");
            
    //          if (!isAvailable)
    //          {
    //              Console.WriteLine("⚠️  WARNING: REST API is not available!");
    //              Console.WriteLine("   Make sure to start the Python service: python3 pddl_planning_service.py");
    //              Console.WriteLine("   This test will continue but planning will likely fail.");
    //          }
            
    //         // Step 3: Test the complete planning process via Tick
    //         Console.WriteLine("\n📋 STEP 3: Testing Complete Planning Process via Tick");
    //         Console.WriteLine("-".PadRight(40, '-'));
            
    //         // Call Tick to run the complete planning process
    //         bool tickSuccess = pddlPlanner.Tick(0.1f);
    //         Console.WriteLine($"✅ Planning Tick completed: {tickSuccess}");
            
    //         if (tickSuccess)
    //         {
    //             Console.WriteLine($"   Has generated NodeGraph: {pddlPlanner.HasGeneratedNodeGraph()}");
    //             if (pddlPlanner.HasGeneratedNodeGraph())
    //             {
    //                 var generatedGraph = pddlPlanner.GetGeneratedNodeGraph();
    //                 Console.WriteLine($"   Generated NodeGraph actions: {generatedGraph.GetAllActionNodes().Count}");
                    
    //                 // Display action details
    //                 var actions = generatedGraph.GetAllActionNodes();
    //                 Console.WriteLine("   Actions in NodeGraph:");
    //                 foreach (var action in actions)
    //                 {
    //                     Console.WriteLine($"     - {action.InstanceName.ToString()}");
    //                 }
                    
    //                 // Display execution order
    //                 var executionOrder = generatedGraph.GetExecutionOrder();
    //                 Console.WriteLine("   Execution order:");
    //                 for (int i = 0; i < executionOrder.Count; i++)
    //                 {
    //                     Console.WriteLine($"     {i + 1}. {executionOrder[i].InstanceName.ToString()}");
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             Console.WriteLine("❌ Planning failed");
    //             Console.WriteLine("   This might be expected if ENHSP is not installed or the Python service is not running.");
    //             return;
    //         }
            
    //         // Step 4: Verify NodeGraph storage in blackboard
    //         Console.WriteLine("\n📋 STEP 4: Verifying NodeGraph Storage in Blackboard");
    //         Console.WriteLine("-".PadRight(40, '-'));
            
    //         // Verify storage
    //         var allNodeGraphs = blackboard.GetAllNodeGraphs();
    //         Console.WriteLine($"✅ Total NodeGraphs in blackboard: {allNodeGraphs.Count}");
            
    //         if (allNodeGraphs.Count > 0)
    //         {
    //             Console.WriteLine("   NodeGraphs in blackboard:");
    //             for (int i = 0; i < allNodeGraphs.Count; i++)
    //             {
    //                 var nodeGraph = allNodeGraphs[i];
    //                 Console.WriteLine($"     - NodeGraph {i + 1}: {nodeGraph.GetAllActionNodes().Count} actions");
    //             }
    //         }
            
    //         // Step 5: Test with BTFlowNode_Dynamic
    //         Console.WriteLine("\n📋 STEP 5: Testing with BTFlowNode_Dynamic");
    //         Console.WriteLine("-".PadRight(40, '-'));
            
    //         var flowNode = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL);
    //         Console.WriteLine("✅ BTFlowNode_Dynamic created");
            
    //         // Set the planner service
    //         flowNode.SetPlanningService(pddlPlanner);
    //         Console.WriteLine("✅ Planning service set on flow node");
            
    //         // Test a few ticks
    //         Console.WriteLine("   Testing flow node ticks...");
    //         for (int i = 0; i < 3; i++)
    //         {
    //             var tickResult = flowNode.Tick(0.1f);
    //             Console.WriteLine($"   Tick {i + 1}: {tickResult} (Status: {flowNode.LastStatus})");
                
    //             if (flowNode.LastStatus == EBTNodeResult.Succeeded || flowNode.LastStatus == EBTNodeResult.failed)
    //             {
    //                 Console.WriteLine($"   Flow node completed with status: {flowNode.LastStatus}");
    //                 break;
    //             }
    //         }
            
    //         // Final Summary with Action Count
    //         Console.WriteLine("\n" + "=".PadRight(80, '='));
    //         Console.WriteLine("📊 FINAL SUMMARY");
    //         Console.WriteLine("=".PadRight(80, '='));
            
    //         if (pddlPlanner.HasGeneratedNodeGraph())
    //         {
    //             var finalGraph = pddlPlanner.GetGeneratedNodeGraph();
    //             var totalActions = finalGraph.GetAllActionNodes().Count;
    //             var executionOrder = finalGraph.GetExecutionOrder();
                
    //             Console.WriteLine($"🎯 TOTAL ACTIONS IN NODEGRAPH: {totalActions}");
    //             Console.WriteLine($"📋 EXECUTION ORDER: {string.Join(" → ", executionOrder.Select(a => a.InstanceName.ToString()))}");
    //             Console.WriteLine($"✅ PLANNING SUCCESS: {tickSuccess}");
    //             Console.WriteLine($"🔧 PLANNER USED: PDDL (ENHSP)");
    //         }
    //         else
    //         {
    //             Console.WriteLine("❌ NO NODEGRAPH GENERATED");
    //             Console.WriteLine($"✅ PLANNING SUCCESS: {tickSuccess}");
    //         }
            
    //         Console.WriteLine("\n" + "=".PadRight(80, '='));
    //         Console.WriteLine("🎉 PDDL PLANNING PIPELINE TEST COMPLETED SUCCESSFULLY!");
    //         Console.WriteLine("=".PadRight(80, '='));
            

            
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"\n❌ ERROR during PDDL planning pipeline test: {ex.Message}");
    //         Console.WriteLine($"   Stack trace: {ex.StackTrace}");
    //     }
    // }
}







