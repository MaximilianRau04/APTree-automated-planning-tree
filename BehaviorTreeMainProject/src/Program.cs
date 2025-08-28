using System;
using System.Threading.Tasks;
using BehaviorTreeMainProject.Services;

namespace BehaviorTreeMainProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Initialize logging service
            LoggingService.Initialize("MainProgram", enableConsole: true, enableFile: true);

            // Clear the debug log file at the start
            SubtreeInjectionService.ClearLogFile();

            // test the full tree
            LoggingService.LogSection("=== TESTING FULL BEHAVIOR TREE ===");
            await FullTreeTest.RunTest();
            
            // Close logging service
            LoggingService.Close();
        }
    }
}







