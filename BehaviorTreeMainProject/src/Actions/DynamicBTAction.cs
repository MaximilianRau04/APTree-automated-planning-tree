// Single dynamic action class that can handle any behavior
public class DynamicBTAction : GenericBTAction
{
    private readonly Func<Dictionary<string, object>, float, bool> actionLogic;
    private readonly Dictionary<string, object> parameters;

    public DynamicBTAction(
        string actionType,
        string instanceName,
        Blackboard<FastName> blackboard,
        State preconditions,
        State effects,
        List<Parameter> parameters,
        object[] parameterValues,
        Func<Dictionary<string, object>, float, bool> actionLogic) 
        : base(actionType, instanceName, blackboard, preconditions, effects, parameters, parameterValues)
    {
        this.actionLogic = actionLogic;
        this.parameters = new Dictionary<string, object>();
        
        // Store parameters in dictionary for easy access
        for (int i = 0; i < parameters.Count; i++)
        {
            this.parameters[parameters[i].Name.ToString()] = parameterValues[i];
        }
        LastStatus = EBTNodeResult.readyToTick;  // Initialize status
    }

    protected override bool OnTick_NodeLogic(float InDeltaTime)
    {
        try
        {
            LastStatus = EBTNodeResult.InProgress;  // Set to in progress when executing
            bool result = actionLogic(parameters, InDeltaTime);
            return SetStatusAndCalculateReturnvalue(
                result ? EBTNodeResult.Succeeded : EBTNodeResult.failed
            );
        }
        catch (Exception e)
        {
            Console.WriteLine($"{DebugDisplayName} failed: {e.Message}");
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.failed);
        }
    }
}
