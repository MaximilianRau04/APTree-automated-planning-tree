using System;

namespace ModelLoader.PredicateTypes
{
    public class Holding : Predicate
    {
        public Agent agent { get; set; }
        public Element myObject { get; set; }

        public Holding(Agent agent, Element myObject, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("holding");
            this.agent = agent;
            this.myObject = myObject;
        }
    }
}
