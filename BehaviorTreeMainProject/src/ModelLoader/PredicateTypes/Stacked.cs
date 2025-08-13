using System;

namespace ModelLoader.PredicateTypes
{
    public class Stacked : Predicate
    {
        public Element myObject { get; set; }

        public Stacked(Element myObject, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("stacked");
            this.myObject = myObject;
        }
    }
}
