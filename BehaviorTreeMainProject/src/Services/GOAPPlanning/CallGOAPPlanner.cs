public class CallGOAPPlanner: BTServicePlanner
{
    private readonly Blackboard<FastName> blackboard;
    private readonly ActionFactory actionFactory;
  
    public FastName PlannerName = new FastName("GOAPPlanner");

    public CallGOAPPlanner(IBehaviorTree InOwningtree) : base(InOwningtree)
    {
        this.blackboard = InOwningtree.LinkedBlackboard;
        this.actionFactory = ActionFactory.Instance;
    }

    public override List<BTActionNodeBase> GetPlan()
    {
        // Implement GOAP planning logic here
        var actions = new List<BTActionNodeBase>();
        // ... planning logic ...
        return actions;
    }
      public override (List<IBTNode> Actions, List<OrderType> Orders) CreatePlanWithOrders()
    {
        var actions = new List<IBTNode>();
        var orders = new List<OrderType>();
        
        // Your GOAP planning logic here
        // For now, return empty lists
        return (actions, orders);
    }
}