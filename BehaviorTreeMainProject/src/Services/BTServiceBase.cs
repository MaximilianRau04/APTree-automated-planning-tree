public abstract class BTServiceBase : IBTDecorator
{
    
    public IBehaviorTree OwningTree { get; protected set; }

protected BTServiceBase(IBehaviorTree InOwningTree) 
      
    {
        this.OwningTree = InOwningTree;
    }
    public Blackboard<FastName> LinkedBlackboard => OwningTree.LinkedBlackboard;

    public void SetOwiningTree(IBehaviorTree InOwningtree)
    {
        this.OwningTree = InOwningtree;
    }

    public abstract bool Tick(float InDeltaTime);
  
    
}