using System;

namespace ModelLoader.PredicateTypes
{
    public class Empty : Predicate
    {
        public Agent client { get; set; }

        public Empty(Agent client, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("empty");
            this.client = client;
        }
    }
}
