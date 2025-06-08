using System.Reflection;
using System.Reflection.Emit;

public class PredicateFactory
{
    private static PredicateFactory instance;
    public Dictionary<string, Type> registeredPredicateTypes = new();

    public static PredicateFactory Instance
    {
        get
        {
            return instance ??= new PredicateFactory();
        }
    }

    // First step: Define a predicate type
    public void DefinePredicateType(string predicateName, Dictionary<string, Type> parameters)
    {
        // Create a new type builder for the predicate
        var typeBuilder = CreateTypeBuilder(predicateName, typeof(Predicate));
        
        // Add the PredicateName property initialization to constructor
        AddConstructor(typeBuilder, predicateName);
        
        foreach (var param in parameters)
        {
            AddProperty(typeBuilder, param.Key, param.Value);
        }

        AddEvaluateMethod(typeBuilder);
        AddToStringMethod(typeBuilder);

        Type newType = typeBuilder.CreateType();
        registeredPredicateTypes[predicateName] = newType;
    }

    // Second step: Create an instance of a predicate type with specific values
    public Predicate CreatePredicateInstance(, Blackboard<FastName> blackboard)
    {
        if (!registeredPredicateTypes.ContainsKey(predicateTemplate.PredicateName.ToString()))
        {
            throw new ArgumentException($"Predicate type {predicateTemplate.PredicateName.ToString()} not found. Define it first.");
        }

        Type predicateType = registeredPredicateTypes[predicateTemplate.PredicateName.ToString()];
        var instance = (Predicate)Activator.CreateInstance(predicateType);

        // Set the values using reflection
        foreach (var kvp in predicateTemplate.Parameters)
        {
            var property = predicateType.GetProperty(kvp.Key);
            if (property != null)
            {
                // Convert string ID to actual object
                var value = kvp.Value;
                if (value is string id)
                {
                    var key = new FastName(id);
                    if (property.PropertyType == typeof(Element))
                        value = blackboard.GetElement(key);
                    else if (property.PropertyType == typeof(Layer))
                        value = blackboard.GetLayer(key);
                    else if (property.PropertyType == typeof(Location))
                        value = blackboard.GetLocation(key);
                    else if (property.PropertyType == typeof(Agent))
                        value = blackboard.GetAgent(key);
                }
                
                property.SetValue(instance, value);
                Console.WriteLine($"Setting property {kvp.Key} with value: {value}");
                if (value is IEntity thing)
                {
                    Console.WriteLine($"  NameKey: {thing.NameKey}");
                }
            }
        }

        return instance;
    }

    // Add this method to PredicateFactory
    public Type GetPredicateType(string predicateName)
    {
        if (!registeredPredicateTypes.ContainsKey(predicateName))
        {
            throw new ArgumentException($"Predicate type {predicateName} not found");
        }
        return registeredPredicateTypes[predicateName];
    }

    /// <summary>
    /// Creates a new type builder for a predicate
    /// </summary>
    /// <param name="typeName"> The name of the predicate type </param>
    /// <param name="baseType"> The base type for the predicate </param>
    /// <returns> A TypeBuilder for the new predicate type </returns>

    private TypeBuilder CreateTypeBuilder(string typeName, Type baseType)
    {
        var assemblyName = new AssemblyName($"DynamicPredicates_{Guid.NewGuid()}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("PredicateModule");
        return moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class, baseType);
    }
    /// <summary>
    /// Adds a property to the predicate type
    /// </summary>
    /// <param name="typeBuilder"> The TypeBuilder for the predicate type </param>
    /// <param name="propertyName"> The name of the property </param>
    /// <param name="propertyType"> The type of the property </param>

    private void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
    {
        // Field
        var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);

        // Property
        var propertyBuilder = typeBuilder.DefineProperty(propertyName, 
            PropertyAttributes.HasDefault, 
            propertyType, 
            null);

        // Getter
        var getterBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual,
            propertyType,
            Type.EmptyTypes);

        var getterIL = getterBuilder.GetILGenerator();
        getterIL.Emit(OpCodes.Ldarg_0);
        getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getterIL.Emit(OpCodes.Ret);

        // Setter
        var setterBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual,
            null,
            new[] { propertyType });

        var setterIL = setterBuilder.GetILGenerator();
        setterIL.Emit(OpCodes.Ldarg_0);
        setterIL.Emit(OpCodes.Ldarg_1);
        setterIL.Emit(OpCodes.Stfld, fieldBuilder);
        setterIL.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getterBuilder);
        propertyBuilder.SetSetMethod(setterBuilder);
    }
    /// <summary>
    /// Adds a constructor to the predicate type
    /// </summary>
    /// <param name="typeBuilder"></param>
    /// <param name="predicateName"></param>
    /// <exception cref="InvalidOperationException"></exception>

    private void AddConstructor(TypeBuilder typeBuilder, string predicateName)
    {
        try
        {
            // Define constructor
            var constructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                Type.EmptyTypes);

            var il = constructor.GetILGenerator();

            // Call base constructor first
            il.Emit(OpCodes.Ldarg_0);  // Load "this"
            var baseConstructor = typeof(Predicate).GetConstructor(Type.EmptyTypes);
            if (baseConstructor == null)
            {
                throw new InvalidOperationException("Base Predicate constructor not found");
            }
            il.Emit(OpCodes.Call, baseConstructor);

            // Set PredicateName
            il.Emit(OpCodes.Ldarg_0);  // Load "this"
            il.Emit(OpCodes.Ldstr, predicateName);  // Load predicate name
            var setMethod = typeof(Predicate).GetProperty("PredicateName").SetMethod;
            if (setMethod == null)
            {
                throw new InvalidOperationException("PredicateName setter not found");
            }
            il.Emit(OpCodes.Call, setMethod);

            il.Emit(OpCodes.Ret);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddConstructor: {ex.Message}");
            throw;
        }
    }
/// <summary>
/// Add an evaluate method to the predicate type
/// </summary>
/// <param name="typeBuilder"></param>
    private void AddEvaluateMethod(TypeBuilder typeBuilder)
    {
        var methodBuilder = typeBuilder.DefineMethod("Evaluate",
            MethodAttributes.Public | MethodAttributes.Virtual,
            typeof(bool),
            new[] { typeof(Blackboard<FastName>) });

        var il = methodBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldc_I4_1);  // Default implementation returns true
        il.Emit(OpCodes.Ret);
    }

    private void AddToStringMethod(TypeBuilder typeBuilder)
    {
        var methodBuilder = typeBuilder.DefineMethod("ToString",
            MethodAttributes.Public | MethodAttributes.Virtual,
            typeof(string),
            Type.EmptyTypes);

        var il = methodBuilder.GetILGenerator();
        il.Emit(OpCodes.Ldstr, "Predicate: ");
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).GetMethod("ToString", Type.EmptyTypes));
        il.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
        il.Emit(OpCodes.Ret);
    }
}