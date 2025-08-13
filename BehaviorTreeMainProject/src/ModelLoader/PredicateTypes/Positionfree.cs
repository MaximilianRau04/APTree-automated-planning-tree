using System;

namespace ModelLoader.PredicateTypes
{
    public class Positionfree : Predicate
    {
        public Location pos { get; set; }

        public Positionfree(Location pos)
        {
            this.pos = pos;
        }
    }
}
