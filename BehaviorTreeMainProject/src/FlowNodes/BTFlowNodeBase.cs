using System.Collections;

public abstract class BTFlowNodeBase : BTNodeBase, IEnumerable
{
    // is this node allowed to have children?
     public override bool HasChildren => true;
     public abstract string DebugDisplayName { get; protected set; }
     public SuccessCriteria successCriteria { get; protected set; }
     // needed if success criteria is count or percentage
    protected float successThreshold;
     // childResults is no longer needed since we use actionGraph for evaluation
     
     // Replace simple list with node graph structure
     protected NodeGraph actionGraph = new();
     
     protected BTServicePlanner planner;
     
     // Planning service for high-level actions
     public BTServiceBase PlanningService { get; protected set; }
    private readonly IBehaviorTree owningTree;
    
    // Node name property
    public FastName NodeName { get; protected set; }
        
    public abstract IEnumerator<IBTNode> GetEnumerator();
    
    // Explicit implementation for non-generic IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract IBTNode AddChild(IBTNode Innode);

    public BTFlowNodeBase(FastName nodeName, SuccessCriteria criteria = SuccessCriteria.ALL, float threshold = 1.0f)
    {
        this.NodeName = nodeName;
        this.successCriteria = criteria;
        this.successThreshold = threshold;
    }
/// <summary>
/// this function evaluates the success criteria to see if a flow node is successful or not
/// </summary>
/// <returns></returns>
    protected bool EvaluateSuccessCriteria()
    {
        var actionNodes = actionGraph.GetAllActionNodes();
        if (actionNodes.Count == 0) return false;
        
        int successCount = actionNodes.Count(node => node.LastStatus == EBTNodeResult.Succeeded);
        int totalCount = actionNodes.Count;
        
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
    /// Creates a NodeGraph from a list of action nodes with default relations
    /// Default: MEETS temporal constraint and sequential order (left-to-right)
    /// </summary>
    /// <param name="actionNodes">List of action nodes to add to the graph</param>
    /// <returns>The created NodeGraph</returns>
    public NodeGraph CreateNodeGraphFromActions(List<GenericBTAction> actionNodes)
    {
        var graph = new NodeGraph();
        
        Console.WriteLine($"🔧 CreateNodeGraphFromActions: Input actionNodes count: {actionNodes?.Count ?? 0}");
        
        if (actionNodes == null || actionNodes.Count == 0)
        {
            Console.WriteLine("🔧 CreateNodeGraphFromActions: No action nodes provided, returning empty graph");
            return graph;
        }

        // Add all action nodes to the graph
        foreach (var action in actionNodes)
        {
            Console.WriteLine($"🔧 CreateNodeGraphFromActions: Adding action {action.InstanceName.ToString()} to graph");
            graph.AddNode(action);
        }

        Console.WriteLine($"🔧 CreateNodeGraphFromActions: Added {actionNodes.Count} nodes to graph");

        // Create default relations: sequential order with MEETS temporal constraint
        for (int i = 0; i < actionNodes.Count - 1; i++)
        {
            var currentAction = actionNodes[i];
            var nextAction = actionNodes[i + 1];
            
            Console.WriteLine($"🔧 CreateNodeGraphFromActions: Creating relation {currentAction.InstanceName.ToString()} → {nextAction.InstanceName.ToString()}");
            
            // Add order relation (sequential execution)
            graph.AddOrderRelation(currentAction, nextAction);
            
            // Add temporal constraint (MEETS - next action starts when current ends)
            graph.AddTemporalConstraint(currentAction, nextAction, TemporalConstraint.MEETS);
        }

        Console.WriteLine($"🔧 CreateNodeGraphFromActions: Created {actionNodes.Count - 1} relations");
        Console.WriteLine($"🔧 CreateNodeGraphFromActions: Final graph has {graph.GetAllActionNodes().Count} nodes");

        // Debug: Show final graph structure
        Console.WriteLine("\n🔍 DEBUG: Final Graph Structure:");
        var allNodes = graph.GetAllActionNodes();
        for (int i = 0; i < allNodes.Count; i++)
        {
            var node = allNodes[i];
            Console.WriteLine($"   Node {i}: {node.InstanceName.ToString()}");
        }

        return graph;
    }

    /// <summary>
    /// Creates a NodeGraph from a list of action nodes with custom relations
    /// </summary>
    /// <param name="actionNodes">List of action nodes to add to the graph</param>
    /// <param name="useOrderRelations">Whether to create sequential order relations</param>
    /// <param name="defaultTemporalConstraint">Default temporal constraint between consecutive actions</param>
    /// <returns>The created NodeGraph</returns>
    protected NodeGraph CreateNodeGraphFromActions(List<GenericBTAction> actionNodes, bool useOrderRelations, TemporalConstraint defaultTemporalConstraint)
    {
        var graph = new NodeGraph();
        
        if (actionNodes == null || actionNodes.Count == 1)
            return graph;

        // Add all action nodes to the graph
        foreach (var action in actionNodes)
        {
            graph.AddNode(action);
        }

        // Create relations based on parameters
        for (int i = 0; i < actionNodes.Count - 1; i++)
        {
            var currentAction = actionNodes[i];
            var nextAction = actionNodes[i + 1];
            
            // Add order relation if requested
            if (useOrderRelations)
            {
                graph.AddOrderRelation(currentAction, nextAction);
            }
            
            // Add temporal constraint
            graph.AddTemporalConstraint(currentAction, nextAction, defaultTemporalConstraint);
        }

        return graph;
    }

    /// <summary>
    /// Sets the action graph for this flow node
    /// </summary>
    /// <param name="graph">The NodeGraph to use</param>
    public void SetActionGraph(NodeGraph graph)
    {
        actionGraph = graph;
    }
    
    /// <summary>
    /// Gets the action graph for this flow node
    /// </summary>
    /// <returns>The NodeGraph</returns>
    public NodeGraph GetActionGraph()
    {
        return actionGraph;
    }
    
    /// <summary>
    /// Set the planning service for this flow node
    /// </summary>
    /// <param name="service">The planning service to use</param>
    public void SetPlanningService(BTServiceBase service)
    {
        PlanningService = service;
        Console.WriteLine($"🔧 BTFlowNodeBase: Set planning service {service.GetType().Name}");
    }
    
    /// <summary>
    /// Get the node name as a string
    /// </summary>
    /// <returns>The node name as a string</returns>
    public string GetNodeName()
    {
        return NodeName?.ToString() ?? "Unnamed";
    }
}