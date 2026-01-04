using System;
using System.Threading.Tasks;
using BehaviorTreeMainProject.Services;
using BehaviorTreeMainProject.Log.Services;
using BehaviorTreeMainProject.Tests;

namespace BehaviorTreeMainProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Clear the debug log file at the start
            SubtreeInjectionService.ClearLogFile();

            // test the full tree
            //await FullTreeTest.RunTest();
            //test the graph and predicates
            await Neo4jPredicateGraphTest.RunAsync();
        }
    }
}







