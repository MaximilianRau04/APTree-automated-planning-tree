import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTParameterInstanceDef;
import crf._ast.ASTParameterInstanceValue;
import java.util.Optional;
import java.io.*;
import java.lang.reflect.Method;

public class ParameterInstanceExtractor {
    
    private static final String OUTPUT_FILE = "C:/Users/sherk/Documents/BehaviorTreeMainProject/BehaviorTreeMainProject/src/Blackboard/parameter_instances.txt";
    
    public static void main(String[] args) {
        try {
            System.out.println("Extracting Parameter Instances...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                extractParameterInstances(ast);
                System.out.println("Parameter instances extracted successfully!");
            } else {
                System.out.println("Failed to parse CRF model");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    private static void extractParameterInstances(ASTAllowedType ast) throws IOException {
        System.out.println("EXTRACTING: Extracting parameter instances...");
        
        StringBuilder output = new StringBuilder();
        output.append("# Parameter Instances extracted from CRF model\n");
        output.append("# Format: type,name\n");
        output.append("# Example: beam,b1\n\n");
        
        if (ast.getParameterInstanceDefList() != null) {
            System.out.println("FOUND: Found " + ast.getParameterInstanceDefList().size() + " parameter instance definitions");
            
            for (ASTParameterInstanceDef instanceDef : ast.getParameterInstanceDefList()) {
                String parameterTypeName = instanceDef.getName();
                System.out.println("PROCESSING: Processing type: " + parameterTypeName);
                
                if (instanceDef.getParameterInstanceValues() != null && 
                    !instanceDef.getParameterInstanceValues().isEmptyParameterInstanceValues()) {
                    
                    for (int i = 0; i < instanceDef.getParameterInstanceValues().sizeParameterInstanceValues(); i++) {
                        ASTParameterInstanceValue value = instanceDef.getParameterInstanceValues().getParameterInstanceValue(i);
                        String instanceName = extractValueFromAST(value);
                        
                        System.out.println("SUCCESS: Found instance: " + instanceName + " of type: " + parameterTypeName);
                        output.append(parameterTypeName).append(",").append(instanceName).append("\n");
                    }
                }
            }
        }
        
        // Write to file
        try (PrintWriter writer = new PrintWriter(new FileWriter(OUTPUT_FILE))) {
            writer.print(output.toString());
        }
        
        System.out.println("WRITTEN: Written " + output.toString().split("\n").length + " lines to " + OUTPUT_FILE);
    }
    
    private static String extractValueFromAST(ASTParameterInstanceValue value) {
        try {
            Object astValue = value.getValue();
            
            if (astValue != null) {
                try {
                    Method[] methods = astValue.getClass().getMethods();
                    for (Method method : methods) {
                        if (method.getParameterCount() == 0 && 
                            (method.getName().equals("getValue") || 
                             method.getName().equals("getName") ||
                             method.getName().equals("toString"))) {
                            Object result = method.invoke(astValue);
                            if (result != null && !result.toString().contains("@")) {
                                return result.toString();
                            }
                        }
                    }
                } catch (Exception e) {
                    System.out.println("WARNING: Error extracting value from ASTValue: " + e.getMessage());
                }
                
                String astValueStr = astValue.toString();
                if (!astValueStr.contains("@")) {
                    return astValueStr;
                }
            }
        } catch (Exception e) {
            System.out.println("WARNING: Error getting ASTValue: " + e.getMessage());
        }
        
        return "instance_" + System.identityHashCode(value);
    }
}
