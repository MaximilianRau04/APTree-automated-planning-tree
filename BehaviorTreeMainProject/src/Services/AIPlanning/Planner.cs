public class Planner
{
    public FastName Name { get; private set; }
    public string Constraints { get; private set; }
    public string Requirements { get; private set; }

    public Planner(string name, string constraints, string requirements)
    {
        Name = new FastName(name);
        Constraints = constraints;
        Requirements = requirements;
    }

    public override string ToString()
    {
        return $"Planner: {Name}\nConstraints: {Constraints}\nRequirements: {Requirements}";
    }
}