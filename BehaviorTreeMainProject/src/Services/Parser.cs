using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public static class Parser
{
    /// <summary>
    /// Parses a NodeGraph from a string containing the NodeGraph definition
    /// </summary>
    /// <param name="nodeGraphString">The string containing the NodeGraph definition</param>
    /// <param name="blackboard">The blackboard containing parameter instances</param>
    /// <returns>A populated NodeGraph instance</returns>
    public static NodeGraph ParseNodeGraph(string nodeGraphString, Blackboard<FastName> blackboard)
    {
        Console.WriteLine("🔧 Starting NodeGraph parsing...");
        
        var nodeGraph = new NodeGraph();
        var actionInstances = new Dictionary<string, GenericBTAction>();
        var blackboardWriter = new BlackboardWriter(blackboard);
        
        // Split the string into lines and process each line
        string[] lines = nodeGraphString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("//"))
                continue;
            
            // Parse ActionInstance definitions
            if (trimmedLine.StartsWith("ActionInstance:"))
            {
                try
                {
                    // Use BlackboardWriter to create and register the action
                    var action = blackboardWriter.CreateAndRegisterActionInstance(trimmedLine);
                    string instanceName = GetActionInstanceName(trimmedLine);
                    actionInstances[instanceName] = action;
                    nodeGraph.AddNode(action);
                    
                    Console.WriteLine($"✅ Added action instance: {instanceName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error creating action instance: {ex.Message}");
                    throw;
                }
            }
            
            // Parse relation definitions
            else if (trimmedLine.Contains("--[") && trimmedLine.Contains("]-->"))
            {
                try
                {
                    ParseRelation(trimmedLine, actionInstances, nodeGraph);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error parsing relation: {ex.Message}");
                    throw;
                }
            }
        }
        
        Console.WriteLine($"✅ NodeGraph parsing completed. Total actions: {actionInstances.Count}");
        return nodeGraph;
    }
    
    /// <summary>
    /// Extracts the instance name from an ActionInstance definition
    /// </summary>
    private static string GetActionInstanceName(string actionInstanceLine)
    {
        // Remove "ActionInstance: " prefix
        string content = actionInstanceLine.Replace("ActionInstance: ", "").Trim();
        
        // Find the opening parenthesis
        int openParen = content.IndexOf('(');
        int closeParen = content.LastIndexOf(')');
        
        if (openParen == -1 || closeParen == -1)
            throw new ArgumentException($"Invalid ActionInstance format: {actionInstanceLine}");
        
        // Extract the action type name (before the parenthesis)
        string actionType = content.Substring(0, openParen).Trim();
        
        // Extract parameters
        string parametersContent = content.Substring(openParen + 1, closeParen - openParen - 1).Trim();
        
        // Parse parameters to extract instance names (same logic as BlackboardWriter)
        var parameterInstances = ParseActionParameters(parametersContent);
        
        // Create a unique key: actionType_instance1_instance2_... (same as BlackboardWriter)
        string key = actionType;
        foreach (var instance in parameterInstances)
        {
            key += "_" + instance;
        }
        
        return key;
    }
    
    /// <summary>
    /// Parses a relation definition like "source --[MEETS]--> target"
    /// </summary>
    private static void ParseRelation(string relationLine, Dictionary<string, GenericBTAction> actionInstances, NodeGraph nodeGraph)
    {
        // Use regex to parse the relation format: source --[TemporalType]--> target
        var regex = new Regex(@"(\w+)\s*--\[(\w+)\]-->\s*(\w+)");
        var match = regex.Match(relationLine);
        
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid relation format: {relationLine}");
        }
        
        string sourceName = match.Groups[1].Value.Trim();
        string temporalType = match.Groups[2].Value.Trim();
        string targetName = match.Groups[3].Value.Trim();
        
        Console.WriteLine($"🔍 Parsing relation: {sourceName} --[{temporalType}]--> {targetName}");
        
        // Find the action instances
        if (!actionInstances.TryGetValue(sourceName, out var sourceAction))
        {
            throw new ArgumentException($"Source action '{sourceName}' not found in action instances");
        }
        
        if (!actionInstances.TryGetValue(targetName, out var targetAction))
        {
            throw new ArgumentException($"Target action '{targetName}' not found in action instances");
        }
        
        // Convert temporal type string to enum
        if (!Enum.TryParse<TemporalConstraint>(temporalType, true, out var temporalConstraint))
        {
            throw new ArgumentException($"Unknown temporal constraint type: {temporalType}");
        }
        
        // Add the relation to the NodeGraph
        nodeGraph.AddOrderRelation(sourceAction, targetAction);
        nodeGraph.AddTemporalConstraint(sourceAction, targetAction, temporalConstraint);
        
        Console.WriteLine($"✅ Added relation: {sourceName} --[{temporalType}]--> {targetName}");
    }
    
    /// <summary>
    /// Parses action parameters to extract instance names (same as BlackboardWriter)
    /// Expected format: parameter1 : value1, parameter2 : value2, ...
    /// </summary>
    /// <param name="parametersContent">The parameters string to parse</param>
    /// <returns>List of instance names</returns>
    private static List<string> ParseActionParameters(string parametersContent)
    {
        var instances = new List<string>();
        
        if (string.IsNullOrWhiteSpace(parametersContent))
        {
            return instances;
        }
        
        // Split by comma, but be careful about commas inside parentheses
        var parameterPairs = parametersContent.Split(',');
        
        foreach (var pair in parameterPairs)
        {
            var trimmedPair = pair.Trim();
            if (string.IsNullOrWhiteSpace(trimmedPair))
                continue;
                
            // Split by colon
            var colonIndex = trimmedPair.IndexOf(':');
            if (colonIndex == -1)
            {
                throw new ArgumentException($"Invalid parameter format: {trimmedPair}. Expected: parameter : value");
            }
            
            string paramValue = trimmedPair.Substring(colonIndex + 1).Trim();
            
            if (string.IsNullOrWhiteSpace(paramValue))
            {
                throw new ArgumentException($"Parameter value cannot be empty in: {trimmedPair}");
            }
            
            instances.Add(paramValue);
        }
        
        return instances;
    }
    
    /// <summary>
    /// Parses a NodeGraph from a file
    /// </summary>
    /// <param name="filePath">Path to the file containing the NodeGraph definition</param>
    /// <param name="blackboard">The blackboard containing parameter instances</param>
    /// <returns>A populated NodeGraph instance</returns>
    public static NodeGraph ParseNodeGraphFromFile(string filePath, Blackboard<FastName> blackboard)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"NodeGraph file not found: {filePath}");
        }
        
        string content = File.ReadAllText(filePath);
        return ParseNodeGraph(content, blackboard);
    }
}
