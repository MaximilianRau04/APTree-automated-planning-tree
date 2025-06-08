using System;

public interface IEntity 
{
   public FastName NameKey { get;  set; }
   public DateTime LastModified { get;  set; }
   public string ID { get;  set; }
   public FastName TypeName { get; set; }
    public IEntity BaseType { get; set; }

}
