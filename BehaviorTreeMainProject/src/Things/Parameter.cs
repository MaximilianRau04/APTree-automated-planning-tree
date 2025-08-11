using System;

public class Parameter
{
    public FastName Name { get; }
    public Type Type { get; }

    public Parameter(string name, Type type)
    {
        Name = new FastName(name);
        Type = type;
    }
} 