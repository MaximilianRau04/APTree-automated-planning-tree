using System;

public abstract class Element : IEntity  {
    public FastName NameKey { get; set; }
    public DateTime LastModified { get; set; }
    public string ID { get; set; }
 

    public Element(string InName)
    {
        NameKey = new FastName(InName);
        TypeName = GetType().Name;
        BaseType = GetType().BaseType;
    }

}
