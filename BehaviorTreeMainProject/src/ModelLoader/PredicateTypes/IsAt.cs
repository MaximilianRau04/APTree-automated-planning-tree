using System;

namespace ModelLoader.PredicateTypes
{
    public class IsAt : Predicate
    {
        public Element myObject { get; set; }
        public Location location { get; set; }

        public IsAt(Element myObject, Location location)
        {
            this.myObject = myObject;
            this.location = location;
        }
    }
}
