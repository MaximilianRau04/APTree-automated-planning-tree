using System;

namespace ModelLoader.PredicateTypes
{
    public class Glued : Predicate
    {
        public Element myObject { get; set; }

        public Glued(Element myObject, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("glued");
            this.myObject = myObject;
        }
    }
}
