public class BTInstance : IBehaviorTree
{
    public string DebugDisplayName { get; set; } = "Behavior Tree";

    public Blackboard<FastName> LinkedBlackboard { get; protected set; }

    public IBTNode RootNode { get;  set; }

    public BTInstance()
    {
        DebugDisplayName = "Default Tree";
        LinkedBlackboard = null;
        RootNode = null;
    }

   
    public void Initialise( Blackboard<FastName> InBlackboard, string InRootNodeName = "Root")
    {
        if (!string.IsNullOrEmpty(InRootNodeName))
            DebugDisplayName = InRootNodeName;
        LinkedBlackboard = InBlackboard;
       
        RootNode = new BTFlowNode_Dynamic(this);
        RootNode.SetOwiningTree(this);
    }

     public IBTNode AddChildToRootNode<NodeType>(IBTNode InNode) 
    {
        InNode.SetOwiningTree(this);
        return (RootNode as IBTFlowNode).AddChild(InNode);
        
    }

     public bool HasFinished()
    {
        return RootNode?.HasFinished ?? true;
    }

    public void Reset()
    {
        RootNode.Reset();
    }

    public EBTNodeResult Tick(float InDeltaTime)
    {
       return RootNode.Tick(InDeltaTime);
    }
}