using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

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

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public Place(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj, Firstposition placePos, Robot client)
            : base(actionType, instanceName, blackboard)
        {
            this.obj = obj;
            this.placePos = placePos;
            this.client = client;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("place_preconditions"));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("place_effects"));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for Place
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
