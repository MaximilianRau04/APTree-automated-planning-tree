using System;

namespace ModelLoader.PredicateTypes
{
    public class Belongstolayer : Predicate
    {
        public Element myObject { get; set; }
        public Layer lay { get; set; }

        public Belongstolayer(Element myObject, Layer lay, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("belongstolayer");
            this.myObject = myObject;
            this.lay = lay;
        }
    }
}
