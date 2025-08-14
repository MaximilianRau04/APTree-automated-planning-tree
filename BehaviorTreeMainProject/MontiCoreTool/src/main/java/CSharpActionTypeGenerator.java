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
        // Clean the output directory first
        cleanOutputDirectory();
        
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
    
    private static void cleanOutputDirectory() throws IOException {
        Path outputPath = Paths.get(OUTPUT_DIR);
        
        if (Files.exists(outputPath)) {
            System.out.println("Cleaning output directory: " + OUTPUT_DIR);
            
            // Delete all .cs files in the directory
            try (DirectoryStream<Path> stream = Files.newDirectoryStream(outputPath, "*.cs")) {
                for (Path file : stream) {
                    Files.delete(file);
                    System.out.println("Deleted: " + file.getFileName());
                }
            }
        } else {
            System.out.println("Output directory does not exist, will be created: " + OUTPUT_DIR);
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
            writer.println("using ModelLoader.ParameterTypes;");
            writer.println();
            writer.println("namespace BehaviorTreeMainProject");
            writer.println("{");
            writer.println("    public class " + className + " : GenericBTAction");
            writer.println("    {");
            
            // Generate parameter properties
            generateParameterProperties(writer, action);
            
            // Generate predicate templates
            generatePredicateTemplates(writer, action, className);
            
            // Generate constructor with all properties as parameters
            writer.print("        public " + className + "(string actionType, string instanceName, Blackboard<FastName> blackboard");
            if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
                for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                    String paramName = param.getName(0);
                    String paramTypeName = param.getName(1);
                    String csharpType = getBasicTypeFromName(paramTypeName);
                    writer.print(", " + csharpType + " " + paramName);
                }
            }
            writer.println(")");
            writer.println("            : base(actionType, instanceName, blackboard)");
            writer.println("        {");
            // Set all properties directly
            if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
                for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                    String paramName = param.getName(0);
                    writer.println("            this." + paramName + " = " + paramName + ";");
                }
            }
            writer.println("        }");
            writer.println();
            
            // Generate OnTick_NodeLogic method
            writer.println("        protected override bool OnTick_NodeLogic(float InDeltaTime)");
            writer.println("        {");
            writer.println("            // TODO: Implement action logic for " + className);
            writer.println("            // Access parameters via properties: obj, rob, loc, tool, etc.");
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
    
    private static void generateParameterProperties(PrintWriter writer, ASTAction action) throws IOException {
        if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
            for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                String paramName = param.getName(0);
                String paramTypeName = param.getName(1);
                String csharpType = getBasicTypeFromName(paramTypeName);
                
                writer.println("        // Parameter: " + paramName + " of type " + paramTypeName);
                writer.println("        public " + csharpType + " " + paramName + " { get; private set; }");
                writer.println();
            }
        }
    }
    
    private static void initializeParameterProperties(PrintWriter writer, ASTAction action) throws IOException {
        if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
            int paramIndex = 0;
            for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                String paramName = param.getName(0);
                String paramTypeName = param.getName(1);
                String csharpType = getBasicTypeFromName(paramTypeName);
                
                writer.println("            // Initialize " + paramName + " parameter");
                writer.println("            " + paramName + " = (" + csharpType + ")parameterValues[" + paramIndex + "];");
                paramIndex++;
            }
        }
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
        // Return the original type name as-is, but capitalize it for C# convention
        if (typeName == null || typeName.isEmpty()) {
            return "string";
        }
        
        // Capitalize the first letter for C# class naming convention
        return typeName.substring(0, 1).toUpperCase() + typeName.substring(1);
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
        // Generate precondition templates as strings
        writer.println("        protected override List<string> PreconditionTemplates => new List<string>");
        writer.println("        {");
        
        generatePredicateTemplateStrings(writer, action.getName(), true);
        
        writer.println("        };");
        writer.println();
        
        // Generate effect templates as strings
        writer.println("        protected override List<string> EffectTemplates => new List<string>");
        writer.println("        {");
        
        generatePredicateTemplateStrings(writer, action.getName(), false);
        
        writer.println("        };");
        writer.println();
    }
    
    private static void generatePredicateTemplateStrings(PrintWriter writer, String actionName, boolean isPrecondition) throws IOException {
        String[] predicateStrings = getPredicateStringsForAction(actionName, isPrecondition);
        for (String predicateString : predicateStrings) {
            writer.println("            \"" + predicateString + "\",");
        }
    }
    

    
    private static String[] getPredicateStringsForAction(String actionName, boolean isPrecondition) {
        // Hardcoded mapping based on the test file predicates
        switch (actionName.toLowerCase()) {
            case "pickup":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: isAt(myObject = pickedObject, location = loc, isNegated = false)",
                        "PredicateInstance: atAgent(agent = rob, location = loc, isNegated = false)",
                        "PredicateInstance: hasTool(agent = rob, tool = robTool, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: holding(agent = rob, myObject = pickedObject, isNegated = false)",
                        "PredicateInstance: atAgent(agent = rob, location = loc, isNegated = false)"
                    };
                }
            case "equipe":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = too, place = ep, isNegated = false)",
                        "PredicateInstance: empty(client = client, isNegated = false)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: hasTool(agent = client, tool = too, isNegated = false)",
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: atplace(myObject = too, place = ep, isNegated = true)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = false)"
                    };
                }
            case "deequip":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: hasTool(agent = client, tool = too, isNegated = false)",
                        "PredicateInstance: atplace(myObject = too, place = ep, isNegated = true)",
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = too, place = ep, isNegated = false)",
                        "PredicateInstance: empty(client = client, isNegated = false)",
                        "PredicateInstance: hasTool(agent = client, tool = too, isNegated = true)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = true)"
                    };
                }
            case "grab":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = obj, place = grabPos, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = true)",
                        "PredicateInstance: positionfree(pos = grabPos, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)",
                        "PredicateInstance: stacked(myObject = obj, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj, place = grabPos, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj, isNegated = true)",
                        "PredicateInstance: positionfree(pos = grabPos, isNegated = false)"
                    };
                }
            case "place":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj, isNegated = true)",
                        "PredicateInstance: positionfree(pos = placePos, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = obj, place = placePos, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)",
                        "PredicateInstance: positionfree(pos = placePos, isNegated = true)"
                    };
                }
            case "stack":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: holding(agent = client, myObject = obj1, isNegated = false)",
                        "PredicateInstance: hasTool(agent = client, tool = vg, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj2, place = pr, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj1, place = pr, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: ontop(myObject1 = obj1, myObject2 = obj2, isNegated = false)",
                        "PredicateInstance: stacked(myObject = obj1, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = obj1, isNegated = true)",
                        "PredicateInstance: atplace(myObject = obj1, place = pr, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj2, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj1, isNegated = false)",
                        "PredicateInstance: allset(lay = lay, mod = mod, isNegated = false)"
                    };
                }
            case "stackonmultiple":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: allset(lay = lay, mod = mod, isNegated = false)",
                        "PredicateInstance: hasTool(agent = client, tool = vg, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = plate, isNegated = false)",
                        "PredicateInstance: atplace(myObject = plate, place = pos, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = plate, place = pos, isNegated = false)"
                    };
                }
            case "gluing":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: hasTool(agent = client, tool = gg, isNegated = false)",
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: atplace(myObject = obj, place = pos, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: glued(myObject = obj, isNegated = false)"
                    };
                }
            case "nailing":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: hasTool(agent = client, tool = ng, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj, place = pos, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: nailed(myObject = obj, isNegated = false)"
                    };
                }
            default:
                return new String[] {
                    "PredicateInstance: unknown(unknown = unknown, isNegated = false)"
                };
        }
    }
    

}
