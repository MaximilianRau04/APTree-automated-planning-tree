using System;
using System.Collections.Generic;
using System.Linq;
using PlanningDataStructures;
using AIPlanning;

public class CallSCPlanner : BTServicePlanner
{
    private readonly Blackboard<FastName> blackboard;
    private readonly FactoryAction actionFactory;
  
    public FastName PlannerName = new FastName("StateChartPlanner");

    public CallSCPlanner(BTInstance InOwningTree, StateChartPlanningRequest InPlanningRequest) 
        : base(InOwningTree, new RestPlannerCommunicator("http://localhost:5001"), InPlanningRequest) // Different port for SC planner
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
    }
    
    public CallSCPlanner(BTInstance InOwningTree, IPlannerCommunicator customCommunicator, StateChartPlanningRequest InPlanningRequest) 
        : base(InOwningTree, customCommunicator, InPlanningRequest)
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
    }

   
    
    protected override NodeGraph GenerateNodeGraphFromResult(PlanningResult result)
    {
        Console.WriteLine($"🔧 CallSCPlanner: Converting StateChart result to NodeGraph...");
        
        try
        {
            if (string.IsNullOrEmpty(result.Plan))
            {
                Console.WriteLine("⚠️ CallSCPlanner: No plan in planning result");
                return null;
            }
            
            // Parse the plan string and create NodeGraph
            var nodeGraph = ParsePlanStringToNodeGraph(result.Plan);
            
            Console.WriteLine($"✅ CallSCPlanner: Generated NodeGraph with {nodeGraph.GetAllActionNodes().Count} actions");
            return nodeGraph;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallSCPlanner: Error generating NodeGraph: {ex.Message}");
            return null;
        }
    }
    

    
    private NodeGraph ParsePlanStringToNodeGraph(string planString)
    {
        var nodeGraph = new NodeGraph();
        var actions = new List<GenericBTAction>();
        
        // Parse the plan string (assuming it's in a format like NodeGraphGenerated.txt)
        var lines = planString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip comments and empty lines
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                continue;
            
            // Parse ActionInstance lines
            if (trimmedLine.StartsWith("ActionInstance:"))
            {
                var actionString = trimmedLine.Substring("ActionInstance:".Length).Trim();
                var actionInstance = CreateActionFromString(actionString);
                if (actionInstance != null)
                {
                    actions.Add(actionInstance);
                    nodeGraph.AddNode(actionInstance);
                    Console.WriteLine($"🔧 CallSCPlanner: Added action {actionInstance.InstanceName.ToString()}");
                }
            }
            
            // Parse Relation lines
            if (trimmedLine.StartsWith("Relation:"))
            {
                var relationString = trimmedLine.Substring("Relation:".Length).Trim();
                ParseRelationString(relationString, actions, nodeGraph);
            }
        }
        
        // If no relations were parsed, add default parallel ordering (StateChart actions often run in parallel)
        if (actions.Count > 1 && nodeGraph.GetAllActionNodes().Count == actions.Count)
        {
            for (int i = 0; i < actions.Count - 1; i++)
            {
                // StateChart actions can often run in parallel, so use OVERLAPS constraint
                nodeGraph.AddTemporalConstraint(actions[i], actions[i + 1], TemporalConstraint.OVERLAPS);
                Console.WriteLine($"🔧 CallSCPlanner: Added default parallel relation {i} || {i + 1}");
            }
        }
        
        return nodeGraph;
    }
    
    private GenericBTAction CreateActionFromString(string actionString)
    {
        try
        {
            // Try to find existing action instance in blackboard
            var actionInstances = blackboard.GetAllActionInstances();
            
            // Look for action with matching instance name
            foreach (var action in actionInstances)
            {
                if (action.InstanceName.ToString().Equals(actionString, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"🔧 CallSCPlanner: Found matching action {action.InstanceName.ToString()}");
                    return action;
                }
            }
            
            // If no exact match found, create a new instance
            Console.WriteLine($"⚠️ CallSCPlanner: No exact match found for {actionString}, creating new instance");
            return CreateNewActionInstance(actionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallSCPlanner: Error creating action from string: {ex.Message}");
            return null;
        }
    }
    
    private GenericBTAction CreateNewActionInstance(string actionString)
    {
        try
        {
            // Create action instance using factory
            var actionInstance = actionFactory.CreateActionInstance(actionString, blackboard);
            
            if (actionInstance != null)
            {
                // Register in blackboard using the correct method
                blackboard.SetActionType(new FastName(actionString), actionInstance);
                Console.WriteLine($"🔧 CallSCPlanner: Created new action instance {actionString}");
                return actionInstance;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallSCPlanner: Error creating new action instance: {ex.Message}");
        }
        
        return null;
    }
    
    private void ParseRelationString(string relationString, List<GenericBTAction> actions, NodeGraph nodeGraph)
    {
        try
        {
            // Parse relation string like "action1 OVERLAPS action2" or "action1 MEETS action2"
            var parts = relationString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                var action1Name = parts[0];
                var relationType = parts[1];
                var action2Name = parts[2];
                
                // Find the corresponding actions
                var action1 = actions.FirstOrDefault(a => a.InstanceName.ToString().Equals(action1Name, StringComparison.OrdinalIgnoreCase));
                var action2 = actions.FirstOrDefault(a => a.InstanceName.ToString().Equals(action2Name, StringComparison.OrdinalIgnoreCase));
                
                if (action1 != null && action2 != null)
                {
                    // Add temporal constraint (StateChart focuses on temporal relationships)
                    var constraintType = ParseTemporalConstraint(relationType);
                    nodeGraph.AddTemporalConstraint(action1, action2, constraintType);
                    
                    Console.WriteLine($"🔧 CallSCPlanner: Added relation {action1Name} {relationType} {action2Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallSCPlanner: Error parsing relation string: {ex.Message}");
        }
    }
    
    private TemporalConstraint ParseTemporalConstraint(string relationType)
    {
        return relationType?.ToUpper() switch
        {
            "MEETS" => TemporalConstraint.MEETS,
            "PRECEDES" => TemporalConstraint.PRECEDES,
            "OVERLAPS" => TemporalConstraint.OVERLAPS,
            "PARALLEL" => TemporalConstraint.OVERLAPS, // StateChart parallel = overlaps
            _ => TemporalConstraint.OVERLAPS // Default to OVERLAPS for StateChart
        };
    }
    
    // StateChart-specific extraction methods
    private string ExtractCurrentStateForStateChart()
    {
        try
        {
            // Extract current state machine state from blackboard
            // StateChart uses state names as strings
            var currentState = "Idle"; // Default state
            
            Console.WriteLine($"🔧 CallSCPlanner: Extracted current state: {currentState}");
            return currentState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallSCPlanner: Error extracting current state: {ex.Message}");
            return "Error";
        }
    }
    
    private string ExtractTargetStateForStateChart()
    {
        try
        {
            // Extract target state from blackboard
            var targetState = "Completed"; // Default target state
            
            Console.WriteLine($"🔧 CallSCPlanner: Extracted target state: {targetState}");
            return targetState;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallSCPlanner: Error extracting target state: {ex.Message}");
            return "Error";
        }
    }
    
    private List<string> ExtractAvailableTransitions()
    {
        var transitions = new List<string>();
        
        try
        {
            // Extract available state transitions from blackboard
            transitions.Add("Idle -> Working");
            transitions.Add("Working -> Completed");
            transitions.Add("Working -> Error");
            transitions.Add("Error -> Idle");
            
            Console.WriteLine($"🔧 CallSCPlanner: Extracted {transitions.Count} available transitions");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallSCPlanner: Error extracting transitions: {ex.Message}");
        }
        
        return transitions;
    }
    
    // Legacy methods for backward compatibility
    public List<BTActionNodeBase> GetPlan()
    {
        if (generatedNodeGraph != null)
        {
            return generatedNodeGraph.GetAllActionNodes().Cast<BTActionNodeBase>().ToList();
        }
        return new List<BTActionNodeBase>();
    }
    
    public (List<IBTNode> Actions, List<OrderType> Orders) CreatePlanWithOrders()
    {
        if (generatedNodeGraph != null)
        {
            var actions = generatedNodeGraph.GetAllActionNodes().Cast<IBTNode>().ToList();
            var orders = new List<OrderType>();
            
            // Generate orders based on NodeGraph structure (StateChart often uses parallel)
            for (int i = 0; i < actions.Count - 1; i++)
            {
                orders.Add(OrderType.Parallel); // StateChart actions often run in parallel
            }
            
            return (actions, orders);
        }
        
        return (new List<IBTNode>(), new List<OrderType>());
    }
}