using System.Collections;

public abstract class BTFlowNodeBase : BTNodeBase, IBTFlowNode
{
    // is this node allowed to have children?
     public override bool HasChildren => true;
     public abstract string DebugDisplayName { get; protected set; }
     public SuccessCriteria successCriteria { get; protected set; }
      protected float successThreshold;
     public List<bool> childResults = new();
       public List<IBTNode> Children = new();
     protected BTServicePlanner planner;
    private readonly IBehaviorTree owningTree;
        
    public abstract IEnumerator<IBTNode> GetEnumerator();
    
    // Explicit implementation for non-generic IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract IBTNode AddChild(IBTNode Innode);

    public BTFlowNodeBase(SuccessCriteria criteria = SuccessCriteria.All, float threshold = 1.0f)
    {
        this.successCriteria = criteria;
        this.successThreshold = threshold;
    }


    protected bool EvaluateSuccessCriteria()
    {
        int successCount = childResults.Count(result => result);
        
        return successCriteria switch
        {
            SuccessCriteria.All => successCount == childResults.Count,
            SuccessCriteria.Any => successCount > 0,
            SuccessCriteria.Count => successCount >= successThreshold,
            SuccessCriteria.Percentage => successCount >= (childResults.Count * successThreshold),
            _ => false
        };
    }
   /// <summary>
      /// this function selects the type of the planner and creates an instance of it
      /// </summary>
      /// <exception cref="ArgumentException"></exception>
 
       
    
    protected override bool OnTick_NodeLogic(float inDeltaTime)
    {
    //    
    return true;


    }
    
}