public class Obj
{
    public string Name { get; private set; }
    public string Type { get; private set; }
    public bool CanBePickedUp { get; private set; }

    public Obj(string name, string type, bool canBePickedUp = true)
    {
        Name = name;
        Type = type;
        CanBePickedUp = canBePickedUp;
    }

    public override string ToString()
    {
        return $"{Type}:{Name}";
    }
}