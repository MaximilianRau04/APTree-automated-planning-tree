using Neo4j.Driver;
using ModelLoader;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Setting up Blackboard...");
            
            // Create blackboard instance
            using var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "12345678");
            
            // Test the connection
            Console.WriteLine("Testing Neo4j connection...");
            bool connectionSuccess = await blackboard.TestNeo4jConnection();
            
            if (connectionSuccess)
            {
                Console.WriteLine("✅ Neo4j connection successful!");
                
                // Create BlackboardWriter for type registration
                var blackboardWriter = new BlackboardWriter(blackboard);
                
                // Register all types
                Console.WriteLine("\n=== REGISTERING ALL TYPES ===");
                blackboardWriter.RegisterAllTypes();
                
                // Register all instances from files
                Console.WriteLine("\n=== REGISTERING ALL INSTANCES FROM FILES ===");
                string actionInstancesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "ActionInstances.txt");
                blackboardWriter.RegisterAllInstances(actionInstancesFile);
                
                Console.WriteLine("✅ All operations completed successfully!");
                
                // Test Flow Node Logic and Graph
                Console.WriteLine("\n=== TESTING FLOW NODE LOGIC AND GRAPH ===");
                await TestFlowNodeLogic(blackboard);
            }
            else
            {
                Console.WriteLine("❌ Neo4j connection failed!");
                Console.WriteLine("Please make sure:");
                Console.WriteLine("1. Neo4j Desktop is running");
                Console.WriteLine("2. Your database is started");
                Console.WriteLine("3. The password '12345678' is correct");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("Please check your Neo4j setup and credentials.");
        }
    }
    
    /// <summary>
    /// Comprehensive test function to demonstrate flow node logic and graph execution
    /// This function shows exactly what happens when a flow node is created and ticked
    /// </summary>
    private static async Task TestFlowNodeLogic(Blackboard<FastName> blackboard)
    {
        Console.WriteLine("\n🔧 STEP 1: Creating Behavior Tree Instance");
        Console.WriteLine("==========================================");
        
        // Create behavior tree instance
        var behaviorTree = new BTInstance();
        behaviorTree.Initialise(blackboard, "TestFlowTree");
        Console.WriteLine($"✅ Created behavior tree: {behaviorTree.DebugDisplayName}");
        
        Console.WriteLine("\n🔧 STEP 2: Creating Dynamic Flow Node");
        Console.WriteLine("=====================================");
        
        // Create a dynamic flow node with different success criteria for testing
        var flowNode = new BTFlowNode_Dynamic(
            behaviorTree, 
            SuccessCriteria.ANY,  // At least one child must succeed
            0.0f
        );
        Console.WriteLine($"✅ Created flow node: {flowNode.DebugDisplayName}");
        Console.WriteLine($"   Success Criteria: {flowNode.successCriteria}");
        Console.WriteLine($"   Success Threshold: 0.0f (configured for ANY criteria)");
        
        Console.WriteLine("\n🔧 STEP 3: Getting Registered Action Instances from Blackboard");
        Console.WriteLine("=============================================================");
        
        
        
        // Get all action instances directly from blackboard
        var allActionInstances = blackboard.GetAllActionInstances();
        Console.WriteLine($"📋 Available action instances: {allActionInstances.Count}");
        
        if (allActionInstances.Count > 0)
        {
            // Use all available action instances for testing
          
            
                         foreach (var actionInstance in allActionInstances)
             {
                 Console.WriteLine($"✅ Retrieved action: {actionInstance.InstanceName.ToString()} (Type: {actionInstance.actionType.ToString()})");
             }
        }
        else
        {
            Console.WriteLine("❌ No action instances found in blackboard!");
            Console.WriteLine("Make sure the InputInstances folder contains action definitions.");
            return; // Exit the test if no actions are available
        }
        
        Console.WriteLine($"📊 Total action nodes created: {allActionInstances.Count}");
        
        Console.WriteLine("\n🔧 STEP 4: Creating NodeGraph from Actions");
        Console.WriteLine("===========================================");
        
        // Create a NodeGraph from the action instances with default relations
        var nodeGraph = flowNode.CreateNodeGraphFromActions(allActionInstances);
        
        // Replace the flow node's action graph with our new one
        // Note: This would require making actionGraph accessible or adding a method to set it
        // For now, we'll add actions individually and show the graph structure
        
        Console.WriteLine($"📊 Created NodeGraph with {nodeGraph.GetAllActionNodes().Count} nodes");
        Console.WriteLine($"📊 Execution order: {string.Join(" → ", nodeGraph.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
        
        Console.WriteLine("\n🔧 STEP 5: Using NodeGraph for Flow Node");
        Console.WriteLine("=========================================");
        
        // Use the NodeGraph we created instead of adding actions individually
        flowNode.SetActionGraph(nodeGraph);
        Console.WriteLine($"✅ Set NodeGraph with {nodeGraph.GetAllActionNodes().Count} nodes as flow node's action graph");
        Console.WriteLine($"📊 NodeGraph execution order: {string.Join(" → ", nodeGraph.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
        
        // Debug: Show what's in the NodeGraph
        Console.WriteLine("\n🔍 DEBUG: NodeGraph Contents:");
        var allNodes = nodeGraph.GetAllActionNodes();
        for (int i = 0; i < allNodes.Count; i++)
        {
            Console.WriteLine($"   Node {i}: {allNodes[i].InstanceName.ToString()}");
        }
        
        // Note: We don't need to add actions individually anymore since the NodeGraph contains them
        
        Console.WriteLine("\n🔧 STEP 6: Examining Flow Node Structure");
        Console.WriteLine("=========================================");
        
        // Examine the flow node structure through public interface
        Console.WriteLine($"📊 Flow node debug name: {flowNode.DebugDisplayName}");
        Console.WriteLine($"📊 Flow node has children: {flowNode.HasChildren}");
        Console.WriteLine($"📊 Flow node last status: {flowNode.LastStatus}");
        Console.WriteLine($"📊 Flow node has finished: {flowNode.HasFinished}");
        
        Console.WriteLine("\n🔧 STEP 7: Testing Flow Node Tick Logic");
        Console.WriteLine("======================================");
        
        // Test the tick logic step by step
        float deltaTime = 0.016f; // 60 FPS simulation
        
        for (int tick = 1; tick <= 5; tick++)
        {
            Console.WriteLine($"\n🔄 TICK {tick}:");
            Console.WriteLine("   " + new string('-', 40));
            
            // Show flow node status before tick
            Console.WriteLine($"   📊 Flow node status before tick: {flowNode.LastStatus}");
            Console.WriteLine($"   🎯 Flow node finished: {flowNode.HasFinished}");
            
            // Execute the tick
            Console.WriteLine($"   ⚡ Executing flow node tick...");
            var tickResult = flowNode.Tick(deltaTime);
            Console.WriteLine($"   📊 Tick result: {tickResult}");
            
            // Check flow node status after tick
            Console.WriteLine($"   📊 Flow node status after tick: {flowNode.LastStatus}");
            Console.WriteLine($"   🎯 Flow node finished: {flowNode.HasFinished}");
            
            // If flow node is finished, break
            if (flowNode.HasFinished)
            {
                Console.WriteLine($"   🏁 Flow node completed on tick {tick}");
                break;
            }
            
            // Small delay to simulate real-time execution
            await Task.Delay(100);
        }
        
        Console.WriteLine("\n🔧 STEP 8: Final Results");
        Console.WriteLine("=======================");
        
        Console.WriteLine($"📊 Final flow node status: {flowNode.LastStatus}");
        Console.WriteLine($"🎯 Flow node finished: {flowNode.HasFinished}");
        Console.WriteLine($"✅ Success criteria evaluation completed");
        
        Console.WriteLine("\n📋 Test completed successfully!");
        
        Console.WriteLine("\n✅ Flow Node Logic Test Completed!");
        Console.WriteLine("This test demonstrates:");
        Console.WriteLine("1. Flow node creation and configuration");
        Console.WriteLine("2. Action node addition to the graph");
        Console.WriteLine("3. Flow node structure examination");
        Console.WriteLine("4. Step-by-step tick execution");
        Console.WriteLine("5. Success criteria evaluation");
        Console.WriteLine("6. Node status tracking");
        
        // Test different success criteria
        Console.WriteLine("\n🔧 STEP 9: Testing Different Success Criteria");
        Console.WriteLine("============================================");
        
        await TestDifferentSuccessCriteria(blackboard);
    }
    
        /// <summary>
    /// Test different success criteria to show how they affect flow node behavior
    /// </summary>
    private static async Task TestDifferentSuccessCriteria(Blackboard<FastName> blackboard)
    {
        // Reset all action instances before testing different success criteria
        Console.WriteLine("🔄 Resetting all action instances for clean test...");
        var actionInstancesToReset = blackboard.GetAllActionInstances();
        foreach (var actionInstance in actionInstancesToReset)
        {
            actionInstance.Reset();
        }
        Console.WriteLine($"✅ Reset {actionInstancesToReset.Count} action instances");
        
        Console.WriteLine("\n🔧 Testing SuccessCriteria.ALL (all children must succeed)");
        Console.WriteLine("========================================================");
        
        var behaviorTree = new BTInstance();
        behaviorTree.Initialise(blackboard, "TestALL");
        
        var flowNodeALL = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ALL, 1.0f);
        
        // Get real actions from blackboard and create a proper NodeGraph
        var allActionInstances = blackboard.GetAllActionInstances();
        if (allActionInstances.Count >= 3)
        {
            // Take first 3 actions and create a NodeGraph with proper relations
            var testActions = allActionInstances.Take(3).Cast<GenericBTAction>().ToList();
            var nodeGraphALL = flowNodeALL.CreateNodeGraphFromActions(testActions);
            flowNodeALL.SetActionGraph(nodeGraphALL);
            
            Console.WriteLine($"   Created NodeGraph with {nodeGraphALL.GetAllActionNodes().Count} nodes");
            Console.WriteLine($"   Execution order: {string.Join(" → ", nodeGraphALL.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
        }
        else
        {
            Console.WriteLine("   ⚠️  Not enough actions available for ALL test");
            return;
        }
        
        // Tick until completion
        for (int tick = 1; tick <= 5; tick++)
        {
            Console.WriteLine($"   Tick {tick}: Status = {flowNodeALL.LastStatus}, Finished = {flowNodeALL.HasFinished}");
            flowNodeALL.Tick(0.016f);
            if (flowNodeALL.HasFinished) break;
            await Task.Delay(50);
        }
        
        Console.WriteLine($"   Final Result: {flowNodeALL.LastStatus}");
        
        Console.WriteLine("\n🔧 Testing SuccessCriteria.ANY (at least one child must succeed)");
        Console.WriteLine("=============================================================");
        
        var flowNodeANY = new BTFlowNode_Dynamic(behaviorTree, SuccessCriteria.ANY, 0.0f);
        
        // Get real actions from blackboard and create a proper NodeGraph
        if (allActionInstances.Count >= 3)
        {
            // Take first 3 actions and create a NodeGraph with proper relations
            var testActions = allActionInstances.Take(3).Cast<GenericBTAction>().ToList();
            var nodeGraphANY = flowNodeANY.CreateNodeGraphFromActions(testActions);
            flowNodeANY.SetActionGraph(nodeGraphANY);
            
            Console.WriteLine($"   Created NodeGraph with {nodeGraphANY.GetAllActionNodes().Count} nodes");
            Console.WriteLine($"   Execution order: {string.Join(" → ", nodeGraphANY.GetExecutionOrder().Select(a => a.InstanceName.ToString()))}");
        }
        else
        {
            Console.WriteLine("   ⚠️  Not enough actions available for ANY test");
            return;
        }
        
        // Tick until completion
        for (int tick = 1; tick <= 5; tick++)
        {
            Console.WriteLine($"   Tick {tick}: Status = {flowNodeANY.LastStatus}, Finished = {flowNodeANY.HasFinished}");
            flowNodeANY.Tick(0.016f);
            if (flowNodeANY.HasFinished) break;
            await Task.Delay(50);
        }
        
        Console.WriteLine($"   Final Result: {flowNodeANY.LastStatus}");
        
        Console.WriteLine("\n✅ Success Criteria Tests Completed!");
    }
}







