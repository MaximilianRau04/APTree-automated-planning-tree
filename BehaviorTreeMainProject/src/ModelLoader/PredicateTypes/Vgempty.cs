using System;

namespace ModelLoader.PredicateTypes
{
    public class Vgempty : Predicate
    {
        public Agent client { get; set; }

        public Vgempty(Agent client, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("vgempty");
            this.client = client;
        }
    }
}
