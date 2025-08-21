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
        Console.WriteLine($"🔧 CallPDDLPlanner: Converting PDDL plan to action instances...");
        
        try
        {
            // Convert PDDL plan to ActionInstance strings
            var actionInstanceStrings = ConvertPddlPlanToActionInstances(planString);
            Console.WriteLine($"✅ CallPDDLPlanner: Converted to {actionInstanceStrings.Count} action instances");
            
            // Create BlackboardWriter to handle action creation and registration
            var blackboardWriter = new BlackboardWriter(blackboard);
            
            // Create and register all action instances
            Console.WriteLine($"🔧 CallPDDLPlanner: Attempting to create {actionInstanceStrings.Count} action instances...");
            var createdActions = blackboardWriter.CreateAndRegisterActionInstances(actionInstanceStrings.ToArray());
            Console.WriteLine($"✅ CallPDDLPlanner: Created and registered {createdActions.Count} action instances");
            
            if (createdActions.Count != actionInstanceStrings.Count)
            {
                Console.WriteLine($"\n⚠️⚠️⚠️ ACTION CREATION WARNING ⚠️⚠️⚠️");
                Console.WriteLine($"⚠️ {actionInstanceStrings.Count - createdActions.Count} actions were lost during creation!");
                Console.WriteLine($"⚠️ Expected: {actionInstanceStrings.Count}, Actual: {createdActions.Count}");
                Console.WriteLine($"⚠️⚠️⚠️ END ACTION CREATION WARNING ⚠️⚠️⚠️\n");
            }
            
            // Create NodeGraph with sequential relations
            var nodeGraph = CreateNodeGraphWithSequentialRelations(createdActions);
            Console.WriteLine($"✅ CallPDDLPlanner: Created NodeGraph with {nodeGraph.GetAllActionNodes().Count} actions");
            
            return nodeGraph;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallPDDLPlanner: Error parsing plan string: {ex.Message}");
            return null;
        }
    }
    
    private List<string> ConvertPddlPlanToActionInstances(string pddlPlanString)
    {
        var actionInstanceStrings = new List<string>();
        var totalLines = 0;
        var skippedLines = 0;
        var convertedLines = 0;
        var failedConversions = 0;
        
        // Parse PDDL plan lines
        var lines = pddlPlanString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        Console.WriteLine($"🔍 CallPDDLPlanner: Processing {lines.Length} lines from PDDL plan");
        
        foreach (var line in lines)
        {
            totalLines++;
            var trimmedLine = line.Trim();
            
            // Skip comments and empty lines
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
            {
                skippedLines++;
                continue;
            }
            
            // Parse ActionInstance lines (from Python service output)
            if (trimmedLine.StartsWith("ActionInstance:"))
            {
                var actionString = trimmedLine.Substring("ActionInstance:".Length).Trim();
                var actionInstanceString = ConvertActionToActionInstanceFormat(actionString);
                if (!string.IsNullOrEmpty(actionInstanceString))
                {
                    actionInstanceStrings.Add(actionInstanceString);
                    convertedLines++;
                    Console.WriteLine($"✅ CallPDDLPlanner: Converted {convertedLines}: {actionString} -> {actionInstanceString}");
                }
                else
                {
                    failedConversions++;
                    Console.WriteLine($"❌❌❌ FAILED TO CONVERT: {actionString} ❌❌❌");
                }
            }
            else
            {
                skippedLines++;
                Console.WriteLine($"⚠️ CallPDDLPlanner: Skipped non-ActionInstance line: {trimmedLine}");
            }
        }
        
        Console.WriteLine($"\n🔍🔍🔍 PDDL CONVERSION DEBUG SUMMARY 🔍🔍🔍");
        Console.WriteLine($"🔍 Total lines processed: {totalLines}");
        Console.WriteLine($"🔍 Lines skipped: {skippedLines}");
        Console.WriteLine($"🔍 Actions successfully converted: {convertedLines}");
        Console.WriteLine($"🔍 Actions failed to convert: {failedConversions}");
        Console.WriteLine($"🔍 Final action instances: {actionInstanceStrings.Count}");
        Console.WriteLine($"🔍🔍🔍 END CONVERSION SUMMARY 🔍🔍🔍\n");
        
        return actionInstanceStrings;
    }
    
    private string ConvertActionToActionInstanceFormat(string actionString)
    {
        // Convert from "Grab_b1_fp2_r1" format to "ActionInstance: grab(obj : b1, grabPos : fp2, client : r1)" format
        
        // Parse the action string (e.g., "Grab_b1_fp2_r1")
        var parts = actionString.Split('_');
        if (parts.Length < 2)
        {
            Console.WriteLine($"⚠️ CallPDDLPlanner: Invalid action format: {actionString}");
            return null;
        }
        
        var actionName = parts[0].ToLower(); // Convert to lowercase
        var parameters = parts.Skip(1).ToArray();
        
        // Map PDDL action names to C# action types
        var mappedActionName = MapPddlActionToCSharpAction(actionName);
        if (string.IsNullOrEmpty(mappedActionName))
        {
            Console.WriteLine($"⚠️ CallPDDLPlanner: Unknown action type: {actionName}");
            return null;
        }
        
        // Create parameter string based on action type
        var parameterString = CreateParameterStringForAction(mappedActionName, parameters);
        if (string.IsNullOrEmpty(parameterString))
        {
            Console.WriteLine($"⚠️ CallPDDLPlanner: Invalid parameter count for {actionName}: {parameters.Length}");
            return null;
        }
        
        return $"ActionInstance: {mappedActionName}({parameterString})";
    }
    
    private string MapPddlActionToCSharpAction(string pddlActionName)
    {
        // Simple mapping from PDDL action names to C# action class names
        switch (pddlActionName)
        {
            case "pickuphl":
                return "PickUpHL";
            case "placehl":
                return "PlaceHL";
            case "stackhl":
                return "StackHL";
            case "stackonmultiplehl":
                return "StackOnMultipleHL";
            case "gluingplatehl":
                return "GluingPlateHL";
            case "gluingbeamhl":
                return "GluingBeamHL";
            case "nailinghl":
                return "NailingHL";
            case "grab":
                return "Grab";
            case "place":
                return "Place";
            case "pickup":
                return "PickUp";
            case "stack":
                return "Stack";
            case "gluing":
                return "Gluing";
            case "nailing":
                return "Nailing";
            default:
                Console.WriteLine($"⚠️ CallPDDLPlanner: Unknown PDDL action: {pddlActionName}");
                return null;
        }
    }
    
    private string CreateParameterStringForAction(string actionTypeName, string[] parameters)
    {
        // Create parameter string based on action type and parameter count
        switch (actionTypeName)
        {
            case "PickUpHL":
                if (parameters.Length >= 3)
                    return $"obj : {parameters[0]}, grabPos : {parameters[1]}, client : {parameters[2]}";
                break;
            case "PlaceHL":
                if (parameters.Length >= 3)
                    return $"obj : {parameters[0]}, placePos : {parameters[1]}, client : {parameters[2]}";
                break;
            case "StackHL":
                if (parameters.Length >= 6)
                    return $"obj1 : {parameters[0]}, obj2 : {parameters[1]}, client : {parameters[2]}, pr : {parameters[3]}, lay : {parameters[4]}, mod : {parameters[5]}";
                break;
            case "StackOnMultipleHL":
                if (parameters.Length >= 5)
                    return $"plate : {parameters[0]}, client : {parameters[1]}, pos : {parameters[2]}, mod : {parameters[3]}, lay : {parameters[4]}";
                break;
            case "GluingPlateHL":
                if (parameters.Length >= 3)
                    return $"obj : {parameters[0]}, pos : {parameters[1]}, client : {parameters[2]}";
                break;
            case "GluingBeamHL":
                if (parameters.Length >= 5)
                    return $"obj : {parameters[0]}, pos : {parameters[1]}, client : {parameters[2]}, mod : {parameters[3]}, lay : {parameters[4]}";
                break;
            case "NailingHL":
                if (parameters.Length >= 3)
                    return $"obj : {parameters[0]}, pos : {parameters[1]}, client : {parameters[2]}";
                break;
        }
        
        Console.WriteLine($"⚠️ CallPDDLPlanner: Unsupported action type or parameter count: {actionTypeName} with {parameters.Length} parameters");
        return null;
    }
    
    private NodeGraph CreateNodeGraphWithSequentialRelations(List<GenericBTAction> actions)
    {
        var nodeGraph = new NodeGraph();
        
        // Add all actions to the NodeGraph
        foreach (var action in actions)
        {
            nodeGraph.AddNode(action);
        }
        
        // Add sequential relations (MEETS constraints) between consecutive actions
        for (int i = 0; i < actions.Count - 1; i++)
        {
            nodeGraph.AddOrderRelation(actions[i], actions[i + 1]);
            nodeGraph.AddTemporalConstraint(actions[i], actions[i + 1], TemporalConstraint.MEETS);
            Console.WriteLine($"🔧 CallPDDLPlanner: Added sequential relation: {actions[i].InstanceName} → {actions[i + 1].InstanceName}");
        }
        
        return nodeGraph;
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
            
            // Generate orders based on NodeGraph structure
            for (int i = 0; i < actions.Count - 1; i++)
            {
                orders.Add(OrderType.Total);
            }
            
            return (actions, orders);
        }
        
        return (new List<IBTNode>(), new List<OrderType>());
    }
}
