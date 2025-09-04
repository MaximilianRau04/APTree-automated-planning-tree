using System;
using System.IO;
using System.Text;
using BehaviorTreeMainProject.Log;

namespace BehaviorTreeMainProject.Log.Services
{
    /// <summary>
    /// Service to track the order of ML action node execution in a separate log file
    /// </summary>
    public class ActionExecutionLogger : BaseLogger
    {
        private static ActionExecutionLogger instance;
        private static readonly object lockObject = new object();
        private int executionCounter = 0;
        private readonly DateTime startTime;

        public static ActionExecutionLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new ActionExecutionLogger();
                        }
                    }
                }
                return instance;
            }
        }

        private ActionExecutionLogger()
        {
            startTime = DateTime.Now;
            
            // Initialize the base logger
            base.Initialize("ActionExecution", true, true);
            
            // Write header to log file
            WriteToLog("=== ML Action Execution Order Log ===");
            WriteToLog($"Started at: {startTime:yyyy-MM-dd HH:mm:ss.fff}");
            WriteToLog("Format: [Counter] [Timestamp] [ActionName] [InstanceName] [Status]");
            WriteToLog("=====================================");
        }

        /// <summary>
        /// Log the execution of an ML action node
        /// </summary>
        /// <param name="actionName">The name of the action class (e.g., "PickUpML")</param>
        /// <param name="instanceName">The instance name of the action</param>
        /// <param name="status">The execution status (Started, Completed, Failed)</param>
        /// <param name="additionalInfo">Optional additional information</param>
        public void LogActionExecution(string actionName, string instanceName, string status, string additionalInfo = "")
        {
            lock (lockObject)
            {
                executionCounter++;
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var timeSinceStart = DateTime.Now - startTime;
                
                var logEntry = $"[{executionCounter:D4}] [{timestamp}] [{actionName}] [{instanceName}] [{status}]";
                
                if (!string.IsNullOrEmpty(additionalInfo))
                {
                    logEntry += $" - {additionalInfo}";
                }
                
                logEntry += $" (+{timeSinceStart.TotalMilliseconds:F0}ms)";
                
                WriteToLog(logEntry);
            }
        }

        /// <summary>
        /// Log when an action starts executing
        /// </summary>
        public void LogActionStarted(string actionName, string instanceName, string additionalInfo = "")
        {
            LogActionExecution(actionName, instanceName, "STARTED", additionalInfo);
        }

        /// <summary>
        /// Log when an action completes successfully
        /// </summary>
        public void LogActionCompleted(string actionName, string instanceName, string additionalInfo = "")
        {
            LogActionExecution(actionName, instanceName, "COMPLETED", additionalInfo);
        }

        /// <summary>
        /// Log when an action fails
        /// </summary>
        public void LogActionFailed(string actionName, string instanceName, string additionalInfo = "")
        {
            LogActionExecution(actionName, instanceName, "FAILED", additionalInfo);
        }

        /// <summary>
        /// Log when an action is skipped or not executed
        /// </summary>
        public void LogActionSkipped(string actionName, string instanceName, string reason = "")
        {
            LogActionExecution(actionName, instanceName, "SKIPPED", reason);
        }

        /// <summary>
        /// Write a message to the log file
        /// </summary>
        private void WriteToLog(string message)
        {
            base.WriteLog(message);
        }

        /// <summary>
        /// Get the path to the current log file
        /// </summary>
        public string GetLogFilePath()
        {
            return base.GetLogFilePath();
        }

        /// <summary>
        /// Get the total number of actions logged
        /// </summary>
        public int GetExecutionCount()
        {
            return executionCounter;
        }

        /// <summary>
        /// Clear the log file and reset counter (for testing purposes)
        /// </summary>
        public void ClearLog()
        {
            lock (lockObject)
            {
                executionCounter = 0;
                base.Clear();
                
                // Recreate header
                WriteToLog("=== ML Action Execution Order Log (CLEARED) ===");
                WriteToLog($"Cleared at: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                WriteToLog("Format: [Counter] [Timestamp] [ActionName] [InstanceName] [Status]");
                WriteToLog("===============================================");
            }
        }
    }
}
