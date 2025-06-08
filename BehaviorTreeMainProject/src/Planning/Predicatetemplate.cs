public class PredicateTemplate
{
    public bool IsNegated { get; set; }
    public FastName PredicateName { get; set; }
    public FastName[] ParameterNames { get; set; }

    public PredicateTemplate(FastName predicateName, FastName[] parameterNames, bool isNegated)
    {
        PredicateName = predicateName;
        ParameterNames = parameterNames;
        IsNegated = isNegated;
    }
}