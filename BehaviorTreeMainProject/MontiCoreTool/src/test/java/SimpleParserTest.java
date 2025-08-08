import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

import java.io.IOException;
import java.util.Optional;

// Import the generated parser classes
import behaviortree._parser.BehaviorTreeParser;
import behaviortree._ast.ASTBehaviorTree;

public class SimpleParserTest {
    
    @Test
    void testSimpleParsing() {
        try {
            BehaviorTreeParser parser = new BehaviorTreeParser();
            
            // Test parsing the behavior tree file
            Optional<ASTBehaviorTree> result = parser.parse("test_behavior_tree.txt");
            
            if (result.isPresent()) {
                System.out.println("✅ Successfully parsed Behavior Tree!");
                ASTBehaviorTree ast = result.get();
                System.out.println("AST: " + ast.toString());
                assertTrue(true, "Parsing succeeded");
            } else {
                System.out.println("❌ Failed to parse Behavior Tree");
                fail("Parsing failed");
            }
            
        } catch (Exception e) {
            System.err.println("❌ Error parsing: " + e.getMessage());
            e.printStackTrace();
            fail("Exception during parsing: " + e.getMessage());
        }
    }
}
