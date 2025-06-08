public class ValueEntity<T> : IEntity
{
    private static readonly HashSet<Type> AllowedTypes = new()
    {
        typeof(int),
        typeof(float),
        typeof(double),
        typeof(string),
        typeof(bool)
        // Add other built-in types as needed
    };

    private readonly T value;
    public FastName NameKey { get; set; }
    public T Value => value;

    private ValueEntity(T value, string name)
    {
        if (!IsAllowedType(typeof(T)))
            throw new ArgumentException($"Type {typeof(T)} is not allowed as a value entity");
            
        this.value = value;
        this.NameKey = new FastName(name);
    }

    private static bool IsAllowedType(Type type) => AllowedTypes.Contains(type);

    public static ValueEntity<T> Create(T value, string name)
    {
        return new ValueEntity<T>(value, name);
    }
}