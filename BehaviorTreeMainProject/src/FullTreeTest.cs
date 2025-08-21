using System;
using System.Threading.Tasks;

namespace BehaviorTreeMainProject
{
    public class FullTreeTest
    {
        public async Task RunFullTreeTest()
        {
            Console.WriteLine("\n" + "=".PadRight(80, '='));
            Console.WriteLine("🌳 FULL BEHAVIOR TREE TEST");
            Console.WriteLine("=".PadRight(80, '='));

            try
            {
                // Create blackboard instance and test Neo4j connection
                using var blackboard = new Blackboard<FastName>("bolt://localhost:7687", "neo4j", "12345678");
                
                // Test Neo4j connection
                Console.WriteLine("🔍 Testing Neo4j connection...");
                bool connectionSuccess = await TestNeo4jConnection(blackboard);
                

                Console.WriteLine("\n" + "=".PadRight(80, '='));
                Console.WriteLine("🎉 FULL BEHAVIOR TREE TEST COMPLETED!");
                Console.WriteLine("=".PadRight(80, '='));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR during full tree test: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }

        // Test Neo4j connection
        private async Task<bool> TestNeo4jConnection(Blackboard<FastName> blackboard)
        {
            try
            {
                // Try to connect to Neo4j
                bool connected = await blackboard.TestNeo4jConnection();
                if (connected)
                {
                    Console.WriteLine("✅ Successfully connected to Neo4j");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Neo4j connection test failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to connect to Neo4j: {ex.Message}");
                Console.WriteLine("   Make sure Neo4j is running and accessible at bolt://localhost:7687");
                Console.WriteLine("   Check your Neo4j credentials (neo4j/12345678)");
                return false;
            }
        }

        // Public method to run the test from Program.cs
        public static async Task RunTest()
        {
            var test = new FullTreeTest();
            await test.RunFullTreeTest();
        }
    }
}
