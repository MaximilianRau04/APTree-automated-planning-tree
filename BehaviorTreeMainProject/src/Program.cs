using Neo4j.Driver;
using ModelLoader;
using ModelLoader.ParameterTypes;
using ModelLoader.PredicateTypes;

public class Program
{
    // Test method for ParameterFactory functions
    public static async Task Main(string[] args)
    {
        await TestBlackboardandNeo4JConnection();
    }

    static async Task TestBlackboardandNeo4JConnection()
    {
        try
        {
            Console.WriteLine("Setting up Blackboard...");

            // setting up the blackboard
            using var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "12345678");

            // Test the connection
            Console.WriteLine("Testing Neo4j connection...");
            bool connectionSuccess = await blackboard.TestNeo4jConnection();

            if (connectionSuccess)
            {
                Console.WriteLine("✅ Neo4j connection successful!");

                // Create BlackboardWriter for type registration
                var blackboardWriter = new BlackboardWriter(blackboard);

                // Register all types
                Console.WriteLine("\n=== REGISTERING ALL TYPES ===");
                blackboardWriter.RegisterAllTypes();
                
                // Parse and register parameter instances from MontiCore grammar file
                Console.WriteLine("\n=== PARSING MONTICORE GRAMMAR FILE ===");
                string parameterInstancesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "ParameterInstances.txt");
                blackboardWriter.ParseAndRegisterMontiCoreGrammarFile(parameterInstancesPath);

                // Parse and register predicate instances from MontiCore grammar file
                Console.WriteLine("\n=== PARSING MONTICORE PREDICATE FILE ===");
                string predicateInstancesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "InputInstances", "PredicateInstances.txt");
                blackboardWriter.ParseAndRegisterMontiCorePredicateFile(predicateInstancesPath, blackboard);

                // Inspect the blackboard contents after registration
                Console.WriteLine("\n=== INSPECTING BLACKBOARD CONTENTS ===");
                InspectBlackboardContents(blackboard);
                
                
            }
            else
            {
                Console.WriteLine("❌ Neo4j connection failed!");
                Console.WriteLine("Please make sure:");
                Console.WriteLine("1. Neo4j Desktop is running");
                Console.WriteLine("2. Your database is started");
                Console.WriteLine("3. The password '12345678' is correct");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("Please check your Neo4j setup and credentials.");
        }
    }
    
    static void InspectBlackboardContents(Blackboard<FastName> blackboard)
    {
        Console.WriteLine("\n--- Available Types Lists ---");
        
        // Get available types using reflection since they're private fields
        var availableEntityTypes = GetPrivateField<List<FastName>>(blackboard, "AvailableEntityTypes");
        var availablePredicateTypes = GetPrivateField<List<FastName>>(blackboard, "AvailablePredicateTypes");
        var availableActionTypes = GetPrivateField<List<FastName>>(blackboard, "AvailableActionTypes");
        
        Console.WriteLine($"Available Entity Types ({availableEntityTypes?.Count ?? 0}):");
        if (availableEntityTypes != null)
        {
            foreach (var type in availableEntityTypes)
            {
                Console.WriteLine($"  - {type}");
            }
        }
        
        Console.WriteLine($"\nAvailable Predicate Types ({availablePredicateTypes?.Count ?? 0}):");
        if (availablePredicateTypes != null)
        {
            foreach (var type in availablePredicateTypes)
            {
                Console.WriteLine($"  - {type}");
            }
        }
        
        Console.WriteLine($"\nAvailable Action Types ({availableActionTypes?.Count ?? 0}):");
        if (availableActionTypes != null)
        {
            foreach (var type in availableActionTypes)
            {
                Console.WriteLine($"  - {type}");
            }
        }
        
        Console.WriteLine("\n--- Registered Type Dictionaries ---");
        
       
        
        Console.WriteLine("\n--- Instance Dictionaries ---");
        
        // Get instance dictionaries
        var elementValues = GetPrivateField<Dictionary<FastName, Element>>(blackboard, "ElementValues");
        var locationValues = GetPrivateField<Dictionary<FastName, Location>>(blackboard, "LocationValues");
        var agentValues = GetPrivateField<Dictionary<FastName, Agent>>(blackboard, "AgentValues");
        var toolValues = GetPrivateField<Dictionary<FastName, Tool>>(blackboard, "ToolValues");
        var layerValues = GetPrivateField<Dictionary<FastName, Layer>>(blackboard, "LayerValues");
        var moduleValues = GetPrivateField<Dictionary<FastName, Module>>(blackboard, "ModuleValues");
        
        Console.WriteLine($"Element Instances ({elementValues?.Count ?? 0}):");
        if (elementValues != null)
        {
            foreach (var kvp in elementValues)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value?.ID ?? "null"} ({kvp.Value?.GetType().Name ?? "null"})");
            }
        }
        
        Console.WriteLine($"\nLocation Instances ({locationValues?.Count ?? 0}):");
        if (locationValues != null)
        {
            foreach (var kvp in locationValues)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value?.ID ?? "null"} ({kvp.Value?.GetType().Name ?? "null"})");
            }
        }
        
        Console.WriteLine($"\nAgent Instances ({agentValues?.Count ?? 0}):");
        if (agentValues != null)
        {
            foreach (var kvp in agentValues)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value?.ID ?? "null"} ({kvp.Value?.GetType().Name ?? "null"})");
            }
        }
        
        Console.WriteLine($"\nTool Instances ({toolValues?.Count ?? 0}):");
        if (toolValues != null)
        {
            foreach (var kvp in toolValues)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value?.ID ?? "null"} ({kvp.Value?.GetType().Name ?? "null"})");
            }
        }
        
        Console.WriteLine($"\nLayer Instances ({layerValues?.Count ?? 0}):");
        if (layerValues != null)
        {
            foreach (var kvp in layerValues)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value?.ID ?? "null"} ({kvp.Value?.GetType().Name ?? "null"})");
            }
        }
        
        Console.WriteLine($"\nModule Instances ({moduleValues?.Count ?? 0}):");
        if (moduleValues != null)
        {
            foreach (var kvp in moduleValues)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value?.ID ?? "null"} ({kvp.Value?.GetType().Name ?? "null"})");
            }
        }
        
        // Get predicate instances
        var predicateValues = GetPrivateField<Dictionary<FastName, Predicate>>(blackboard, "PredicateValues");
        
        Console.WriteLine($"\nPredicate Instances ({predicateValues?.Count ?? 0}):");
        if (predicateValues != null)
        {
            foreach (var kvp in predicateValues)
            {
                Console.WriteLine($"  - {kvp.Key}: {kvp.Value.GetType().Name} (isNegated: {kvp.Value.isNegated})");
            }
        }
    }
    
    static T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (T)field.GetValue(obj) : default(T);
    }
}







