using System.Collections;

public class BTFlowNode_Composite : BTFlowNodeBase
{
    public override string DebugDisplayName { get; protected set; } = "CompositeFlow";
    
    // List to store flow nodes (since NodeGraph is designed for action nodes)
    private List<IBTNode> flowNodes = new List<IBTNode>();
    
    public BTFlowNode_Composite(
        FastName nodeName,
        IBehaviorTree owningTree,
        SuccessCriteria successCriteria = SuccessCriteria.ALL,
        float threshold = 1.0f)
        : base(nodeName, successCriteria, threshold)
    {
        this.OwningTree = owningTree;
        DebugDisplayName = $"CompositeFlow({nodeName.ToString()})";
    }
    
    /// <summary>
    /// Add a child node (can be any IBTNode, including other flow nodes)
    /// </summary>
    public override IBTNode AddChild(IBTNode childNode)
    {
        childNode.SetOwiningTree(OwningTree);
        
        // Store flow nodes in a separate list since NodeGraph is designed for action nodes
        // We'll use the actionGraph from the base class for action nodes and a separate list for flow nodes
        if (childNode is GenericBTAction actionNode)
        {
            actionGraph.AddNode(actionNode);
            Console.WriteLine($"✅ Added action node: {childNode.DebugDisplayName} to composite flow node actionGraph");
        }
        else
        {
            // For flow nodes, we'll store them in a separate list for now
            // In the future, we could extend NodeGraph to handle flow nodes
            flowNodes.Add(childNode);
            Console.WriteLine($"✅ Added flow node: {childNode.DebugDisplayName} to composite flow node flowNodes list");
        }
        
        return childNode;
    }
    
    /// <summary>
    /// Get all child nodes (both action nodes and flow nodes)
    /// </summary>
    public List<IBTNode> GetChildren()
    {
        var allChildren = new List<IBTNode>();
        
        // Add action nodes from actionGraph
        var actionNodes = actionGraph.GetAllActionNodes();
        allChildren.AddRange(actionNodes.Cast<IBTNode>());
        
        // Add flow nodes from flowNodes list
        allChildren.AddRange(flowNodes);
        
        return allChildren;
    }
    
    /// <summary>
    /// Get the number of child nodes
    /// </summary>
    public int ChildCount => actionGraph.GetAllActionNodes().Count + flowNodes.Count;
    
    /// <summary>
    /// Enumerate through child nodes
    /// </summary>
    public override IEnumerator<IBTNode> GetEnumerator()
    {
        var allChildren = new List<IBTNode>();
        
        // Add action nodes from actionGraph
        var actionNodes = actionGraph.GetAllActionNodes();
        allChildren.AddRange(actionNodes.Cast<IBTNode>());
        
        // Add flow nodes from flowNodes list
        allChildren.AddRange(flowNodes);
        
        return allChildren.GetEnumerator();
    }
    
    /// <summary>
    /// Execute the composite flow node logic
    /// This ticks all child nodes and evaluates success criteria
    /// </summary>
    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
        var allChildren = GetChildren();
        Console.WriteLine($"   🔍 CompositeFlow: Executing {allChildren.Count} child nodes");
        
        // Tick all child nodes
        foreach (var childNode in allChildren)
        {
            Console.WriteLine($"   ⚡ Ticking child: {childNode.DebugDisplayName}");
            var previousStatus = childNode.LastStatus;
            childNode.Tick(inDeltaTime);
            Console.WriteLine($"   📊 Child {childNode.DebugDisplayName}: {previousStatus} → {childNode.LastStatus}");
        }
        
        // Check if all child nodes have finished
        bool allChildrenFinished = allChildren.All(child => child.HasFinished);
        
        if (allChildrenFinished)
        {
            // All children have finished, evaluate success criteria
            bool success = EvaluateCompositeSuccessCriteria();
            Console.WriteLine($"   🎯 Composite success criteria evaluation: {success}");
            
            if (success)
            {
                LastStatus = EBTNodeResult.Succeeded;
                Console.WriteLine($"   🏆 CompositeFlow status set to: {LastStatus}");
            }
            else
            {
                LastStatus = EBTNodeResult.failed;
                Console.WriteLine($"   ❌ CompositeFlow status set to: {LastStatus}");
            }
        }
        else
        {
            // Some children are still running
            LastStatus = EBTNodeResult.InProgress;
            Console.WriteLine($"   🔄 CompositeFlow status set to: {LastStatus} (children still running)");
        }
        
        // Return true if we should continue ticking, false if we're done
        return !allChildrenFinished;
    }
    
    /// <summary>
    /// Evaluate success criteria based on child node results
    /// </summary>
    private bool EvaluateCompositeSuccessCriteria()
    {
        var allChildren = GetChildren();
        if (allChildren.Count == 0) return false;
        
        int successCount = allChildren.Count(node => node.LastStatus == EBTNodeResult.Succeeded);
        int totalCount = allChildren.Count;
        
        Console.WriteLine($"   📊 Composite evaluation: {successCount}/{totalCount} children succeeded");
        
        return successCriteria switch
        {
            SuccessCriteria.ALL => successCount == totalCount,
            SuccessCriteria.ANY => successCount > 0,
            SuccessCriteria.COUNT => successCount >= (int)successThreshold,
            SuccessCriteria.PERCENTAGE => successCount >= (totalCount * successThreshold),
            _ => false
        };
    }
    
    /// <summary>
    /// Children are handled in OnTick_NodeLogic
    /// </summary>
    protected override bool OnTick_Children(float inDeltaTime)
    {
        // Children are handled in OnTick_NodeLogic
        return true;
    }
    
    /// <summary>
    /// Reset all child nodes
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        var allChildren = GetChildren();
        foreach (var childNode in allChildren)
        {
            childNode.Reset();
        }
        Console.WriteLine($"🔄 Reset composite flow node and all {allChildren.Count} children");
    }
}
