using BehaviorTreeMainProject.Services;

/// <summary>
/// Decorator that ensures dynamic planning is completed before allowing node execution.
/// This decorator simply checks the PlanningPhaseDynamic flag on the blackboard.
/// </summary>
public class BTDecorator_DynamicPlanningComplete : BTDecoratorBase
{
     public override bool CanPostProcessTickResult => true;
    public override EBTNodeResult PostProcessTickResult(EBTNodeResult InResult) => InResult;
    public BTDecorator_DynamicPlanningComplete() : base(false)
    {

    }
    

    
    protected override bool OnEvaluate(float InDeltaTime)
    {
        // Check if LinkedBlackboard is available
        if (LinkedBlackboard == null)
        {
            LoggingService.LogWarning($"⚠️ DynamicPlanningCompleteDecorator: LinkedBlackboard is null, allowing execution");
            ExecutionFlowLogger.LogDecoratorTick("DynamicPlanningComplete", "LinkedBlackboard", "Null", "ALLOW_NULL");
            return true; // Allow execution when blackboard is not available
        }
        
        // Check if all cassettes have completed their subtree injection
        if (LinkedBlackboard.CassetteSubtreeCompleted == null)
        {
            LoggingService.LogWarning($"⚠️ DynamicPlanningCompleteDecorator: CassetteSubtreeCompleted array is null, allowing execution");
            ExecutionFlowLogger.LogDecoratorTick("DynamicPlanningComplete", "CassetteSubtreeCompleted", "Null", "ALLOW_NULL");
            return true; // Allow execution when array is not available
        }
        
        // Check if all cassettes have completed subtree injection
        bool allCassettesCompleted = true;
        foreach (bool completed in LinkedBlackboard.CassetteSubtreeCompleted)
        {
            if (!completed)
            {
                allCassettesCompleted = false;
                break;
            }
        }
        
        if (!allCassettesCompleted)
        {
            // Log which cassettes are still pending
            var pendingCassettes = new List<int>();
            for (int i = 0; i < LinkedBlackboard.CassetteSubtreeCompleted.Length; i++)
            {
                if (!LinkedBlackboard.CassetteSubtreeCompleted[i])
                {
                    pendingCassettes.Add(i + 1); // +1 for human-readable cassette numbers
                }
            }
            
            LoggingService.LogInfo($"⏳ DynamicPlanningCompleteDecorator: Waiting for cassettes {string.Join(", ", pendingCassettes)} to complete subtree injection");
            ExecutionFlowLogger.LogDecoratorTick("DynamicPlanningComplete", "CassetteSubtreeCompleted", "Pending", "BLOCK_FOR_RE_EVAL");
            return false; // Block execution until all cassettes complete
        }
        else
        {
            LoggingService.LogInfo($"✅ DynamicPlanningCompleteDecorator: All cassettes have completed subtree injection, allowing execution");
            ExecutionFlowLogger.LogDecoratorTick("DynamicPlanningComplete", "CassetteSubtreeCompleted", "Complete", "ALLOW");
            return true; // Allow execution when all cassettes are complete
        }
    }
}
