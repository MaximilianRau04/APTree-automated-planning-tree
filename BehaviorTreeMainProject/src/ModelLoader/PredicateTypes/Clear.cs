using System;

namespace ModelLoader.PredicateTypes
{
    public class Clear : Predicate
    {
        public Element myObject { get; set; }

        public Clear(Element myObject)
        {
            this.myObject = myObject;
        }
    }
}
