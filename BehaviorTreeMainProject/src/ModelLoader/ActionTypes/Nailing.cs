using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class Nailing : GenericBTAction
    {
        // Parameter: obj of type beam
        public Beam obj { get; private set; }

        // Parameter: pos of type positionOnRail
        public PositionOnRail pos { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: ng of type nailGripper
        public NailGripper ng { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: empty(client = client, isNegated = true)",
            "PredicateInstance: hasTool(agent = client, tool = ng, isNegated = false)",
            "PredicateInstance: atplace(myObject = obj, place = pos, isNegated = false)",
            "PredicateInstance: clear(myObject = obj, isNegated = false)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: nailed(myObject = obj, isNegated = false)",
        };

        public Nailing(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj, PositionOnRail pos, Robot client, NailGripper ng)
            : base(actionType, instanceName, blackboard)
        {
            this.obj = obj;
            this.pos = pos;
            this.client = client;
            this.ng = ng;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Nailing
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
