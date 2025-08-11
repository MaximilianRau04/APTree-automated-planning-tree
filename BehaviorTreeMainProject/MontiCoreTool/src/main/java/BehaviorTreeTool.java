import behaviortree._parser.BehaviorTreeParser;
import behaviortree._ast.ASTBehaviorTree;
import java.util.Optional;

public class BehaviorTreeTool {
    
    public static void main(String[] args) {
        if (args.length == 0) {
            System.out.println("Usage: BehaviorTreeTool <input-file>");
            System.out.println("Testing with default file...");
            testParser("test_behavior_tree.txt");
        } else {
            testParser(args[0]);
        }
    }
    
    private static void testParser(String filename) {
        try {
            BehaviorTreeParser parser = new BehaviorTreeParser();
            
            System.out.println("PARSING: Parsing file: " + filename);
            Optional<ASTBehaviorTree> result = parser.parse(filename);
            
            if (result.isPresent()) {
                System.out.println("SUCCESS: Successfully parsed Behavior Tree!");
                ASTBehaviorTree ast = result.get();
                System.out.println("AST Structure:");
                System.out.println(ast.toString());
            } else {
                System.out.println("FAILED: Failed to parse Behavior Tree");
                System.out.println("Check if the file exists and contains valid syntax");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: Error parsing: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
