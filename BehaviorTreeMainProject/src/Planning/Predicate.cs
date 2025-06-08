using System;
using System.Collections.Generic;

/// <summary>
/// the way to define a predicate is explained here.
/// for defining a predicate one needs to input a name and some input object types. for example to build the predicate at(r1 -robot, l1 -location) , we would need a name and the types robot and location. then multiple instances of this predicate can be built with different objects.
/// </summary>
public abstract class Predicate
{
    public bool isNegated { get; set; }
    public FastName PredicateName { get; protected set; }
    
    // The type of the predicate (e.g., "IsAtLocation", "IsHolding", etc.)
    public virtual FastName PredicateType => PredicateName;
    
    // Abstract method that all predicates must implement
    public abstract bool Evaluate(Blackboard<FastName> blackboard);
    
    // Method to negate the predicate
    public void Negate()
    {
        isNegated = !isNegated;
    }

    // Override ToString for better debugging
    public override string ToString()
    {
        return $"{(isNegated ? "NOT " : "")}{PredicateType}";
    }

    // Add this method to expose parameters
   // public abstract Dictionary<string, IThings> GetParameters();

    public Predicate()
    {
        // Default constructor
    }

    public Dictionary<string, object> GetAllProperties()
    {
        var properties = new Dictionary<string, object>();
        //get all properties of the predicate
        var propertyInfos = this.GetType().GetProperties();
        
        foreach (var prop in propertyInfos)
        {
            properties[prop.Name] = prop.GetValue(this);
        }
        
        return properties;
    }

    public Predicate Clone()
    {
        return (Predicate)this.MemberwiseClone();
    }
}
