using System;

namespace ModelLoader.ParameterTypes
{
    public class Robot : Agent, IEntity
    {
        public string Type { get; set; }
        public double Speed { get; set; }

        public Robot(string type, double speed)
        {
            this.Type = type;
            this.Speed = speed;
        }
    }
}
