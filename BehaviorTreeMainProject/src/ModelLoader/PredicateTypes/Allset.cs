using System;

namespace ModelLoader.PredicateTypes
{
    public class Allset : Predicate
    {
        public Layer lay { get; set; }
        public Module mod { get; set; }

        public Allset(Layer lay, Module mod, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("allset");
            this.lay = lay;
            this.mod = mod;
        }
    }
}
