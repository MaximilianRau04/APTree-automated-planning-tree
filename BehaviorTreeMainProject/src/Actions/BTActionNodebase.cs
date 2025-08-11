using System;
using System.Linq;
using System.Collections.Generic;

public abstract class BTActionNodeBase :BTNodeBase
{
    private readonly Func<Dictionary<string, object>, float, bool> actionLogic;
    protected readonly Blackboard<FastName> blackboard;
    protected FastName instanceName;
    protected string debugDisplayName;

    protected BTActionNodeBase(Blackboard<FastName> blackboard, string instanceName)
    {
        this.blackboard = blackboard;
        this.instanceName = new FastName(instanceName);
    }

    public override bool HasChildren => false;
   
    protected override bool OnTick_Children (float InDeltaTime)
    {
        return false;
    }
    protected bool SetStatusAndCalculateReturnvalue(EBTNodeResult InResult, bool? bOverrideReturnValue = null)
    {
        LastStatus = InResult;
        if (bOverrideReturnValue.HasValue)
            return bOverrideReturnValue.Value;

        return InResult != EBTNodeResult.failed;
    }

}