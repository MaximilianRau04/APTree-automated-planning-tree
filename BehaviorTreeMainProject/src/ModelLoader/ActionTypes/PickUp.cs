using System;
using System.Collections.Generic;

namespace ModelLoader.ActionTypes
{
    public class PickUp : GenericBTAction
    {
        protected override List<ActionPredicateTemplate> PreconditionTemplates => new List<ActionPredicateTemplate>
        {
            new ActionPredicateTemplate("at", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("object", "object", "string"),
                new PredicateParameterMapping("location", "location", "string"),
            }),
            new ActionPredicateTemplate("atAgent", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("agent", "agent", "string"),
                new PredicateParameterMapping("location", "location", "string"),
            }),
            new ActionPredicateTemplate("hasTool", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("robot", "robot", "Agent"),
                new PredicateParameterMapping("tool", "tool", "string"),
            }),
        };

        protected override List<ActionPredicateTemplate> EffectTemplates => new List<ActionPredicateTemplate>
        {
            new ActionPredicateTemplate("holding", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("agent", "agent", "string"),
                new PredicateParameterMapping("object", "object", "string"),
            }),
            new ActionPredicateTemplate("at", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("agent", "agent", "string"),
                new PredicateParameterMapping("location", "location", "string"),
            }),
        };

        public PickUp(string actionType, string instanceName, Blackboard<FastName> blackboard, List<Parameter> parameters, object[] parameterValues)
            : base(actionType, instanceName, blackboard, parameters, parameterValues)
        {
            // Parameters and predicates are handled by the base class
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for PickUp
            // Access parameters via: parameterValues[index] or parameters[index]
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
