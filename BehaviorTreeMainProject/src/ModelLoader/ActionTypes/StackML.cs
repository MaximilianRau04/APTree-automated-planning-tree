using System;
using System.Collections.Generic;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

namespace BehaviorTreeMainProject
{
    public class StackML : GenericBTAction
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

        public StackML(string actionType, string instanceName, Blackboard<FastName> blackboard, Beam obj1, Beam obj2, Robot client, VacuumGripper vg, PositionOnRail pr, Stack lay, Cassette mod)
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
            preconditions = new State(StateType.Precondition, new FastName("stackML_preconditions"));
            preconditions.AddPredicate(new FastName("stackML_pre_0"), new Vgempty(client, true));
            preconditions.AddPredicate(new FastName("stackML_pre_1"), new Holding(client, obj1, false));
            preconditions.AddPredicate(new FastName("stackML_pre_2"), new AtAgent(client, pr, false));
            preconditions.AddPredicate(new FastName("stackML_pre_3"), new ActiveTool(vg, false));
            preconditions.AddPredicate(new FastName("stackML_pre_4"), new Clear(obj2, false));
            preconditions.AddPredicate(new FastName("stackML_pre_5"), new Atplace(obj2, pr, false));

            // Initialize effects
            effects = new State(StateType.Effect, new FastName("stackML_effects"));
            effects.AddPredicate(new FastName("stackML_eff_0"), new Ontop(obj1, obj2, false));
            effects.AddPredicate(new FastName("stackML_eff_1"), new Holding(client, obj1, true));
            effects.AddPredicate(new FastName("stackML_eff_2"), new Atplace(obj1, pr, false));
            effects.AddPredicate(new FastName("stackML_eff_3"), new Vgempty(client, false));
            effects.AddPredicate(new FastName("stackML_eff_4"), new Clear(obj2, true));
            effects.AddPredicate(new FastName("stackML_eff_5"), new Clear(obj1, false));
        }

        protected override State Preconditions => preconditions;
        protected override State Effects => effects;

        protected override bool ExecuteActionLogic(float InDeltaTime)
        {
            // TODO: Implement action logic for StackML
            // Access parameters via properties: obj, rob, loc, tool, etc.
            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);
        }
    }
}
