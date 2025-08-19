using System;
using System.Linq;
using System.Collections.Generic;


// Generic action class that will be created by the factory
public abstract class GenericBTAction : BTActionNodeBase
{
    public readonly FastName actionType;  
    private readonly Blackboard<FastName> blackboard;

    // High-level action support
    public bool IsHighLevelAction { get; protected set; } = false;
    public BTFlowNode_Dynamic HighLevelSubtree { get; protected set; }
    public BTServiceBase PlanningService { get; protected set; }

    // Abstract properties for preconditions and effects
    protected abstract State Preconditions { get; }
    protected abstract State Effects { get; }

    public override string DebugDisplayName 
    { 
        get => debugDisplayName;
        protected set => debugDisplayName = value;
    }

    // Constructor for action instances
    public GenericBTAction(
        string actionType,
        string instanceName,
        Blackboard<FastName> blackboard
    ) : base(blackboard, instanceName)
    {
        this.actionType = new FastName(actionType);
        this.blackboard = blackboard;
    }
    
    /// <summary>
    /// Set this action as a high-level action with a subtree and planning service
    /// </summary>
    public void SetAsHighLevelAction(BTFlowNode_Dynamic subtree, BTServiceBase planningService)
    {
        IsHighLevelAction = true;
        HighLevelSubtree = subtree;
        PlanningService = planningService;
        Console.WriteLine($"🔧 GenericBTAction: Set {InstanceName.ToString()} as high-level action with subtree");
    }

    public void applyEffects()
    {
        // Apply effects to the blackboard
        if (Effects != null)
        {
            foreach (var objectKey in Effects.GetAllObjects())
            {
                var predicates = Effects.GetPredicates(objectKey);
                foreach (var predicate in predicates)
                {
                    blackboard.SetPredicate(predicate.PredicateName, predicate);
                    Console.WriteLine($"Applied effect predicate: {predicate.PredicateName}");
                }
            }
        }
    }

    /// <summary>
    /// Override the base OnTick_NodeLogic to handle high-level actions
    /// </summary>
    protected override bool OnTick_NodeLogic(float InDeltaTime)
    {
        Console.WriteLine($"🚨 DEBUG: OnTick_NodeLogic called for {InstanceName.ToString()}");
        Console.WriteLine($"🔍 GenericBTAction: {InstanceName.ToString()} OnTick_NodeLogic - IsHighLevelAction: {IsHighLevelAction}, HighLevelSubtree: {(HighLevelSubtree != null ? "exists" : "null")}");
        
        if (IsHighLevelAction && HighLevelSubtree != null)
        {
            Console.WriteLine($"🔧 GenericBTAction: {InstanceName.ToString()} is high-level action, delegating to subtree");
            
            // Delegate execution to the subtree
            var subtreeResult = HighLevelSubtree.Tick(InDeltaTime);
            
            // Propagate subtree status to this action
            LastStatus = HighLevelSubtree.LastStatus;
            
            Console.WriteLine($"📊 GenericBTAction: Subtree result: {subtreeResult}, Status: {LastStatus}");
            
            return subtreeResult == EBTNodeResult.InProgress;
        }
        else
        {
            Console.WriteLine($"🔧 GenericBTAction: {InstanceName.ToString()} executing normal action logic");
            // Execute normal action logic
            return ExecuteActionLogic(InDeltaTime);
        }
    }
    
    /// <summary>
    /// Execute the actual action logic (to be implemented by derived classes)
    /// </summary>
    protected abstract bool ExecuteActionLogic(float InDeltaTime);
}
