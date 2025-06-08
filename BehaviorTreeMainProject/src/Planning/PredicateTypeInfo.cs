public class PredicateTypeInfo
{
    public FastName PredicateName { get; set; }
    public Dictionary<string, Type> Parameters { get; set; }

    public PredicateTypeInfo(FastName predicateName, Dictionary<string, Type> parameters)
    {
        Parameters = parameters;
    }


    
}