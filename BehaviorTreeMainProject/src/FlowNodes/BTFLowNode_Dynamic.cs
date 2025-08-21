using BehaviorTreeMainProject;
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
    }

    /// <summary>
    /// this function creates a plan with the planner and adds the action nodes to the graph
    /// </summary>
    /// <returns></returns>
    public override IEnumerator<IBTNode> GetEnumerator()
    {
        Console.WriteLine($"   🔍 FlowNode: GetEnumerator called - Current actionGraph has {actionGraph.GetAllActionNodes().Count} nodes");
        
        // Check if planning has been completed and NodeGraph is available
        if (PlanningService is BTServicePlanner plannerService && plannerService.HasGeneratedNodeGraph())
        {
            var generatedNodeGraph = plannerService.GetGeneratedNodeGraph();
            
            // Add all action nodes from the generated NodeGraph to our actionGraph
            var actions = generatedNodeGraph.GetAllActionNodes();
            Console.WriteLine($"   📊 FlowNode: Found {actions.Count} actions in generated NodeGraph");
            
            int addedCount = 0;
            foreach (var action in actions)
            {
                if (action is GenericBTAction actionNode)
                {
                    actionGraph.AddNode(actionNode);
                    AddChild(action);
                    addedCount++;
                }
            }

            // Copy order relations and temporal constraints from generated NodeGraph
            // Note: The NodeGraph already contains the proper structure, so we just use it
            // The actionGraph will be populated with the same nodes and relations
            
            Console.WriteLine($"   ✅ FlowNode: Loaded {addedCount} actions from generated NodeGraph");
            Console.WriteLine($"   📋 FlowNode: Final actionGraph now contains {actionGraph.GetAllActionNodes().Count} nodes");
        }
        else
        {
            Console.WriteLine($"   ⚠️ FlowNode: No generated NodeGraph available yet");
        }

        return actionGraph.GetAllActionNodes().Cast<IBTNode>().GetEnumerator();
    }

    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
        // First, ensure planning is completed
        if (!planningCompleted)
        {
            Console.WriteLine($"   🔧 FlowNode: Starting planning process...");
            
            // Check if we have a planning service
            if (PlanningService is BTServicePlanner plannerService)
            {
                // Call the planner's Tick method to generate the plan
                bool planningSuccess = plannerService.Tick(inDeltaTime);
                
                                 if (planningSuccess && plannerService.HasGeneratedNodeGraph())
                 {
                     planningCompleted = true;
                     Console.WriteLine($"   ✅ FlowNode: Planning completed successfully");
                     
                     // Now populate the actionGraph with the generated plan
                     var generatedNodeGraph = plannerService.GetGeneratedNodeGraph();
                     var actions = generatedNodeGraph.GetAllActionNodes();
                     
                     Console.WriteLine($"   📊 FlowNode: Generated NodeGraph contains {actions.Count} actions");
                     
                     int addedCount = 0;
                     foreach (var action in actions)
                     {
                         if (action is GenericBTAction actionNode)
                         {
                             actionGraph.AddNode(actionNode);
                             AddChild(action);
                             addedCount++;
                             Console.WriteLine($"   ➕ Added action: {actionNode.InstanceName.ToString()}");
                         }
                     }
                     
                     Console.WriteLine($"   ✅ FlowNode: Successfully loaded {addedCount}/{actions.Count} actions into actionGraph");
                     Console.WriteLine($"   📋 FlowNode: Final actionGraph now contains {actionGraph.GetAllActionNodes().Count} nodes");
                 }
                else
                {
                    Console.WriteLine($"   ❌ FlowNode: Planning failed or no NodeGraph generated");
                    LastStatus = EBTNodeResult.failed;
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"   ❌ FlowNode: No planning service available");
                LastStatus = EBTNodeResult.failed;
                return false;
            }
        }

        // Get nodes that can be executed this tick
        var executableNodes = actionGraph.GetExecutableNodes(inDeltaTime);

        Console.WriteLine($"   🔍 FlowNode: Found {executableNodes.Count} executable nodes");

        // Execute each node that's ready
        foreach (var node in executableNodes)
        {
            Console.WriteLine($"   ⚡ Executing node: {node.InstanceName.ToString()}");
            Console.WriteLine($"   🔍 Node type: {node.GetType().Name}");
            Console.WriteLine($"   🔍 Node status before tick: {node.LastStatus}");
            var previousStatus = node.LastStatus;
            node.Tick(inDeltaTime);
            Console.WriteLine($"   📊 Node {node.InstanceName.ToString()}: {previousStatus} → {node.LastStatus}");

            // Mark completed nodes
            if (node.LastStatus == EBTNodeResult.Succeeded || node.LastStatus == EBTNodeResult.failed)
            {
                actionGraph.MarkNodeCompleted(node);
                Console.WriteLine($"   ✅ Marked {node.InstanceName.ToString()} as completed");
            }
        }

        // Check if all nodes have been processed (completed or failed)
        var allNodes = actionGraph.GetAllActionNodes();

        // Debug: Show status of each node
        Console.WriteLine($"   🔍 FlowNode: Node statuses:");
        foreach (var node in allNodes)
        {
            Console.WriteLine($"      {node.InstanceName.ToString()}: {node.LastStatus}");
        }

        // A node is processed if it has been executed and completed (succeeded or failed)
        // Unexecuted nodes should have status Uninitialized or readyToTick
        bool allNodesProcessed = allNodes.All(node =>
            node.LastStatus == EBTNodeResult.Succeeded ||
            node.LastStatus == EBTNodeResult.failed);

        Console.WriteLine($"   🔍 FlowNode: All nodes processed: {allNodesProcessed}");

        if (allNodesProcessed)
        {
            // All nodes have been processed, evaluate final success criteria
            bool success = EvaluateSuccessCriteria();
            Console.WriteLine($"   🎯 Success criteria evaluation: {success}");

            if (success)
            {
                LastStatus = EBTNodeResult.Succeeded;
                Console.WriteLine($"   🏆 FlowNode status set to: {LastStatus} (all nodes processed)");
            }
            else
            {
                LastStatus = EBTNodeResult.failed;
                Console.WriteLine($"   ❌ FlowNode status set to: {LastStatus} (all nodes processed but failed)");
            }
        }
        else
        {
            // Still processing nodes, check if any are in progress
            bool anyInProgress = allNodes.Any(node => node.LastStatus == EBTNodeResult.InProgress);

            if (anyInProgress)
            {
                LastStatus = EBTNodeResult.InProgress;
                Console.WriteLine($"   🔄 FlowNode status set to: {LastStatus} (nodes in progress)");
            }
            else
            {
                // No nodes in progress but not all processed - continue ticking
                LastStatus = EBTNodeResult.InProgress;
                Console.WriteLine($"   🔄 FlowNode status set to: {LastStatus} (waiting for next tick)");
            }
        }

        // Return true if we should continue ticking, false if we're done
        return !allNodesProcessed;
    }

    public override IBTNode AddChild(IBTNode Innode)
    {
        if (Innode is GenericBTAction actionNode)
        {
            actionGraph.AddNode(actionNode);
            return Innode;
        }
        throw new ArgumentException("Dynamic flow node can only accept action nodes as children");
    }

    protected override bool OnTick_Children(float inDeltaTime)
    {
        // Children are handled in OnTick_NodeLogic through the NodeGraph
        // This method is not needed since we're using the graph-based approach
        return true;
    }
}