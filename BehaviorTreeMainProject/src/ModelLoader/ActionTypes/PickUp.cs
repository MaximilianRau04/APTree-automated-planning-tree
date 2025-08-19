using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class PickUp : GenericBTAction
    {
        // Parameter: pickedObject of type element
        public Element pickedObject { get; private set; }

        // Parameter: rob of type robot
        public Robot rob { get; private set; }

        // Parameter: loc of type location
        public Location loc { get; private set; }

        // Parameter: robTool of type vacuumGripper
        public VacuumGripper robTool { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public PickUp(string actionType, string instanceName, Blackboard<FastName> blackboard, Element pickedObject, Robot rob, Location loc, VacuumGripper robTool)
            : base(actionType, instanceName, blackboard)
        {
            this.pickedObject = pickedObject;
            this.rob = rob;
            this.loc = loc;
            this.robTool = robTool;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("pickUp_preconditions"));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("pickUp_effects"));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool ExecuteActionLogic(float InDeltaTime)
        {
            Console.WriteLine($"PickUp: {pickedObject.ToString()} is being picked up by {rob.ToString()} at {loc.ToString()} using {robTool.ToString()}");
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
