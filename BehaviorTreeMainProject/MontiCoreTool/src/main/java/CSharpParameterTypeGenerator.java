import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTParameterTypeDef;
import crf._ast.ASTParameterProperty;
import crf._ast.ASTParameterPropertyList;
import java.util.Optional;
import java.io.*;
import java.nio.file.*;

public class CSharpParameterTypeGenerator {
    
    private static final String OUTPUT_DIR = "C:/Users/sherk/Documents/BehaviorTreeMainProject/BehaviorTreeMainProject/src/ModelLoader/ParameterTypes";
    
    public static void main(String[] args) {
        try {
            System.out.println("Generating C# Parameter Type Classes...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                generateCSharpClasses(ast);
                System.out.println("C# parameter type classes generated successfully!");
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
        
        if (ast.getParameterTypeDefList() != null) {
            for (ASTParameterTypeDef parameterType : ast.getParameterTypeDefList()) {
                generateParameterTypeClass(parameterType);
            }
        }
    }
    
    public static void generateParameterTypeClass(ASTParameterTypeDef parameterType) throws IOException {
        String className = capitalizeFirst(parameterType.getName());
        String fileName = className + ".cs";
        String filePath = OUTPUT_DIR + "/" + fileName;
        
        try (PrintWriter writer = new PrintWriter(new FileWriter(filePath))) {
            // Generate the C# class
            writer.println("using System;");
            writer.println();
            writer.println("namespace ModelLoader.ParameterTypes");
            writer.println("{");
            
            // Determine inheritance
            String baseType = getBasicTypeName(parameterType.getBasicType());
            String inheritance = getInheritance(baseType);
            
            // Debug output
            System.out.println("Debug: " + className + " baseType=" + baseType + " inheritance=" + inheritance);
            
            writer.println("    public class " + className + " : " + inheritance);
            writer.println("    {");
            
            // Generate properties
            if (parameterType.isPresentParameterPropertyList()) {
                ASTParameterPropertyList propertyList = parameterType.getParameterPropertyList();
                for (ASTParameterProperty property : propertyList.getParameterPropertyList()) {
                    String propertyName = property.getName();
                    String propertyType = getBasicTypeName(property.getBasicType());
                    writer.println("        public " + propertyType + " " + capitalizeFirst(propertyName) + " { get; set; }");
                }
            }
            
            writer.println();
            
            // Generate empty constructor - required by Entity
            writer.println("        // Empty constructor - required by Entity");
            writer.println("        public " + className + "() : base()");
            writer.println("        {");
            writer.println("            BaseType = new FastName(\"" + baseType + "\");");
            writer.println("            // TypeName is automatically set in base constructor");
            writer.println("        }");
            writer.println();
            
            // Generate constructor with parameters (if any properties exist)
            if (parameterType.isPresentParameterPropertyList()) {
                ASTParameterPropertyList propertyList = parameterType.getParameterPropertyList();
                if (!propertyList.isEmptyParameterPropertys()) {
                    writer.println("        // Constructor with parameters");
                    writer.print("        public " + className + "(");
                    
                    for (int i = 0; i < propertyList.sizeParameterPropertys(); i++) {
                        ASTParameterProperty property = propertyList.getParameterProperty(i);
                        String propertyType = getBasicTypeName(property.getBasicType());
                        String propertyName = property.getName();
                        writer.print(propertyType + " " + propertyName);
                        if (i < propertyList.sizeParameterPropertys() - 1) {
                            writer.print(", ");
                        }
                    }
                    writer.println(") : this()");
                    writer.println("        {");
                    
                    // Generate constructor body
                    for (ASTParameterProperty property : propertyList.getParameterPropertyList()) {
                        String propertyName = property.getName();
                        writer.println("            this." + capitalizeFirst(propertyName) + " = " + propertyName + ";");
                    }
                    
                    writer.println("        }");
                    writer.println();
                    
                    // Generate constructor with name and parameters
                    writer.println("        // Constructor with name and parameters");
                    writer.print("        public " + className + "(string name, ");
                    
                    for (int i = 0; i < propertyList.sizeParameterPropertys(); i++) {
                        ASTParameterProperty property = propertyList.getParameterProperty(i);
                        String propertyType = getBasicTypeName(property.getBasicType());
                        String propertyName = property.getName();
                        writer.print(propertyType + " " + propertyName);
                        if (i < propertyList.sizeParameterPropertys() - 1) {
                            writer.print(", ");
                        }
                    }
                    writer.println(") : base(name)");
                    writer.println("        {");
                    
                    // Generate constructor body
                    for (ASTParameterProperty property : propertyList.getParameterPropertyList()) {
                        String propertyName = property.getName();
                        writer.println("            this." + capitalizeFirst(propertyName) + " = " + propertyName + ";");
                    }
                    writer.println("            BaseType = new FastName(\"" + baseType + "\");");
                    writer.println("            // TypeName is automatically set in base constructor");
                    writer.println("        }");
                }
            }
            
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
    
    private static String getInheritance(String baseType) {
        // Since base types like Element, Agent, etc. already inherit from Entity,
        // we only need to inherit from the base type
        switch (baseType) {
            case "Element":
            case "Agent":
            case "Location":
            case "Layer":
            case "Module":
            case "Tool":
                return baseType; // Just inherit from the base type (which already inherits from Entity)
            default:
                return "Entity"; // For primitive types, inherit directly from Entity
        }
    }
}
