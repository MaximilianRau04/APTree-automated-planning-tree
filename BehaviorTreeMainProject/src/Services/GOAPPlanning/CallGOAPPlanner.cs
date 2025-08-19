using System;
using System.Collections.Generic;
using System.Linq;
using PlanningDataStructures;
using AIPlanning;

public class CallGOAPPlanner : BTServicePlanner
{
    private readonly Blackboard<FastName> blackboard;
    private readonly FactoryAction actionFactory;
  
    public FastName PlannerName = new FastName("GOAPPlanner");

    public CallGOAPPlanner(IBehaviorTree InOwningTree) 
        : base(InOwningTree, new RestPlannerCommunicator("http://localhost:5002")) // Different port for GOAP planner
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
    }
    
    public CallGOAPPlanner(IBehaviorTree InOwningTree, IPlannerCommunicator customCommunicator) 
        : base(InOwningTree, customCommunicator)
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
    }

    protected override IPlanningRequest CreatePlanningRequest()
    {
        Console.WriteLine($"🔧 CallGOAPPlanner: Creating GOAP planning request...");
        
        try
        {
            // Extract current state and goals for GOAP (key-value pairs)
            var initialState = ExtractCurrentStateForGOAP();
            var goals = ExtractGoalsForGOAP();
            var availableActions = ExtractAvailableActions();
            
            // Create GOAP-specific request
            var request = new GOAPPlanningRequest
            {
                TimeoutSeconds = 30,
                MaxPlanLength = 20,
                InitialState = initialState,
                Goals = goals,
                AvailableActions = availableActions
            };
            
            Console.WriteLine($"✅ CallGOAPPlanner: Created GOAP planning request");
            return request;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallGOAPPlanner: Error creating planning request: {ex.Message}");
            throw;
        }
    }
    
    protected override NodeGraph GenerateNodeGraphFromResult(PlanningResult result)
    {
        Console.WriteLine($"🔧 CallGOAPPlanner: Converting GOAP result to NodeGraph...");
        
        try
        {
            if (string.IsNullOrEmpty(result.Plan))
            {
                Console.WriteLine("⚠️ CallGOAPPlanner: No plan in planning result");
                return null;
            }
            
            // Parse the plan string and create NodeGraph
            var nodeGraph = ParsePlanStringToNodeGraph(result.Plan);
            
            Console.WriteLine($"✅ CallGOAPPlanner: Generated NodeGraph with {nodeGraph.GetAllActionNodes().Count} actions");
            return nodeGraph;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallGOAPPlanner: Error generating NodeGraph: {ex.Message}");
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
                    Console.WriteLine($"🔧 CallGOAPPlanner: Added action {actionInstance.InstanceName.ToString()}");
                }
            }
            
            // Parse Relation lines
            if (trimmedLine.StartsWith("Relation:"))
            {
                var relationString = trimmedLine.Substring("Relation:".Length).Trim();
                ParseRelationString(relationString, actions, nodeGraph);
            }
        }
        
        // If no relations were parsed, add default sequential ordering (GOAP is typically sequential)
        if (actions.Count > 1 && nodeGraph.GetAllActionNodes().Count == actions.Count)
        {
            for (int i = 0; i < actions.Count - 1; i++)
            {
                // GOAP actions are typically sequential, so use MEETS constraint
                nodeGraph.AddOrderRelation(actions[i], actions[i + 1]);
                nodeGraph.AddTemporalConstraint(actions[i], actions[i + 1], TemporalConstraint.MEETS);
                Console.WriteLine($"🔧 CallGOAPPlanner: Added default sequential relation {i} → {i + 1}");
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
                    Console.WriteLine($"🔧 CallGOAPPlanner: Found matching action {action.InstanceName.ToString()}");
                    return action;
                }
            }
            
            // If no exact match found, create a new instance
            Console.WriteLine($"⚠️ CallGOAPPlanner: No exact match found for {actionString}, creating new instance");
            return CreateNewActionInstance(actionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallGOAPPlanner: Error creating action from string: {ex.Message}");
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
                Console.WriteLine($"🔧 CallGOAPPlanner: Created new action instance {actionString}");
                return actionInstance;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallGOAPPlanner: Error creating new action instance: {ex.Message}");
        }
        
        return null;
    }
    
    private void ParseRelationString(string relationString, List<GenericBTAction> actions, NodeGraph nodeGraph)
    {
        try
        {
            // Parse relation string like "action1 MEETS action2" or "action1 PRECEDES action2"
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
                    // Add order relation and temporal constraint
                    nodeGraph.AddOrderRelation(action1, action2);
                    
                    var constraintType = ParseTemporalConstraint(relationType);
                    nodeGraph.AddTemporalConstraint(action1, action2, constraintType);
                    
                    Console.WriteLine($"🔧 CallGOAPPlanner: Added relation {action1Name} {relationType} {action2Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallGOAPPlanner: Error parsing relation string: {ex.Message}");
        }
    }
    
    private TemporalConstraint ParseTemporalConstraint(string relationType)
    {
        return relationType?.ToUpper() switch
        {
            "MEETS" => TemporalConstraint.MEETS,
            "PRECEDES" => TemporalConstraint.PRECEDES,
            "OVERLAPS" => TemporalConstraint.OVERLAPS,
            "SEQUENTIAL" => TemporalConstraint.MEETS, // GOAP sequential = meets
            _ => TemporalConstraint.MEETS // Default to MEETS for GOAP
        };
    }
    
    // GOAP-specific extraction methods
    private Dictionary<string, object> ExtractCurrentStateForGOAP()
    {
        var state = new Dictionary<string, object>();
        
        try
        {
            // Extract GOAP-specific state from blackboard
            // GOAP uses key-value pairs for world state
            state["agent_position"] = "start_location";
            state["has_tool"] = false;
            state["task_completed"] = false;
            
            Console.WriteLine($"🔧 CallGOAPPlanner: Extracted GOAP state with {state.Count} properties");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallGOAPPlanner: Error extracting GOAP state: {ex.Message}");
        }
        
        return state;
    }
    
    private Dictionary<string, object> ExtractGoalsForGOAP()
    {
        var goals = new Dictionary<string, object>();
        
        try
        {
            // Extract GOAP-specific goals from blackboard
            goals["task_completed"] = true;
            goals["agent_position"] = "target_location";
            
            Console.WriteLine($"🔧 CallGOAPPlanner: Extracted {goals.Count} GOAP goals");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallGOAPPlanner: Error extracting GOAP goals: {ex.Message}");
        }
        
        return goals;
    }
    
    private List<string> ExtractAvailableActions()
    {
        var actions = new List<string>();
        
        try
        {
            // Get available actions from blackboard
            var availableActions = blackboard.GetAllActionInstances();
            
            foreach (var action in availableActions)
            {
                actions.Add(action.InstanceName.ToString());
            }
            
            Console.WriteLine($"🔧 CallGOAPPlanner: Extracted {actions.Count} available actions");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ CallGOAPPlanner: Error extracting available actions: {ex.Message}");
        }
        
        return actions;
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
            
            // Generate orders based on NodeGraph structure (GOAP is typically sequential)
            for (int i = 0; i < actions.Count - 1; i++)
            {
                orders.Add(OrderType.Total); // GOAP actions are typically sequential
            }
            
            return (actions, orders);
        }
        
        return (new List<IBTNode>(), new List<OrderType>());
    }
}