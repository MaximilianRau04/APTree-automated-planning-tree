using System;

namespace ModelLoader.ParameterTypes
{
    public class VacuumGripper : Tool
    {
        public string IsOn { get; set; }

        // Empty constructor - required by Entity
        public VacuumGripper() : base()
        {
            BaseType = new FastName("Tool");
            // TypeName is automatically set in base constructor
        }

        // Constructor with parameters
        public VacuumGripper(string isOn) : this()
        {
            this.IsOn = isOn;
        }

        // Constructor with name and parameters
        public VacuumGripper(string name, string isOn) : base(name)
        {
            this.IsOn = isOn;
            BaseType = new FastName("Tool");
            // TypeName is automatically set in base constructor
        }
    }
}
