using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class Stackonmultiple : GenericBTAction
    {
        // Parameter: plate of type element
        public Element plate { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: pos of type location
        public Location pos { get; private set; }

        // Parameter: vg of type vacuumGripper
        public VacuumGripper vg { get; private set; }

        // Parameter: mod of type cassette
        public Cassette mod { get; private set; }

        // Parameter: lay of type stack
        public Stack lay { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public Stackonmultiple(string actionType, string instanceName, Blackboard<FastName> blackboard, Element plate, Robot client, Location pos, VacuumGripper vg, Cassette mod, Stack lay)
            : base(actionType, instanceName, blackboard)
        {
            this.plate = plate;
            this.client = client;
            this.pos = pos;
            this.vg = vg;
            this.mod = mod;
            this.lay = lay;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("stackonmultiple_preconditions"));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("stackonmultiple_effects"));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool ExecuteActionLogic(float InDeltaTime)
        {
            Console.WriteLine($"Stackonmultiple: {plate.ToString()} at {pos.ToString()} by {client.ToString()} using {vg.ToString()} in {lay.ToString()} of {mod.ToString()}");
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
