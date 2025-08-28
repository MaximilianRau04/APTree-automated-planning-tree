using System;
using System.Threading.Tasks;
using PlanningDataStructures;
using AIPlanning;
using System.Collections.Generic;
using BehaviorTreeMainProject.Services;

/// <summary>
/// Base class for all planning services with enhanced tracking capabilities.
/// 
/// Usage Examples:
/// 
/// // Check if planning was successful and plan was generated
/// if (planner.HasCompleted)
/// {
///     if (planner.WasSuccessful && planner.HasPlanGenerated)
///     {
///         Console.WriteLine($"✅ Planning successful! Generated {planner.GetGeneratedNodeGraph().GetAllActionNodes().Count} actions");
///     }
///     else
///     {
///         Console.WriteLine($"❌ Planning failed: {planner.LastError}");
///     }
/// }
/// 
/// // Get planning status summary
/// string status = planner.GetPlanningStatusSummary();
/// Console.WriteLine($"Planning Status: {status}");
/// 
/// // Get detailed statistics
/// var stats = planner.GetPlanningStatistics();
/// foreach (var kvp in stats)
/// {
///     Console.WriteLine($"{kvp.Key}: {kvp.Value}");
/// }
/// </summary>
public abstract class BTServicePlanner : BTServiceBase
{
    // Store the generated NodeGraph
    protected NodeGraph generatedNodeGraph;
    
    // Property to access the generated NodeGraph
    public NodeGraph GeneratedNodeGraph => generatedNodeGraph;

    // The communicator for external planners
    protected IPlannerCommunicator plannerCommunicator;
    protected IPlanningRequest planningRequest;
    
    // Execution tracking
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsExecuting { get; private set; } = false;
    public bool HasCompleted { get; private set; } = false;
    public bool WasSuccessful { get; private set; } = false; // True if planning succeeded and plan was generated
    public bool HasPlanGenerated { get; private set; } = false; // True if NodeGraph was successfully created
    public string LastError { get; private set; } = null; // Last error message if planning failed
    public TimeSpan ExecutionDuration => HasCompleted ? EndTime - StartTime : TimeSpan.Zero;
    public string PlannerName => GetType().Name;

    protected BTServicePlanner(IBehaviorTree InOwningTree, IPlannerCommunicator communicator, IPlanningRequest InPlanningRequest)
        : base(InOwningTree)
    {
        generatedNodeGraph = null;
        plannerCommunicator = communicator ?? throw new ArgumentNullException(nameof(communicator));
        planningRequest = InPlanningRequest ?? throw new ArgumentNullException(nameof(InPlanningRequest));
    }

    public override bool Tick(float InDeltaTime)
    {
        // If planning has already completed (success or failure), don't run again
        if (HasCompleted)
        {
            if (WasSuccessful && HasPlanGenerated && generatedNodeGraph != null)
            {
                LoggingService.LogInfo($"⏭️ {GetType().Name}: Planning already completed successfully, preserving existing NodeGraph (HashCode: {generatedNodeGraph.GetHashCode()})");
                return true; // Return true to indicate success
            }
            else
            {
                LoggingService.LogInfo($"⏭️ {GetType().Name}: Planning already completed and failed, not retrying");
                return false; // Return false to indicate failure
            }
        }
        
        // If already executing, don't start again
        if (IsExecuting)
        {
            LoggingService.LogInfo($"⏳ {GetType().Name}: Planning already in progress, waiting...");
            return true; // Return true to indicate we're still working
        }
        
        // Start execution tracking
        StartTime = DateTime.Now;
        IsExecuting = true;
        
        LoggingService.LogInfo($"🚀 {GetType().Name}: Starting planning process at {StartTime:HH:mm:ss.fff}");
        
        try
        {
            // Step 2: Send to external planner via communicator
            var result = Task.Run(async () => await plannerCommunicator.SendPlanningRequestAsync(planningRequest)).Result;
            
            if (!result.Success)
            {
                EndTime = DateTime.Now;
                IsExecuting = false;
                HasCompleted = true; // Mark as completed even on failure to prevent infinite retries
                WasSuccessful = false;
                HasPlanGenerated = false;
                LastError = result.Error;
                LoggingService.LogError($"⚠️ {GetType().Name}: Planning failed at {EndTime:HH:mm:ss.fff} - {result.Error}");
                LoggingService.LogInfo($"⏱️ {GetType().Name}: Execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
                LoggingService.LogInfo($"📋 {GetType().Name}: Planning Status - Completed: {HasCompleted}, Successful: {WasSuccessful}, Plan Generated: {HasPlanGenerated}");
                LoggingService.LogWarning($"🔄 {GetType().Name}: Planning failed - this node will fail. No retries will be attempted.");
                return false;
            }
            
            // Step 3: Generate NodeGraph from planner result (implemented by each planner type)
            generatedNodeGraph = GenerateNodeGraphFromResult(result);
            
            if (generatedNodeGraph == null)
            {
                EndTime = DateTime.Now;
                IsExecuting = false;
                HasCompleted = true; // Mark as completed even on failure to prevent infinite retries
                WasSuccessful = false;
                HasPlanGenerated = false;
                LastError = "Failed to generate NodeGraph from planner result";
                LoggingService.LogError($"⚠️ {GetType().Name}: Failed to generate NodeGraph at {EndTime:HH:mm:ss.fff}");
                LoggingService.LogInfo($"⏱️ {GetType().Name}: Execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
                LoggingService.LogInfo($"📋 {GetType().Name}: Planning Status - Completed: {HasCompleted}, Successful: {WasSuccessful}, Plan Generated: {HasPlanGenerated}");
                LoggingService.LogWarning($"🔄 {GetType().Name}: NodeGraph generation failed - this node will fail. No retries will be attempted.");
                return false;
            }
            
            // Step 4: Store in blackboard
            StoreNodeGraphInBlackboard();
            
            // Complete execution tracking
            EndTime = DateTime.Now;
            IsExecuting = false;
            HasCompleted = true;
            WasSuccessful = true;
            HasPlanGenerated = true;
            LastError = null;
            
            LoggingService.LogSuccess($"✅ {GetType().Name}: Planning process completed successfully at {EndTime:HH:mm:ss.fff}");
            LoggingService.LogInfo($"⏱️ {GetType().Name}: Total execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
            LoggingService.LogInfo($"📊 {GetType().Name}: Generated {generatedNodeGraph.GetAllActionNodes().Count} actions");
            LoggingService.LogInfo($"📋 {GetType().Name}: Planning Status - Completed: {HasCompleted}, Successful: {WasSuccessful}, Plan Generated: {HasPlanGenerated}");
            
            return true;
        }
        catch (Exception ex)
        {
            EndTime = DateTime.Now;
            IsExecuting = false;
            HasCompleted = true; // Mark as completed even on failure to prevent infinite retries
            WasSuccessful = false;
            HasPlanGenerated = false;
            LastError = ex.Message;
            LoggingService.LogError($"❌ {GetType().Name}: Error during planning process at {EndTime:HH:mm:ss.fff}: {ex.Message}");
            LoggingService.LogInfo($"⏱️ {GetType().Name}: Execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
            LoggingService.LogInfo($"📋 {GetType().Name}: Planning Status - Completed: {HasCompleted}, Successful: {WasSuccessful}, Plan Generated: {HasPlanGenerated}");
            LoggingService.LogWarning($"🔄 {GetType().Name}: Planning exception occurred - this node will fail. No retries will be attempted.");
            return false;
        }
    }


    
    /// <summary>
    /// Generate NodeGraph from planner result (to be implemented by each planner type)
    /// </summary>
    /// <param name="result">Result from external planner</param>
    /// <returns>Generated NodeGraph</returns>
    protected abstract NodeGraph GenerateNodeGraphFromResult(PlanningResult result);
    

    
    /// <summary>
    /// Store the generated NodeGraph in the blackboard
    /// </summary>
    protected virtual void StoreNodeGraphInBlackboard()
    {
        if (generatedNodeGraph == null)
        {
            LoggingService.LogWarning("⚠️ BTServicePlanner: No NodeGraph to store in blackboard");
            return;
        }
        
        try
        {
            // Generate a unique name for the NodeGraph
            string nodeGraphName = $"GeneratedPlan_{GetType().Name}_{DateTime.Now:yyyyMMdd_HHmmss}";
            var nodeGraphKey = new FastName(nodeGraphName);
            
            // Store in blackboard
            LinkedBlackboard.SetNodeGraph(nodeGraphKey, generatedNodeGraph);
            
            LoggingService.LogSuccess($"✅ BTServicePlanner: Stored NodeGraph '{nodeGraphName}' in blackboard");
            LoggingService.LogInfo($"   📊 NodeGraph contains {generatedNodeGraph.GetAllActionNodes().Count} actions");
        }
        catch (Exception ex)
        {
            LoggingService.LogError($"❌ BTServicePlanner: Error storing NodeGraph in blackboard: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get the generated NodeGraph (if planning has been completed)
    /// </summary>
    /// <returns>The generated NodeGraph or null if planning hasn't been completed</returns>
    public NodeGraph GetGeneratedNodeGraph()
    {
        return generatedNodeGraph;
    }

    
    
    /// <summary>
    /// Check if planning has been completed and NodeGraph is available
    /// </summary>
    /// <returns>True if NodeGraph is available, false otherwise</returns>
    public bool HasGeneratedNodeGraph()
    {
        return generatedNodeGraph != null && HasCompleted;
    }
    
    /// <summary>
    /// Check if planning has failed
    /// </summary>
    /// <returns>True if planning has completed and failed, false otherwise</returns>
    public bool HasPlanningFailed()
    {
        return HasCompleted && !WasSuccessful;
    }
    
    /// <summary>
    /// Check if planning has succeeded
    /// </summary>
    /// <returns>True if planning has completed and succeeded, false otherwise</returns>
    public bool HasPlanningSucceeded()
    {
        return HasCompleted && WasSuccessful && HasPlanGenerated;
    }
    
    /// <summary>
    /// Get a summary of the planning status
    /// </summary>
    /// <returns>String describing the current planning status</returns>
    public string GetPlanningStatusSummary()
    {
        if (!HasCompleted)
        {
            return IsExecuting ? "Planning in progress..." : "Planning not started";
        }
        
        if (WasSuccessful && HasPlanGenerated)
        {
            int actionCount = generatedNodeGraph?.GetAllActionNodes().Count ?? 0;
            return $"Planning successful - Generated plan with {actionCount} actions";
        }
        
        if (WasSuccessful && !HasPlanGenerated)
        {
            return "Planning succeeded but no plan was generated";
        }
        
        return $"Planning failed - {LastError ?? "Unknown error"}";
    }
    
    /// <summary>
    /// Get detailed planning statistics
    /// </summary>
    /// <returns>Dictionary with planning statistics</returns>
    public Dictionary<string, object> GetPlanningStatistics()
    {
        var stats = new Dictionary<string, object>
        {
            ["PlannerName"] = PlannerName,
            ["HasCompleted"] = HasCompleted,
            ["WasSuccessful"] = WasSuccessful,
            ["HasPlanGenerated"] = HasPlanGenerated,
            ["IsExecuting"] = IsExecuting,
            ["ExecutionDuration"] = ExecutionDuration,
            ["ActionCount"] = generatedNodeGraph?.GetAllActionNodes().Count ?? 0
        };
        
        if (!string.IsNullOrEmpty(LastError))
        {
            stats["LastError"] = LastError;
        }
        
        if (HasCompleted)
        {
            stats["StartTime"] = StartTime;
            stats["EndTime"] = EndTime;
        }
        
        return stats;
    }
    
 
    
    /// <summary>
    /// Check if the planning service has successfully completed and should be preserved
    /// </summary>
    /// <returns>True if planning completed successfully and should be preserved</returns>
    public bool ShouldPreservePlanningResult()
    {
        return HasCompleted && WasSuccessful && HasPlanGenerated && generatedNodeGraph != null;
    }
    
    /// <summary>
    /// Reset the planning service state (useful when tree is reset)
    /// </summary>
    public void ResetPlanningService()
    {
        generatedNodeGraph = null;
        IsExecuting = false;
        HasCompleted = false;
        WasSuccessful = false;
        HasPlanGenerated = false;
        LastError = null;
        LoggingService.LogWarning($"🔄 {GetType().Name}: Planning service reset");
    }
}