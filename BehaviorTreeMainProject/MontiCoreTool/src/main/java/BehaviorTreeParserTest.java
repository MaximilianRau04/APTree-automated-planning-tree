import behaviortree._parser.BehaviorTreeParser;
import behaviortree._ast.ASTBehaviorTree;
import behaviortree._ast.ASTBTFlowNode;
import behaviortree._ast.ASTBTActionNode;
import behaviortree._ast.ASTService;
import behaviortree._ast.ASTDecorator;
import behaviortree._ast.ASTRootNode;
import java.util.Optional;
import java.io.*;

public class BehaviorTreeParserTest {
    
    public static void main(String[] args) {
        try {
            System.out.println("=== BEHAVIOR TREE PARSER TEST ===");
            System.out.println("Parsing test_behavior_tree.txt...");
            
            // Define the file to parse
            String filePath = "src/test/resources/valid/behavior_trees/test_behavior_tree.txt";
            
            // Check if file exists
            File testFile = new File(filePath);
            if (!testFile.exists()) {
                System.err.println("ERROR: Test file not found: " + filePath);
                System.err.println("Current working directory: " + System.getProperty("user.dir"));
                return;
            }
            
            // Create parser instance
            BehaviorTreeParser parser = new BehaviorTreeParser();
            
            // Parse the file
            Optional<ASTBehaviorTree> result = parser.parse(filePath);
            
            if (result.isPresent()) {
                ASTBehaviorTree behaviorTree = result.get();
                System.out.println("SUCCESS: Parsed behavior tree: " + behaviorTree.getName());
                
                // Analyze the parsed tree
                analyzeBehaviorTree(behaviorTree);
                
            } else {
                System.out.println("ERROR: Failed to parse " + filePath);
                System.out.println("Please check the grammar and test file for syntax errors.");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    public static void analyzeBehaviorTree(ASTBehaviorTree behaviorTree) {
        System.out.println("\n=== BEHAVIOR TREE ANALYSIS ===");
        System.out.println("Tree Name: " + behaviorTree.getName());
        
        // Analyze root node
        ASTRootNode rootNode = behaviorTree.getRootNode();
        System.out.println("Root Node: RootNode"); // Fixed: Use literal name since RootNode is always "RootNode"
        
        // Count and analyze nodes
        int flowNodeCount = countFlowNodes(rootNode);
        int actionNodeCount = countActionNodes(rootNode);
        int serviceCount = countServices(rootNode);
        int decoratorCount = countDecorators(rootNode);
        
        System.out.println("\n=== STATISTICS ===");
        System.out.println("Flow Nodes: " + flowNodeCount);
        System.out.println("Action Nodes: " + actionNodeCount);
        System.out.println("Services: " + serviceCount);
        System.out.println("Decorators: " + decoratorCount);
        
        // Print tree structure
        System.out.println("\n=== TREE STRUCTURE ===");
        printTreeStructure(rootNode, 0);
        
        System.out.println("\n=== PARSING COMPLETE ===");
    }
    
    private static int countFlowNodes(ASTRootNode rootNode) {
        int count = 1; // Root node itself
        if (rootNode.getBTFlowNodeList() != null) {
            count += rootNode.getBTFlowNodeList().size();
            for (ASTBTFlowNode flowNode : rootNode.getBTFlowNodeList()) {
                count += countNestedFlowNodes(flowNode);
            }
        }
        return count;
    }
    
    private static int countNestedFlowNodes(ASTBTFlowNode flowNode) {
        int count = 0;
        if (flowNode.getBTFlowNodeList() != null) {
            count += flowNode.getBTFlowNodeList().size();
            for (ASTBTFlowNode nestedFlowNode : flowNode.getBTFlowNodeList()) {
                count += countNestedFlowNodes(nestedFlowNode);
            }
        }
        return count;
    }
    
    private static int countActionNodes(ASTRootNode rootNode) {
        int count = 0;
        if (rootNode.getBTActionNodeList() != null) {
            count += rootNode.getBTActionNodeList().size();
        }
        if (rootNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode flowNode : rootNode.getBTFlowNodeList()) {
                count += countNestedActionNodes(flowNode);
            }
        }
        return count;
    }
    
    private static int countNestedActionNodes(ASTBTFlowNode flowNode) {
        int count = 0;
        if (flowNode.getBTActionNodeList() != null) {
            count += flowNode.getBTActionNodeList().size();
        }
        if (flowNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode nestedFlowNode : flowNode.getBTFlowNodeList()) {
                count += countNestedActionNodes(nestedFlowNode);
            }
        }
        return count;
    }
    
    private static int countServices(ASTRootNode rootNode) {
        int count = 0;
        if (rootNode.getServiceList() != null) {
            count += rootNode.getServiceList().size();
        }
        if (rootNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode flowNode : rootNode.getBTFlowNodeList()) {
                count += countNestedServices(flowNode);
            }
        }
        return count;
    }
    
    private static int countNestedServices(ASTBTFlowNode flowNode) {
        int count = 0;
        if (flowNode.getServiceList() != null) {
            count += flowNode.getServiceList().size();
        }
        if (flowNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode nestedFlowNode : flowNode.getBTFlowNodeList()) {
                count += countNestedServices(nestedFlowNode);
            }
        }
        return count;
    }
    
    private static int countDecorators(ASTRootNode rootNode) {
        int count = 0;
        if (rootNode.getDecoratorList() != null) {
            count += rootNode.getDecoratorList().size();
        }
        if (rootNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode flowNode : rootNode.getBTFlowNodeList()) {
                count += countNestedDecorators(flowNode);
            }
        }
        return count;
    }
    
    private static int countNestedDecorators(ASTBTFlowNode flowNode) {
        int count = 0;
        if (flowNode.getDecoratorList() != null) {
            count += flowNode.getDecoratorList().size();
        }
        if (flowNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode nestedFlowNode : flowNode.getBTFlowNodeList()) {
                count += countNestedDecorators(nestedFlowNode);
            }
        }
        return count;
    }
    
    private static void printTreeStructure(ASTRootNode rootNode, int indentLevel) {
        String indent = createIndent(indentLevel);
        System.out.println(indent + "RootNode: RootNode"); // Fixed: Use literal name
        
        // Print services
        if (rootNode.getServiceList() != null) {
            for (ASTService service : rootNode.getServiceList()) {
                System.out.println(indent + "  ├─ Service: " + service.getName());
            }
        }
        
        // Print decorators
        if (rootNode.getDecoratorList() != null) {
            for (ASTDecorator decorator : rootNode.getDecoratorList()) {
                System.out.println(indent + "  ├─ Decorator: " + decorator.getName());
            }
        }
        
        // Print flow nodes
        if (rootNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode flowNode : rootNode.getBTFlowNodeList()) {
                printFlowNodeStructure(flowNode, indentLevel + 1);
            }
        }
        
        // Print action nodes
        if (rootNode.getBTActionNodeList() != null) {
            for (ASTBTActionNode actionNode : rootNode.getBTActionNodeList()) {
                printActionNodeStructure(actionNode, indentLevel + 1);
            }
        }
    }
    
    private static void printFlowNodeStructure(ASTBTFlowNode flowNode, int indentLevel) {
        String indent = createIndent(indentLevel);
        System.out.println(indent + "├─ FlowNode: " + flowNode.getName());
        
        // Print services
        if (flowNode.getServiceList() != null) {
            for (ASTService service : flowNode.getServiceList()) {
                System.out.println(indent + "  ├─ Service: " + service.getName());
            }
        }
        
        // Print decorators
        if (flowNode.getDecoratorList() != null) {
            for (ASTDecorator decorator : flowNode.getDecoratorList()) {
                System.out.println(indent + "  ├─ Decorator: " + decorator.getName());
            }
        }
        
        // Print nested flow nodes
        if (flowNode.getBTFlowNodeList() != null) {
            for (ASTBTFlowNode nestedFlowNode : flowNode.getBTFlowNodeList()) {
                printFlowNodeStructure(nestedFlowNode, indentLevel + 1);
            }
        }
        
        // Print action nodes
        if (flowNode.getBTActionNodeList() != null) {
            for (ASTBTActionNode actionNode : flowNode.getBTActionNodeList()) {
                printActionNodeStructure(actionNode, indentLevel + 1);
            }
        }
    }
    
    private static void printActionNodeStructure(ASTBTActionNode actionNode, int indentLevel) {
        String indent = createIndent(indentLevel);
        System.out.println(indent + "├─ ActionNode: " + actionNode.getName());
        
        // Print services
        if (actionNode.getServiceList() != null) {
            for (ASTService service : actionNode.getServiceList()) {
                System.out.println(indent + "  ├─ Service: " + service.getName());
            }
        }
        
        // Print decorators
        if (actionNode.getDecoratorList() != null) {
            for (ASTDecorator decorator : actionNode.getDecoratorList()) {
                System.out.println(indent + "  ├─ Decorator: " + decorator.getName());
            }
        }
    }
    
    // Helper method to create indentation (Java 8 compatible)
    private static String createIndent(int indentLevel) {
        StringBuilder indent = new StringBuilder();
        for (int i = 0; i < indentLevel; i++) {
            indent.append("  ");
        }
        return indent.toString();
    }
}