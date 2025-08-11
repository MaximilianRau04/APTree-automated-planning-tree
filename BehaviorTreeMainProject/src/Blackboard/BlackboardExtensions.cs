public static class BlackboardExtensions
{
    public static bool HasElement<T>(this Blackboard<T> blackboard, FastName key) where T : class
    {
        try
        {
            blackboard.GetElement(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool HasAgent<T>(this Blackboard<T> blackboard, FastName key) where T : class
    {
        try
        {
            blackboard.GetAgent(key);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool HasLocation<T>(this Blackboard<T> blackboard, FastName key) where T : class
    {
        try
        {
            blackboard.GetLocation(key);
            return true;
        }
        catch
        {
            return false;
        }
    }
    // Add HasPredicate extension method
    public static bool HasPredicate<T>(this Blackboard<T> blackboard, FastName key, Predicate predicate) where T : class
    {
        try
        {
            var predicates = blackboard.GetPredicates(key);
            return predicates.Contains(predicate);
        }
        catch
        {
            return false;
        }
    }

    // Add RemovePredicate extension method
    public static bool RemovePredicate<T>(this Blackboard<T> blackboard, FastName key, Predicate predicate) where T : class 
    {
        try
        {
            var predicates = blackboard.GetPredicates(key);
            return predicates.Remove(predicate);
        }
        catch
        {
            return false;
        }
    }
public static string FormatPredicate(Predicate predicate)
{
    var parameters = predicate.GetAllProperties()
        .Where(p => p.Key != "PredicateName" && p.Key != "PredicateType" && p.Key != "isNegated")
        .Select(p => GetInstanceId(p.Value as Entity))
        .ToList();

    return $"{predicate.PredicateName}({string.Join(",", parameters)})";
}

private static string GetInstanceId(Entity obj)
{
    
    return obj.NameKey.ToString() ?? "";
}
    
}