using System;

namespace ModelLoader.PredicateTypes
{
    public class Ontop : Predicate
    {
        public Element myObject1 { get; set; }
        public Element myObject2 { get; set; }

        public Ontop(Element myObject1, Element myObject2, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("ontop");
            this.myObject1 = myObject1;
            this.myObject2 = myObject2;
        }
    }
}
