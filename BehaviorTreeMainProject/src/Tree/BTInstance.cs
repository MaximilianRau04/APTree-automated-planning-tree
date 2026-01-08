public class BTInstance : IBehaviorTree
{
    public string DebugDisplayName { get; set; } = "Behavior Tree";

    public Blackboard<FastName> LinkedBlackboard { get; protected set; }

    public BTFlowNodeBase RootNode { get;  set; }

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
       
        // Use composite flow node as root to support hierarchical structure
        RootNode = new BTFlowNode_Composite(new FastName(InRootNodeName), this);
        RootNode.SetOwiningTree(this);
    }

     public IBTNode AddChildToRootNode<NodeType>(IBTNode InNode) 
    {
        InNode.SetOwiningTree(this);
        
        // Set the tree for all services that don't have it set yet
        InNode.SetTreeForAllServices(this);
        
        // If this is a GenericBTAction, also set the tree for its SubtreeInjectionService
        if (InNode is PActionNode action)
        {
            action.SetTreeForSubtreeInjectionService(this);
        }
        
        return (RootNode as BTFlowNode_Composite).AddChild(InNode);
        
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