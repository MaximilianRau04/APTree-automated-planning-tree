using System;

public class Layer : IEntity  
{
	public FastName NameKey { get; set; }
	public DateTime LastModified { get; set; }
	public string ID { get; set; }
	
	/// <summary>
	/// Each element should have a nameid. the name id should comply with PDDL naming conventions
	/// </summary>

	private Module module;

	public Layer(string InName)
	{
		NameKey = new FastName(InName);
	}

}
