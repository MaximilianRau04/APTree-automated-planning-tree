using System;

namespace ModelLoader.PredicateTypes
{
    public class Atplace : Predicate
    {
        public Element myObject { get; set; }
        public Location place { get; set; }

        public Atplace(Element myObject, Location place)
        {
            this.myObject = myObject;
            this.place = place;
        }
    }
}
