using System;

namespace ModelLoader.PredicateTypes
{
    public class Belongstolayer : Predicate
    {
        public Element myObject { get; set; }
        public Layer lay { get; set; }

        public Belongstolayer(Element myObject, Layer lay)
        {
            this.myObject = myObject;
            this.lay = lay;
        }
    }
}
