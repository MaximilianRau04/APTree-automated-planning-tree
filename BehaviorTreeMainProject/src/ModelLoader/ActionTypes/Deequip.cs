using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class Deequip : GenericBTAction
    {
        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: too of type vacuumGripper
        public VacuumGripper too { get; private set; }

        // Parameter: ep of type equipposition
        public Equipposition ep { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: hasTool(agent = client, tool = too, isNegated = false)",
            "PredicateInstance: atplace(myObject = too, place = ep, isNegated = true)",
            "PredicateInstance: empty(client = client, isNegated = true)",
            "PredicateInstance: positionfree(pos = ep, isNegated = false)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: atplace(myObject = too, place = ep, isNegated = false)",
            "PredicateInstance: empty(client = client, isNegated = false)",
            "PredicateInstance: hasTool(agent = client, tool = too, isNegated = true)",
            "PredicateInstance: positionfree(pos = ep, isNegated = true)",
        };

        public Deequip(string actionType, string instanceName, Blackboard<FastName> blackboard, Robot client, VacuumGripper too, Equipposition ep)
            : base(actionType, instanceName, blackboard)
        {
            this.client = client;
            this.too = too;
            this.ep = ep;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Deequip
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
