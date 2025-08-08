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
            System.out.println("🔍 Generating C# Action Type Classes...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                generateCSharpClasses(ast);
                System.out.println("✅ C# action type classes generated successfully!");
            } else {
                System.out.println("❌ Failed to parse CRF model");
            }
            
        } catch (Exception e) {
            System.err.println("❌ ERROR: " + e.getMessage());
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
            writer.println();
            writer.println("namespace ModelLoader.ActionTypes");
            writer.println("{");
            writer.println("    public class " + className + " : BTActionNodeBase");
            writer.println("    {");
            
            // Generate properties for parameters
            if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
                for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                    String paramName = param.getName(0); // First name is the parameter name
                    String paramTypeRef = param.getName(1); // Second name is the parameter type reference
                    String paramType = getParameterTypeFromReference(paramTypeRef);
                    writer.println("        public " + paramType + " " + capitalizeFirst(paramName) + " { get; set; }");
                }
            }
            
            writer.println();
            
            // Generate constructor
            writer.print("        public " + className + "(");
            if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
                for (int i = 0; i < action.getActionParametersBlock().getParameterInstanceList().size(); i++) {
                    ASTParameterInstance param = action.getActionParametersBlock().getParameterInstanceList().get(i);
                    String paramType = getParameterTypeFromReference(param.getName(1));
                    String paramName = param.getName(0);
                    writer.print(paramType + " " + paramName);
                    if (i < action.getActionParametersBlock().getParameterInstanceList().size() - 1) {
                        writer.print(", ");
                    }
                }
            }
            writer.println(")");
            writer.println("        {");
            
            // Generate constructor body
            if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
                for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                    String paramName = param.getName(0);
                    writer.println("            this." + capitalizeFirst(paramName) + " = " + paramName + ";");
                }
            }
            
            writer.println("        }");
            writer.println();
            
            // Generate OnTick_NodeLogic method
            writer.println("        protected override bool OnTick_NodeLogic(float InDeltaTime)");
            writer.println("        {");
            
            // Add implementation from the action definition
            if (action.getImplementationBlock() != null && action.getImplementationBlock().getFunctionReference() != null) {
                String functionName = action.getImplementationBlock().getFunctionReference().getName();
                writer.println("            // Call implementation function: " + functionName);
                writer.println("            return " + functionName + "();");
            } else {
                writer.println("            // TODO: Implement action logic");
                writer.println("            return true;");
            }
            
            writer.println("        }");
            writer.println("    }");
            writer.println("}");
            
            System.out.println("✅ Generated: " + fileName);
        }
    }
    
    private static String capitalizeFirst(String str) {
        if (str == null || str.isEmpty()) {
            return str;
        }
        return str.substring(0, 1).toUpperCase() + str.substring(1);
    }
    
    private static String getParameterTypeFromReference(String paramTypeRef) {
        // Map parameter type references to their actual types
        // This should be based on the parameter types defined in the file
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
}
