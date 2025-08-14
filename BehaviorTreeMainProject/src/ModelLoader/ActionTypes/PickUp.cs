using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;

namespace BehaviorTreeMainProject
{
    public class PickUp : GenericBTAction
    {
        // Parameter: pickedObject of type beam
        public Beam pickedObject { get; private set; }

        // Parameter: rob of type robot
        public Robot rob { get; private set; }

        // Parameter: loc of type firstposition
        public Firstposition loc { get; private set; }

        // Parameter: robTool of type vacuumGripper
        public VacuumGripper robTool { get; private set; }

        protected override List<string> PreconditionTemplates => new List<string>
        {
            "PredicateInstance: isAt(myObject = pickedObject, location = loc, isNegated = false)",
            "PredicateInstance: atAgent(agent = rob, location = loc, isNegated = false)",
            "PredicateInstance: hasTool(agent = rob, tool = robTool, isNegated = false)",
        };

        protected override List<string> EffectTemplates => new List<string>
        {
            "PredicateInstance: holding(agent = rob, myObject = pickedObject, isNegated = false)",
            "PredicateInstance: atAgent(agent = rob, location = loc, isNegated = false)",
        };

        public PickUp(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam pickedObject, Robot rob, Firstposition loc, VacuumGripper robTool)
            : base(actionType, instanceName, blackboard)
        {
            this.pickedObject = pickedObject;
            this.rob = rob;
            this.loc = loc;
            this.robTool = robTool;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for PickUp
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
