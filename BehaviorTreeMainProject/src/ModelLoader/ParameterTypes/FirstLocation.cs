using System;

namespace ModelLoader.ParameterTypes
{
    public class FirstLocation : Location, IEntity
    {
        public double Xcoordinate { get; set; }
        public double Ycoordinate { get; set; }

        public FirstLocation(double xcoordinate, double ycoordinate)
        {
            this.Xcoordinate = xcoordinate;
            this.Ycoordinate = ycoordinate;
        }
    }
}
