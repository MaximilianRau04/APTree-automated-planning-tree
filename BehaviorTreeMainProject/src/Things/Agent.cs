public class Agent: IEntity
{
    public FastName NameKey { get; set; }
    public DateTime LastModified { get; set; }
    public string ID { get; set; }
    public Agent(string InName){
        NameKey = new FastName(InName);
    }
}