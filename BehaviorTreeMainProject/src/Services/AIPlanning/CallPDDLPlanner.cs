using static BlackboardExtensions; 
using System;
using System.IO;

public class CallPDDLPlanner: BTServicePlanner
{
    private readonly Blackboard<FastName> blackboard;
    private readonly ActionFactory actionFactory;
    public FastName PlannerName = new FastName("PDDLPlANNER");


    public CallPDDLPlanner(IBehaviorTree InOwningtree) : base(InOwningtree)
    {
        this.blackboard = InOwningtree.LinkedBlackboard;
        this.actionFactory = ActionFactory.Instance;
    }
public override (List<IBTNode> Actions, List<OrderType> Orders) CreatePlanWithOrders()
    {
        var actions = new List<IBTNode>();
        var orders = new List<OrderType>();
        
        // Your PDDL planning logic here
        // For now, return empty lists
        return (actions, orders);
    }
    // New method to read and process PDDL plan from file
    // public BTFlowNode_Sequence CreateActionSequenceFromPDDLFile(string fileName)
    // {
    //     try
    //     {
    //         // Get the current directory where the executable is running
    //         string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    //         // Combine with the filename
    //         string filePath = Path.Combine(currentDirectory, fileName);

    //         // Check if file exists
    //         if (!File.Exists(filePath))
    //         {
    //             Console.WriteLine($"Error: Plan file not found at {filePath}");
    //             return null;
    //         }

    //         // Read the plan from file
    //         Console.WriteLine($"Reading plan from {filePath}");
    //         string pddlPlan = File.ReadAllText(filePath);
            
    //         // Create action sequence from the plan
    //         return CreateActionSequence(pddlPlan);
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine($"Error reading plan file: {e.Message}");
    //         return null;
    //     }
    // }

    // public BTFlowNode_Sequence CreateActionSequence(string pddlPlan)
    // {
    //     var sequence = new BTFlowNode_Sequence("Generated Plan Sequence");
        
    //     // Split the plan into lines and clean up each action string
    //     var actions = pddlPlan
    //         .Split('\n', StringSplitOptions.RemoveEmptyEntries)
    //         .Select(a => a.Trim())
    //         .Where(a => !string.IsNullOrWhiteSpace(a));

    //     foreach (var actionStr in actions)
    //     {
    //         try
    //         {
    //             // Remove parentheses and extra whitespace
    //             var cleanActionStr = actionStr.Trim('(', ')', ' ');
    //             Console.WriteLine($"Processing action: {cleanActionStr}"); // Debug output

    //             // Split the action string into parts
    //             var parts = cleanActionStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //             if (parts.Length < 3)
    //             {
    //                 Console.WriteLine($"Invalid action format: {cleanActionStr}");
    //                 continue;
    //             }

    //             string actionType = parts[0].ToLower();
    //             var elementKey = new FastName(parts[1]);
    //             var locationKey = new FastName(parts[2]);

    //             // Get the actual objects from the blackboard
    //             var element = blackboard.GetElement(elementKey);
    //             var location = blackboard.GetLocation(locationKey);

    //             // Create the action based on type
    //             BTActionNodeBase action;
    //             switch (actionType)
    //             {
    //                 case "pickup":
    //                     action = new BTAction_PickUp(blackboard, element, location);
    //                     break;
    //                 case "place":
    //                     action = new BTAction_Place(blackboard, element, location);
    //                     break;
    //                 default:
    //                     throw new Exception($"Unknown action type: {actionType}");
    //             }

    //             sequence.AddChild(action);
    //             Console.WriteLine($"Successfully added action: {actionType} {parts[1]} {parts[2]}");
    //         }
    //         catch (Exception e)
    //         {
    //             Console.WriteLine($"Error creating action from '{actionStr}': {e.Message}");
    //         }
    //     }

    //     return sequence;
    // }


    public override List<BTActionNodeBase> GetPlan()
    {
        // Implement GOAP planning logic here
        var actions = new List<BTActionNodeBase>();
        // ... planning logic ...
        return actions; 
    }
}
