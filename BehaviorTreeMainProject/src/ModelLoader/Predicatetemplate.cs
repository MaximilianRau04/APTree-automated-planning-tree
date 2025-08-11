using System;
using System.Collections.Generic;

public class PredicateTemplate
{
    public FastName PredicateName { get; set; }
    public Dictionary<string, object> ParameterNames { get; set; }

    public PredicateTemplate(FastName predicateName, Dictionary<string, object> parameterNames)
    {
        PredicateName = predicateName;
        ParameterNames = parameterNames ?? new Dictionary<string, object>();
    }

    public PredicateTemplate(FastName predicateName) : this(predicateName, new Dictionary<string, object>())
    {
    }
}
