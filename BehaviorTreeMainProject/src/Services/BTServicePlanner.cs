using System;
using System.Threading.Tasks;
using PlanningDataStructures;
using AIPlanning;
using System.Collections.Generic;

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
        // Start execution tracking
        StartTime = DateTime.Now;
        IsExecuting = true;
        HasCompleted = false;
        
        Console.WriteLine($"🚀 {GetType().Name}: Starting planning process at {StartTime:HH:mm:ss.fff}");
        
        try
        {
            // Step 2: Send to external planner via communicator
            var result = Task.Run(async () => await plannerCommunicator.SendPlanningRequestAsync(planningRequest)).Result;
            
            if (!result.Success)
            {
                EndTime = DateTime.Now;
                IsExecuting = false;
                Console.WriteLine($"⚠️ {GetType().Name}: Planning failed at {EndTime:HH:mm:ss.fff} - {result.Error}");
                Console.WriteLine($"⏱️ {GetType().Name}: Execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
                return false;
            }
            
            // Step 3: Generate NodeGraph from planner result (implemented by each planner type)
            generatedNodeGraph = GenerateNodeGraphFromResult(result);
            
            if (generatedNodeGraph == null)
            {
                EndTime = DateTime.Now;
                IsExecuting = false;
                Console.WriteLine($"⚠️ {GetType().Name}: Failed to generate NodeGraph at {EndTime:HH:mm:ss.fff}");
                Console.WriteLine($"⏱️ {GetType().Name}: Execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
                return false;
            }
            
            // Step 4: Store in blackboard
            StoreNodeGraphInBlackboard();
            
            // Complete execution tracking
            EndTime = DateTime.Now;
            IsExecuting = false;
            HasCompleted = true;
            
            Console.WriteLine($"✅ {GetType().Name}: Planning process completed successfully at {EndTime:HH:mm:ss.fff}");
            Console.WriteLine($"⏱️ {GetType().Name}: Total execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
            Console.WriteLine($"📊 {GetType().Name}: Generated {generatedNodeGraph.GetAllActionNodes().Count} actions");
            
            return true;
        }
        catch (Exception ex)
        {
            EndTime = DateTime.Now;
            IsExecuting = false;
            Console.WriteLine($"❌ {GetType().Name}: Error during planning process at {EndTime:HH:mm:ss.fff}: {ex.Message}");
            Console.WriteLine($"⏱️ {GetType().Name}: Execution time: {EndTime - StartTime:hh\\:mm\\:ss\\.fff}");
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
            Console.WriteLine("⚠️ BTServicePlanner: No NodeGraph to store in blackboard");
            return;
        }
        
        try
        {
            // Generate a unique name for the NodeGraph
            string nodeGraphName = $"GeneratedPlan_{GetType().Name}_{DateTime.Now:yyyyMMdd_HHmmss}";
            var nodeGraphKey = new FastName(nodeGraphName);
            
            // Store in blackboard
            LinkedBlackboard.SetNodeGraph(nodeGraphKey, generatedNodeGraph);
            
            Console.WriteLine($"✅ BTServicePlanner: Stored NodeGraph '{nodeGraphName}' in blackboard");
            Console.WriteLine($"   📊 NodeGraph contains {generatedNodeGraph.GetAllActionNodes().Count} actions");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ BTServicePlanner: Error storing NodeGraph in blackboard: {ex.Message}");
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
        return generatedNodeGraph != null;
    }
    
    /// <summary>
    /// Clear the generated NodeGraph (useful for resetting the planner)
    /// </summary>
    public void ClearGeneratedNodeGraph()
    {
        generatedNodeGraph = null;
        Console.WriteLine("🔄 BTServicePlanner: Cleared generated NodeGraph");
    }
}