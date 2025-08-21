using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class DeequipML : GenericBTAction
    {
        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: too of type vacuumGripper
        public VacuumGripper too { get; private set; }

        // Parameter: ep of type equipposition
        public Equipposition ep { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public DeequipML(string actionType, string instanceName, Blackboard<FastName> blackboard, Robot client, VacuumGripper too, Equipposition ep)
            : base(actionType, instanceName, blackboard)
        {
            this.client = client;
            this.too = too;
            this.ep = ep;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("deequipML_preconditions"));
            preconditions.AddPredicate(new FastName("deequipML_pre_0"), new HasTool(client, too, false));
            preconditions.AddPredicate(new FastName("deequipML_pre_1"), new Empty(client, true));
            preconditions.AddPredicate(new FastName("deequipML_pre_2"), new Positionfree(ep, false));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("deequipML_effects"));
            effects.AddPredicate(new FastName("deequipML_eff_0"), new Empty(client, false));
            effects.AddPredicate(new FastName("deequipML_eff_1"), new HasTool(client, too, true));
            effects.AddPredicate(new FastName("deequipML_eff_2"), new Positionfree(ep, true));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool ExecuteActionLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for DeequipML
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
