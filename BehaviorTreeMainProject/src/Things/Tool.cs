using System;

public abstract class Tool : Entity
{
    public FastName NameKey { get; set; }
    public DateTime LastModified { get; set; }
    public string ID { get; set; }
    public FastName TypeName { get; set; }
    public override FastName BaseType { get; set; }

    // Empty constructor - required by Entity
    protected Tool() : base()
    {
        BaseType = new FastName("Tool");
        TypeName = new FastName("Tool");
    }

    public Tool(string InName) : base(InName)
    {
        BaseType = new FastName("Tool");
        TypeName = new FastName("Tool");
    }
}
