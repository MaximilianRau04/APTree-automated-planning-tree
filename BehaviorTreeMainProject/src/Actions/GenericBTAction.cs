using System;
using System.Linq;
using System.Collections.Generic;


// Generic action class that will be created by the factory
public abstract class GenericBTAction : BTActionNodeBase
{
    public readonly FastName actionType;  
    private readonly Blackboard<FastName> blackboard;

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

    protected abstract override bool OnTick_NodeLogic(float InDeltaTime);
}
