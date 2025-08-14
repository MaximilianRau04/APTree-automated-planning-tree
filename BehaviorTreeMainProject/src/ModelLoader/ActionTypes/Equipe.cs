using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class Equipe : GenericBTAction
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

        public Equipe(string actionType, string instanceName, Blackboard<FastName> blackboard, Robot client, VacuumGripper too, Equipposition ep)
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
            preconditions = new State(StateType.Precondition, new FastName("equipe_preconditions"));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("equipe_effects"));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Equipe
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
