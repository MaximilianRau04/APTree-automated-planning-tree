public class ActionTypeInfo
{
  
    public List<Parameter> Parameters { get; set; }
    public List<PredicateTemplate> PreconditionTemplates { get; set; }
    public List<PredicateTemplate> EffectTemplates { get; set; }
    public Func<Dictionary<string, object>, float, bool> ActionLogic { get; set; }

    public ActionTypeInfo( List<Parameter> parameters, List<PredicateTemplate> preconditionTemplates, List<PredicateTemplate> effectTemplates, Func<Dictionary<string, object>, float, bool> actionLogic)
    {
        
        Parameters = parameters;
        PreconditionTemplates = preconditionTemplates;
        EffectTemplates = effectTemplates;
        ActionLogic = actionLogic;
    }
    
}