public abstract class BTDecoratorBase : IBTDecorator
{
    // gets the responsible agent
   // public Agent self => LinkedBlackboard.GetAgent();
   public Agent self  {get;  protected set;}

    public IBehaviorTree OwningTree { get;  protected set;}

    public Blackboard<FastName> LinkedBlackboard => OwningTree.LinkedBlackboard;

    public abstract bool CanpostProcessTickresult { get; }
    public bool bIsInverted { get; protected set; } = false;
    protected bool? bLastResult;
    protected BTDecoratorBase(bool bInIsInverted = false)
    {
        this.bIsInverted = bInIsInverted;
    }

    // public virtual EBTNodeResult PostProcessTickresult(EBTNodeResult InResult)
    // {
    //     return InResult;
    // }

    public void SetOwiningTree(IBehaviorTree InOwningtree)
    {
        this.OwningTree = InOwningtree;
    }

    public bool Tick(float InDeltaTime)
    {
        this.bLastResult = OnEvaluate(InDeltaTime);
        return bIsInverted ? !bLastResult.Value : bLastResult.Value;
    }
    protected abstract bool OnEvaluate(float InDeltaTime);
}