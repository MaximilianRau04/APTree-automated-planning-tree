using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public static class Parser
{
    /// <summary>
    /// Parses a NodeGraph from a list of action instance strings and relation strings
    /// </summary>
    /// <param name="actionInstanceStrings">List of action instance strings in MontiCore format</param>
    /// <param name="relationStrings">List of relation strings in the format "source --[TemporalType]--> target"</param>
    /// <param name="blackboard">The blackboard containing parameter instances</param>
    /// <returns>A populated NodeGraph instance</returns>
    public static NodeGraph ParseNodeGraph(List<string> actionInstanceStrings, List<string> relationStrings, Blackboard<FastName> blackboard)
    {
        Console.WriteLine("🔧 Parser: Starting NodeGraph parsing from separate inputs...");
        Console.WriteLine($"📋 Parser: Action instance strings count: {actionInstanceStrings?.Count ?? 0}");
        Console.WriteLine($"📋 Parser: Relation strings count: {relationStrings?.Count ?? 0}");
        Console.WriteLine($"📋 Parser: Blackboard is null: {blackboard == null}");

        var nodeGraph = new NodeGraph();
        var actionInstances = new Dictionary<string, GenericBTAction>();
        var blackboardWriter = new BlackboardWriter(blackboard);

        // First, create all action instances
        Console.WriteLine($"🔧 Parser: Creating {actionInstanceStrings?.Count ?? 0} action instances...");
        if (actionInstanceStrings != null)
        {
            int actionIndex = 0;
            foreach (string actionString in actionInstanceStrings)
            {
                actionIndex++;
                Console.WriteLine($"🔍 Parser: Processing action {actionIndex}/{actionInstanceStrings.Count}: '{actionString}'");

                try
                {
                    // Use BlackboardWriter to create and register the action
                    Console.WriteLine($"🔧 Parser: Calling BlackboardWriter.CreateAndRegisterActionInstance...");
                    var action = blackboardWriter.CreateAndRegisterActionInstance(actionString);
                    Console.WriteLine($"🔍 Parser: Action created: {action?.InstanceName.ToString() ?? "NULL"}");

                    string instanceName = GetActionInstanceName(actionString);
                    Console.WriteLine($"🔍 Parser: Instance name: '{instanceName}'");

                    actionInstances[instanceName] = action;
                    nodeGraph.AddNode(action);

                    Console.WriteLine($"✅ Parser: Added action instance: {instanceName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Parser: Error creating action instance: {ex.Message}");
                    Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
        }
        else
        {
            Console.WriteLine("⚠️ Parser: Action instance strings list is null");
        }

        // Then, create all relations
        Console.WriteLine($"🔧 Parser: Creating {relationStrings?.Count ?? 0} relations...");
        if (relationStrings != null)
        {
            int relationIndex = 0;
            foreach (string relationString in relationStrings)
            {
                relationIndex++;
                Console.WriteLine($"🔍 Parser: Processing relation {relationIndex}/{relationStrings.Count}: '{relationString}'");

                try
                {
                    ParseRelation(relationString, actionInstances, nodeGraph);
                    Console.WriteLine($"✅ Parser: Successfully parsed relation: {relationString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Parser: Error parsing relation: {ex.Message}");
                    Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
                    throw;
                }
            }
        }
        else
        {
            Console.WriteLine("⚠️ Parser: Relation strings list is null");
        }

        Console.WriteLine($"📊 Parser: NodeGraph parsing summary:");
        Console.WriteLine($"   - Total actions created: {actionInstances.Count}");
        Console.WriteLine($"   - Total relations processed: {relationStrings?.Count ?? 0}");
        Console.WriteLine($"   - NodeGraph action nodes: {nodeGraph.GetAllActionNodes().Count}");
        Console.WriteLine($"✅ Parser: NodeGraph parsing completed successfully");

        return nodeGraph;
    }

    /// <summary>
    /// Parses a NodeGraph from a string containing the NodeGraph definition (legacy method)
    /// </summary>
    /// <param name="nodeGraphString">The string containing the NodeGraph definition</param>
    /// <param name="blackboard">The blackboard containing parameter instances</param>
    /// <returns>A populated NodeGraph instance</returns>
    public static NodeGraph ParseNodeGraph(string nodeGraphString, Blackboard<FastName> blackboard)
    {
        Console.WriteLine("🔧 Starting NodeGraph parsing from string...");

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
    /// Updated to handle full action instance names with parameters
    /// </summary>
    private static void ParseRelation(string relationLine, Dictionary<string, GenericBTAction> actionInstances, NodeGraph nodeGraph)
    {
        // Use regex to parse the relation format: source --[TemporalType]--> target
        // Updated regex to handle action names with underscores and parameters
        var regex = new Regex(@"([\w_]+)\s*--\[(\w+)\]-->\s*([\w_]+)");
        var match = regex.Match(relationLine);

        if (!match.Success)
        {
            throw new ArgumentException($"Invalid relation format: {relationLine}");
        }

        string sourceName = match.Groups[1].Value.Trim();
        string temporalType = match.Groups[2].Value.Trim();
        string targetName = match.Groups[3].Value.Trim();

        Console.WriteLine($"🔍 Parsing relation: {sourceName} --[{temporalType}]--> {targetName}");

        // Find the action instances - try exact match first, then partial match
        GenericBTAction sourceAction = null;
        GenericBTAction targetAction = null;

        // Try exact match first
        if (actionInstances.TryGetValue(sourceName, out sourceAction))
        {
            Console.WriteLine($"✅ Found source action with exact match: {sourceName}");
        }
        else
        {
            // Try partial match by finding actions that start with the base name
            var matchingSourceActions = actionInstances.Keys.Where(key => key.StartsWith(sourceName + "_")).ToList();
            if (matchingSourceActions.Count == 1)
            {
                sourceAction = actionInstances[matchingSourceActions[0]];
                Console.WriteLine($"✅ Found source action with partial match: {matchingSourceActions[0]}");
            }
            else if (matchingSourceActions.Count > 1)
            {
                throw new ArgumentException($"Multiple source actions found for '{sourceName}': {string.Join(", ", matchingSourceActions)}");
            }
            else
            {
                throw new ArgumentException($"Source action '{sourceName}' not found in action instances. Available actions: {string.Join(", ", actionInstances.Keys.Take(10))}");
            }
        }

        // Try exact match first for target
        if (actionInstances.TryGetValue(targetName, out targetAction))
        {
            Console.WriteLine($"✅ Found target action with exact match: {targetName}");
        }
        else
        {
            // Try partial match by finding actions that start with the base name
            var matchingTargetActions = actionInstances.Keys.Where(key => key.StartsWith(targetName + "_")).ToList();
            if (matchingTargetActions.Count == 1)
            {
                targetAction = actionInstances[matchingTargetActions[0]];
                Console.WriteLine($"✅ Found target action with partial match: {matchingTargetActions[0]}");
            }
            else if (matchingTargetActions.Count > 1)
            {
                throw new ArgumentException($"Multiple target actions found for '{targetName}': {string.Join(", ", matchingTargetActions)}");
            }
            else
            {
                throw new ArgumentException($"Target action '{targetName}' not found in action instances. Available actions: {string.Join(", ", actionInstances.Keys.Take(10))}");
            }
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

    /// <summary>
    /// Parses ENHSP planner output and converts it to a list of action strings in MontiCore format
    /// </summary>
    /// <param name="enhspOutput">Raw ENHSP planner output</param>
    /// <returns>List of action strings in MontiCore format</returns>
    public static List<string> ParseENHSPOutput(string enhspOutput)
    {
        Console.WriteLine("🔧 Parser: Starting ENHSP output parsing...");
        Console.WriteLine($"📋 Parser: Input length: {enhspOutput?.Length ?? 0} characters");
        Console.WriteLine($"📋 Parser: Input preview: {enhspOutput?.Substring(0, Math.Min(200, enhspOutput.Length))}...");

        var actionStrings = new List<string>();

        if (string.IsNullOrEmpty(enhspOutput))
        {
            Console.WriteLine("⚠️ Parser: ENHSP output is null or empty");
            return actionStrings;
        }

        var lines = enhspOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"📋 Parser: Found {lines.Length} lines in ENHSP output");

        int processedLines = 0;
        int matchedLines = 0;

        foreach (var line in lines)
        {
            processedLines++;
            var trimmedLine = line.Trim();

            Console.WriteLine($"🔍 Parser: Processing line {processedLines}: '{trimmedLine}'");

            // Look for ENHSP action lines like "0.0: (pickUpHL lp4 fp25 r1)"
            if (trimmedLine.Contains(": (") && trimmedLine.EndsWith(")"))
            {
                matchedLines++;
                Console.WriteLine($"🎯 Parser: Found potential ENHSP action line: '{trimmedLine}'");

                try
                {
                    // Extract the part after the colon and before the parentheses
                    var colonIndex = trimmedLine.IndexOf(':');
                    Console.WriteLine($"🔍 Parser: Colon index: {colonIndex}");

                    if (colonIndex != -1)
                    {
                        var actionPart = trimmedLine.Substring(colonIndex + 1).Trim();
                        Console.WriteLine($"🔍 Parser: Action part: '{actionPart}'");

                        if (actionPart.StartsWith("(") && actionPart.EndsWith(")"))
                        {
                            // Parse action like "(pickUpHL lp4 fp25 r1)"
                            var actionStr = actionPart.Substring(1, actionPart.Length - 2); // Remove parentheses
                            Console.WriteLine($"🔍 Parser: Action string: '{actionStr}'");

                            var parts = actionStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            Console.WriteLine($"🔍 Parser: Split into {parts.Length} parts: [{string.Join(", ", parts)}]");

                            if (parts.Length >= 2)
                            {
                                var actionName = parts[0];
                                var parameters = parts.Skip(1).ToArray();

                                Console.WriteLine($"🔍 Parser: Action name: '{actionName}'");
                                Console.WriteLine($"🔍 Parser: Parameters: [{string.Join(", ", parameters)}]");

                                // Convert to MontiCore format: "ActionInstance: ActionName(param1 : value1, param2 : value2, ...)"
                                var montiCoreAction = ConvertToMontiCoreFormat(actionName, parameters);
                                Console.WriteLine($"🔍 Parser: MontiCore action result: '{montiCoreAction}'");

                                if (!string.IsNullOrEmpty(montiCoreAction))
                                {
                                    actionStrings.Add(montiCoreAction);
                                    Console.WriteLine($"✅ Parser: Successfully parsed ENHSP action: {actionStr} -> {montiCoreAction}");
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Parser: ConvertToMontiCoreFormat returned null for: {actionStr}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Parser: Not enough parts in action string: {parts.Length} < 2");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Parser: Action part doesn't start with '(' and end with ')': '{actionPart}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Parser: No colon found in line: '{trimmedLine}'");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Parser: Error parsing ENHSP line '{trimmedLine}': {ex.Message}");
                    Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"🔍 Parser: Line doesn't match ENHSP pattern: '{trimmedLine}'");
            }
        }

        Console.WriteLine($"📊 Parser: ENHSP parsing summary:");
        Console.WriteLine($"   - Total lines processed: {processedLines}");
        Console.WriteLine($"   - Lines matching pattern: {matchedLines}");
        Console.WriteLine($"   - Actions successfully parsed: {actionStrings.Count}");
        Console.WriteLine($"✅ Parser: Parsed {actionStrings.Count} actions from ENHSP output");

        return actionStrings;
    }

    /// <summary>
    /// Parses already-formatted MontiCore output (when Python service returns pre-formatted strings)
    /// </summary>
    /// <param name="montiCoreOutput">Already formatted MontiCore output from Python service</param>
    /// <returns>List of action strings in MontiCore format</returns>
    public static List<string> ParseMontiCoreOutput(string montiCoreOutput)
    {
        Console.WriteLine("🔧 Parser: Starting MontiCore output parsing...");
        Console.WriteLine($"📋 Parser: Input length: {montiCoreOutput?.Length ?? 0} characters");
        Console.WriteLine($"📋 Parser: Input preview: {montiCoreOutput?.Substring(0, Math.Min(200, montiCoreOutput.Length))}...");

        var actionStrings = new List<string>();

        if (string.IsNullOrEmpty(montiCoreOutput))
        {
            Console.WriteLine("⚠️ Parser: MontiCore output is null or empty");
            return actionStrings;
        }

        var lines = montiCoreOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"📋 Parser: Found {lines.Length} lines in MontiCore output");

        int processedLines = 0;
        int actionLines = 0;

        foreach (var line in lines)
        {
            processedLines++;
            var trimmedLine = line.Trim();

            Console.WriteLine($"🔍 Parser: Processing line {processedLines}: '{trimmedLine}'");

            // Look for ActionInstance lines
            if (trimmedLine.StartsWith("ActionInstance:"))
            {
                actionLines++;
                Console.WriteLine($"🎯 Parser: Found ActionInstance line: '{trimmedLine}'");

                string processedAction;

                // Check if it's underscore format (like Pickuphl_lp4_fp25_r1)
                if (trimmedLine.Contains("_") && !trimmedLine.Contains("("))
                {
                    // Convert underscore format to parentheses format with parameter mapping
                    processedAction = ConvertUnderscoreFormatToParenthesesFormat(trimmedLine);
                    Console.WriteLine($"🔧 Parser: Converted underscore format: '{trimmedLine}' -> '{processedAction}'");
                }
                else
                {
                    // Apply parameter mapping to parentheses format
                    processedAction = ApplyParameterMappingToActionDefinition(trimmedLine);
                    Console.WriteLine($"🔧 Parser: Applied parameter mapping: '{trimmedLine}' -> '{processedAction}'");
                }

                actionStrings.Add(processedAction);
                Console.WriteLine($"✅ Parser: Added action: {processedAction}");
            }
            else if (trimmedLine.StartsWith("Relation:"))
            {
                Console.WriteLine($"🔍 Parser: Skipping relation line: '{trimmedLine}'");
            }
            else
            {
                Console.WriteLine($"🔍 Parser: Skipping non-action line: '{trimmedLine}'");
            }
        }

        Console.WriteLine($"📊 Parser: MontiCore parsing summary:");
        Console.WriteLine($"   - Total lines processed: {processedLines}");
        Console.WriteLine($"   - Action lines found: {actionLines}");
        Console.WriteLine($"   - Actions successfully parsed: {actionStrings.Count}");
        Console.WriteLine($"✅ Parser: Parsed {actionStrings.Count} actions from MontiCore output");

        return actionStrings;
    }

    /// <summary>
    /// Converts underscore format to parentheses format with proper parameter mapping
    /// </summary>
    /// <param name="underscoreFormat">Action string in format "ActionInstance: ActionName_param1_param2_param3"</param>
    /// <returns>Action string in format "ActionInstance: ActionName(param1 : param1, param2 : param2, param3 : param3)" with correct parameter names</returns>
    private static string ConvertUnderscoreFormatToParenthesesFormat(string underscoreFormat)
    {
        Console.WriteLine($"🔧 Parser: ConvertUnderscoreFormatToParenthesesFormat called with: '{underscoreFormat}'");

        try
        {
            if (string.IsNullOrEmpty(underscoreFormat))
            {
                Console.WriteLine("❌ Parser: Input string is null or empty");
                return underscoreFormat;
            }

            // Remove "ActionInstance: " prefix
            if (!underscoreFormat.StartsWith("ActionInstance: "))
            {
                Console.WriteLine("❌ Parser: String doesn't start with 'ActionInstance: '");
                return underscoreFormat;
            }

            var content = underscoreFormat.Substring("ActionInstance: ".Length).Trim();
            Console.WriteLine($"🔍 Parser: Content after prefix: '{content}'");

            // Split by underscores
            var parts = content.Split('_');
            Console.WriteLine($"🔍 Parser: Split into {parts.Length} parts: [{string.Join(", ", parts)}]");

            if (parts.Length < 1)
            {
                Console.WriteLine("❌ Parser: No parts found after splitting");
                return underscoreFormat;
            }

            // First part is the action name
            var actionName = parts[0];
            Console.WriteLine($"🔍 Parser: Action name: '{actionName}'");

            // Remaining parts are parameters
            var parameters = parts.Skip(1).ToArray();
            Console.WriteLine($"🔍 Parser: Parameters: [{string.Join(", ", parameters)}]");

            if (parameters.Length == 0)
            {
                // No parameters
                var noParamResult = $"ActionInstance: {actionName}()";
                Console.WriteLine($"✅ Parser: Generated result: '{noParamResult}'");
                return noParamResult;
            }

            // Get correct parameter names for this action type
            string[] parameterNames = GetParameterNamesForAction(actionName, parameters.Length);

            if (parameterNames == null || parameterNames.Length != parameters.Length)
            {
                Console.WriteLine($"❌ Parser: Could not map parameters for action '{actionName}' with {parameters.Length} parameters");
                // Fallback to generic parameter names
                var fallbackParameterString = string.Join(", ", parameters.Select(param => $"{param} : {param}"));
                var fallbackResult = $"ActionInstance: {actionName}({fallbackParameterString})";
                Console.WriteLine($"⚠️ Parser: Using fallback format: '{fallbackResult}'");
                return fallbackResult;
            }

            // Create parameter string with correct parameter names
            var parameterString = string.Join(", ", parameters.Select((param, index) => $"{parameterNames[index]} : {param}"));
            Console.WriteLine($"🔍 Parser: Parameter string with correct names: '{parameterString}'");

            var result = $"ActionInstance: {actionName}({parameterString})";
            Console.WriteLine($"✅ Parser: Generated result: '{result}'");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parser: Error converting format: {ex.Message}");
            Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
            return underscoreFormat;
        }
    }

    /// <summary>
    /// Applies parameter mapping to a pre-formatted action definition
    /// </summary>
    /// <param name="actionDefinition">Action definition like "ActionInstance: Pickuphl(lp4 : lp4, fp25 : fp25, r1 : r1)"</param>
    /// <returns>Action definition with correct parameter names</returns>
    private static string ApplyParameterMappingToActionDefinition(string actionDefinition)
    {
        Console.WriteLine($"🔧 Parser: ApplyParameterMappingToActionDefinition called with: '{actionDefinition}'");

        try
        {
            if (string.IsNullOrEmpty(actionDefinition))
            {
                Console.WriteLine("❌ Parser: Action definition is null or empty");
                return actionDefinition;
            }

            // Remove "ActionInstance: " prefix
            if (!actionDefinition.StartsWith("ActionInstance: "))
            {
                Console.WriteLine("❌ Parser: Action definition doesn't start with 'ActionInstance: '");
                return actionDefinition;
            }

            var content = actionDefinition.Substring("ActionInstance: ".Length).Trim();
            Console.WriteLine($"🔍 Parser: Content after prefix: '{content}'");

            // Find the opening and closing parentheses
            int openParenIndex = content.IndexOf('(');
            int closeParenIndex = content.LastIndexOf(')');

            if (openParenIndex == -1 || closeParenIndex == -1)
            {
                Console.WriteLine("❌ Parser: No parentheses found in action definition");
                return actionDefinition;
            }

            // Extract action name and parameters
            string actionName = content.Substring(0, openParenIndex).Trim();
            string parametersContent = content.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();

            Console.WriteLine($"🔍 Parser: Action name: '{actionName}'");
            Console.WriteLine($"🔍 Parser: Parameters content: '{parametersContent}'");

            // Parse parameters to get parameter values
            var parameterValues = ParseParameterValues(parametersContent);
            Console.WriteLine($"🔍 Parser: Parameter values: [{string.Join(", ", parameterValues)}]");

            // Get correct parameter names for this action type
            string[] parameterNames = GetParameterNamesForAction(actionName, parameterValues.Length);

            if (parameterNames == null || parameterNames.Length != parameterValues.Length)
            {
                Console.WriteLine($"❌ Parser: Could not map parameters for action '{actionName}' with {parameterValues.Length} parameters");
                return actionDefinition; // Return original if mapping fails
            }

            // Create new parameter string with correct names
            var newParameterString = string.Join(", ", parameterValues.Select((value, index) => $"{parameterNames[index]} : {value}"));
            Console.WriteLine($"🔍 Parser: New parameter string: '{newParameterString}'");

            var result = $"ActionInstance: {actionName}({newParameterString})";
            Console.WriteLine($"✅ Parser: Generated mapped action definition: '{result}'");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parser: Error applying parameter mapping: {ex.Message}");
            Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
            return actionDefinition; // Return original if error occurs
        }
    }

    /// <summary>
    /// Parses parameter values from a parameter string like "lp4 : lp4, fp25 : fp25, r1 : r1"
    /// </summary>
    /// <param name="parametersContent">Parameter string</param>
    /// <returns>Array of parameter values</returns>
    private static string[] ParseParameterValues(string parametersContent)
    {
        if (string.IsNullOrWhiteSpace(parametersContent))
        {
            return new string[0];
        }

        var values = new List<string>();
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
                Console.WriteLine($"⚠️ Parser: Invalid parameter format: {trimmedPair}");
                continue;
            }

            string paramValue = trimmedPair.Substring(colonIndex + 1).Trim();
            if (!string.IsNullOrWhiteSpace(paramValue))
            {
                values.Add(paramValue);
            }
        }

        return values.ToArray();
    }

    /// <summary>
    /// Parses FF planner output and converts it to a list of action strings in MontiCore format
    /// </summary>
    /// <param name="ffOutput">Raw FF planner output</param>
    /// <returns>List of action strings in MontiCore format</returns>
    public static List<string> ParseFFOutput(string ffOutput)
    {
        Console.WriteLine("🔧 Parser: Starting FF output parsing...");
        Console.WriteLine($"📋 Parser: Input length: {ffOutput?.Length ?? 0} characters");
        Console.WriteLine($"📋 Parser: Input preview: {ffOutput?.Substring(0, Math.Min(200, ffOutput.Length))}...");

        var actionStrings = new List<string>();

        if (string.IsNullOrEmpty(ffOutput))
        {
            Console.WriteLine("⚠️ Parser: FF output is null or empty");
            return actionStrings;
        }

        var lines = ffOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine($"📋 Parser: Found {lines.Length} lines in FF output");

        int processedLines = 0;
        int matchedLines = 0;

        foreach (var line in lines)
        {
            processedLines++;
            var trimmedLine = line.Trim();

            Console.WriteLine($"🔍 Parser: Processing line {processedLines}: '{trimmedLine}'");

            // Look for FF action lines like "step    0: TRAVELML R1 PR2 EP1" (raw FF output)
            // OR converted action lines like "ActionInstance: Travelml_R1_PR2_EP1" (converted format)
            if ((trimmedLine.StartsWith("step") && trimmedLine.Contains(":")) ||
                trimmedLine.StartsWith("ActionInstance:"))
            {
                matchedLines++;
                Console.WriteLine($"🎯 Parser: Found potential FF action line: '{trimmedLine}'");

                try
                {
                    string actionName;
                    string[] parameters;

                    if (trimmedLine.StartsWith("ActionInstance:"))
                    {
                        // Handle converted format: "ActionInstance: Travelml_R1_PR2_EP1"
                        var actionPart = trimmedLine.Substring("ActionInstance:".Length).Trim();
                        Console.WriteLine($"🔍 Parser: Action part (converted format): '{actionPart}'");

                        // Split by underscore to get action name and parameters
                        var parts = actionPart.Split('_');
                        Console.WriteLine($"🔍 Parser: Split into {parts.Length} parts: [{string.Join(", ", parts)}]");

                        if (parts.Length >= 1)
                        {
                            actionName = parts[0];
                            parameters = parts.Skip(1).ToArray();

                            Console.WriteLine($"🔍 Parser: Action name: '{actionName}'");
                            Console.WriteLine($"🔍 Parser: Parameters: [{string.Join(", ", parameters)}]");

                            // Convert to MontiCore format: "ActionInstance: ActionName(param1 : value1, param2 : value2, ...)"
                            var montiCoreAction = ConvertToMontiCoreFormat(actionName, parameters);
                            Console.WriteLine($"🔍 Parser: MontiCore action result: '{montiCoreAction}'");

                            if (!string.IsNullOrEmpty(montiCoreAction))
                            {
                                actionStrings.Add(montiCoreAction);
                                Console.WriteLine($"✅ Parser: Successfully parsed converted FF action: {actionPart} -> {montiCoreAction}");
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Parser: ConvertToMontiCoreFormat returned null for: {actionPart}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Parser: Not enough parts in converted action: {parts.Length} < 1");
                        }
                    }
                    else
                    {
                        // Handle raw FF format: "step    0: TRAVELML R1 PR2 EP1"
                        var colonIndex = trimmedLine.IndexOf(':');
                        Console.WriteLine($"🔍 Parser: Colon index: {colonIndex}");

                        if (colonIndex != -1)
                        {
                            var actionPart = trimmedLine.Substring(colonIndex + 1).Trim();
                            Console.WriteLine($"🔍 Parser: Action part (raw format): '{actionPart}'");

                            var parts = actionPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            Console.WriteLine($"🔍 Parser: Split into {parts.Length} parts: [{string.Join(", ", parts)}]");

                            if (parts.Length >= 2)
                            {
                                actionName = parts[0];
                                parameters = parts.Skip(1).ToArray();

                                Console.WriteLine($"🔍 Parser: Action name: '{actionName}'");
                                Console.WriteLine($"🔍 Parser: Parameters: [{string.Join(", ", parameters)}]");

                                // Convert to MontiCore format: "ActionInstance: ActionName(param1 : value1, param2 : value2, ...)"
                                var montiCoreAction = ConvertToMontiCoreFormat(actionName, parameters);
                                Console.WriteLine($"🔍 Parser: MontiCore action result: '{montiCoreAction}'");

                                if (!string.IsNullOrEmpty(montiCoreAction))
                                {
                                    actionStrings.Add(montiCoreAction);
                                    Console.WriteLine($"✅ Parser: Successfully parsed raw FF action: {actionPart} -> {montiCoreAction}");
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Parser: ConvertToMontiCoreFormat returned null for: {actionPart}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Parser: Not enough parts in action part: {parts.Length} < 2");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Parser: No colon found in line: '{trimmedLine}'");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Parser: Error parsing FF line '{trimmedLine}': {ex.Message}");
                    Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"🔍 Parser: Line doesn't match FF pattern: '{trimmedLine}'");
            }
        }

        Console.WriteLine($"📊 Parser: FF parsing summary:");
        Console.WriteLine($"   - Total lines processed: {processedLines}");
        Console.WriteLine($"   - Lines matching pattern: {matchedLines}");
        Console.WriteLine($"   - Actions successfully parsed: {actionStrings.Count}");
        Console.WriteLine($"✅ Parser: Parsed {actionStrings.Count} actions from FF output");

        return actionStrings;
    }

    /// <summary>
    /// Converts planner action format to MontiCore ActionInstance format
    /// </summary>
    /// <param name="actionName">Action name from planner</param>
    /// <param name="parameters">Parameters from planner</param>
    /// <returns>Action string in MontiCore format</returns>
    private static string ConvertToMontiCoreFormat(string actionName, string[] parameters)
    {
        Console.WriteLine($"🔧 Parser: ConvertToMontiCoreFormat called with:");
        Console.WriteLine($"   - Action name: '{actionName}'");
        Console.WriteLine($"   - Parameters: [{string.Join(", ", parameters)}]");

        try
        {
            if (string.IsNullOrEmpty(actionName))
            {
                Console.WriteLine("❌ Parser: Action name is null or empty");
                return null;
            }

            if (parameters == null || parameters.Length == 0)
            {
                Console.WriteLine("⚠️ Parser: No parameters provided, creating action without parameters");
                return $"ActionInstance: {actionName}()";
            }

            // Map parameters based on action type
            string[] parameterNames = GetParameterNamesForAction(actionName, parameters.Length);

            if (parameterNames == null || parameterNames.Length != parameters.Length)
            {
                Console.WriteLine($"❌ Parser: Could not map parameters for action '{actionName}' with {parameters.Length} parameters");
                return null;
            }

            // Create parameter string in MontiCore format with correct parameter names
            var parameterString = string.Join(", ", parameters.Select((param, index) => $"{parameterNames[index]} : {param}"));
            Console.WriteLine($"🔍 Parser: Generated parameter string: '{parameterString}'");

            var result = $"ActionInstance: {actionName}({parameterString})";
            Console.WriteLine($"✅ Parser: Generated MontiCore format: '{result}'");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parser: Error converting to MontiCore format: {ex.Message}");
            Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Gets the correct parameter names for a given action type
    /// </summary>
    /// <param name="actionName">Action name from planner</param>
    /// <param name="parameterCount">Number of parameters</param>
    /// <returns>Array of parameter names in the correct order</returns>
    private static string[] GetParameterNamesForAction(string actionName, int parameterCount)
    {
        Console.WriteLine($"🔍 Parser: Mapping parameters for action '{actionName}' with {parameterCount} parameters");

        try
        {
            // Get the assembly containing GenericBTAction types
            var assembly = typeof(GenericBTAction).Assembly;

            // Search for the action type by name (case-insensitive)
            var actionType = assembly.GetTypes()
                .FirstOrDefault(t => t.IsSubclassOf(typeof(GenericBTAction)) &&
                                   !t.IsAbstract &&
                                   string.Equals(t.Name, actionName, StringComparison.OrdinalIgnoreCase));

            if (actionType == null)
            {
                // Try more flexible matching for common case variations
                var normalizedActionName = NormalizeActionName(actionName);
                Console.WriteLine($"🔍 Parser: Trying normalized action name: '{normalizedActionName}'");

                actionType = assembly.GetTypes()
                    .FirstOrDefault(t => t.IsSubclassOf(typeof(GenericBTAction)) &&
                                       !t.IsAbstract &&
                                       string.Equals(t.Name, normalizedActionName, StringComparison.OrdinalIgnoreCase));
            }

            if (actionType == null)
            {
                Console.WriteLine($"❌ Parser: Action type '{actionName}' not found in assembly");
                Console.WriteLine($"🔍 Parser: Available action types: {string.Join(", ", assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(GenericBTAction)) && !t.IsAbstract).Select(t => t.Name))}");
                return null;
            }

            Console.WriteLine($"✅ Parser: Found action type: {actionType.Name}");

            // Get the constructor that takes the most parameters (excluding the base constructor parameters)
            var constructors = actionType.GetConstructors()
                .Where(c => c.GetParameters().Length >= 3) // Must have at least actionType, instanceName, blackboard
                .OrderByDescending(c => c.GetParameters().Length)
                .ToList();

            if (!constructors.Any())
            {
                Console.WriteLine($"❌ Parser: No suitable constructor found for action type '{actionName}'");
                return null;
            }

            // Get the constructor with the most parameters (likely the one with all action parameters)
            var targetConstructor = constructors.First();
            var constructorParams = targetConstructor.GetParameters();

            Console.WriteLine($"🔍 Parser: Using constructor with {constructorParams.Length} parameters");
            Console.WriteLine($"🔍 Parser: Constructor parameters: {string.Join(", ", constructorParams.Select(p => $"{p.Name}:{p.ParameterType.Name}"))}");

            // Skip the first 3 parameters (actionType, instanceName, blackboard) which are from the base class
            var actionParameters = constructorParams.Skip(3).ToArray();

            if (actionParameters.Length != parameterCount)
            {
                Console.WriteLine($"❌ Parser: Parameter count mismatch. Expected {parameterCount} from planner, but action type has {actionParameters.Length} parameters");
                Console.WriteLine($"🔍 Parser: Action parameters: {string.Join(", ", actionParameters.Select(p => p.Name))}");
                return null;
            }

            // Extract parameter names from the constructor
            var parameterNames = actionParameters.Select(p => p.Name).ToArray();

            Console.WriteLine($"✅ Parser: Successfully mapped parameters for '{actionName}': {string.Join(", ", parameterNames)}");

            return parameterNames;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parser: Error getting parameter names for action '{actionName}': {ex.Message}");
            Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Normalizes action names to handle common case variations
    /// </summary>
    /// <param name="actionName">Original action name from planner</param>
    /// <returns>Normalized action name</returns>
    private static string NormalizeActionName(string actionName)
    {
        if (string.IsNullOrEmpty(actionName))
            return actionName;

        // Common case variations mapping
        var caseVariations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "pickuphl", "PickUpHL" },
            { "pickupml", "PickUpML" },
            { "placehl", "PlaceHL" },
            { "placeml", "PlaceML" },
            { "travelml", "TravelML" },
            { "equipeml", "EquipeML" },
            { "deequipml", "DeequipML" },
            { "initializeml", "InitializeML" },
            { "closetoolml", "CloseToolML" },
            { "stackml", "StackML" },
            { "stackhl", "StackHL" },
            { "stackonmultipleml", "StackonmultipleML" },
            { "stackonmultiplehl", "StackonmultipleHL" },
            { "gluingml", "GluingML" },
            { "gluinghl", "GluingHL" },
            { "gluingbeaml", "GluingBeamHL" },
            { "gluingplatehl", "GluingPLateHL" },
            { "nailingml", "NailingML" },
            { "nailinghl", "NailingHL" }
        };

        if (caseVariations.TryGetValue(actionName, out string normalizedName))
        {
            Console.WriteLine($"🔍 Parser: Normalized '{actionName}' to '{normalizedName}'");
            return normalizedName;
        }

        // If no specific mapping, try to apply common patterns
        // For example, "pickuphl" -> "PickUpHL"
        if (actionName.Length >= 3)
        {
            // Try to find patterns like "pickup" + "hl" -> "PickUp" + "HL"
            var lowerAction = actionName.ToLowerInvariant();

            // Check for common suffixes
            string[] suffixes = { "hl", "ml" };
            foreach (var suffix in suffixes)
            {
                if (lowerAction.EndsWith(suffix))
                {
                    var prefix = lowerAction.Substring(0, lowerAction.Length - suffix.Length);
                    var normalizedPrefix = NormalizePrefix(prefix);
                    var normalizedSuffix = suffix.ToUpperInvariant();
                    var result = normalizedPrefix + normalizedSuffix;

                    Console.WriteLine($"🔍 Parser: Applied pattern normalization: '{actionName}' -> '{result}'");
                    return result;
                }
            }
        }

        Console.WriteLine($"🔍 Parser: No normalization applied for '{actionName}'");
        return actionName;
    }

    /// <summary>
    /// Normalizes common action prefixes
    /// </summary>
    /// <param name="prefix">Action prefix</param>
    /// <returns>Normalized prefix</returns>
    private static string NormalizePrefix(string prefix)
    {
        var prefixMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "pickup", "PickUp" },
            { "place", "Place" },
            { "travel", "Travel" },
            { "equipe", "Equipe" },
            { "deequip", "Deequip" },
            { "initialize", "Initialize" },
            { "closetool", "CloseTool" },
            { "stack", "Stack" },
            { "stackonmultiple", "Stackonmultiple" },
            { "gluing", "Gluing" },
            { "gluingbeam", "GluingBeam" },
            { "gluingplate", "GluingPlate" },
            { "nailing", "Nailing" }
        };

        if (prefixMappings.TryGetValue(prefix, out string normalizedPrefix))
        {
            return normalizedPrefix;
        }

        // Default: capitalize first letter
        if (prefix.Length > 0)
        {
            return char.ToUpperInvariant(prefix[0]) + prefix.Substring(1).ToLowerInvariant();
        }

        return prefix;
    }

    /// <summary>
    /// Configuration for generating relations between actions
    /// </summary>
    public enum RelationConfiguration
    {
        Sequential,      // All actions run sequentially (MEETS)
        Parallel,        // Actions run in parallel (OVERLAPS)
        Hybrid           // Mix of sequential and parallel
    }

    /// <summary>
    /// Generates relation strings from a list of action instance strings based on configuration
    /// Updated to use full action instance names with parameters
    /// </summary>
    /// <param name="actionInstanceStrings">List of action instance strings in MontiCore format</param>
    /// <param name="configuration">Configuration for generating relations</param>
    /// <returns>List of relation strings in the format "source --[TemporalType]--> target"</returns>
    public static List<string> GenerateRelationsFromActionInstances(List<string> actionInstanceStrings, RelationConfiguration configuration)
    {
        Console.WriteLine("🔧 Parser: Starting GenerateRelationsFromActionInstances...");
        Console.WriteLine($"📋 Parser: Input action instances count: {actionInstanceStrings?.Count ?? 0}");
        Console.WriteLine($"📋 Parser: Configuration: {configuration}");

        var relationStrings = new List<string>();

        if (actionInstanceStrings == null || actionInstanceStrings.Count < 2)
        {
            Console.WriteLine("⚠️ Parser: Need at least 2 action instances to generate relations");
            return relationStrings;
        }

        Console.WriteLine($"🔧 Parser: Generating relations with {configuration} configuration for {actionInstanceStrings.Count} actions");

        // Extract full action instance names from action instance strings
        var actionInstanceNames = new List<string>();
        Console.WriteLine("🔍 Parser: Extracting full action instance names from action instance strings...");

        foreach (var actionString in actionInstanceStrings)
        {
            Console.WriteLine($"🔍 Parser: Processing action string: '{actionString}'");
            var actionInstanceName = GetActionInstanceName(actionString);
            Console.WriteLine($"🔍 Parser: Extracted action instance name: '{actionInstanceName}'");

            if (!string.IsNullOrEmpty(actionInstanceName))
            {
                actionInstanceNames.Add(actionInstanceName);
                Console.WriteLine($"✅ Parser: Added action instance name: '{actionInstanceName}'");
            }
            else
            {
                Console.WriteLine($"⚠️ Parser: Failed to extract action instance name from: '{actionString}'");
            }
        }

        Console.WriteLine($"📊 Parser: Extracted {actionInstanceNames.Count} action instance names: [{string.Join(", ", actionInstanceNames)}]");

        if (actionInstanceNames.Count < 2)
        {
            Console.WriteLine("⚠️ Parser: Could not extract enough action instance names to generate relations");
            return relationStrings;
        }

        Console.WriteLine($"🔧 Parser: Calling relation generation method for {configuration} configuration...");

        switch (configuration)
        {
            case RelationConfiguration.Sequential:
                return GenerateSequentialRelations(actionInstanceNames);

            case RelationConfiguration.Parallel:
                return GenerateParallelRelations(actionInstanceNames);

            case RelationConfiguration.Hybrid:
                return GenerateHybridRelations(actionInstanceNames);

            default:
                Console.WriteLine($"⚠️ Parser: Unknown configuration '{configuration}', defaulting to Sequential");
                return GenerateSequentialRelations(actionInstanceNames);
        }
    }

    /// <summary>
    /// Generates sequential relations (MEETS) between consecutive action instances
    /// </summary>
    /// <param name="actionInstanceNames">List of full action instance names with parameters</param>
    /// <returns>List of sequential relation strings</returns>
    private static List<string> GenerateSequentialRelations(List<string> actionInstanceNames)
    {
        var relationStrings = new List<string>();

        Console.WriteLine("🔧 Parser: Generating sequential relations (MEETS)");

        for (int i = 0; i < actionInstanceNames.Count - 1; i++)
        {
            var relationString = $"{actionInstanceNames[i]} --[MEETS]--> {actionInstanceNames[i + 1]}";
            relationStrings.Add(relationString);
            Console.WriteLine($"✅ Parser: Added sequential relation: {relationString}");
        }

        return relationStrings;
    }

    /// <summary>
    /// Generates parallel relations (OVERLAPS) where first action instance starts, then all others run in parallel
    /// </summary>
    /// <param name="actionInstanceNames">List of full action instance names with parameters</param>
    /// <returns>List of parallel relation strings</returns>
    private static List<string> GenerateParallelRelations(List<string> actionInstanceNames)
    {
        var relationStrings = new List<string>();

        Console.WriteLine("🔧 Parser: Generating parallel relations (OVERLAPS)");

        if (actionInstanceNames.Count == 1)
        {
            Console.WriteLine("🔧 Parser: Single action - no relations needed");
            return relationStrings;
        }

        // First action starts, then all others run in parallel
        for (int i = 1; i < actionInstanceNames.Count; i++)
        {
            var relationString = $"{actionInstanceNames[0]} --[OVERLAPS]--> {actionInstanceNames[i]}";
            relationStrings.Add(relationString);
            Console.WriteLine($"✅ Parser: Added parallel relation: {relationString}");
        }

        return relationStrings;
    }

    /// <summary>
    /// Generates hybrid relations: first action instance sequential, then parallel groups
    /// </summary>
    /// <param name="actionInstanceNames">List of full action instance names with parameters</param>
    /// <returns>List of hybrid relation strings</returns>
    private static List<string> GenerateHybridRelations(List<string> actionInstanceNames)
    {
        var relationStrings = new List<string>();

        Console.WriteLine("🔧 Parser: Generating hybrid relations");

        if (actionInstanceNames.Count <= 2)
        {
            return GenerateParallelRelations(actionInstanceNames);
        }

        // First action to second action (sequential)
        var sequentialRelation = $"{actionInstanceNames[0]} --[MEETS]--> {actionInstanceNames[1]}";
        relationStrings.Add(sequentialRelation);
        Console.WriteLine($"✅ Parser: Added sequential relation: {sequentialRelation}");

        // Second action to third action (parallel)
        if (actionInstanceNames.Count > 2)
        {
            var parallelRelation = $"{actionInstanceNames[1]} --[OVERLAPS]--> {actionInstanceNames[2]}";
            relationStrings.Add(parallelRelation);
            Console.WriteLine($"✅ Parser: Added parallel relation: {parallelRelation}");
        }

        // Remaining actions in parallel with second action
        for (int i = 3; i < actionInstanceNames.Count; i++)
        {
            var parallelRelation = $"{actionInstanceNames[1]} --[OVERLAPS]--> {actionInstanceNames[i]}";
            relationStrings.Add(parallelRelation);
            Console.WriteLine($"✅ Parser: Added parallel relation: {parallelRelation}");
        }

        return relationStrings;
    }

    /// <summary>
    /// Extracts action name from MontiCore format action instance string
    /// </summary>
    /// <param name="montiCoreAction">Action instance string in MontiCore format</param>
    /// <returns>Action name or null if extraction fails</returns>
    private static string ExtractActionNameFromMontiCoreFormat(string montiCoreAction)
    {
        Console.WriteLine($"🔧 Parser: ExtractActionNameFromMontiCoreFormat called with: '{montiCoreAction}'");

        try
        {
            if (string.IsNullOrEmpty(montiCoreAction))
            {
                Console.WriteLine("❌ Parser: Input string is null or empty");
                return null;
            }

            // Extract action name from "ActionInstance: ActionName(param1 : value1, ...)"
            if (montiCoreAction.StartsWith("ActionInstance: "))
            {
                Console.WriteLine("✅ Parser: String starts with 'ActionInstance: '");
                var content = montiCoreAction.Substring("ActionInstance: ".Length);
                Console.WriteLine($"🔍 Parser: Content after prefix: '{content}'");

                var openParenIndex = content.IndexOf('(');
                Console.WriteLine($"🔍 Parser: Open parenthesis index: {openParenIndex}");

                if (openParenIndex != -1)
                {
                    var actionName = content.Substring(0, openParenIndex).Trim();
                    Console.WriteLine($"✅ Parser: Successfully extracted action name: '{actionName}'");
                    return actionName;
                }
                else
                {
                    Console.WriteLine("⚠️ Parser: No opening parenthesis found");
                }
            }
            else
            {
                Console.WriteLine("⚠️ Parser: String doesn't start with 'ActionInstance: '");
            }

            Console.WriteLine("❌ Parser: Failed to extract action name");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parser: Error extracting action name from '{montiCoreAction}': {ex.Message}");
            Console.WriteLine($"❌ Parser: Stack trace: {ex.StackTrace}");
            return null;
        }
    }
    
    public static string ConvertMultiplePredicatesToPDDL(List<Predicate> predicates)
    {
        var pddlPredicates = new List<string>();
        foreach (var predicate in predicates)
        {
            var pddlPredicate = ConvertPredicateToPDDL(predicate);
            if (!string.IsNullOrEmpty(pddlPredicate))
            {
                pddlPredicates.Add(pddlPredicate);
            }
        }
        return string.Join("\n", pddlPredicates);
    }

    /// <summary>
    /// Converts a single predicate to PDDL format
    /// </summary>
    /// <param name="predicate">The predicate to convert</param>
    /// <returns>PDDL formatted predicate string</returns>
    private static string ConvertPredicateToPDDL(Predicate predicate)
    {
        try
        {
            if (predicate == null)
                return string.Empty;

            // Get all properties of the predicate (these are the parameters)
            var properties = predicate.GetAllProperties();
            
            // Get the predicate name
            string predicateName = predicate.PredicateName?.ToString() ?? "unknown";
            
            // Filter out non-parameter properties (isNegated, PredicateName, etc.)
            var parameterProperties = properties
                .Where(kvp => kvp.Key != "isNegated" && 
                              kvp.Key != "PredicateName" && 
                              kvp.Key != "PredicateType" &&
                              kvp.Value != null)
                .ToList();

            // Build PDDL format: (predicateName param1 param2 ...)
            var parameters = new List<string>();
            
            foreach (var prop in parameterProperties)
            {
                // Get the parameter value
                var paramValue = prop.Value;
                
                // If it's an object with a name property, use that
                if (paramValue is Entity entity)
                {
                    parameters.Add(entity.NameKey?.ToString() ?? paramValue.ToString());
                }
                else
                {
                    parameters.Add(paramValue.ToString());
                }
            }

            // Create PDDL format
            string pddlFormat = $"({predicateName} {string.Join(" ", parameters)})";
            
            // Handle negation
            if (predicate.isNegated)
            {
                pddlFormat = $"(not {pddlFormat})";
            }

            return pddlFormat;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parser: Error converting predicate to PDDL: {ex.Message}");
            return string.Empty;
        }
    }
}
