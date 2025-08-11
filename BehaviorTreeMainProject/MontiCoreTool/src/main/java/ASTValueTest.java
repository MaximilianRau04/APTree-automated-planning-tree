import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTParameterInstanceDef;
import crf._ast.ASTParameterInstanceValue;
import java.util.Optional;
import java.lang.reflect.Method;

public class ASTValueTest {
    public static void main(String[] args) {
        try {
            System.out.println("TESTING: Testing AST Value Structure...");
            
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
                        
                        // Try to access the values
                        if (instance.getParameterInstanceValues() != null) {
                            System.out.println("  Has values: " + !instance.getParameterInstanceValues().isEmptyParameterInstanceValues());
                            if (!instance.getParameterInstanceValues().isEmptyParameterInstanceValues()) {
                                var value = instance.getParameterInstanceValues().getParameterInstanceValue(0);
                                System.out.println("  First value: " + value.toString());
                                System.out.println("  Value class: " + value.getClass().getName());
                                
                                // Try to inspect the value object
                                inspectASTValue(value);
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
    
    private static void inspectASTValue(ASTParameterInstanceValue value) {
        System.out.println("INSPECTING: Inspecting AST value...");
        System.out.println("  Class: " + value.getClass().getName());
        System.out.println("  toString(): " + value.toString());
        
        // Try to find all public methods
        Method[] methods = value.getClass().getMethods();
        System.out.println("  Available methods:");
        for (Method method : methods) {
            if (method.getParameterCount() == 0 && !method.getName().equals("getClass")) {
                try {
                    Object result = method.invoke(value);
                    System.out.println("    " + method.getName() + "() -> " + result);
                } catch (Exception e) {
                    System.out.println("    " + method.getName() + "() -> [ERROR: " + e.getMessage() + "]");
                }
            }
        }
    }
}
