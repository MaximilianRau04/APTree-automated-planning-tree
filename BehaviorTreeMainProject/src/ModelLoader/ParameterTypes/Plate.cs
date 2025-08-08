using System;

namespace ModelLoader.ParameterTypes
{
    public class Plate : Element, IEntity
    {
        public double Thickness { get; set; }
        public string Material { get; set; }

        public Plate(double thickness, string material)
        {
            this.Thickness = thickness;
            this.Material = material;
        }
    }
}
