import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTParameterInstanceDef;
import crf._ast.ASTParameterInstanceValue;
import java.util.Optional;

public class TestMultipleInstances {
    
    public static void main(String[] args) {
        try {
            System.out.println("Testing Multiple Parameter Instances...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                analyzeParameterInstances(ast);
            } else {
                System.out.println("Failed to parse CRF model");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    private static void analyzeParameterInstances(ASTAllowedType ast) {
        System.out.println("ANALYZING: Analyzing parameter instances...");
        
        if (ast.getParameterInstanceDefList() != null) {
            System.out.println("TOTAL: Total parameter instance definitions: " + ast.getParameterInstanceDefList().size());
            
            for (int i = 0; i < ast.getParameterInstanceDefList().size(); i++) {
                ASTParameterInstanceDef instanceDef = ast.getParameterInstanceDefList().get(i);
                System.out.println("\nANALYZING: Parameter Instance Definition " + (i + 1) + ":");
                System.out.println("  Name: " + instanceDef.getName());
                System.out.println("  Has values: " + (instanceDef.getParameterInstanceValues() != null));
                
                if (instanceDef.getParameterInstanceValues() != null) {
                    System.out.println("  Values count: " + instanceDef.getParameterInstanceValues().sizeParameterInstanceValues());
                    
                    for (int j = 0; j < instanceDef.getParameterInstanceValues().sizeParameterInstanceValues(); j++) {
                        ASTParameterInstanceValue value = instanceDef.getParameterInstanceValues().getParameterInstanceValue(j);
                        System.out.println("    Value " + (j + 1) + ": " + value);
                        
                        // Try to extract the actual value
                        try {
                            Object astValue = value.getValue();
                            System.out.println("    ASTValue: " + astValue);
                            
                            if (astValue != null) {
                                // Try reflection to get the name
                                try {
                                    Object name = astValue.getClass().getMethod("getName").invoke(astValue);
                                    System.out.println("    Extracted name: " + name);
                                } catch (Exception e) {
                                    System.out.println("    Could not extract name: " + e.getMessage());
                                }
                            }
                        } catch (Exception e) {
                            System.out.println("    Error getting ASTValue: " + e.getMessage());
                        }
                    }
                }
            }
        } else {
            System.out.println("WARNING: No parameter instances found in AST");
        }
    }
}
