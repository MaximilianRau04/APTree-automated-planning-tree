using System;

namespace ModelLoader.PredicateTypes
{
    public class Atplace : Predicate
    {
        public Element myObject { get; set; }
        public Location place { get; set; }


        public Atplace(Element myObject, Location place, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("atplace");
            this.myObject = myObject;
            this.place = place;
        }
    }
}
