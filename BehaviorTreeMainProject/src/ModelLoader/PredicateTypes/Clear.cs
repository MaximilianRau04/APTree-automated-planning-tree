using System;

namespace ModelLoader.PredicateTypes
{
    public class Clear : Predicate
    {
        public Element myObject { get; set; }

        public Clear(Element myObject, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("clear");
            this.myObject = myObject;
        }
    }
}
