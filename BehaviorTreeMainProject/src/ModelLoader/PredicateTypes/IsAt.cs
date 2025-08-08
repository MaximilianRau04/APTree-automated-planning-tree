using System;

namespace ModelLoader.PredicateTypes
{
    public class IsAt : Predicate
    {
        public String agent { get; set; }
        public String location { get; set; }

        public IsAt(String agent, String location)
        {
            this.agent = agent;
            this.location = location;
        }
    }
}
