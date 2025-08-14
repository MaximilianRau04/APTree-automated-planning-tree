public class BTFlowNode_Dynamic : BTFlowNodeBase
{
    public override string DebugDisplayName { get; protected set; } = "DynamicFlow";

    public BTFlowNode_Dynamic(
        IBehaviorTree owningTree,
        SuccessCriteria successCriteria = SuccessCriteria.ALL,
        float threshold = 1.0f)
        : base(successCriteria, threshold)
    {
        this.OwningTree = owningTree;
        this.planner = new CallPDDLPlanner(OwningTree);
    }

    
/// <summary>
/// this function creates a plan with the planner and adds the action nodes to the graph
/// </summary>
/// <returns></returns>
    public override IEnumerator<IBTNode> GetEnumerator()
    {
        var (actions, orderTypes) = planner.CreatePlanWithOrders();
        
        // Add all action nodes to the graph
        foreach (var action in actions)
        {
            if (action is GenericBTAction actionNode)
            {
                actionGraph.AddNode(actionNode);
                AddChild(action);
            }
        }
        
        // Add order relations (like Hasse diagram - left to right)
        for (int i = 0; i < actions.Count - 1; i++)
        {
            if (actions[i] is BTActionNodeBase fromAction && actions[i + 1] is BTActionNodeBase toAction)
            {
                // Add order relation based on order type
                var orderType = orderTypes[i];
                switch (orderType)
                {
                    case OrderType.Total:
                        // Sequential execution
                        actionGraph.AddOrderRelation(fromAction, toAction);
                        break;
                    case OrderType.Strictparallel:
                        // Parallel execution - no order relation
                        break;
                    case OrderType.Parallel:
                        // Overlapping execution - add temporal constraint
                        actionGraph.AddTemporalConstraint(fromAction, toAction, TemporalConstraint.OVERLAPS);
                        break;
                }
            }
        }

        return actionGraph.GetAllActionNodes().Cast<IBTNode>().GetEnumerator();
    }

  
  
    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
        // Get nodes that can be executed this tick
        var executableNodes = actionGraph.GetExecutableNodes(inDeltaTime);
        
        Console.WriteLine($"   🔍 FlowNode: Found {executableNodes.Count} executable nodes");
        
        // Execute each node that's ready
        foreach (var node in executableNodes)
        {
            Console.WriteLine($"   ⚡ Executing node: {node.InstanceName.ToString()}");
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