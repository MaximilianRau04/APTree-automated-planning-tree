using System;

namespace ModelLoader.PredicateTypes
{
    public class IsAt : Predicate
    {
        public Element myObject { get; set; }
        public Location location { get; set; }

        public IsAt(Element myObject, Location location, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("isAt");
            this.myObject = myObject;
            this.location = location;
        }
    }
}
