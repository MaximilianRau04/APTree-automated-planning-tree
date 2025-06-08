public interface IOrderStrategy
{
    bool CanExecute(EBTNodeResult currentState, EBTNodeResult nextState, float elapsedTime, float delay);
}

public class TotalOrder : IOrderStrategy
{
    public bool CanExecute(EBTNodeResult currentState, EBTNodeResult nextState, float elapsedTime, float delay)
    {
         // Next action can only start when current action is completely finished
        return currentState == EBTNodeResult.Succeeded || 
               currentState == EBTNodeResult.failed;
    }
}

public class StrictParalellOrder : IOrderStrategy
{
    public bool CanExecute(EBTNodeResult currentState, EBTNodeResult nextState, float elapsedTime, float delay)
    {
         // Next action can start as soon as current action has started
        return currentState == EBTNodeResult.InProgress;
    }
}

public class ParallelOrder : IOrderStrategy
{
    public bool CanExecute(EBTNodeResult currentState, EBTNodeResult nextState, float elapsedTime, float delay)
    {
        // Next action can start after current action has been running for 'delay' time
        return currentState == EBTNodeResult.InProgress && 
               elapsedTime >= delay;
    }
}