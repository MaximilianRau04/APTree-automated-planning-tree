import behaviortree._parser.BehaviorTreeParser;
import behaviortree._ast.ASTBehaviorTree;
import java.util.Optional;
import java.io.*;

public class ASTStorage {
    
    public static void main(String[] args) {
        try {
            // 1. Parse and store in memory
            BehaviorTreeParser parser = new BehaviorTreeParser();
            Optional<ASTBehaviorTree> result = parser.parse("src/test/resources/valid/behavior_trees/test_behavior_tree.txt");
            
            if (result.isPresent()) {
                ASTBehaviorTree ast = result.get();
                System.out.println("✅ AST created and stored in memory");
                
                // 2. Serialize to file (Handbook recommended)
                serializeAST(ast, "ast_objects.ser");
                
                // 3. Export to JSON (for C# integration)
                exportToJSON(ast, "ast_data.json");
                
                // 4. Export to XML (alternative format)
                exportToXML(ast, "ast_data.xml");
                
            } else {
                System.out.println("FAILED: Failed to parse file");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    // Method 1: Java Serialization (Handbook recommended)
    public static void serializeAST(ASTBehaviorTree ast, String filename) {
        try (ObjectOutputStream oos = new ObjectOutputStream(new FileOutputStream(filename))) {
            oos.writeObject(ast);
            System.out.println("SUCCESS: AST serialized to: " + filename);
        } catch (IOException e) {
            System.err.println("ERROR: Serialization failed: " + e.getMessage());
        }
    }
    
    // Method 2: Load serialized AST
    public static ASTBehaviorTree deserializeAST(String filename) {
        try (ObjectInputStream ois = new ObjectInputStream(new FileInputStream(filename))) {
            ASTBehaviorTree ast = (ASTBehaviorTree) ois.readObject();
            System.out.println("SUCCESS: AST loaded from: " + filename);
            return ast;
        } catch (IOException | ClassNotFoundException e) {
            System.err.println("ERROR: Deserialization failed: " + e.getMessage());
            return null;
        }
    }
    
    // Method 3: Export to JSON (for C# integration)
    public static void exportToJSON(ASTBehaviorTree ast, String filename) {
        try (PrintWriter writer = new PrintWriter(new FileWriter(filename))) {
            writer.println("{");
            writer.println("  \"behaviorTree\": {");
            writer.println("    \"name\": \"" + ast.getName() + "\",");
            
            // Export blackboard data
            if (ast.getBlackboard() != null) {
                writer.println("    \"blackboard\": {");
                writer.println("      \"variables\": [");
                // Add variable export logic here
                writer.println("      ]");
                writer.println("    },");
            }
            
            // Export root node data
            if (ast.getRootNode() != null) {
                writer.println("    \"rootNode\": {");
                // Add root node export logic here
                writer.println("    }");
            }
            
            writer.println("  }");
            writer.println("}");
            System.out.println("SUCCESS: AST exported to JSON: " + filename);
        } catch (IOException e) {
            System.err.println("ERROR: JSON export failed: " + e.getMessage());
        }
    }
    
    // Method 4: Export to XML
    public static void exportToXML(ASTBehaviorTree ast, String filename) {
        try (PrintWriter writer = new PrintWriter(new FileWriter(filename))) {
            writer.println("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            writer.println("<behaviorTree name=\"" + ast.getName() + "\">");
            
            // Export blackboard
            if (ast.getBlackboard() != null) {
                writer.println("  <blackboard>");
                // Add blackboard export logic here
                writer.println("  </blackboard>");
            }
            
            // Export root node
            if (ast.getRootNode() != null) {
                writer.println("  <rootNode>");
                // Add root node export logic here
                writer.println("  </rootNode>");
            }
            
            writer.println("</behaviorTree>");
            System.out.println("SUCCESS: AST exported to XML: " + filename);
        } catch (IOException e) {
            System.err.println("ERROR: XML export failed: " + e.getMessage());
        }
    }
}
