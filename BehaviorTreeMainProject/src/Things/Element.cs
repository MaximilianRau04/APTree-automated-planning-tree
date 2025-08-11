using System;

public abstract class Element : Entity  {
    public FastName NameKey { get; set; }
    public DateTime LastModified { get; set; }
    public string ID { get; set; }
    public FastName TypeName { get; set; }
    public override FastName BaseType { get; set; }

    // Empty constructor - required by Entity
    protected Element() : base()
    {
        BaseType = new FastName("Element");
        TypeName = new FastName("Element");
    }

    // Named constructor - existing functionality
    public Element(string InName) : base(InName)
    {
        BaseType = new FastName("Element");
        TypeName = new FastName("Element");
    }
}
