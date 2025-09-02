using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq; // Added for OrderByDescending

namespace BehaviorTreeMainProject.Services
{
    /// <summary>
    /// Dedicated logger for tracking execution flow of nodes, services, and decorators
    /// Provides a clean, focused view of what's being ticked during behavior tree execution
    /// </summary>
    public static class ExecutionFlowLogger
    {
        private static string logFilePath;
        private static bool enableConsole;
        private static bool enableFile;
        private static StreamWriter fileWriter;
        private static readonly object logLock = new object();
        private static int tickCounter = 0;
        private static DateTime sessionStartTime;
        private static bool isInitialized = false;

        // Statistics tracking
        private static Dictionary<string, int> nodeTickCounts = new Dictionary<string, int>();
        private static Dictionary<string, int> serviceTickCounts = new Dictionary<string, int>();
        private static Dictionary<string, int> decoratorTickCounts = new Dictionary<string, int>();

        /// <summary>
        /// Initialize the execution flow logger
        /// </summary>
        /// <param name="serviceName">Name for the log file</param>
        /// <param name="enableConsole">Whether to output to console</param>
        /// <param name="enableFile">Whether to write to file</param>
        public static void Initialize(string serviceName, bool enableConsole = true, bool enableFile = true)
        {
            if (isInitialized) return;

            sessionStartTime = DateTime.Now;
            ExecutionFlowLogger.enableConsole = enableConsole;
            ExecutionFlowLogger.enableFile = enableFile;

            if (enableFile)
            {
                // Create logs directory if it doesn't exist
                string logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logFilePath = Path.Combine(logsDirectory, $"ExecutionFlow_{serviceName}_{timestamp}.log");
                
                try
                {
                    fileWriter = new StreamWriter(logFilePath, false, Encoding.UTF8);
                    fileWriter.AutoFlush = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to create execution flow log file: {ex.Message}");
                    enableFile = false;
                }
            }

            isInitialized = true;
            LogHeader($"🚀 EXECUTION FLOW LOGGER INITIALIZED - {serviceName}");
            LogHeader($"📅 Session started: {sessionStartTime:yyyy-MM-dd HH:mm:ss.fff}");
            LogHeader($"📁 Log file: {(enableFile ? logFilePath : "Console only")}");
            LogHeader("=".PadRight(80, '='));
        }

        /// <summary>
        /// Log a tick event for a node
        /// </summary>
        /// <param name="nodeName">Name of the node being ticked</param>
        /// <param name="nodeType">Type of the node</param>
        /// <param name="tickPhase">Current tick phase</param>
        /// <param name="status">Current status</param>
        public static void LogNodeTick(string nodeName, string nodeType, string tickPhase, string status)
        {
            tickCounter++;
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var message = $"🔄 TICK #{tickCounter:0000} | {timestamp} | NODE: {nodeName} ({nodeType}) | PHASE: {tickPhase} | STATUS: {status}";
            
            WriteLog(message);
            TrackNodeTick(nodeName);
        }

        /// <summary>
        /// Log a tick event for a service
        /// </summary>
        /// <param name="serviceName">Name of the service being ticked</param>
        /// <param name="serviceType">Type of the service</param>
        /// <param name="nodeName">Name of the node that owns the service</param>
        /// <param name="result">Service tick result</param>
        public static void LogServiceTick(string serviceName, string serviceType, string nodeName, string result)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var message = $"🔧 SERVICE | {timestamp} | {serviceName} ({serviceType}) | OWNER: {nodeName} | RESULT: {result}";
            
            WriteLog(message);
            TrackServiceTick(serviceName);
        }

        /// <summary>
        /// Log a tick event for a decorator
        /// </summary>
        /// <param name="decoratorName">Name of the decorator being ticked</param>
        /// <param name="decoratorType">Type of the decorator</param>
        /// <param name="nodeName">Name of the node that owns the decorator</param>
        /// <param name="result">Decorator evaluation result</param>
        public static void LogDecoratorTick(string decoratorName, string decoratorType, string nodeName, string result)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var message = $"🎭 DECORATOR | {timestamp} | {decoratorName} ({decoratorType}) | OWNER: {nodeName} | RESULT: {result}";
            
            WriteLog(message);
            TrackDecoratorTick(decoratorName);
        }

        /// <summary>
        /// Log a phase transition
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="fromPhase">Previous phase</param>
        /// <param name="toPhase">New phase</param>
        public static void LogPhaseTransition(string nodeName, string fromPhase, string toPhase)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var message = $"🔄 PHASE TRANSITION | {timestamp} | {nodeName} | {fromPhase} → {toPhase}";
            
            WriteLog(message);
        }

        /// <summary>
        /// Log a planning phase event
        /// </summary>
        /// <param name="eventType">Type of planning event</param>
        /// <param name="details">Additional details</param>
        public static void LogPlanningEvent(string eventType, string details = "")
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var message = $"📋 PLANNING | {timestamp} | {eventType}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" | {details}";
            }
            
            WriteLog(message);
        }

        /// <summary>
        /// Log an execution phase event
        /// </summary>
        /// <param name="eventType">Type of execution event</param>
        /// <param name="details">Additional details</param>
        public static void LogExecutionEvent(string eventType, string details = "")
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var message = $"🚀 EXECUTION | {timestamp} | {eventType}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" | {details}";
            }
            
            WriteLog(message);
        }

        /// <summary>
        /// Log a separator line for better readability
        /// </summary>
        public static void LogSeparator()
        {
            WriteLog("-".PadRight(80, '-'));
        }

        /// <summary>
        /// Log a section header
        /// </summary>
        /// <param name="header">Header text</param>
        public static void LogHeader(string header)
        {
            WriteLog($"\n{header}");
        }

        /// <summary>
        /// Generate and log execution statistics
        /// </summary>
        public static void LogStatistics()
        {
            var sessionDuration = DateTime.Now - sessionStartTime;
            
            LogHeader("📊 EXECUTION FLOW STATISTICS");
            LogHeader($"⏱️ Session Duration: {sessionDuration:hh\\:mm\\:ss\\.fff}");
            LogHeader($"🔄 Total Ticks: {tickCounter}");
            
            LogHeader("📈 NODE TICK COUNTS:");
            foreach (var kvp in nodeTickCounts.OrderByDescending(x => x.Value))
            {
                WriteLog($"   {kvp.Key}: {kvp.Value} ticks");
            }
            
            LogHeader("🔧 SERVICE TICK COUNTS:");
            foreach (var kvp in serviceTickCounts.OrderByDescending(x => x.Value))
            {
                WriteLog($"   {kvp.Key}: {kvp.Value} ticks");
            }
            
            LogHeader("🎭 DECORATOR TICK COUNTS:");
            foreach (var kvp in decoratorTickCounts.OrderByDescending(x => x.Value))
            {
                WriteLog($"   {kvp.Key}: {kvp.Value} ticks");
            }
        }

        /// <summary>
        /// Close the logger and write final statistics
        /// </summary>
        public static void Close()
        {
            if (!isInitialized) return;

            LogStatistics();
            LogHeader("🏁 EXECUTION FLOW LOGGER CLOSED");
            
            if (fileWriter != null)
            {
                fileWriter.Close();
                fileWriter.Dispose();
                fileWriter = null;
            }
            
            isInitialized = false;
        }

        /// <summary>
        /// Get the log file path
        /// </summary>
        /// <returns>Path to the log file</returns>
        public static string GetLogFilePath()
        {
            return logFilePath;
        }

        /// <summary>
        /// Get the current tick counter
        /// </summary>
        /// <returns>Current tick count</returns>
        public static int GetTickCount()
        {
            return tickCounter;
        }

        /// <summary>
        /// Clear the log file
        /// </summary>
        public static void ClearLog()
        {
            if (fileWriter != null)
            {
                fileWriter.Close();
                fileWriter.Dispose();
            }
            
            if (enableFile && !string.IsNullOrEmpty(logFilePath))
            {
                fileWriter = new StreamWriter(logFilePath, false, Encoding.UTF8);
                fileWriter.AutoFlush = true;
            }
            
            tickCounter = 0;
            nodeTickCounts.Clear();
            serviceTickCounts.Clear();
            decoratorTickCounts.Clear();
            sessionStartTime = DateTime.Now;
        }

        #region Private Methods

        private static void WriteLog(string message)
        {
            lock (logLock)
            {
                if (enableConsole)
                {
                    Console.WriteLine(message);
                }
                
                if (enableFile && fileWriter != null)
                {
                    fileWriter.WriteLine(message);
                }
            }
        }

        private static void TrackNodeTick(string nodeName)
        {
            if (!nodeTickCounts.ContainsKey(nodeName))
                nodeTickCounts[nodeName] = 0;
            nodeTickCounts[nodeName]++;
        }

        private static void TrackServiceTick(string serviceName)
        {
            if (!serviceTickCounts.ContainsKey(serviceName))
                serviceTickCounts[serviceName] = 0;
            serviceTickCounts[serviceName]++;
        }

        private static void TrackDecoratorTick(string decoratorName)
        {
            if (!decoratorTickCounts.ContainsKey(decoratorName))
                decoratorTickCounts[decoratorName] = 0;
            decoratorTickCounts[decoratorName]++;
        }

        #endregion
    }
}
