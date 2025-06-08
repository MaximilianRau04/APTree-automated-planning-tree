using System;
using System.Linq;
using System.Collections.Generic;


// Generic action class that will be created by the factory
public abstract class GenericBTAction : BTActionNodeBase, IBTActionNode
{
    private readonly FastName actionType;  
    private readonly object[] parameterValues;
    private readonly List<Parameter> parameters;
    private readonly State preconditions;
    private readonly State effects;
    private string debugDisplayName;

    public override string DebugDisplayName 
    { 
        get => debugDisplayName;
        protected set => debugDisplayName = value;
    }

    public GenericBTAction(
        string actionType,
        string instanceName,
        Blackboard<FastName> blackboard,
        State preconditions,
        State effects,
        List<Parameter> parameters,
        object[] parameterValues
    ) : base(blackboard, instanceName)
    {
        this.parameters = parameters;
        this.parameterValues = parameterValues;
        this.preconditions = preconditions;
        this.effects = effects;

        // Set debug name based on action type and parameters
        var paramNames = parameterValues.Select(p => p.ToString()).ToList();
        this.DebugDisplayName = $"{instanceName} ({actionType} {string.Join(" ", paramNames)})";
    }
    public void applyEffects()
    {
        // foreach (var effect in effects)
        // {
        //     blackboard.SetValue(effect.Key, effect.Value);
        // }
    }


    protected abstract override bool OnTick_NodeLogic(float InDeltaTime);
//     {
//     //     try
//     //     {
//     //         Console.WriteLine($"{DebugDisplayName}: Executing action");
//     //         Console.WriteLine($"- Parameters: {string.Join(", ", parameterValues.Select(p => p.ToString()))}");
//     //         return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
//     //     }
//     //     catch (Exception e)
//     //     {
//     //         Console.WriteLine($"{DebugDisplayName}: Failed - {e.Message}");
//     //         return SetStatusAndCalculateReturnvalue(EBTNodeResult.failed);
//     //     }
//     // }
 }