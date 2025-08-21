using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class CloseToolML : GenericBTAction
    {
        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: too of type vacuumGripper
        public VacuumGripper too { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public CloseToolML(string actionType, string instanceName, Blackboard<FastName> blackboard, Robot client, VacuumGripper too)
            : base(actionType, instanceName, blackboard)
        {
            this.client = client;
            this.too = too;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("closeToolML_preconditions"));
            preconditions.AddPredicate(new FastName("closeToolML_pre_0"), new ActiveTool(too, false));
            preconditions.AddPredicate(new FastName("closeToolML_pre_1"), new Vgempty(client, false));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("closeToolML_effects"));
            effects.AddPredicate(new FastName("closeToolML_eff_0"), new ActiveTool(too, true));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool ExecuteActionLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for CloseToolML
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
