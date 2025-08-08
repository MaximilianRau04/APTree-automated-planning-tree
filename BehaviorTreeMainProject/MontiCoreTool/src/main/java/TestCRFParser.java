import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import java.util.Optional;

public class TestCRFParser {
    public static void main(String[] args) {
        try {
            System.out.println("🔍 Testing CRF Multiple Predicates Parser...");
            
            CRFParser parser = new CRFParser();
            
            // Test parsing multiple predicate definitions
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                System.out.println("✅ SUCCESS: CRF Multiple Predicates parsed successfully!");
                ASTAllowedType ast = result.get();
                System.out.println("AST: " + ast.toString());
                
                // Print details about the parsed predicates
                if (ast.getPredicateTypeDefList() != null) {
                    System.out.println("Number of Predicates: " + ast.getPredicateTypeDefList().size());
                    for (int i = 0; i < ast.getPredicateTypeDefList().size(); i++) {
                        var predicate = ast.getPredicateTypeDefList().get(i);
                        System.out.println("Predicate " + (i+1) + ": " + predicate.getName());
                        if (predicate.getParameterDeclarationList() != null) {
                            System.out.println("  Parameters: " + predicate.getParameterDeclarationList().size());
                            for (var param : predicate.getParameterDeclarationList()) {
                                System.out.println("    - " + param.toString());
                            }
                        }
                    }
                }
                
            } else {
                System.out.println("❌ FAILED: Could not parse CRF Multiple Predicates");
            }
            
        } catch (Exception e) {
            System.err.println("❌ ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
