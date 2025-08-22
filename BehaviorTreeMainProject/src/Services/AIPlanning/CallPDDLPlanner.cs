using System;
using System.Collections.Generic;
using System.Linq;
using PlanningDataStructures;
using AIPlanning;

public class CallPDDLPlanner : BTServicePlanner
{
    private readonly Blackboard<FastName> blackboard;
    private readonly FactoryAction actionFactory;
    public FastName PlannerName = new FastName("PDDLPlanner");
    public PDDLPlanningRequest PlanningRequest;
    
    // Parallel execution configuration
    public enum ParallelExecutionMode
    {
        Sequential,      // All actions run sequentially (MEETS)
        Parallel,        // Actions run in parallel (OVERLAPS)
        Hybrid           // Mix of sequential and parallel
    }
    
    public ParallelExecutionMode ExecutionMode { get; set; } = ParallelExecutionMode.Parallel;

    public CallPDDLPlanner(BTInstance InOwningTree, PDDLPlanningRequest InPlanningRequest)
        : base(InOwningTree, new RestPlannerCommunicator("http://localhost:5000"), InPlanningRequest)
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
        this.PlanningRequest = InPlanningRequest;
    }
    
    public CallPDDLPlanner(BTInstance InOwningTree, IPlannerCommunicator customCommunicator, PDDLPlanningRequest InPlanningRequest) 
        : base(InOwningTree, customCommunicator, InPlanningRequest)
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
    }

   
    
    protected override NodeGraph GenerateNodeGraphFromResult(PlanningResult result)
    {
        Console.WriteLine($"🔧 CallPDDLPlanner: Converting PDDL result to NodeGraph...");
        Console.WriteLine($"📋 CallPDDLPlanner: Execution Mode: {ExecutionMode}");
        Console.WriteLine($"📋 CallPDDLPlanner: Problem File: {PlanningRequest.ProblemFile}");
        
        try
        {
            if (string.IsNullOrEmpty(result.Plan))
            {
                Console.WriteLine("⚠️ CallPDDLPlanner: No plan in planning result");
                return null;
            }
            
            // Parse the plan string and create NodeGraph
            var nodeGraph = ParsePlanStringToNodeGraph(result.Plan);
            
            Console.WriteLine($"✅ CallPDDLPlanner: Generated NodeGraph with {nodeGraph.GetAllActionNodes().Count} actions");
            Console.WriteLine($"✅ CallPDDLPlanner: Execution Mode applied: {ExecutionMode}");
            return nodeGraph;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallPDDLPlanner: Error generating NodeGraph: {ex.Message}");
            return null;
        }
    }
    

    
    private NodeGraph ParsePlanStringToNodeGraph(string planString)
    {
        Console.WriteLine($"🔧 CallPDDLPlanner: Converting PDDL plan to NodeGraph...");
        
        try
        {
            // Step 1: Parse planner output to get action instance strings
            var actionInstanceStrings = ParsePlannerOutputToActionInstances(planString);
            Console.WriteLine($"✅ CallPDDLPlanner: Parsed {actionInstanceStrings.Count} action instances");
            
            if (actionInstanceStrings.Count == 0)
            {
                Console.WriteLine("⚠️ CallPDDLPlanner: No action instances found in planner output");
                return null;
            }
            
            // Step 2: Generate relations based on execution mode
            var relationConfiguration = GetRelationConfigurationFromExecutionMode();
            var relationStrings = Parser.GenerateRelationsFromActionInstances(actionInstanceStrings, relationConfiguration);
            Console.WriteLine($"✅ CallPDDLPlanner: Generated {relationStrings.Count} relations with {relationConfiguration} configuration");
            
            // Step 3: Create NodeGraph using Parser
            var nodeGraph = Parser.ParseNodeGraph(actionInstanceStrings, relationStrings, blackboard);
            Console.WriteLine($"✅ CallPDDLPlanner: Created NodeGraph with {nodeGraph.GetAllActionNodes().Count} actions");
            
            return nodeGraph;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallPDDLPlanner: Error parsing plan string: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Parses planner output and converts it to action instance strings using the appropriate parser
    /// </summary>
    /// <param name="planString">Raw planner output string</param>
    /// <returns>List of action instance strings in MontiCore format</returns>
    private List<string> ParsePlannerOutputToActionInstances(string planString)
    {
        var plannerName = PlanningRequest.PlannerName?.ToUpper() ?? "ENHSP";
        Console.WriteLine($"🔍 CallPDDLPlanner: Using {plannerName} parser for plan conversion");
        
        List<string> actionInstanceStrings;
        
        if (plannerName == "FF")
        {
            // Use FF parser for FF planner output
            actionInstanceStrings = Parser.ParseFFOutput(planString);
        }
        else
        {
            // Use MontiCore parser for ENHSP planner output (Python service returns pre-formatted)
            actionInstanceStrings = Parser.ParseMontiCoreOutput(planString);
        }
        
        return actionInstanceStrings;
    }
    
    /// <summary>
    /// Maps execution mode to relation configuration
    /// </summary>
    /// <returns>Relation configuration for the current execution mode</returns>
    private Parser.RelationConfiguration GetRelationConfigurationFromExecutionMode()
    {
        switch (ExecutionMode)
        {
            case ParallelExecutionMode.Sequential:
                return Parser.RelationConfiguration.Sequential;
            case ParallelExecutionMode.Parallel:
                return Parser.RelationConfiguration.Parallel;
            case ParallelExecutionMode.Hybrid:
                return Parser.RelationConfiguration.Hybrid;
            default:
                return Parser.RelationConfiguration.Sequential;
        }
    }
    
    private NodeGraph CreateNodeGraphWithExecutionMode(List<GenericBTAction> actions)
    {
        var nodeGraph = new NodeGraph();
        
        // Add all actions to the NodeGraph
        foreach (var action in actions)
        {
            nodeGraph.AddNode(action);
        }
        
        if (actions.Count == 0) return nodeGraph;
        
        Console.WriteLine($"🔧 CallPDDLPlanner: Creating NodeGraph with {ExecutionMode} execution mode for {actions.Count} actions");
        
        switch (ExecutionMode)
        {
            case ParallelExecutionMode.Sequential:
                return CreateSequentialNodeGraph(actions, nodeGraph);
                
            case ParallelExecutionMode.Parallel:
                return CreateParallelNodeGraph(actions, nodeGraph);
                
            case ParallelExecutionMode.Hybrid:
                return CreateHybridNodeGraph(actions, nodeGraph);
                
            default:
                return CreateParallelNodeGraph(actions, nodeGraph);
        }
    }
    
    private NodeGraph CreateSequentialNodeGraph(List<GenericBTAction> actions, NodeGraph nodeGraph)
    {
        Console.WriteLine($"🔧 CallPDDLPlanner: Creating sequential execution pattern");
        
        // Add sequential relations (MEETS constraints) between consecutive actions
        for (int i = 0; i < actions.Count - 1; i++)
        {
            nodeGraph.AddOrderRelation(actions[i], actions[i + 1]);
            nodeGraph.AddTemporalConstraint(actions[i], actions[i + 1], TemporalConstraint.MEETS);
            Console.WriteLine($"🔧 CallPDDLPlanner: Added sequential relation: {actions[i].InstanceName} → {actions[i + 1].InstanceName}");
        }
        
        return nodeGraph;
    }
    
    private NodeGraph CreateParallelNodeGraph(List<GenericBTAction> actions, NodeGraph nodeGraph)
    {
        Console.WriteLine($"🔧 CallPDDLPlanner: Creating parallel execution pattern");
        
        if (actions.Count == 1)
        {
            Console.WriteLine($"🔧 CallPDDLPlanner: Single action execution");
            return nodeGraph;
        }
        
        // First action starts, then all others run in parallel
        for (int i = 1; i < actions.Count; i++)
        {
            nodeGraph.AddOrderRelation(actions[0], actions[i]);
            nodeGraph.AddTemporalConstraint(actions[0], actions[i], TemporalConstraint.OVERLAPS);
            Console.WriteLine($"🔧 CallPDDLPlanner: Added parallel relation: {actions[0].InstanceName} || {actions[i].InstanceName}");
        }
        
        return nodeGraph;
    }
    
    private NodeGraph CreateHybridNodeGraph(List<GenericBTAction> actions, NodeGraph nodeGraph)
    {
        Console.WriteLine($"🔧 CallPDDLPlanner: Creating hybrid execution pattern");
        
        if (actions.Count <= 2)
        {
            return CreateParallelNodeGraph(actions, nodeGraph);
        }
        
        // Hybrid pattern: First action sequential, then parallel groups
        // Group 1: First action
        // Group 2: Actions 2-3 run in parallel
        // Group 3: Actions 4+ run in parallel after group 2
        
        // First action to second action (sequential)
        nodeGraph.AddOrderRelation(actions[0], actions[1]);
        nodeGraph.AddTemporalConstraint(actions[0], actions[1], TemporalConstraint.MEETS);
        Console.WriteLine($"🔧 CallPDDLPlanner: Added sequential relation: {actions[0].InstanceName} → {actions[1].InstanceName}");
        
        // Second action to third action (parallel)
        if (actions.Count > 2)
        {
            nodeGraph.AddOrderRelation(actions[1], actions[2]);
            nodeGraph.AddTemporalConstraint(actions[1], actions[2], TemporalConstraint.OVERLAPS);
            Console.WriteLine($"🔧 CallPDDLPlanner: Added parallel relation: {actions[1].InstanceName} || {actions[2].InstanceName}");
        }
        
        // Remaining actions in parallel
        for (int i = 3; i < actions.Count; i++)
        {
            nodeGraph.AddOrderRelation(actions[1], actions[i]);
            nodeGraph.AddTemporalConstraint(actions[1], actions[i], TemporalConstraint.OVERLAPS);
            Console.WriteLine($"🔧 CallPDDLPlanner: Added parallel relation: {actions[1].InstanceName} || {actions[i].InstanceName}");
        }
        
        return nodeGraph;
    }
    


}
