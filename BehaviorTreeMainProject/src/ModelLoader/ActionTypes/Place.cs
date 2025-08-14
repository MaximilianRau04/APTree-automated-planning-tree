using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class Place : GenericBTAction
    {
        // Parameter: obj of type beam
        public Beam obj { get; private set; }

        // Parameter: placePos of type firstposition
        public Firstposition placePos { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: holding(agent = client, myObject = obj, isNegated = false)",
            "PredicateInstance: clear(myObject = obj, isNegated = true)",
            "PredicateInstance: positionfree(pos = placePos, isNegated = false)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: atplace(myObject = obj, place = placePos, isNegated = false)",
            "PredicateInstance: holding(agent = client, myObject = obj, isNegated = true)",
            "PredicateInstance: clear(myObject = obj, isNegated = false)",
            "PredicateInstance: positionfree(pos = placePos, isNegated = true)",
        };

        public Place(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj, Firstposition placePos, Robot client)
            : base(actionType, instanceName, blackboard)
        {
            this.obj = obj;
            this.placePos = placePos;
            this.client = client;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Place
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
