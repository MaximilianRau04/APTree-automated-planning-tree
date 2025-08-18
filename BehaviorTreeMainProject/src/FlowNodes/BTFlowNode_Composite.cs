using System.Collections;

public class BTFlowNode_Composite : BTFlowNodeBase
{
    public override string DebugDisplayName { get; protected set; } = "CompositeFlow";
    
    // List to store child flow nodes (not action nodes)
    private List<IBTNode> childNodes = new List<IBTNode>();
    
    public BTFlowNode_Composite(
        IBehaviorTree owningTree,
        SuccessCriteria successCriteria = SuccessCriteria.ALL,
        float threshold = 1.0f)
        : base(successCriteria, threshold)
    {
        this.OwningTree = owningTree;
        DebugDisplayName = $"CompositeFlow({successCriteria})";
    }
    
    /// <summary>
    /// Add a child node (can be any IBTNode, including other flow nodes)
    /// </summary>
    public override IBTNode AddChild(IBTNode childNode)
    {
        childNode.SetOwiningTree(OwningTree);
        childNodes.Add(childNode);
        Console.WriteLine($"✅ Added child node: {childNode.DebugDisplayName} to composite flow node");
        return childNode;
    }
    
    /// <summary>
    /// Get all child nodes
    /// </summary>
    public List<IBTNode> GetChildren()
    {
        return new List<IBTNode>(childNodes);
    }
    
    /// <summary>
    /// Get the number of child nodes
    /// </summary>
    public int ChildCount => childNodes.Count;
    
    /// <summary>
    /// Enumerate through child nodes
    /// </summary>
    public override IEnumerator<IBTNode> GetEnumerator()
    {
        return childNodes.GetEnumerator();
    }
    
    /// <summary>
    /// Execute the composite flow node logic
    /// This ticks all child nodes and evaluates success criteria
    /// </summary>
    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
        Console.WriteLine($"   🔍 CompositeFlow: Executing {childNodes.Count} child nodes");
        
        // Tick all child nodes
        foreach (var childNode in childNodes)
        {
            Console.WriteLine($"   ⚡ Ticking child: {childNode.DebugDisplayName}");
            var previousStatus = childNode.LastStatus;
            childNode.Tick(inDeltaTime);
            Console.WriteLine($"   📊 Child {childNode.DebugDisplayName}: {previousStatus} → {childNode.LastStatus}");
        }
        
        // Check if all child nodes have finished
        bool allChildrenFinished = childNodes.All(child => child.HasFinished);
        
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
        if (childNodes.Count == 0) return false;
        
        int successCount = childNodes.Count(node => node.LastStatus == EBTNodeResult.Succeeded);
        int totalCount = childNodes.Count;
        
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
        foreach (var childNode in childNodes)
        {
            childNode.Reset();
        }
        Console.WriteLine($"🔄 Reset composite flow node and all {childNodes.Count} children");
    }
}
