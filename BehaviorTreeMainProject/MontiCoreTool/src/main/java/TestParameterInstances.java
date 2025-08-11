import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTParameterInstanceDef;
import java.util.Optional;

public class TestParameterInstances {
    public static void main(String[] args) {
        try {
            System.out.println("TESTING: Testing Parameter Instance AST Structure...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                System.out.println("SUCCESS: CRF parsed successfully!");
                ASTAllowedType ast = result.get();
                
                // Print details about parameter instances
                if (ast.getParameterInstanceDefList() != null) {
                    System.out.println("Number of Parameter Instances: " + ast.getParameterInstanceDefList().size());
                    for (int i = 0; i < ast.getParameterInstanceDefList().size(); i++) {
                        var instance = ast.getParameterInstanceDefList().get(i);
                        System.out.println("Parameter Instance " + (i+1) + ": " + instance.getName());
                        System.out.println("  Full AST: " + instance.toString());
                        
                        // Try to access the values
                        if (instance.getParameterInstanceValues() != null) {
                            System.out.println("  Has values: " + !instance.getParameterInstanceValues().isEmptyParameterInstanceValues());
                            if (!instance.getParameterInstanceValues().isEmptyParameterInstanceValues()) {
                                var value = instance.getParameterInstanceValues().getParameterInstanceValue(0);
                                System.out.println("  First value: " + value.toString());
                            }
                        }
                    }
                } else {
                    System.out.println("No parameter instances found");
                }
                
            } else {
                System.out.println("FAILED: Could not parse CRF");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
