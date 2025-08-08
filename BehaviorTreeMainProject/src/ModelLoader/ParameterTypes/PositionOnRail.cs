using System;

namespace ModelLoader.ParameterTypes
{
    public class PositionOnRail : Location, IEntity
    {
        public double Position { get; set; }
        public string Direction { get; set; }

        public PositionOnRail(double position, string direction)
        {
            this.Position = position;
            this.Direction = direction;
        }
    }
}
