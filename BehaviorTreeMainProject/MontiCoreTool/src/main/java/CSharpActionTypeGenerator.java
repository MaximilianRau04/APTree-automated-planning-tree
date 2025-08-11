import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTAction;
import crf._ast.ASTParameterInstance;
import java.util.Optional;
import java.io.*;
import java.nio.file.*;

public class CSharpActionTypeGenerator {
    
    private static final String OUTPUT_DIR = "C:/Users/sherk/Documents/BehaviorTreeMainProject/BehaviorTreeMainProject/src/ModelLoader/ActionTypes";
    
    public static void main(String[] args) {
        try {
            System.out.println("Generating C# Action Type Classes...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                generateCSharpClasses(ast);
                System.out.println("C# action type classes generated successfully!");
            } else {
                System.out.println("Failed to parse CRF model");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    public static void generateCSharpClasses(ASTAllowedType ast) throws IOException {
        // Ensure output directory exists
        Files.createDirectories(Paths.get(OUTPUT_DIR));
        
        System.out.println("Debug: Checking AST for Action nodes...");
        System.out.println("Debug: Action list is null? " + (ast.getActionList() == null));
        
        if (ast.getActionList() != null) {
            System.out.println("Debug: Found " + ast.getActionList().size() + " Action nodes");
            for (ASTAction action : ast.getActionList()) {
                System.out.println("Debug: Processing Action: " + action.getName());
                generateActionTypeClass(action);
            }
        } else {
            System.out.println("Debug: No Action nodes found in AST");
        }
    }
    
    public static void generateActionTypeClass(ASTAction action) throws IOException {
        String className = capitalizeFirst(action.getName());
        String fileName = className + ".cs";
        String filePath = OUTPUT_DIR + "/" + fileName;
        
        try (PrintWriter writer = new PrintWriter(new FileWriter(filePath))) {
            // Generate the C# class
            writer.println("using System;");
            writer.println("using System.Collections.Generic;");
            writer.println();
            writer.println("namespace ModelLoader.ActionTypes");
            writer.println("{");
            writer.println("    public class " + className + " : GenericBTAction");
            writer.println("    {");
            
            // Generate predicate templates
            generatePredicateTemplates(writer, action, className);
            
            // Generate constructor with automatic predicate instantiation
                         writer.print("        public " + className + "(string actionType, string instanceName, Blackboard blackboard, List<Parameter> parameters, object[] parameterValues");
            writer.println(")");
            writer.println("            : base(actionType, instanceName, blackboard, parameters, parameterValues)");
            writer.println("        {");
            writer.println("            // Parameters and predicates are handled by the base class");
            writer.println("        }");
            writer.println();
            
            // Generate OnTick_NodeLogic method
            writer.println("        protected override bool OnTick_NodeLogic(float InDeltaTime)");
            writer.println("        {");
            writer.println("            // TODO: Implement action logic for " + className);
            writer.println("            // Access parameters via: parameterValues[index] or parameters[index]");
            writer.println("            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);");
            writer.println("        }");
            writer.println("    }");
            writer.println("}");
            
                         System.out.println("Generated: " + fileName);
        }
    }
    
    private static String capitalizeFirst(String str) {
        if (str == null || str.isEmpty()) {
            return str;
        }
        return str.substring(0, 1).toUpperCase() + str.substring(1);
    }
    
    private static String getParameterTypeFromReference(String paramTypeRef, ASTAction action) {
        // First, try to find the parameter in the action's parameter list
        if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
            for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                if (param.getName(0).equals(paramTypeRef)) {
                    // Found the parameter, now get its type
                    String paramTypeName = param.getName(1);
                    return getBasicTypeFromName(paramTypeName);
                }
            }
        }
        
        // Fallback to the original mapping
        switch (paramTypeRef.toLowerCase()) {
            case "beam":
            case "plate":
                return "Element";
            case "robot":
                return "Agent";
            case "firstlocation":
            case "positiononrail":
                return "Location";
            default:
                System.out.println("Debug getParameterTypeFromReference: Unknown reference '" + paramTypeRef + "', returning 'string'");
                return "string"; // Default fallback
        }
    }
    
    private static String getBasicTypeFromName(String typeName) {
        switch (typeName.toLowerCase()) {
            case "element":
                return "Element";
            case "agent":
                return "Agent";
            case "location":
                return "Location";
            case "layer":
                return "Layer";
            case "module":
                return "Module";
            case "tool":
                return "Tool";
            case "string":
                return "string";
            case "double":
                return "double";
            case "integer":
                return "int";
            case "boolean":
                return "bool";
            default:
                return "string"; // Default fallback
        }
    }
    
    private static String getBasicTypeName(Object basicType) {
        // Check if the basicType is an ASTBasicType and use the named alternative methods
        if (basicType instanceof crf._ast.ASTBasicType) {
            crf._ast.ASTBasicType astBasicType = (crf._ast.ASTBasicType) basicType;
            
            // Use the named alternative methods to determine the type
            if (astBasicType.isPresentElement()) {
                return "Element";
            } else if (astBasicType.isPresentAgent()) {
                return "Agent";
            } else if (astBasicType.isPresentLocation()) {
                return "Location";
            } else if (astBasicType.isPresentLayer()) {
                return "Layer";
            } else if (astBasicType.isPresentModule()) {
                return "Module";
            } else if (astBasicType.isPresentTool()) {
                return "Tool";
            } else if (astBasicType.isPresentString()) {
                return "string";
            } else if (astBasicType.isPresentDouble()) {
                return "double";
            } else if (astBasicType.isPresentInteger()) {
                return "int";
            }
        }
        
        // Fallback: check the class name as before
        String className = basicType.getClass().getSimpleName();
        System.out.println("Debug getBasicTypeName: class = '" + className + "'");
        
        // Map specific AST classes to their corresponding types
        switch (className) {
            case "ASTElementType":
                return "Element";
            case "ASTAgentType":
                return "Agent";
            case "ASTLocationType":
                return "Location";
            case "ASTLayerType":
                return "Layer";
            case "ASTModuleType":
                return "Module";
            case "ASTToolType":
                return "Tool";
            case "ASTStringType":
                return "string";
            case "ASTIntegerType":
                return "int";
            case "ASTDoubleType":
                return "double";
            default:
                System.out.println("Debug getBasicTypeName: Unknown class '" + className + "', returning 'string'");
                return "string"; // Default fallback
        }
    }
    
    private static void generatePredicateTemplates(PrintWriter writer, ASTAction action, String className) throws IOException {
        // Generate precondition templates
        writer.println("        protected override List<ActionPredicateTemplate> PreconditionTemplates => new List<ActionPredicateTemplate>");
        writer.println("        {");
        
        if (action.getPreconditionBlock() != null && action.getPreconditionBlock().getPredicateInstanceList() != null) {
            for (var predicateInstance : action.getPreconditionBlock().getPredicateInstanceList()) {
                generatePredicateTemplate(writer, predicateInstance, action);
            }
        }
        
        writer.println("        };");
        writer.println();
        
        // Generate effect templates
        writer.println("        protected override List<ActionPredicateTemplate> EffectTemplates => new List<ActionPredicateTemplate>");
        writer.println("        {");
        
        if (action.getEffectBlock() != null && action.getEffectBlock().getPredicateInstanceList() != null) {
            for (var predicateInstance : action.getEffectBlock().getPredicateInstanceList()) {
                generatePredicateTemplate(writer, predicateInstance, action);
            }
        }
        
        writer.println("        };");
        writer.println();
    }
    
    private static void generatePredicateTemplate(PrintWriter writer, crf._ast.ASTPredicateInstance predicateInstance, ASTAction action) throws IOException {
        try {
            String predicateName = predicateInstance.getName();
            System.out.println("Debug: Processing predicate: " + predicateName);
            
            writer.println("            new ActionPredicateTemplate(\"" + predicateName + "\", new List<PredicateParameterMapping>");
            writer.println("            {");
            
            if (predicateInstance.getPredicateArgumentList() != null) {
                System.out.println("Debug: Found " + predicateInstance.getPredicateArgumentList().size() + " arguments");
                for (var argument : predicateInstance.getPredicateArgumentList()) {
                    try {
                        String predicateParamName = argument.getName();
                        System.out.println("Debug: Predicate param name: " + predicateParamName);
                        
                                                 // For now, just use the predicate parameter name as the action parameter name
                         // This avoids the Monticore AST issues entirely
                         String actionParamName = predicateParamName;
                         System.out.println("Debug: Using predicate param name as action param name: " + actionParamName);
                        
                        String paramType = getParameterTypeFromReference(actionParamName, action);
                        System.out.println("Debug: Parameter type: " + paramType);
                        
                        writer.println("                new PredicateParameterMapping(\"" + actionParamName + "\", \"" + predicateParamName + "\", \"" + paramType + "\"),");
                    } catch (Exception e) {
                        System.out.println("Debug: Error processing argument: " + e.getMessage());
                        // Continue with next argument
                    }
                }
            } else {
                System.out.println("Debug: No predicate arguments found");
            }
            
            writer.println("            }),");
        } catch (Exception e) {
            System.out.println("Debug: Error processing predicate template: " + e.getMessage());
            e.printStackTrace();
            // Write a minimal template to avoid breaking the generation
            writer.println("            new ActionPredicateTemplate(\"unknown\", new List<PredicateParameterMapping>()),");
        }
    }
}
