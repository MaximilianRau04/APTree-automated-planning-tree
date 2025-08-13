using System;

namespace ModelLoader.PredicateTypes
{
    public class Belongstomodule : Predicate
    {
        public Element myObject { get; set; }
        public Module mod { get; set; }

        public Belongstomodule(Element myObject, Module mod)
        {
            this.myObject = myObject;
            this.mod = mod;
        }
    }
}
