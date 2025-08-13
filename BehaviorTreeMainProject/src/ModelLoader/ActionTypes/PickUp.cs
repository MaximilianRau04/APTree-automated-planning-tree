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

        protected override List<ActionPredicateTemplate> PreconditionTemplates => new List<ActionPredicateTemplate>
        {
            new ActionPredicateTemplate("isAt", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("pickedObject", "pickedObject", "Beam"),
                new PredicateParameterMapping("loc", "loc", "Firstposition"),
            }),
            new ActionPredicateTemplate("atAgent", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("rob", "rob", "Robot"),
                new PredicateParameterMapping("loc", "loc", "Firstposition"),
            }),
            new ActionPredicateTemplate("hasTool", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("rob", "rob", "Robot"),
                new PredicateParameterMapping("robTool", "robTool", "VacuumGripper"),
            }),
        };

        protected override List<ActionPredicateTemplate> EffectTemplates => new List<ActionPredicateTemplate>
        {
            new ActionPredicateTemplate("holding", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("rob", "rob", "Robot"),
                new PredicateParameterMapping("pickedObject", "pickedObject", "Beam"),
            }),
            new ActionPredicateTemplate("atAgent", new List<PredicateParameterMapping>
            {
                new PredicateParameterMapping("rob", "rob", "Robot"),
                new PredicateParameterMapping("loc", "loc", "Firstposition"),
            }),
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
