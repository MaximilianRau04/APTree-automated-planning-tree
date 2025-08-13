using System;

namespace ModelLoader.PredicateTypes
{
    public class Empty : Predicate
    {
        public Agent client { get; set; }

        public Empty(Agent client)
        {
            this.client = client;
        }
    }
}
