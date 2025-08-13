using System;

namespace ModelLoader.PredicateTypes
{
    public class Belongstomodule : Predicate
    {
        public Element myObject { get; set; }
        public Module mod { get; set; }

        public Belongstomodule(Element myObject, Module mod, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("belongstomodule");
            this.myObject = myObject;
            this.mod = mod;
        }
    }
}
