using System;

namespace ModelLoader.PredicateTypes
{
    public class ActiveTool : Predicate
    {
        public Tool tool { get; set; }

        public ActiveTool(Tool tool, bool isNegated) : base(isNegated)
        {
            PredicateName = new FastName("activeTool");
            this.tool = tool;
        }
    }
}
