using System;

namespace ModelLoader.ParameterTypes
{
    public class FirstLocation : Location
    {
        public double Xcoordinate { get; set; }
        public double Ycoordinate { get; set; }

        // Empty constructor - required by Entity
        public FirstLocation() : base()
        {
            BaseType = new FastName("Location");
            // TypeName is automatically set in base constructor
        }

        // Constructor with parameters
        public FirstLocation(double xcoordinate, double ycoordinate) : this()
        {
            this.Xcoordinate = xcoordinate;
            this.Ycoordinate = ycoordinate;
        }

        // Constructor with name and parameters
        public FirstLocation(string name, double xcoordinate, double ycoordinate) : base(name)
        {
            this.Xcoordinate = xcoordinate;
            this.Ycoordinate = ycoordinate;
            BaseType = new FastName("Location");
            // TypeName is automatically set in base constructor
        }
    }
}
