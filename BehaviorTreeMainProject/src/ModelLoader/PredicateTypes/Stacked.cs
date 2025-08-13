using System;

namespace ModelLoader.PredicateTypes
{
    public class Stacked : Predicate
    {
        public Element myObject { get; set; }

        public Stacked(Element myObject)
        {
            this.myObject = myObject;
        }
    }
}
