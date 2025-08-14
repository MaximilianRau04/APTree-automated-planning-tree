using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class Nailing : GenericBTAction
    {
        // Parameter: obj of type beam
        public Beam obj { get; private set; }

        // Parameter: pos of type positionOnRail
        public PositionOnRail pos { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: ng of type nailGripper
        public NailGripper ng { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public Nailing(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj, PositionOnRail pos, Robot client, NailGripper ng)
            : base(actionType, instanceName, blackboard)
        {
            this.obj = obj;
            this.pos = pos;
            this.client = client;
            this.ng = ng;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("nailing_preconditions"));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("nailing_effects"));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Nailing
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
