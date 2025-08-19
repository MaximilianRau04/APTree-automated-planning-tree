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

    public CallPDDLPlanner(IBehaviorTree InOwningTree) 
        : base(InOwningTree, new RestPlannerCommunicator("http://localhost:5000"))
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
    }
    
    public CallPDDLPlanner(IBehaviorTree InOwningTree, IPlannerCommunicator customCommunicator) 
        : base(InOwningTree, customCommunicator)
    {
        this.blackboard = InOwningTree.LinkedBlackboard;
        this.actionFactory = FactoryAction.Instance;
    }

    protected override IPlanningRequest CreatePlanningRequest()
    {
        Console.WriteLine($"🔧 CallPDDLPlanner: Creating PDDL planning request...");
        
        try
        {
            // Create PDDL-specific request
            var request = new PDDLPlanningRequest
            {
                DomainFile = "./Plannerinputs/domain.pddl",
                ProblemFile = "./Plannerinputs/problem.pddl",
                PlannerPath = "/home/shermin/ENHSP-Public/enhsp.jar", // Path to ENHSP JAR file
                TimeoutSeconds = 30,
                MaxPlanLength = 20
            };
            
            Console.WriteLine($"✅ CallPDDLPlanner: Created PDDL planning request");
            return request;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallPDDLPlanner: Error creating planning request: {ex.Message}");
            throw;
        }
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
        Console.WriteLine($"🔧 CallPDDLPlanner: Converting ENHSP plan to Monticore format...");
        
        try
        {
            // Convert ENHSP plan to Monticore format
            string monticoreFormat = ConvertEnhspPlanToMonticoreFormat(planString);
            Console.WriteLine($"✅ CallPDDLPlanner: Converted to Monticore format:\n{monticoreFormat}");
            
            // Use the existing Parser to create NodeGraph from Monticore format
            var nodeGraph = Parser.ParseNodeGraph(monticoreFormat, blackboard);
            
            Console.WriteLine($"✅ CallPDDLPlanner: Created NodeGraph with {nodeGraph.GetAllActionNodes().Count} actions");
            return nodeGraph;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CallPDDLPlanner: Error parsing plan string: {ex.Message}");
            return null;
        }
    }
    
    private string ConvertEnhspPlanToMonticoreFormat(string enhspPlanString)
    {
        var monticoreLines = new List<string>();
        
        // Add NodeGraph header
        monticoreLines.Add("Nodegraph PDDLPlan {");
        monticoreLines.Add("");
        
        // Parse ENHSP plan lines
        var lines = enhspPlanString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var actionInstances = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip comments and empty lines
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                continue;
            
            // Parse ActionInstance lines (from Python service output)
            if (trimmedLine.StartsWith("ActionInstance:"))
            {
                var actionString = trimmedLine.Substring("ActionInstance:".Length).Trim();
                var monticoreAction = ConvertActionToMonticoreFormat(actionString);
                if (!string.IsNullOrEmpty(monticoreAction))
                {
                    actionInstances.Add(monticoreAction);
                    monticoreLines.Add($"    {monticoreAction}");
                }
            }
        }
        
        // Add sequential relations
        monticoreLines.Add("");
        for (int i = 0; i < actionInstances.Count - 1; i++)
        {
            var action1Name = GetActionInstanceName(actionInstances[i]);
            var action2Name = GetActionInstanceName(actionInstances[i + 1]);
            monticoreLines.Add($"    {action1Name} --[MEETS]--> {action2Name}");
        }
        
        monticoreLines.Add("");
        monticoreLines.Add("}");
        
        return string.Join("\n", monticoreLines);
    }
    
    private string ConvertActionToMonticoreFormat(string actionString)
    {
        // Convert from "Grab_b1_fp2_r1" format to Monticore "ActionInstance: grab(obj : b1, grabPos : fp2, client : r1)" format
        
        // Parse the action string (e.g., "Grab_b1_fp2_r1")
        var parts = actionString.Split('_');
        if (parts.Length < 2)
        {
            Console.WriteLine($"⚠️ CallPDDLPlanner: Invalid action format: {actionString}");
            return null;
        }
        
        var actionName = parts[0].ToLower(); // Convert to lowercase for Monticore
        var parameters = parts.Skip(1).ToArray();
        
        // Map action names and parameters to Monticore format based on test_crf.txt
        switch (actionName)
        {
            case "grab":
                if (parameters.Length >= 3)
                {
                    return $"ActionInstance: grab(obj : {parameters[0]}, grabPos : {parameters[1]}, client : {parameters[2]})";
                }
                break;
                
            case "place":
                if (parameters.Length >= 3)
                {
                    return $"ActionInstance: place(obj : {parameters[0]}, placePos : {parameters[1]}, client : {parameters[2]})";
                }
                break;
                
            case "pickup":
                if (parameters.Length >= 4)
                {
                    return $"ActionInstance: pickUp(pickedObject : {parameters[0]}, rob : {parameters[1]}, loc : {parameters[2]}, robTool : {parameters[3]})";
                }
                break;
                
            case "stack":
                if (parameters.Length >= 7)
                {
                    return $"ActionInstance: stackHL(obj1 : {parameters[0]}, obj2 : {parameters[1]}, client : {parameters[2]}, vg : {parameters[3]}, pr : {parameters[4]}, lay : {parameters[5]}, mod : {parameters[6]})";
                }
                break;
                
            case "stackonmultiple":
                if (parameters.Length >= 6)
                {
                    return $"ActionInstance: stackonmultiple(plate : {parameters[0]}, client : {parameters[1]}, pos : {parameters[2]}, vg : {parameters[3]}, mod : {parameters[4]}, lay : {parameters[5]})";
                }
                break;
                
            case "gluing":
                if (parameters.Length >= 4)
                {
                    return $"ActionInstance: gluing(obj : {parameters[0]}, pos : {parameters[1]}, client : {parameters[2]}, gg : {parameters[3]})";
                }
                break;
                
            case "gluingplate":
                if (parameters.Length >= 3)
                {
                    return $"ActionInstance: gluingPLate(obj : {parameters[0]}, pos : {parameters[1]}, client : {parameters[2]})";
                }
                break;
                
            case "gluingbeam":
                if (parameters.Length >= 5)
                {
                    return $"ActionInstance: gluingBeam(obj : {parameters[0]}, pos : {parameters[1]}, client : {parameters[2]}, gg : {parameters[3]}, lay : {parameters[4]})";
                }
                break;
                
            case "nailing":
                if (parameters.Length >= 3)
                {
                    return $"ActionInstance: nailing(obj : {parameters[0]}, pos : {parameters[1]}, client : {parameters[2]})";
                }
                break;
                
            default:
                Console.WriteLine($"⚠️ CallPDDLPlanner: Unknown action type: {actionName}");
                return null;
        }
        
        Console.WriteLine($"⚠️ CallPDDLPlanner: Invalid parameter count for {actionName}: {parameters.Length}");
        return null;
    }
    
    private string GetActionInstanceName(string actionInstanceLine)
    {
        // Extract the action name from Monticore format
        // e.g., "ActionInstance: grab(obj : b1, grabPos : fp2, client : r1)" -> "grab_b1_fp2_r1"
        
        if (!actionInstanceLine.StartsWith("ActionInstance:"))
            return actionInstanceLine;
            
        var actionPart = actionInstanceLine.Substring("ActionInstance:".Length).Trim();
        var openParenIndex = actionPart.IndexOf('(');
        var closeParenIndex = actionPart.LastIndexOf(')');
        
        if (openParenIndex == -1 || closeParenIndex == -1)
            return actionPart;
            
        var actionName = actionPart.Substring(0, openParenIndex).Trim();
        var parametersPart = actionPart.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
        
        // Extract parameter values
        var parameters = new List<string>();
        var paramPairs = parametersPart.Split(',');
        
        foreach (var pair in paramPairs)
        {
            var colonIndex = pair.IndexOf(':');
            if (colonIndex != -1)
            {
                var value = pair.Substring(colonIndex + 1).Trim();
                parameters.Add(value);
            }
        }
        
        // Create the action instance name
        return $"{actionName}_{string.Join("_", parameters)}";
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
