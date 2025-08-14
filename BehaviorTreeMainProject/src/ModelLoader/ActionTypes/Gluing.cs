using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class Gluing : GenericBTAction
    {
        // Parameter: obj of type beam
        public Beam obj { get; private set; }

        // Parameter: pos of type positionOnRail
        public PositionOnRail pos { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: gg of type glueGun
        public GlueGun gg { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: hasTool(agent = client, tool = gg, isNegated = false)",
            "PredicateInstance: empty(client = client, isNegated = true)",
            "PredicateInstance: atplace(myObject = obj, place = pos, isNegated = false)",
            "PredicateInstance: clear(myObject = obj, isNegated = false)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: glued(myObject = obj, isNegated = false)",
        };

        public Gluing(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj, PositionOnRail pos, Robot client, GlueGun gg)
            : base(actionType, instanceName, blackboard)
        {
            this.obj = obj;
            this.pos = pos;
            this.client = client;
            this.gg = gg;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Gluing
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
