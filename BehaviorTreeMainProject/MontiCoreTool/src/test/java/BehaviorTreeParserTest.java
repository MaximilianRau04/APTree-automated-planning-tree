import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.BeforeEach;
import static org.junit.jupiter.api.Assertions.*;

import java.io.IOException;
import java.util.Optional;

// Import the generated parser classes
import behaviortree._parser.BehaviorTreeParser;
import behaviortree._ast.ASTBehaviorTree;

public class BehaviorTreeParserTest {
    
    private BehaviorTreeParser parser;
    
    @BeforeEach
    void setUp() {
        parser = new BehaviorTreeParser();
    }
    
    @Test
    void testValidBehaviorTreeParsing() throws IOException {
        // Parse a valid behavior tree file from the organized test resources
        Optional<ASTBehaviorTree> parseResult = parser.parse("src/test/resources/valid/behavior_trees/test_behavior_tree.txt");
        
        // Assert that parsing was successful
        assertTrue(parseResult.isPresent(), "Parsing should succeed");
        
        ASTBehaviorTree ast = parseResult.get();
        
        // Add your specific assertions here
        // For example:
        // assertNotNull(ast.getRootNode());
        // assertEquals("MyTree", ast.getName());
        
        System.out.println("Successfully parsed Behavior Tree:");
        System.out.println(ast.toString());
    }
}
