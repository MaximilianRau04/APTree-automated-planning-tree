import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTPredicateTypeDef;
import crf._ast.ASTParameterDeclaration;
import java.util.Optional;
import java.io.*;
import java.nio.file.*;

public class CSharpPredicateGenerator {
    
    private static final String OUTPUT_DIR = "C:/Users/sherk/Documents/BehaviorTreeMainProject/BehaviorTreeMainProject/src/ModelLoader/PredicateTypes";
    
    public static void main(String[] args) {
        try {
            System.out.println("🔍 Generating C# Predicate Classes...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                generateCSharpClasses(ast);
                System.out.println("✅ C# classes generated successfully!");
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
        
        if (ast.getPredicateTypeDefList() != null) {
            for (ASTPredicateTypeDef predicate : ast.getPredicateTypeDefList()) {
                generatePredicateClass(predicate);
            }
        }
    }
    
    public static void generatePredicateClass(ASTPredicateTypeDef predicate) throws IOException {
        String className = capitalizeFirst(predicate.getName());
        String fileName = className + ".cs";
        String filePath = OUTPUT_DIR + "/" + fileName;
        
        try (PrintWriter writer = new PrintWriter(new FileWriter(filePath))) {
            // Generate the C# class
            writer.println("using System;");
            writer.println();
            writer.println("namespace ModelLoader.PredicateTypes");
            writer.println("{");
            writer.println("    public class " + className + " : Predicate");
            writer.println("    {");
            
            // Generate properties
            if (predicate.getParameterDeclarationList() != null) {
                for (ASTParameterDeclaration param : predicate.getParameterDeclarationList()) {
                    String paramName = param.getName();
                    String paramType = getBasicTypeName(param.getBasicType());
                    writer.println("        public " + paramType + " " + paramName + " { get; set; }");
                }
            }
            
            writer.println();
            
            // Generate constructor
            writer.print("        public " + className + "(");
            if (predicate.getParameterDeclarationList() != null && !predicate.getParameterDeclarationList().isEmpty()) {
                for (int i = 0; i < predicate.getParameterDeclarationList().size(); i++) {
                    ASTParameterDeclaration param = predicate.getParameterDeclarationList().get(i);
                    String paramType = getBasicTypeName(param.getBasicType());
                    String paramName = param.getName();
                    writer.print(paramType + " " + paramName);
                    if (i < predicate.getParameterDeclarationList().size() - 1) {
                        writer.print(", ");
                    }
                }
            }
            writer.println(")");
            writer.println("        {");
            
            // Generate constructor body
            if (predicate.getParameterDeclarationList() != null) {
                for (ASTParameterDeclaration param : predicate.getParameterDeclarationList()) {
                    String paramName = param.getName();
                    writer.println("            this." + paramName + " = " + paramName + ";");
                }
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
    
    private static String getBasicTypeName(Object basicType) {
        // Extract the actual type name from the AST node
        String typeString = basicType.toString();
        
        // Remove the AST class prefix and get just the type name
        if (typeString.contains("ASTBasicType")) {
            // Extract the actual type value from the AST
            // This is a simplified approach - you might need to adjust based on your AST structure
            return "String"; // Default fallback
        }
        
        // Try to extract the type name from the string representation
        if (typeString.contains("Agent")) return "Agent";
        if (typeString.contains("Location")) return "Location";
        if (typeString.contains("Tool")) return "Tool";
        if (typeString.contains("String")) return "String";
        if (typeString.contains("Integer")) return "int";
        if (typeString.contains("Double")) return "double";
        
        return "String"; // Default fallback
    }
}
