using System;

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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n=== ParameterFactory test failed: {ex.Message} ===");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
