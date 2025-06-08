public enum OrderType
{
    None,               // No ordering constraint
    Total,             // Must complete in sequence, one action should return the result as success/failure until the next action can happen. 
    Strictparallel,   // Can run simultaneously, both starting at the same time.
    Parallel,        // Can overlap but , one action can start before the other one is finished.
    Partial,        // Partial ordering, one action can start before the other one is finished, but the other action should be able to start.
}