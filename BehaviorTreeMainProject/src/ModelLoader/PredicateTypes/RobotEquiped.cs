using System;

namespace ModelLoader.PredicateTypes
{
    public class RobotEquiped : Predicate
    {
        public Agent client { get; set; }

        public RobotEquiped(Agent client, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("robotEquiped");
            this.client = client;
        }
    }
}
