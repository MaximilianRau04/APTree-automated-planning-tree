using System;

namespace ModelLoader.PredicateTypes
{
    public class AtAgent : Predicate
    {
        public String agent { get; set; }
        public String location { get; set; }

        public AtAgent(String agent, String location)
        {
            this.agent = agent;
            this.location = location;
        }
    }
}
