using System;

namespace ModelLoader.ActionTypes
{
    public class PickUp : BTActionNodeBase
    {
        public Element Obj { get; set; }
        public Agent Rob { get; set; }
        public Location Loc { get; set; }

        public PickUp(Element obj, Agent rob, Location loc)
        {
            this.Obj = obj;
            this.Rob = rob;
            this.Loc = loc;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            // Call implementation function: pickupImplementation
            return pickupImplementation();
        }
    }
}
