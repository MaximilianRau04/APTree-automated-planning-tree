using System;

namespace ModelLoader.PredicateTypes
{
    public class Allset : Predicate
    {
        public Layer lay { get; set; }
        public Module mod { get; set; }

        public Allset(Layer lay, Module mod)
        {
            this.lay = lay;
            this.mod = mod;
        }
    }
}
