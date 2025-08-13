using System;

namespace ModelLoader.PredicateTypes
{
    public class AtAgent : Predicate
    {
        public Agent agent { get; set; }
        public Location location { get; set; }

        public AtAgent(Agent agent, Location location)
        {
            this.agent = agent;
            this.location = location;
        }
    }
}
