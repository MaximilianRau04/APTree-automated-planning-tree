import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTPredicateTypeDef;
import crf._ast.ASTParameterDeclaration;
import java.util.Optional;

public class ASTDebugTest {
    public static void main(String[] args) {
        try {
            System.out.println("DEBUG: Testing AST Parameter Declaration Structure...");
            
            CRFParser parser = new CRFParser();
            Optional<ASTAllowedType> result = parser.parse("src/test/resources/valid/crf/test_crf.txt");
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                
                if (ast.getPredicateTypeDefList() != null) {
                    for (ASTPredicateTypeDef predicate : ast.getPredicateTypeDefList()) {
                        System.out.println("Predicate: " + predicate.getName());
                        
                        if (predicate.getParameterDeclarationList() != null) {
                            for (ASTParameterDeclaration param : predicate.getParameterDeclarationList()) {
                                System.out.println("  Parameter: name='" + param.getName() + "', type='" + param.getBasicType() + "'");
                                System.out.println("  Parameter toString: " + param.toString());
                            }
                        }
                    }
                }
            } else {
                System.out.println("FAILED: Failed to parse CRF model");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
