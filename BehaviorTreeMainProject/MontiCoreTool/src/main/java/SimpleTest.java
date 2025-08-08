import behaviortree._parser.BehaviorTreeParser;
import behaviortree._ast.ASTBehaviorTree;
import java.util.Optional;

public class SimpleTest {
    public static void main(String[] args) {
        try {
            System.out.println("🔍 Testing Behavior Tree Parser...");
            
            BehaviorTreeParser parser = new BehaviorTreeParser();
            
            // Test parsing
            Optional<ASTBehaviorTree> result = parser.parse("test_behavior_tree.txt");
            
            if (result.isPresent()) {
                System.out.println("✅ SUCCESS: Behavior Tree parsed successfully!");
                ASTBehaviorTree ast = result.get();
                System.out.println("AST: " + ast.toString());
            } else {
                System.out.println("❌ FAILED: Could not parse Behavior Tree");
            }
            
        } catch (Exception e) {
            System.err.println("❌ ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
