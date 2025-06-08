public class BTFlowNode_Dynamic : BTFlowNodeBase
{

    private readonly ActionGraph actionGraph = new();
    public override string DebugDisplayName { get; protected set; } = "DynamicFlow";

    public BTFlowNode_Dynamic(
        IBehaviorTree owningTree,
        SuccessCriteria successCriteria = SuccessCriteria.All,
        float threshold = 1.0f)
        : base(successCriteria, threshold)
    {
        this.OwningTree = owningTree;
        this.planner = new CallPDDLPlanner(OwningTree);
    }

    

    public override IEnumerator<IBTNode> GetEnumerator()
    {
        var (actions, orderTypes) = planner.CreatePlanWithOrders();
        
        for (int i = 0; i < actions.Count - 1; i++)
        {
            AddChild(actions[i]);
            IOrderStrategy strategy = orderTypes[i] switch
            {
                OrderType.Total => new TotalOrder(),
                OrderType.Strictparallel => new StrictParalellOrder(),
                OrderType.Parallel => new ParallelOrder(),
                _ => new TotalOrder() // default
            };
            actionGraph.AddOrderConstraint(actions[i], actions[i + 1], strategy);
        }

        if (actions.Count > 0)
        {
            AddChild(actions[^1]);
        }

        return Children.GetEnumerator();
    }

  
  
    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
        // Get nodes that can be executed this tick
        var executableNodes = actionGraph.GetExecutableNodes(inDeltaTime);
        
        // Execute each node that's ready
        foreach (var node in executableNodes)
        {
            node.Tick(inDeltaTime);
        }

        // Return true if all nodes have completed
        return Children.All(node => node.LastStatus == EBTNodeResult.Succeeded);
    }

    private bool EvaluateSuccessCriteria()
    {
        var succeededNodes = Children.Count(node => node.LastStatus == EBTNodeResult.Succeeded);
        var totalNodes = Children.Count;

        return successCriteria switch
        {
            SuccessCriteria.All => succeededNodes == totalNodes,
            SuccessCriteria.Any => succeededNodes > 0,
            SuccessCriteria.Count => succeededNodes >= (int)(successThreshold),
            SuccessCriteria.Percentage => succeededNodes >= (totalNodes * successThreshold),
            _ => false
        };
    }

    public override IBTNode AddChild(IBTNode Innode)
    {
        if (Innode is BTActionNodeBase)
        {
            Children.Add(Innode);
            return Innode;
        }
        throw new ArgumentException("Dynamic flow node can only accept action nodes as children");
    }

    protected override bool OnTick_Children(float inDeltaTime)
    {
        // Get nodes that can be executed this tick
        var executableNodes = actionGraph.GetExecutableNodes(inDeltaTime);
        
        // Execute each node that's ready
        foreach (var node in executableNodes)
        {
            node.Tick(inDeltaTime);
        }

        return true;  // Return success after executing children
    }
}