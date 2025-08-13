using System;

namespace ModelLoader.PredicateTypes
{
    public class Positionfree : Predicate
    {
        public Location pos { get; set; }

        public Positionfree(Location pos, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("positionfree");
            this.pos = pos;
        }
    }
}
