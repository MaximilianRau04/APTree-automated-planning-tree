using System;

public abstract class Location : IEntity  {
    public FastName NameKey { get; set; }
    public DateTime LastModified { get; set; }
    public string ID { get; set; }

    public Location(string InName) 
    {
        NameKey = new FastName(InName);
    }

}
