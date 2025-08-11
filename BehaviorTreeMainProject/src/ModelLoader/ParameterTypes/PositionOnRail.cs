using System;

namespace ModelLoader.ParameterTypes
{
    public class PositionOnRail : Location
    {
        public double Position { get; set; }
        public string Direction { get; set; }

        // Empty constructor - required by Entity
        public PositionOnRail() : base()
        {
            BaseType = new FastName("Location");
            // TypeName is automatically set in base constructor
        }

        // Constructor with parameters
        public PositionOnRail(double position, string direction) : this()
        {
            this.Position = position;
            this.Direction = direction;
        }

        // Constructor with name and parameters
        public PositionOnRail(string name, double position, string direction) : base(name)
        {
            this.Position = position;
            this.Direction = direction;
            BaseType = new FastName("Location");
            // TypeName is automatically set in base constructor
        }
    }
}
