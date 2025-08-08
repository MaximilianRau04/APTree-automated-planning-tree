using System;

namespace ModelLoader.PredicateTypes
{
    public class HasTool : Predicate
    {
        public String agent { get; set; }
        public String tool { get; set; }

        public HasTool(String agent, String tool)
        {
            this.agent = agent;
            this.tool = tool;
        }
    }
}
