public interface IBTNode
{
    // who is responsible for handling this node
    Agent? self { get; }
    // which tree does this node belong to
    IBehaviorTree OwningTree { get; }
    Blackboard<FastName> LinkedBlackboard { get; }  //= new();
    // what is the current state of this node
    EBTNodeResult LastStatus{ get; }
    // is the node already finished?
    bool HasFinished { get; }
    bool HasChildren{ get; }
     // Add debug display name property
    string DebugDisplayName { get; }
    void SetOwiningTree(IBehaviorTree InOwningtree);
bool DoDecoratorsNowPermitRunning(float InDeltaTime);
    void Reset();
    EBTNodeResult Tick(float InDeltaTime);
    IBTNode AddService(BTServiceBase InService, bool InIsAlwaysOn = false);
    IBTNode AddDecorator(IBTDecorator InDecorator);


}