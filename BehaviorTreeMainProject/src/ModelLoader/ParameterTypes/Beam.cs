using System;

namespace ModelLoader.ParameterTypes
{
    public class Beam : Element, IEntity
    {
        public double Length { get; set; }

        public Beam(double length)
        {
            this.Length = length;
        }
    }
}
