using BehaviorTreeMainProject;
using BehaviorTreeMainProject.Services;
using System.Collections.Generic;
using System.Linq;

public class BTFlowNode_Dynamic : BTFlowNodeBase
{
    public override string DebugDisplayName { get; protected set; } = "DynamicFlow";
    private bool planningCompleted = false;
    


    public BTFlowNode_Dynamic(
        FastName nodeName,
        IBehaviorTree owningTree,
        SuccessCriteria successCriteria = SuccessCriteria.ALL,
        float threshold = 1.0f)
        : base(nodeName, successCriteria, threshold)
    {
        this.OwningTree = owningTree;
        DebugDisplayName = $"DynamicFlow({nodeName.ToString()})";
        
        // Track this flow node
        LoggingService.TrackNodeStart(nodeName.ToString(), "BTFlowNode_Dynamic", System.DateTime.Now);
    }

    /// <summary>
    /// this function creates a plan with the planner and adds the action nodes to the graph
    /// </summary>
    /// <returns></returns>
    public override IEnumerator<IBTNode> GetEnumerator()
    {
        // Console.WriteLine($"   🔍 FlowNode: GetEnumerator called - Current actionGraph has {actionGraph.GetAllActionNodes().Count} nodes");
        
        // Return the current actionGraph nodes
        return actionGraph.GetAllActionNodes().Cast<IBTNode>().GetEnumerator();
    }

    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
        LoggingService.LogInfo($"🚨 DEBUG: BTFlowNode_Dynamic.OnTick_NodeLogic called for {DebugDisplayName}");
        LoggingService.LogInfo($"🔍 FlowNode: Planning completed: {planningCompleted}");
        LoggingService.LogInfo($"🔍 FlowNode: PlanningService type: {PlanningService?.GetType().Name ?? "null"}");
        LoggingService.LogInfo($"🔍 FlowNode: ActionGraph exists: {actionGraph != null}");
        LoggingService.LogInfo($"🔍 FlowNode: Current LastStatus: {LastStatus}");
        LoggingService.LogInfo($"🔍 FlowNode: HasChildren: {HasChildren}");
        
        // Check if planning has been completed by the service
        if (!planningCompleted)
        {
            LoggingService.LogInfo($"   ⏳ FlowNode: Planning not completed yet, checking planning service...");
            
            // Check if planning service has completed and generated a NodeGraph
            if (PlanningService is BTServicePlanner plannerService)
            {
                // Check if planning has failed
                if (plannerService.HasPlanningFailed())
                {
                    LoggingService.LogError($"❌ FlowNode: Planning failed - {plannerService.LastError}");
                    LoggingService.LogError($"❌ FlowNode: Setting node status to failed due to planning failure");
                    LastStatus = EBTNodeResult.failed;
                    return false; // Fail the node immediately
                }
                
                // Check if planning has completed successfully and generated a NodeGraph
                if (plannerService.HasPlanningSucceeded())
                {
                    planningCompleted = true;
                    LoggingService.LogInfo($"   ✅ FlowNode: Planning completed by service");
                    
                    // Get the generated NodeGraph and assign it to actionGraph
                    var generatedNodeGraph = plannerService.GetGeneratedNodeGraph();
                    
                    LoggingService.LogInfo($"   📊 FlowNode: Generated NodeGraph contains {generatedNodeGraph.GetAllActionNodes().Count} actions");
                    
                    // Set the generated NodeGraph (will be prevented if already set)
                    LoggingService.LogInfo($"🔧 BTFlowNode_Dynamic: Setting actionGraph - HashCode: {generatedNodeGraph?.GetHashCode()}");
                    SetActionGraph(generatedNodeGraph);
                                    
                    LoggingService.LogInfo($"   ✅ FlowNode: Successfully loaded NodeGraph with {actionGraph.GetAllActionNodes().Count} nodes");
                    
                    // Log the complete graph structure for debugging
                    LoggingService.LogInfo($"   📊 FlowNode: Logging NodeGraph structure...");
                    actionGraph.LogGraphStructure();
                    
                    // IMPORTANT: Call AddChild for each action to set up services properly
                    LoggingService.LogInfo($"   🔧 FlowNode: Setting up services for all actions in NodeGraph...");
                    var allActions = actionGraph.GetAllActionNodes();
                    for (int i = 0; i < allActions.Count; i++)
                    {
                        var action = allActions[i];
                        LoggingService.LogInfo($"   🔧 FlowNode: Setting up services for action {i+1}: {action.InstanceName.ToString()}");
                        AddChild(action);
                    }
                    LoggingService.LogInfo($"   ✅ FlowNode: Completed service setup for {allActions.Count} actions");
                }
                else
                {
                    // Planning not completed yet, continue waiting
                    LoggingService.LogInfo($"   ⏳ FlowNode: Waiting for planning service to complete...");
                    LoggingService.LogInfo($"   🔍 FlowNode: PlannerService HasPlanningSucceeded: {plannerService.HasPlanningSucceeded()}");
                    LoggingService.LogInfo($"   🔍 FlowNode: PlannerService HasPlanningFailed: {plannerService.HasPlanningFailed()}");
                    LoggingService.LogInfo($"   🔍 FlowNode: PlannerService HasCompleted: {plannerService.HasCompleted}");
                    LastStatus = EBTNodeResult.InProgress;
                    LoggingService.LogInfo($"   🔄 FlowNode: Setting status to InProgress and returning true");
                    return true; // Continue ticking
                }
            }
            else
            {
                LoggingService.LogError($"❌ FlowNode: No planning service available");
                LastStatus = EBTNodeResult.failed;
                return false; // Fail the node
            }
        }

        // Planning is completed, now just set InProgress and let OnTick_Children handle execution
        LoggingService.LogInfo($"   📋 FlowNode: Planning completed, setting InProgress for OnTick_Children to handle execution");
        LastStatus = EBTNodeResult.InProgress;
        LoggingService.LogInfo($"   🔄 FlowNode: Setting status to InProgress (OnTick_Children will handle execution and final status)");
        LoggingService.LogInfo($"   🔄 FlowNode: Returning true to continue ticking (OnTick_Children will handle execution)");
        
        // Return true to continue ticking (let OnTick_Children handle the actual execution and final status)
        return true;
    }

   

    /// <summary>
    /// Get the action graph for debugging and monitoring purposes
    /// </summary>
    public NodeGraph GetActionGraph()
    {
        return actionGraph;
    }
    
    public override void Reset()
    {
        bool wasUninitialized = (LastStatus == EBTNodeResult.Uninitialized);
        base.Reset();
        
        // Don't reset planning service during initialization if other planning is in progress
        if (wasUninitialized)
        {
            LoggingService.LogInfo($"🔄 FlowNode: Initialization reset - preserving existing planning services");
            LoggingService.LogInfo($"🔄 FlowNode: Node {DebugDisplayName} was uninitialized, checking if planning is already completed");
            
            // Check if planning has already been completed by checking if NodeGraph exists
            if (PlanningService is BTServicePlanner initPlannerService && initPlannerService.GetGeneratedNodeGraph() != null)
            {
                LoggingService.LogInfo($"🔄 FlowNode: Planning already completed (NodeGraph exists), setting planningCompleted = true");
                planningCompleted = true;
            }
            else
            {
                LoggingService.LogInfo($"🔄 FlowNode: Planning not completed yet (no NodeGraph), keeping planningCompleted = false");
                planningCompleted = false;
            }
            
            return;
        }
        
        // Only reset planning if it hasn't completed yet (success or failure)
        if (PlanningService is BTServicePlanner plannerService)
        {
            // If planning has already completed (success or failure), don't reset it
            if (plannerService.HasCompleted)
            {
                if (plannerService.HasPlanningSucceeded())
                {
                    LoggingService.LogInfo($"🔄 FlowNode: Planning already completed successfully, preserving NodeGraph (HashCode: {plannerService.GetGeneratedNodeGraph()?.GetHashCode()})");
                    // Keep the planning completed flag true to preserve the NodeGraph
                    planningCompleted = true;
                }
                else
                {
                    LoggingService.LogInfo($"🔄 FlowNode: Planning already completed and failed, preserving failure state - {plannerService.LastError}");
                    // Keep the planning completed flag false to maintain failure state
                    planningCompleted = false;
                }
            }
            else
            {
                LoggingService.LogInfo($"🔄 FlowNode: Resetting planning service (not completed yet)");
                plannerService.ResetPlanningService();
                planningCompleted = false;
                // Clear the action graph when resetting planning
                ClearActionGraph();
            }
        }
        else
        {
            planningCompleted = false;
            // Clear the action graph when no planning service
            ClearActionGraph();
        }
    }

    protected override bool OnTick_Children(float inDeltaTime)
    {
        LoggingService.LogInfo($"🚨 DEBUG: BTFlowNode_Dynamic.OnTick_Children called for {DebugDisplayName}");
        LoggingService.LogInfo($"🔍 FlowNode: Planning completed: {planningCompleted}");
        LoggingService.LogInfo($"🔍 FlowNode: ActionGraph exists: {actionGraph != null}");
        
        // Only execute children if planning is completed
        if (!planningCompleted)
        {
            LoggingService.LogInfo($"   ⏳ FlowNode: Planning not completed, skipping children execution");
            return true; // Continue waiting for planning to complete
        }

        LoggingService.LogInfo($"   📋 FlowNode: Planning completed, getting executable nodes...");

        // Step 1: Get current executable nodes from NodeGraph based on order relations and temporal constraints
        var executableNodes = actionGraph.GetExecutableNodes(inDeltaTime);
        
        LoggingService.LogInfo($"   📊 FlowNode: Found {executableNodes.Count} executable nodes");
        
        if (executableNodes.Count == 0)
        {
            // No nodes are executable at this time, but we're still in progress
            LoggingService.LogInfo($"   ⏳ FlowNode: No executable nodes at this time");
            return true; // Continue ticking
        }

        LoggingService.LogInfo($"   🔍 FlowNode: Found {executableNodes.Count} executable nodes");

        // Step 2: Execute each executable node (only current ones, no dynamic updates within this tick)
        foreach (var node in executableNodes)
        {
            LoggingService.LogInfo($"   ⚡ Executing node: {node.InstanceName.ToString()}");
            LoggingService.LogInfo($"   🔍 Node type: {node.GetType().Name}");
            LoggingService.LogInfo($"   🔍 Node status before tick: {node.LastStatus}");
             
            // Mark node as started if it's the first time executing
            if (node.LastStatus == EBTNodeResult.readyToTick)
            {
                actionGraph.MarkNodeStarted(node);
                LoggingService.LogInfo($"   🚀 Marked {node.InstanceName.ToString()} as started");
            }
             
            var previousStatus = node.LastStatus;
            LoggingService.LogInfo($"   🔄 Calling node.Tick() for {node.InstanceName.ToString()}");
            node.Tick(inDeltaTime);
            LoggingService.LogInfo($"   📊 Node {node.InstanceName.ToString()}: {previousStatus} → {node.LastStatus}");

            // Mark completed nodes and track completion
            if (node.LastStatus == EBTNodeResult.Succeeded || node.LastStatus == EBTNodeResult.failed)
            {
                actionGraph.MarkNodeCompleted(node);
                LoggingService.LogInfo($"   ✅ Marked {node.InstanceName.ToString()} as completed");
                
                // Track node completion
                bool success = node.LastStatus == EBTNodeResult.Succeeded;
                LoggingService.TrackNodeCompletion(node.InstanceName.ToString(), System.DateTime.Now, success);
                
                // FIXED: For ALL success criteria, check immediately if this failure should cause the flow node to fail
                if (node.LastStatus == EBTNodeResult.failed && successCriteria == SuccessCriteria.ALL)
                {
                    LoggingService.LogWarning($"   ❌ FlowNode: Action {node.InstanceName.ToString()} failed, evaluating ALL success criteria immediately");
                    bool overallSuccess = EvaluateSuccessCriteria();
                    LoggingService.LogInfo($"   🎯 FlowNode: Immediate success criteria evaluation result: {overallSuccess}");
                    
                    // Set the node status based on the success criteria evaluation result
                    if (overallSuccess)
                    {
                        LastStatus = EBTNodeResult.Succeeded;
                        LoggingService.LogSuccess($"   ✅ FlowNode: Setting status to Succeeded (ALL criteria met despite action failure)");
                    }
                    else
                    {
                        LastStatus = EBTNodeResult.failed;
                        LoggingService.LogError($"   ❌ FlowNode: Setting status to failed (ALL criteria not met due to action failure)");
                    }
                    
                    // Track completion of this flow node
                    LoggingService.TrackNodeCompletion(DebugDisplayName, System.DateTime.Now, overallSuccess);
                    LoggingService.LogInfo($"   📊 FlowNode: Final status set to {LastStatus}");
                    
                    // Return false to stop the parent from ticking this node again
                    LoggingService.LogInfo($"   🔄 FlowNode: OnTick_Children completed with final status {LastStatus}, returning false to stop ticking");
                    return false; // Stop ticking this node - it has completed (either succeeded or failed)
                }
            }
            else
            {
                LoggingService.LogInfo($"   ⏳ Node {node.InstanceName.ToString()} not completed yet (status: {node.LastStatus})");
            }
        }

        // Step 3: Check if all nodes are completed
        var allNodes = actionGraph.GetAllActionNodes();
        bool allNodesProcessed = allNodes.All(node =>
            node.LastStatus == EBTNodeResult.Succeeded || node.LastStatus == EBTNodeResult.failed);

        LoggingService.LogInfo($"   🔍 FlowNode: All nodes processed: {allNodesProcessed}");

        if (allNodesProcessed && allNodes.Count > 0)
        {
            // FIXED: Handle final status evaluation here after all nodes are completed
            LoggingService.LogInfo($"   🎯 FlowNode: All nodes processed, evaluating success criteria");
            bool success = EvaluateSuccessCriteria();
            LoggingService.LogInfo($"   🎯 FlowNode: Success criteria evaluation result: {success}");

            if (success)
            {
                LastStatus = EBTNodeResult.Succeeded;
                LoggingService.LogInfo($"   🏆 FlowNode: Setting status to Succeeded (all nodes processed successfully)");
            }
            else
            {
                LastStatus = EBTNodeResult.failed;
                LoggingService.LogInfo($"   ❌ FlowNode: Setting status to failed (all nodes processed but failed)");
            }
            
            // Track completion of this flow node
            LoggingService.TrackNodeCompletion(DebugDisplayName, System.DateTime.Now, success);
            LoggingService.LogInfo($"   📊 FlowNode: Final status set to {LastStatus}");
            
            // FIXED: Return false to stop the parent from ticking this node again
            LoggingService.LogInfo($"   🔄 FlowNode: OnTick_Children completed with final status {LastStatus}, returning false to stop ticking");
            return false; // Stop ticking this node - it has completed
        }
        else
        {
            // Still processing nodes - keep in progress
            LastStatus = EBTNodeResult.InProgress;
            LoggingService.LogInfo($"   🔄 FlowNode: Still processing nodes, keeping status as InProgress");
        }

        // FIXED: Only return true if we're still processing nodes
        // This ensures the parent node continues to tick this flow node until completion
        LoggingService.LogInfo($"   🔄 FlowNode: OnTick_Children completed, returning true to continue ticking");
        return true; // Continue ticking children until all nodes are completed
    }
}