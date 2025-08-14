using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class Stack : GenericBTAction
    {
        // Parameter: obj1 of type beam
        public Beam obj1 { get; private set; }

        // Parameter: obj2 of type beam
        public Beam obj2 { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: vg of type vacuumGripper
        public VacuumGripper vg { get; private set; }

        // Parameter: pr of type positionOnRail
        public PositionOnRail pr { get; private set; }

        // Parameter: lay of type stack
        public Stack lay { get; private set; }

        // Parameter: mod of type cassette
        public Cassette mod { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: holding(agent = client, myObject = obj1, isNegated = false)",
            "PredicateInstance: hasTool(agent = client, tool = vg, isNegated = false)",
            "PredicateInstance: atplace(myObject = obj2, place = pr, isNegated = false)",
            "PredicateInstance: atplace(myObject = obj1, place = pr, isNegated = true)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: ontop(myObject1 = obj1, myObject2 = obj2, isNegated = false)",
            "PredicateInstance: stacked(myObject = obj1, isNegated = false)",
            "PredicateInstance: holding(agent = client, myObject = obj1, isNegated = true)",
            "PredicateInstance: atplace(myObject = obj1, place = pr, isNegated = false)",
            "PredicateInstance: clear(myObject = obj2, isNegated = true)",
            "PredicateInstance: clear(myObject = obj1, isNegated = false)",
            "PredicateInstance: allset(lay = lay, mod = mod, isNegated = false)",
        };

        public Stack(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj1, Beam obj2, Robot client, VacuumGripper vg, PositionOnRail pr, Stack lay, Cassette mod)
            : base(actionType, instanceName, blackboard)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
            this.client = client;
            this.vg = vg;
            this.pr = pr;
            this.lay = lay;
            this.mod = mod;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Stack
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
