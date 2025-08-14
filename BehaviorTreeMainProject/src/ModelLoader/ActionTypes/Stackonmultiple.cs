using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class Stackonmultiple : GenericBTAction
    {
        // Parameter: plate of type plate
        public Plate plate { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: pos of type positionOnRail
        public PositionOnRail pos { get; private set; }

        // Parameter: vg of type vacuumGripper
        public VacuumGripper vg { get; private set; }

        // Parameter: mod of type cassette
        public Cassette mod { get; private set; }

        // Parameter: lay of type stack
        public Stack lay { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: allset(lay = lay, mod = mod, isNegated = false)",
            "PredicateInstance: hasTool(agent = client, tool = vg, isNegated = false)",
            "PredicateInstance: holding(agent = client, myObject = plate, isNegated = false)",
            "PredicateInstance: atplace(myObject = plate, place = pos, isNegated = true)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: atplace(myObject = plate, place = pos, isNegated = false)",
        };

        public Stackonmultiple(string actionType, string instanceName, Blackboard<FastName> blackboard, Plate plate, Robot client, PositionOnRail pos, VacuumGripper vg, Cassette mod, Stack lay)
            : base(actionType, instanceName, blackboard)
        {
            this.plate = plate;
            this.client = client;
            this.pos = pos;
            this.vg = vg;
            this.mod = mod;
            this.lay = lay;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Stackonmultiple
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
