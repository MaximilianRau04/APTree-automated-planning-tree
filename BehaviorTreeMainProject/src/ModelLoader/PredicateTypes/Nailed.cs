using System;

namespace ModelLoader.PredicateTypes
{
    public class Nailed : Predicate
    {
        public Element myObject { get; set; }

        public Nailed(Element myObject, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("nailed");
            this.myObject = myObject;
        }
    }
}
