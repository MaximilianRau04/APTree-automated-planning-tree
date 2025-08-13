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
            System.out.println("GENERATING: Generating C# Predicate Classes...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                generateCSharpClasses(ast);
                System.out.println("SUCCESS: C# classes generated successfully!");
            } else {
                System.out.println("FAILED: Failed to parse CRF model");
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
        
        if (ast.getPredicateTypeDefList() != null) {
            for (ASTPredicateTypeDef predicate : ast.getPredicateTypeDefList()) {
                generatePredicateClass(predicate);
            }
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
            
            System.out.println("SUCCESS: Generated: " + fileName);
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
                return "String";
            } else if (astBasicType.isPresentDouble()) {
                return "double";
            } else if (astBasicType.isPresentInteger()) {
                return "int";
            } else if (astBasicType.isPresentBoolean()) {
                return "bool";
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
                return "String";
            case "ASTIntegerType":
                return "int";
            case "ASTDoubleType":
                return "double";
            case "ASTBooleanType":
                return "bool";
            default:
                System.err.println("WARNING: Could not determine type for: " + className);
                return "String"; // Default fallback
        }
    }
}

