using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

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

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public PickUp(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam pickedObject, Robot rob, Firstposition loc, VacuumGripper robTool)
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

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for PickUp
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
