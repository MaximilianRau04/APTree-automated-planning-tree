using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class Grab : GenericBTAction
    {
        // Parameter: obj of type beam
        public Beam obj { get; private set; }

        // Parameter: grabPos of type firstposition
        public Firstposition grabPos { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: atplace(myObject = obj, place = grabPos, isNegated = false)",
            "PredicateInstance: holding(agent = client, myObject = obj, isNegated = true)",
            "PredicateInstance: positionfree(pos = grabPos, isNegated = true)",
            "PredicateInstance: clear(myObject = obj, isNegated = false)",
            "PredicateInstance: stacked(myObject = obj, isNegated = true)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: holding(agent = client, myObject = obj, isNegated = false)",
            "PredicateInstance: atplace(myObject = obj, place = grabPos, isNegated = true)",
            "PredicateInstance: clear(myObject = obj, isNegated = true)",
            "PredicateInstance: positionfree(pos = grabPos, isNegated = false)",
        };

        public Grab(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj, Firstposition grabPos, Robot client)
            : base(actionType, instanceName, blackboard)
        {
            this.obj = obj;
            this.grabPos = grabPos;
            this.client = client;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Grab
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
