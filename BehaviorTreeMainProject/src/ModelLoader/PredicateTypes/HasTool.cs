using System;

namespace ModelLoader.PredicateTypes
{
    public class HasTool : Predicate
    {
        public Agent agent { get; set; }
        public Tool tool { get; set; }

        public HasTool(Agent agent, Tool tool)
        {
            this.agent = agent;
            this.tool = tool;
        }
    }
}
