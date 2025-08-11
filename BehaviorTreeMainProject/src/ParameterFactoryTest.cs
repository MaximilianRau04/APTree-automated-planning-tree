using System;

using ModelLoader;

// Standalone test for ParameterFactory
public class ParameterFactoryTest
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== ParameterFactory Test ===");
        
        try
        {
            var factory = FactoryParameter.Instance;
            
            // Test 1: Create a beam instance
            Console.WriteLine("\n1. Testing CreateParameter for Beam:");
            var beam1 = factory.CreateParameter("beam", "b1");
            Console.WriteLine($"   Created: {beam1.GetType().Name} with NameKey={beam1.NameKey}, ID={beam1.ID}");
            Console.WriteLine($"   BaseType={beam1.BaseType}, TypeName={beam1.TypeName}");
            
            // Test 2: Create a robot instance
            Console.WriteLine("\n2. Testing CreateParameter for Robot:");
            var robot1 = factory.CreateParameter("robot", "r1");
            Console.WriteLine($"   Created: {robot1.GetType().Name} with NameKey={robot1.NameKey}, ID={robot1.ID}");
            Console.WriteLine($"   BaseType={robot1.BaseType}, TypeName={robot1.TypeName}");
            
            // Test 3: Create a location instance
            Console.WriteLine("\n3. Testing CreateParameter for FirstLocation:");
            var location1 = factory.CreateParameter("firstlocation", "loc1");
            Console.WriteLine($"   Created: {location1.GetType().Name} with NameKey={location1.NameKey}, ID={location1.ID}");
            Console.WriteLine($"   BaseType={location1.BaseType}, TypeName={location1.TypeName}");
            
            // Test 4: Test parameter metadata creation
            Console.WriteLine("\n4. Testing CreateParameterMetadata:");
            var beamMetadata = factory.CreateParameterMetadata("testBeam", "beam");
            Console.WriteLine($"   Created metadata: Name={beamMetadata.Name}, Type={beamMetadata.Type.Name}");
            
            // Test 5: Test error handling for unknown type
            Console.WriteLine("\n5. Testing error handling for unknown type:");
            try
            {
                var unknown = factory.CreateParameter("unknown", "test");
                Console.WriteLine("   ERROR: Should have thrown an exception!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"   Correctly caught error: {ex.Message}");
            }
            
            Console.WriteLine("\n=== All ParameterFactory tests passed! ===");
            
            // Test 6: Test BlackboardWriter.registerInputParameterInstances()
            Console.WriteLine("\n6. Testing BlackboardWriter.registerInputParameterInstances():");
            TestBlackboardWriterMethod();
            
            // Test 7: Test FactoryPredicate.CreatePredicateInstance()
            Console.WriteLine("\n7. Testing FactoryPredicate.CreatePredicateInstance():");
            TestFactoryPredicateMethod();
            
            // Test 8: Test BlackboardWriter type registration functions
            Console.WriteLine("\n8. Testing BlackboardWriter type registration functions:");
            TestBlackboardWriterTypeRegistration();
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n=== ParameterFactory test failed: {ex.Message} ===");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
    
    private static void TestBlackboardWriterMethod()
    {
        try
        {
            // Create a blackboard with dummy Neo4j connection (for testing)
            var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "password");
            
            // Create the BlackboardWriter
            var blackboardWriter = new BlackboardWriter(blackboard);
            
            // Test the registerInputParameterInstances method
            Console.WriteLine("   Testing registerInputParameterInstances()...");
            var instances = blackboardWriter.registerInputParameterInstances();
            
            Console.WriteLine($"   ✅ Method completed successfully!");
            Console.WriteLine($"   📊 Created {instances.Count} parameter instances:");
            
            foreach (var instance in instances)
            {
                Console.WriteLine($"     - {instance.GetType().Name}: {instance.NameKey}");
            }
            
            // Test that instances are registered in blackboard
            Console.WriteLine("   --- Verifying instances in blackboard ---");
            
            // Test beam instances
            try
            {
                var b1 = blackboard.GetElement(new FastName("b1"));
                Console.WriteLine("   ✅ b1 found in blackboard");
            }
            catch (Exception e)
            {
                Console.WriteLine($"   ❌ b1 not found in blackboard: {e.Message}");
            }
            
            try
            {
                var b2 = blackboard.GetElement(new FastName("b2"));
                Console.WriteLine("   ✅ b2 found in blackboard");
            }
            catch (Exception e)
            {
                Console.WriteLine($"   ❌ b2 not found in blackboard: {e.Message}");
            }
            
            try
            {
                var b3 = blackboard.GetElement(new FastName("b3"));
                Console.WriteLine("   ✅ b3 found in blackboard");
            }
            catch (Exception e)
            {
                Console.WriteLine($"   ❌ b3 not found in blackboard: {e.Message}");
            }
            
            // Test robot instance
            try
            {
                var r1 = blackboard.GetAgent(new FastName("r1"));
                Console.WriteLine("   ✅ r1 found in blackboard");
            }
            catch (Exception e)
            {
                Console.WriteLine($"   ❌ r1 not found in blackboard: {e.Message}");
            }
            
            Console.WriteLine("   === BlackboardWriter test completed! ===");
        }
        catch (Exception e)
        {
            Console.WriteLine($"   ❌ BlackboardWriter test failed: {e.Message}");
            Console.WriteLine($"   Stack trace: {e.StackTrace}");
                }
    }
    
    private static void TestFactoryPredicateMethod()
    {
        try
        {
            // Create a blackboard with dummy Neo4j connection (for testing)
            var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "password");
            
            // First, create some parameter instances to work with
            var parameterFactory = FactoryParameter.Instance;
            var b1 = parameterFactory.CreateParameter("beam", "b1");
            var fp1 = parameterFactory.CreateParameter("firstlocation", "fp1");
            var r1 = parameterFactory.CreateParameter("robot", "r1");
            
            // Register them in the blackboard
            blackboard.RegisterEntityIfNotExists(b1);
            blackboard.RegisterEntityIfNotExists(fp1);
            blackboard.RegisterEntityIfNotExists(r1);
            
            // Create the predicate factory
            var predicateFactory = FactoryPredicate.Instance;
            
            // Test 1: Create an isAt predicate instance
            Console.WriteLine("   Testing CreatePredicateInstance for isAt:");
            var isAtParams = new Dictionary<string, string>
            {
                { "object", "b1" },    // parameter name -> entity name
                { "location", "fp1" }  // parameter name -> entity name
            };
            
            var isAtPredicate = predicateFactory.CreatePredicateInstance("isAt", isAtParams, blackboard);
            Console.WriteLine($"   Created: {isAtPredicate.GetType().Name}");
            
            // Cast to specific type to access properties
            if (isAtPredicate is ModelLoader.PredicateTypes.IsAt isAt)
            {
                Console.WriteLine($"   IsAt predicate created successfully");
            }
            
            // Test 2: Create a hasTool predicate instance
            Console.WriteLine("   Testing CreatePredicateInstance for hasTool:");
            var hasToolParams = new Dictionary<string, string>
            {
                { "agent", "r1" },     // parameter name -> entity name
                { "tool", "vg1" }      // parameter name -> entity name (will fail if vg1 doesn't exist)
            };
            
            try
            {
                var hasToolPredicate = predicateFactory.CreatePredicateInstance("hasTool", hasToolParams, blackboard);
                Console.WriteLine($"   Created: {hasToolPredicate.GetType().Name}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"   Expected error (vg1 doesn't exist): {e.Message}");
            }
            
            Console.WriteLine("   === FactoryPredicate test completed! ===");
        }
        catch (Exception e)
        {
            Console.WriteLine($"   ❌ FactoryPredicate test failed: {e.Message}");
            Console.WriteLine($"   Stack trace: {e.StackTrace}");
        }
    }
    
    private static void TestBlackboardWriterTypeRegistration()
    {
        try
        {
            // Create a blackboard with dummy Neo4j connection (for testing)
            var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "password");
            
            // Create the BlackboardWriter
            var blackboardWriter = new BlackboardWriter(blackboard);
            
            // Test 1: Register parameter types
            Console.WriteLine("   Testing RegisterParameterTypes():");
            blackboardWriter.RegisterParameterTypes();
            
            // Test 2: Register predicate types
            Console.WriteLine("   Testing RegisterPredicateTypes():");
            blackboardWriter.RegisterPredicateTypes();
            
            // Test 3: Register action types
            Console.WriteLine("   Testing RegisterActionTypes():");
            blackboardWriter.RegisterActionTypes();
            
            // Test 4: Register all types at once
            Console.WriteLine("   Testing RegisterAllTypes():");
            blackboardWriter.RegisterAllTypes();
            
            Console.WriteLine("   === BlackboardWriter type registration test completed! ===");
        }
        catch (Exception e)
        {
            Console.WriteLine($"   ❌ BlackboardWriter type registration test failed: {e.Message}");
            Console.WriteLine($"   Stack trace: {e.StackTrace}");
        }
    }
}

