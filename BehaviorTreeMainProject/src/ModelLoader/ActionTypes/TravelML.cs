using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class TravelML : GenericBTAction
    {
        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: from of type firstposition
        public Firstposition from { get; private set; }

        // Parameter: to of type firstposition
        public Firstposition to { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public TravelML(string actionType, string instanceName, Blackboard<FastName> blackboard, Robot client, Firstposition from, Firstposition to)
            : base(actionType, instanceName, blackboard)
        {
            this.client = client;
            this.from = from;
            this.to = to;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("travelML_preconditions"));
            preconditions.AddPredicate(new FastName("travelML_pre_0"), new AtAgent(client, from, false));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("travelML_effects"));
            effects.AddPredicate(new FastName("travelML_eff_0"), new AtAgent(client, from, true));
            effects.AddPredicate(new FastName("travelML_eff_1"), new AtAgent(client, to, false));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool ExecuteActionLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for TravelML
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
