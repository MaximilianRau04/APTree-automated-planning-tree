using System.Collections;
using BehaviorTreeMainProject.Services;

public class BTFlowNode_Composite : BTFlowNodeBase
{
    public override string DebugDisplayName { get; protected set; } = "CompositeFlow";
    
    // List to store flow nodes (since NodeGraph is designed for action nodes)
    private List<IBTNode> flowNodes = new List<IBTNode>();
    
    // State tracking for subtree execution - only execute one child at a time
    private int currentChildIndex = 0;
    private bool isExecutingSubtree = false;
    
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
    public IBTNode AddChild(IBTNode childNode)
    {
        childNode.SetOwiningTree(OwningTree);
        
        // Set the tree for all services that don't have it set yet
        childNode.SetTreeForAllServices(OwningTree);
        
        // If this is a GenericBTAction, also set the tree for its SubtreeInjectionService
        if (childNode is GenericBTAction action)
        {
            action.SetTreeForSubtreeInjectionService(OwningTree);
        }
        
        // Store flow nodes in a separate list since NodeGraph is designed for action nodes
        // We'll use the actionGraph from the base class for action nodes and a separate list for flow nodes
        if (childNode is GenericBTAction actionNode)
        {
            actionGraph.AddNode(actionNode);
            // Console.WriteLine($"✅ Added action node: {childNode.DebugDisplayName} to composite flow node actionGraph");
        }
        else
        {
            // For flow nodes, we'll store them in a separate list for now
            // In the future, we could extend NodeGraph to handle flow nodes
            flowNodes.Add(childNode);
            // Console.WriteLine($"✅ Added flow node: {childNode.DebugDisplayName} to composite flow node flowNodes list");
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
    /// This executes one child subtree at a time until completion
    /// </summary>
    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
        var allChildren = GetChildren();
        
        // If no children, fail immediately
        if (allChildren.Count == 0)
        {
            LastStatus = EBTNodeResult.failed;
            return false;
        }
        
        // If we haven't started executing yet, start with the first child
        if (!isExecutingSubtree)
        {
            currentChildIndex = 0;
            isExecutingSubtree = true;
            LoggingService.LogInfo($"🔄 CompositeFlow: Starting subtree execution with {allChildren.Count} children");
        }
        
        // Execute only the current child until it completes
        if (currentChildIndex < allChildren.Count)
        {
            var currentChild = allChildren[currentChildIndex];
            var previousStatus = currentChild.LastStatus;
            
            LoggingService.LogInfo($"🎯 CompositeFlow: Executing child {currentChildIndex + 1}/{allChildren.Count}: {currentChild.DebugDisplayName}");
            
            // Tick the current child
            currentChild.Tick(inDeltaTime);
            
            LoggingService.LogInfo($"📊 CompositeFlow: Child {currentChild.DebugDisplayName}: {previousStatus} → {currentChild.LastStatus}");
            
            // Check if current child has finished
            if (currentChild.HasFinished)
            {
                LoggingService.LogInfo($"✅ CompositeFlow: Child {currentChild.DebugDisplayName} completed with status: {currentChild.LastStatus}");
                
                // Move to next child
                currentChildIndex++;
                
                // Check if we've completed all children
                if (currentChildIndex >= allChildren.Count)
                {
                    // All children have finished, evaluate success criteria
                    bool success = EvaluateCompositeSuccessCriteria();
                    LoggingService.LogInfo($"🎯 CompositeFlow: All children completed, success criteria evaluation: {success}");
                    
                    if (success)
                    {
                        LastStatus = EBTNodeResult.Succeeded;
                        LoggingService.LogSuccess($"🏆 CompositeFlow: Subtree execution completed successfully");
                    }
                    else
                    {
                        LastStatus = EBTNodeResult.failed;
                        LoggingService.LogWarning($"❌ CompositeFlow: Subtree execution failed");
                    }
                    
                    // Reset execution state
                    isExecutingSubtree = false;
                    return false; // We're done
                }
                else
                {
                    LoggingService.LogInfo($"🔄 CompositeFlow: Moving to next child ({currentChildIndex + 1}/{allChildren.Count})");
                }
            }
            else
            {
                LoggingService.LogInfo($"⏳ CompositeFlow: Child {currentChild.DebugDisplayName} still running, continuing execution");
            }
        }
        
        // Still executing
        LastStatus = EBTNodeResult.InProgress;
        return true; // Continue ticking
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
        
        // Console.WriteLine($"   📊 Composite evaluation: {successCount}/{totalCount} children succeeded");
        
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
    /// Reset all child nodes and execution state
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        
        // Reset execution state
        currentChildIndex = 0;
        isExecutingSubtree = false;
        
        // Reset all child nodes
        var allChildren = GetChildren();
        foreach (var childNode in allChildren)
        {
            childNode.Reset();
        }
        
        LoggingService.LogInfo($"🔄 CompositeFlow: Reset execution state and all {allChildren.Count} children");
    }
}
