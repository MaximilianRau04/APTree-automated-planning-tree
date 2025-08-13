using System;

namespace ModelLoader.PredicateTypes
{
    public class Holding : Predicate
    {
        public Agent agent { get; set; }
        public Element myObject { get; set; }

        public Holding(Agent agent, Element myObject)
        {
            this.agent = agent;
            this.myObject = myObject;
        }
    }
}
