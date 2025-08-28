using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BehaviorTreeMainProject.Services
{
    public static class LoggingService
    {
        private static string logFileName;
        private static bool enableConsole;
        private static bool enableFile;
        private static StreamWriter fileWriter;
        
        // Node tracking statistics
        private static Dictionary<string, NodeExecutionInfo> nodeExecutionStats = new Dictionary<string, NodeExecutionInfo>();
        private static Dictionary<string, PlanningServiceInfo> planningServiceStats = new Dictionary<string, PlanningServiceInfo>();
        private static int totalNodes = 0;
        private static int flowNodeCount = 0;
        private static int actionNodeCount = 0;

        public static void Initialize(string serviceName, bool enableConsole = true, bool enableFile = true)
        {
            LoggingService.enableConsole = enableConsole;
            LoggingService.enableFile = enableFile;

            if (enableFile)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                logFileName = $"logs/{serviceName}_{timestamp}.log";
                
                // Ensure logs directory exists
                Directory.CreateDirectory("logs");
                
                fileWriter = new StreamWriter(logFileName, true);
                fileWriter.AutoFlush = true;
            }
        }

        public static void LogDebug(string message)
        {
            WriteLog("🔍 DEBUG", message, ConsoleColor.Gray);
        }

        public static void LogInfo(string message)
        {
            WriteLog("ℹ️ INFO", message, ConsoleColor.White);
        }

        public static void LogSuccess(string message)
        {
            WriteLog("✅ SUCCESS", message, ConsoleColor.Green);
        }

        public static void LogWarning(string message)
        {
            WriteLog("⚠️ WARNING", message, ConsoleColor.Yellow);
        }

        public static void LogError(string message)
        {
            WriteLog("❌ ERROR", message, ConsoleColor.Red);
        }

        public static void LogSection(string message)
        {
            WriteLog("================================================================================", message, ConsoleColor.Cyan);
        }

        public static void LogSubsection(string message)
        {
            WriteLog("----------------------------------------", message, ConsoleColor.Magenta);
        }

        private static void WriteLog(string prefix, string message, ConsoleColor color)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] {prefix}: {message}";

            if (enableConsole)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(logMessage);
                Console.ResetColor();
            }

            if (enableFile && fileWriter != null)
            {
                fileWriter.WriteLine(logMessage);
            }
        }

        // Node tracking methods
        public static void TrackNodeStart(string nodeName, string nodeType, DateTime startTime)
        {
            if (!nodeExecutionStats.ContainsKey(nodeName))
            {
                nodeExecutionStats[nodeName] = new NodeExecutionInfo
                {
                    NodeName = nodeName,
                    NodeType = nodeType,
                    StartTime = startTime
                };

                totalNodes++;
                if (nodeType.Contains("FlowNode"))
                {
                    flowNodeCount++;
                }
                else if (nodeType.Contains("GenericBTAction"))
                {
                    actionNodeCount++;
                }
            }
        }

        public static void TrackNodeCompletion(string nodeName, DateTime endTime, bool success)
        {
            if (nodeExecutionStats.ContainsKey(nodeName))
            {
                var nodeInfo = nodeExecutionStats[nodeName];
                nodeInfo.EndTime = endTime;
                nodeInfo.CompletionTime = endTime - nodeInfo.StartTime;
                nodeInfo.Success = success;
                nodeInfo.Completed = true;
            }
        }

        public static void TrackPlanningService(string serviceName, string plannerType, DateTime startTime, bool success, int actionsGenerated, DateTime? endTime = null)
        {
            var planningInfo = new PlanningServiceInfo
            {
                ServiceName = serviceName,
                PlannerType = plannerType,
                StartTime = startTime,
                EndTime = endTime,
                Success = success,
                ActionsGenerated = actionsGenerated,
                Completed = endTime.HasValue
            };

            if (endTime.HasValue)
            {
                planningInfo.PlanningTime = endTime.Value - startTime;
            }

            planningServiceStats[serviceName] = planningInfo;
        }

        public static void GenerateSummaryTable()
        {
            LogSection("📊 EXECUTION SUMMARY REPORT");
            
            // Node Statistics
            LogSubsection("NODE STATISTICS");
            LogInfo($"Total Nodes: {totalNodes}");
            LogInfo($"Flow Nodes: {flowNodeCount}");
            LogInfo($"Action Nodes: {actionNodeCount}");
            LogInfo("");

            // Node Execution Details
            LogSubsection("NODE EXECUTION DETAILS");
            LogInfo("Node Name | Type | Duration | Status");
            LogInfo("----------|------|----------|--------");
            
            foreach (var nodeInfo in nodeExecutionStats.Values)
            {
                string duration = nodeInfo.Completed ? 
                    $"{nodeInfo.CompletionTime.TotalMilliseconds:F1}ms" : "N/A";
                string status = nodeInfo.Completed ? 
                    (nodeInfo.Success ? "✅ Success" : "❌ Failed") : "⏳ Running";
                
                LogInfo($"{nodeInfo.NodeName,-20} | {nodeInfo.NodeType,-15} | {duration,-9} | {status}");
            }
            LogInfo("");

            // Planning Service Details
            LogSubsection("PLANNING SERVICE DETAILS");
            LogInfo("Service Name | Planner Type | Duration | Success | Actions Generated");
            LogInfo("-------------|--------------|----------|---------|-------------------");
            
            foreach (var planningInfo in planningServiceStats.Values)
            {
                string duration = planningInfo.Completed ? 
                    $"{planningInfo.PlanningTime.TotalMilliseconds:F1}ms" : "N/A";
                string success = planningInfo.Success ? "✅ Yes" : "❌ No";
                
                LogInfo($"{planningInfo.ServiceName,-13} | {planningInfo.PlannerType,-12} | {duration,-9} | {success,-7} | {planningInfo.ActionsGenerated}");
            }
            LogInfo("");

            // Summary Statistics
            LogSubsection("SUMMARY STATISTICS");
            
            int completedNodes = nodeExecutionStats.Values.Count(n => n.Completed);
            int successfulNodes = nodeExecutionStats.Values.Count(n => n.Completed && n.Success);
            int failedNodes = nodeExecutionStats.Values.Count(n => n.Completed && !n.Success);
            
            var totalExecutionTime = nodeExecutionStats.Values
                .Where(n => n.Completed)
                .Sum(n => n.CompletionTime.TotalMilliseconds);
            
            var totalPlanningTime = planningServiceStats.Values
                .Where(p => p.Completed)
                .Sum(p => p.PlanningTime.TotalMilliseconds);
            
            int totalActionsGenerated = planningServiceStats.Values.Sum(p => p.ActionsGenerated);
            int successfulPlanners = planningServiceStats.Values.Count(p => p.Success);
            int totalPlanners = planningServiceStats.Count;

            LogInfo($"Node Completion Rate: {completedNodes}/{totalNodes} ({((double)completedNodes/totalNodes*100):F1}%)");
            LogInfo($"Node Success Rate: {successfulNodes}/{completedNodes} ({((double)successfulNodes/completedNodes*100):F1}%)");
            LogInfo($"Total Execution Time: {totalExecutionTime:F1}ms");
            LogInfo($"Total Planning Time: {totalPlanningTime:F1}ms");
            LogInfo($"Total Actions Generated: {totalActionsGenerated}");
            LogInfo($"Planning Success Rate: {successfulPlanners}/{totalPlanners} ({((double)successfulPlanners/totalPlanners*100):F1}%)");
            
            LogSection("END OF SUMMARY REPORT");
        }

        public static void Close()
        {
            if (fileWriter != null)
            {
                fileWriter.Close();
                fileWriter.Dispose();
            }
        }

        // Helper classes for tracking
        private class NodeExecutionInfo
        {
            public string NodeName { get; set; }
            public string NodeType { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public TimeSpan CompletionTime { get; set; }
            public bool Success { get; set; }
            public bool Completed { get; set; }
        }

        private class PlanningServiceInfo
        {
            public string ServiceName { get; set; }
            public string PlannerType { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public TimeSpan PlanningTime { get; set; }
            public bool Success { get; set; }
            public int ActionsGenerated { get; set; }
            public bool Completed { get; set; }
        }
    }
}
