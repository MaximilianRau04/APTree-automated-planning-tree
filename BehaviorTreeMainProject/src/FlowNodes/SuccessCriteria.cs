public enum SuccessCriteria
{
    All,            // All children must succeed
    Any,            // At least one child must succeed
    Count,          // X number of children must succeed
    Percentage,     // X% of children must succeed
    index
}