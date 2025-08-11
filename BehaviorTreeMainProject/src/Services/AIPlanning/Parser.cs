using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class Parser
{
    /// <summary>
    /// This method generates entity types from a PDDL domain file.
    /// </summary>
    /// <param name="pddlFilePath"></param>
    /// <returns></returns>
    public static Dictionary<string, EntityTypeInfo> generateEntityTypesfromPDDLDomain(string pddlFilePath)
    {
        var entityTypes = new Dictionary<string, EntityTypeInfo>();
        
        try
        {
            // Read the PDDL file
            string content = File.ReadAllText(pddlFilePath);
            
            // Extract types section
            int typesStart = content.IndexOf("(:types");
            int typesEnd = content.IndexOf(")", typesStart);
            string typesSection = content.Substring(typesStart, typesEnd - typesStart);

            // Split into lines and process each type definition
            string[] lines = typesSection.Split('\n');
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("(:types") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // Split the line by '-' to separate types and their base type
                string[] parts = trimmedLine.Split('-');
                if (parts.Length != 2)
                    continue;

                // Get the base type (right side of '-')
                string baseTypeStr = parts[1].Trim().ToLower();
                Type baseType = baseTypeStr switch
                {
                    "location" => typeof(Location),
                    "element" => typeof(Element),
                    "agent" => typeof(Agent),
                    "layer" => typeof(Layer),
                    "module" => typeof(Module),
                    _ => throw new ArgumentException($"Unknown base type: {baseTypeStr}")
                };

                // Get all subtypes (left side of '-')
                string[] subtypes = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (string subtype in subtypes)
                {
                    entityTypes.Add(subtype.Trim(), new EntityTypeInfo(
                        baseType,
                        new Dictionary<string, Type>()
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating entity types from PDDL domain: {ex.Message}");
        }

        return entityTypes;
    }

/// <summary>
///   generates predicate types from a PDDL domain file  
/// </summary>
/// <param name="pddlFilePath"></param>
/// <returns></returns>

    // public static Dictionary<string, PredicateTypeInfo> generatePredicateTypesfromPDDLDomain(string pddlFilePath)
    // {
    //     var predicateTypes = new Dictionary<string, PredicateTypeInfo>();
        
    //     try
    //     {
    //         // Read the PDDL file
    //         string content = File.ReadAllText(pddlFilePath);
            
    //         // Extract predicates section
    //         int predicatesStart = content.IndexOf("(:predicates");
    //         int predicatesEnd = predicatesStart;
    //         int parenthesesCount = 0;
    //         bool inPredicates = false;

    //         // Find matching closing parenthesis
    //         for (int i = predicatesStart; i < content.Length; i++)
    //         {
    //             if (content[i] == '(')
    //             {
    //                 parenthesesCount++;
    //                 inPredicates = true;
    //             }
    //             else if (content[i] == ')')
    //             {
    //                 parenthesesCount--;
    //                 if (inPredicates && parenthesesCount == 0)
    //                 {
    //                     predicatesEnd = i;
    //                     break;
    //                 }
    //             }
    //         }

    //         string predicatesSection = content.Substring(predicatesStart, predicatesEnd - predicatesStart + 1);

    //         Console.WriteLine("Extracted Predicates Section:");
    //         Console.WriteLine(predicatesSection);  // Debug output

    //         // Split into lines and process each predicate definition
    //         string[] lines = predicatesSection.Split('\n');
    //         foreach (string line in lines)
    //         {
    //             string trimmedLine = line.Trim();
    //             if (trimmedLine.StartsWith("(:predicates") || string.IsNullOrWhiteSpace(trimmedLine))
    //                 continue;

    //             // Remove comments if any
    //             int commentIndex = trimmedLine.IndexOf(';');
    //             if (commentIndex != -1)
    //             {
    //                 trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
    //             }

    //             if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

    //             // Extract predicate name and parameters
    //             int openParenIndex = trimmedLine.IndexOf('(');
    //             if (openParenIndex == -1) continue;

    //             string predicateContent = trimmedLine.Substring(openParenIndex + 1).Trim();
    //             string[] parts = predicateContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
    //             if (parts.Length < 1) continue;

    //             string predicateName = parts[0];
    //             var parameters = new Dictionary<string, Type>();

    //             // Process parameters
    //             for (int i = 1; i < parts.Length; i += 3)
    //             {
    //                 if (i + 2 >= parts.Length) break;

    //                 string paramName = parts[i].TrimStart('?');
    //                 string paramType = parts[i + 2].Trim(')').ToLower();

    //                 // First check if it's a base type
    //                 Type parameterType = paramType.ToLower() switch
    //                 {
    //                     "location" => typeof(Location),
    //                     "element" => typeof(Element),
    //                     "agent" => typeof(Agent),
    //                     "robots" => typeof(Agent),
    //                     "layer" => typeof(Layer),
    //                     "module" => typeof(Module),
    //                     _ => null
    //                 };

    //                 // If not a base type, look in registered types
    //                 if (parameterType == null)
    //                 {
    //                     var registeredType = EntityFactory.Instance.registeredEntityTypes
    //                         .FirstOrDefault(t => t.Key.Equals(paramType, StringComparison.OrdinalIgnoreCase));
                        
    //                     if (registeredType.Key == null)
    //                     {
    //                         throw new ArgumentException($"Entity type '{paramType}' not found in registered types for predicate '{predicateName}'");
    //                     }
                        
    //                     parameterType = registeredType.Value;
    //                 }

    //                 parameters.Add(paramName, parameterType);
    //             }

    //             predicateTypes.Add(predicateName, new PredicateTypeInfo(parameters));
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine($"Error parsing PDDL predicates: {e.Message}");
    //         throw;
    //     }

    //     return predicateTypes;
    // }

/// <summary>
///  generates action types from a PDDL domain file  
/// </summary>
/// <param name="pddlFilePath"></param>
/// <returns></returns>

    // public static Dictionary<string, ActionTypeInfo> generateActionTypesfromPDDLDomain(string pddlFilePath)
    // {
    //     var actionTypes = new Dictionary<string, ActionTypeInfo>();
        
    //     try
    //     {
    //         string content = File.ReadAllText(pddlFilePath);
            
    //         // Debug the content
    //         Console.WriteLine("Action File Content:");
    //         Console.WriteLine(content);

    //         var actions = content.Split("(:action").Skip(1); // Skip first empty part

    //         foreach (var actionContent in actions)
    //         {
    //             string actionName = "";
    //             try
    //             {
    //                 // Debug each action content
    //                 Console.WriteLine($"\nParsing Action Content:");
    //                 Console.WriteLine(actionContent);

    //                 // Parse parameters section
    //                 int parametersStart = actionContent.IndexOf(":parameters");
    //                 int preconditionsStart = actionContent.IndexOf(":precondition");
    //                 string parametersSection = actionContent.Substring(
    //                     parametersStart, 
    //                     preconditionsStart - parametersStart
    //                 ).Trim();
    //                 Console.WriteLine($"Parameters section: {parametersSection}");

    //                 // Parse preconditions section
    //                 int effectsStart = actionContent.IndexOf(":effect");
    //                 string preconditionsSection = actionContent.Substring(
    //                     preconditionsStart, 
    //                     effectsStart - preconditionsStart
    //                 ).Trim();
    //                 Console.WriteLine($"Preconditions section: {preconditionsSection}");

    //                 // Parse effects section
    //                 string effectsSection = actionContent.Substring(effectsStart).Trim();
    //                 Console.WriteLine($"Effects section: {effectsSection}");

    //                 // Parse action name
    //                 actionName = actionContent.Split('\n')[0].Trim();

    //                 // Parse parameters
    //                 var parameters = new List<Parameter>();
    //                 if (parametersStart != -1 && preconditionsStart != -1)
    //                 {
    //                     var paramMatches = parametersSection.Split('?').Skip(1); // Skip first empty part
    //                     foreach (var param in paramMatches)
    //                     {
    //                         string[] parts = param.Trim().Split('-');
    //                         if (parts.Length == 2)
    //                         {
    //                             string paramName = parts[0].Trim();
    //                             string paramType = parts[1].Trim().TrimEnd(')').ToLower();

    //                             // First check if it's a base type
    //                             Type parameterType = paramType.ToLower() switch
    //                             {
    //                                 "location" => typeof(Location),
    //                                 "element" => typeof(Element),
    //                                 "agent" => typeof(Agent),
                                   
    //                                 "layer" => typeof(Layer),
    //                                 "module" => typeof(Module),
    //                                 _ => null
    //                             };

    //                             // If not a base type, look in registered types
    //                             if (parameterType == null)
    //                             {
    //                                 var registeredType = EntityFactory.Instance.registeredEntityTypes
    //                                     .FirstOrDefault(t => t.Key.Equals(paramType, StringComparison.OrdinalIgnoreCase));
                                    
    //                                 if (registeredType.Key == null)
    //                                 {
    //                                     throw new ArgumentException($"Action parameter type '{paramType}' not found in registered types");
    //                                 }
                                    
    //                                 parameterType = registeredType.Value;
    //                             }

    //                             parameters.Add(new Parameter(paramName, parameterType));
    //                         }
    //                     }
    //                 }

    //                 // Parse preconditions
    //                 var preconditions = new List<PredicateTemplate>();
    //                 if (preconditionsStart != -1 && effectsStart != -1)
    //                 {
    //                     // Extract predicates between (and ... )
    //                     int andStart = preconditionsSection.IndexOf("(and") + 4;
    //                     int andEnd = preconditionsSection.LastIndexOf(")");
    //                     if (andStart != -1 && andEnd != -1)
    //                     {
    //                         string predicates = preconditionsSection.Substring(andStart, andEnd - andStart);
    //                         foreach (var pred in predicates.Split('\n', StringSplitOptions.RemoveEmptyEntries))
    //                         {
    //                             string trimmedPred = pred.Trim();
    //                             if (string.IsNullOrWhiteSpace(trimmedPred)) continue;

    //                             Console.WriteLine($"Raw predicate: {trimmedPred}"); // Debug raw input

    //                             bool isNegated = trimmedPred.StartsWith("(not");
    //                             string predContent;
                                
    //                             if (isNegated)
    //                             {
    //                                 // Remove outer parentheses and 'not', and any trailing parentheses
    //                                 predContent = trimmedPred
    //                                     .Substring(5, trimmedPred.Length - 6)  // Remove (not ( and last ))
    //                                     .Trim()
    //                                     .TrimEnd(')');  // Remove any additional trailing )
    //                             }
    //                             else
    //                             {
    //                                 // Remove outer parentheses and any trailing ones
    //                                 predContent = trimmedPred
    //                                     .Trim()
    //                                     .TrimStart('(')
    //                                     .TrimEnd(')')
    //                                     .Trim();
    //                             }

    //                             Console.WriteLine($"After trimming: {predContent}"); // Debug after trimming

    //                             var parts = predContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //                             if (parts.Length > 0)
    //                             {
    //                                 string predicateName = parts[0].Trim().TrimStart('(').TrimEnd(')');
    //                                 Console.WriteLine($"Final predicate name: {predicateName}"); // Debug final name
    //                                 Console.WriteLine($"Registered predicates: {string.Join(", ", PredicateFactory.Instance.registeredPredicateTypes.Keys)}");

    //                                 if (!PredicateFactory.Instance.registeredPredicateTypes.ContainsKey(predicateName))
    //                                 {
    //                                     throw new ArgumentException($"Action '{actionName}' uses unregistered predicate '{predicateName}' in preconditions");
    //                                 }

    //                                 // Add the predicate template with cleaned parameters
    //                                 preconditions.Add(new PredicateTemplate(
    //                                     predicateName,
    //                                     CleanParameters(parts.Skip(1).ToArray()),
    //                                     isNegated
    //                                 ));
    //                             }
    //                         }
    //                     }
    //                 }

    //                 // Parse effects
    //                 var effects = new List<PredicateTemplate>();
    //                 if (effectsStart != -1)
    //                 {
    //                     // Extract predicates between (and ... )
    //                     int andStart = effectsSection.IndexOf("(and");
    //                     if (andStart != -1)
    //                     {
    //                         string effectsContent = effectsSection.Substring(andStart + 4);
    //                         Console.WriteLine($"Effects content: {effectsContent}"); // Debug output

    //                         // Split by newlines and process each effect
    //                         var effectLines = effectsContent.Split('\n')
    //                             .Select(l => l.Trim())
    //                             .Where(l => !string.IsNullOrWhiteSpace(l) && 
    //                                        !l.StartsWith("(and") && 
    //                                        !l.Equals(")") &&
    //                                        !l.EndsWith("))"));

    //                         foreach (var pred in effectLines)
    //                         {
    //                             string trimmedPred = pred.Trim();
    //                             Console.WriteLine($"Raw effect: {trimmedPred}"); // Debug raw input

    //                             bool isNegated = trimmedPred.StartsWith("(not");
    //                             string predContent;
                                
    //                             if (isNegated)
    //                             {
    //                                 // Remove outer parentheses and 'not', and any trailing parentheses
    //                                 predContent = trimmedPred
    //                                     .Substring(5, trimmedPred.Length - 6)  // Remove (not ( and last ))
    //                                     .Trim()
    //                                     .TrimEnd(')');  // Remove any additional trailing )
    //                             }
    //                             else
    //                             {
    //                                 // Remove outer parentheses and any trailing ones
    //                                 predContent = trimmedPred
    //                                     .Trim()
    //                                     .TrimStart('(')
    //                                     .TrimEnd(')')
    //                                     .Trim();
    //                             }

    //                             var parts = predContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //                             if (parts.Length > 0)
    //                             {
    //                                 string predicateName = parts[0].Trim().TrimStart('(').TrimEnd(')');
    //                                 Console.WriteLine($"Final effect name: {predicateName}"); // Debug final name

    //                                 if (!PredicateFactory.Instance.registeredPredicateTypes.ContainsKey(predicateName))
    //                                 {
    //                                     throw new ArgumentException($"Action '{actionName}' uses unregistered predicate '{predicateName}' in effects");
    //                                 }

    //                                 // Add the predicate template with cleaned parameters
    //                                 effects.Add(new PredicateTemplate(
    //                                     predicateName,
    //                                     CleanParameters(parts.Skip(1).ToArray()),
    //                                     isNegated
    //                                 ));
    //                             }
    //                         }
    //                     }
    //                 }

    //                 // Validate parameters against registered entity types
    //                 foreach (var param in parameters)
    //                 {
    //                     // Debug output
    //                     Console.WriteLine($"Checking type: {param.Type.Name}");
    //                     Console.WriteLine($"Registered types: {string.Join(", ", EntityFactory.Instance.registeredEntityTypes.Keys)}");
                        
    //                     // Check if it's a base type first
    //                     bool isBaseType = param.Type == typeof(Element) ||
    //                                      param.Type == typeof(Location) ||
    //                                      param.Type == typeof(Agent) ||
    //                                      param.Type == typeof(Layer) ||
    //                                      param.Type == typeof(Module);

    //                     if (!isBaseType && !EntityFactory.Instance.registeredEntityTypes.Any(t => t.Value == param.Type))
    //                     {
    //                         throw new ArgumentException($"Action '{actionName}' uses unregistered type '{param.Type.Name}'");
    //                     }
    //                 }

    //                 // Validate predicates in preconditions
    //                 foreach (var precond in preconditions)
    //                 {
    //                     if (!PredicateFactory.Instance.registeredPredicateTypes.ContainsKey(precond.PredicateName))
    //                     {
    //                         throw new ArgumentException($"Action '{actionName}' uses unregistered predicate '{precond.PredicateName}' in preconditions");
    //                     }
    //                 }

    //                 // Validate predicates in effects
    //                 foreach (var effect in effects)
    //                 {
    //                     if (!PredicateFactory.Instance.registeredPredicateTypes.ContainsKey(effect.PredicateName))
    //                     {
    //                         throw new ArgumentException($"Action '{actionName}' uses unregistered predicate '{effect.PredicateName}' in effects");
    //                     }
    //                 }

    //                 actionTypes.Add(actionName, new ActionTypeInfo(
    //                     parameters,
    //                     preconditions,
    //                     effects,
    //                     (parameters, deltaTime) => true
    //                 ));

    //                 Console.WriteLine($"Parsed action: {actionName}");
    //             }
    //             catch (Exception e)
    //             {
    //                 Console.WriteLine($"Error parsing action {actionName}: {e.Message}");
    //                 throw;
    //             }
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine($"Error parsing PDDL actions: {e.Message}");
    //         throw;
    //     }

    //     return actionTypes;
    // }

    // // Helper method to clean predicate parameters
    // private static string[] CleanParameters(string[] parameters)
    // {
    //     return parameters.Select(p => p.TrimStart('?').TrimEnd(')')).ToArray();
    // }


}
