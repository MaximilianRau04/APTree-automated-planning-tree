using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Collections.Generic;

public class EntityFactory : Singleton<EntityFactory>
{
     public Dictionary<string, Type> registeredEntityTypes = new();
     private Dictionary<string, Dictionary<string, Type>> typeProperties = new();
    

    // Register a new entity type with its base type and properties
    public Type RegisterEntityType(string typeName, Type baseType, Dictionary<string, Type> properties =null)
    {
        // Create new type dynamically
        var typeBuilder = CreateTypeBuilder(typeName, baseType);
        
        // if properties are provided, add them
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                AddProperty(typeBuilder, prop.Key, prop.Value);
            }
        }

        // Create and register the type
        Type newType = typeBuilder.CreateType();
        registeredEntityTypes[typeName] = newType;
        //if the properties are not null, add them to the typeProperties dictionary
        if (properties != null)
        {
            typeProperties[typeName] = properties;
        }
        return newType;
    }

    // Create instance of registered type
    public IEntity CreateInstance(string typeName, Dictionary<string, object> propertyValues = null)
    {
        if (!registeredEntityTypes.TryGetValue(typeName, out Type type))
        {
            throw new ArgumentException($"Type {typeName} not registered");
        }

        var instance = Activator.CreateInstance(type, new object[] { typeName }) as IEntity;
        
        if (propertyValues != null)
        {
            foreach (var value in propertyValues)
            {
                type.GetProperty(value.Key)?.SetValue(instance, value.Value);
            }
        }

        return instance;
    }

    public TypeBuilder CreateTypeBuilder(string typeName, Type baseType)
    {
        var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid()}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        var typeBuilder = moduleBuilder.DefineType(typeName, 
            TypeAttributes.Public | TypeAttributes.Class, 
            baseType,
            new[] { typeof(IEntity) });

        // Add constructor that calls base constructor with name parameter
        var ctor = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            new[] { typeof(string) });

        var ctorIL = ctor.GetILGenerator();
        ctorIL.Emit(OpCodes.Ldarg_0);  // Load 'this'
        ctorIL.Emit(OpCodes.Ldarg_1);  // Load name parameter
        ctorIL.Emit(OpCodes.Call, baseType.GetConstructor(new[] { typeof(string) })); // Call base constructor
        ctorIL.Emit(OpCodes.Ret);

        return typeBuilder;
    }

     private void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
    {
        // Field
        var fieldBuilder = typeBuilder.DefineField(
            $"_{propertyName}", propertyType, FieldAttributes.Private);

        // Property
        var propertyBuilder = typeBuilder.DefineProperty(
            propertyName,
            PropertyAttributes.HasDefault,
            propertyType,
            null);

        // Getter
        var getterBuilder = typeBuilder.DefineMethod(
            $"get_{propertyName}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual,
            propertyType,
            Type.EmptyTypes);

        var getterIL = getterBuilder.GetILGenerator();
        getterIL.Emit(OpCodes.Ldarg_0);
        getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
        getterIL.Emit(OpCodes.Ret);

        // Setter
        var setterBuilder = typeBuilder.DefineMethod(
            $"set_{propertyName}",
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


}



// New base classes for Layer and Module
