using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class StackHL : GenericBTAction
    {
        // Parameter: obj1 of type beam
        public Beam obj1 { get; private set; }

        // Parameter: obj2 of type beam
        public Beam obj2 { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: vg of type vacuumGripper
        public VacuumGripper vg { get; private set; }

        // Parameter: pr of type positionOnRail
        public PositionOnRail pr { get; private set; }

        // Parameter: lay of type stack
        public Stack lay { get; private set; }

        // Parameter: mod of type cassette
        public Cassette mod { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public StackHL(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj1, Beam obj2, Robot client, VacuumGripper vg, PositionOnRail pr, Stack lay, Cassette mod)
            : base(actionType, instanceName, blackboard)
        {
            this.obj1 = obj1;
            this.obj2 = obj2;
            this.client = client;
            this.vg = vg;
            this.pr = pr;
            this.lay = lay;
            this.mod = mod;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("stackHL_preconditions"));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("stackHL_effects"));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for StackHL
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
