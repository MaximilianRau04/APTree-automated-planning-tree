public abstract class BTServicePlanner : BTServiceBase
{
protected BTServicePlanner(IBehaviorTree InOwningTree) 
        : base(InOwningTree)
    {
    }

    public override bool Tick(float InDeltaTime)
    {
        // Get new plan
        var (actions, orders) = CreatePlanWithOrders();
        
        // Return false if no valid plan was created
        if (actions == null || actions.Count == 0)
        {
            return false;
        }

        // Add actions to tree
        foreach (var action in actions)
        {
            // Add to appropriate node in tree
        }
        
        // Apply orders if they exist
        if (orders != null)
        {
            AddActionOrders(actions.Cast<BTActionNodeBase>().ToList());
        }
        
        return true;  // Plan was successfully created and applied
    }       

    public abstract List<BTActionNodeBase> GetPlan();
    protected void AddActionOrders(List<BTActionNodeBase> actions)
    {
       
    }
    public abstract (List<IBTNode> Actions, List<OrderType> Orders) CreatePlanWithOrders();
}