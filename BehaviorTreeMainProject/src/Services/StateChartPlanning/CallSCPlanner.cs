public class CallSCPlanner: BTServicePlanner
{
    private readonly Blackboard<FastName> blackboard;
    private readonly FactoryAction actionFactory;
  
    public FastName PlannerName = new FastName("StateChartPlanner");

    public CallSCPlanner(IBehaviorTree InOwningtree) : base(InOwningtree)
    {
        this.blackboard = InOwningtree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
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
        
        // Your StateChart planning logic here
        // For now, return empty lists
        return (actions, orders);
    }
}