using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class PlaceML : GenericBTAction
    {
        // Parameter: obj of type beam
        public Beam obj { get; private set; }

        // Parameter: pos of type firstposition
        public Firstposition pos { get; private set; }

        // Parameter: client of type robot
        public Robot client { get; private set; }

        // Parameter: vg of type vacuumGripper
        public VacuumGripper vg { get; private set; }

        // Preconditions and Effects as State objects
        private State preconditions;
        private State effects;

        public PlaceML(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj, Firstposition pos, Robot client, VacuumGripper vg)
            : base(actionType, instanceName, blackboard)
        {
            this.obj = obj;
            this.pos = pos;
            this.client = client;
            this.vg = vg;
            InitializePredicates();
        }

        private void InitializePredicates()
        {
            // Initialize preconditions
            preconditions = new State(StateType.Precondition, new FastName("placeML_preconditions"));
            preconditions.AddPredicate(new FastName("placeML_pre_0"), new Vgempty(client, true));
            preconditions.AddPredicate(new FastName("placeML_pre_1"), new Holding(client, obj, false));
            preconditions.AddPredicate(new FastName("placeML_pre_2"), new AtAgent(client, pos, false));
            preconditions.AddPredicate(new FastName("placeML_pre_3"), new ActiveTool(vg, false));
            preconditions.AddPredicate(new FastName("placeML_pre_4"), new Clear(obj, true));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("placeML_effects"));
            effects.AddPredicate(new FastName("placeML_eff_0"), new Atplace(obj, pos, false));
            effects.AddPredicate(new FastName("placeML_eff_1"), new Holding(client, obj, true));
            effects.AddPredicate(new FastName("placeML_eff_2"), new Vgempty(client, false));
            effects.AddPredicate(new FastName("placeML_eff_3"), new Clear(obj, false));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool ExecuteActionLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for PlaceML
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
