// import behaviortreegrammar._parser.BehaviorTreeGrammarParser;
// import behaviortreegrammar._ast.ASTBehaviorTree;
// import behaviortreegrammar._ast.ASTBTFlowNodeDefinition;
// import behaviortreegrammar._ast.ASTService;
// import behaviortreegrammar._ast.ASTDecorator;
// import behaviortreegrammar._ast.ASTRootNode;
// import behaviortreegrammar._ast.ASTSuccessDefinition;
// import behaviortreegrammar._ast.ASTChildTypeDefinition;
// import java.util.Optional;
// import java.io.*;

// public class BehaviorTreeGrammarParserTest {
    
//     public static void main(String[] args) {
//         try {
//             System.out.println("=== BEHAVIOR TREE GRAMMAR PARSER TEST ===");
//             System.out.println("Parsing behavior tree files...");
            
//             // Test multiple files
//             String[] testFiles = {
//                 "src/test/resources/valid/behavior_trees/test_behavior_tree.txt",
//                 "src/test/resources/valid/behavior_trees/behaviorTree.txt"
//             };
            
//             for (String filePath : testFiles) {
//                 System.out.println("\n" + "=".repeat(50));
//                 System.out.println("Testing file: " + filePath);
//                 System.out.println("=".repeat(50));
                
//                 parseBehaviorTreeFile(filePath);
//             }
            
//         } catch (Exception e) {
//             System.err.println("ERROR: " + e.getMessage());
//             e.printStackTrace();
//         }
//     }
    
//     public static void parseBehaviorTreeFile(String filePath) {
//         try {
//             // Check if file exists
//             File testFile = new File(filePath);
//             if (!testFile.exists()) {
//                 System.err.println("ERROR: Test file not found: " + filePath);
//                 System.err.println("Current working directory: " + System.getProperty("user.dir"));
//                 return;
//             }
            
//             // Create parser instance
//             BehaviorTreeGrammarParser parser = new BehaviorTreeGrammarParser();
            
//             // Parse the file
//             Optional<ASTBehaviorTree> result = parser.parse(filePath);
            
//             if (result.isPresent()) {
//                 ASTBehaviorTree behaviorTree = result.get();
//                 System.out.println("SUCCESS: Parsed behavior tree: " + behaviorTree.getName());
                
//                 // Analyze the parsed tree
//                 analyzeBehaviorTree(behaviorTree);
                
//             } else {
//                 System.out.println("ERROR: Failed to parse " + filePath);
//                 System.out.println("Please check the grammar and test file for syntax errors.");
//             }
            
//         } catch (Exception e) {
//             System.err.println("ERROR parsing " + filePath + ": " + e.getMessage());
//             e.printStackTrace();
//         }
//     }
    
//     public static void analyzeBehaviorTree(ASTBehaviorTree behaviorTree) {
//         System.out.println("\n=== BEHAVIOR TREE ANALYSIS ===");
//         System.out.println("Tree Name: " + behaviorTree.getName());
        
//         // Analyze root node
//         ASTRootNode rootNode = behaviorTree.getRootNode();
//         System.out.println("Root Node Type: " + rootNode.getClass().getSimpleName());
        
//         // Check if root node is a BTFlowNodeDefinition
//         if (rootNode instanceof ASTBTFlowNodeDefinition) {
//             ASTBTFlowNodeDefinition flowNodeDef = (ASTBTFlowNodeDefinition) rootNode;
//             System.out.println("Root Node Name: " + flowNodeDef.getName());
            
//             // Analyze success definition
//             if (flowNodeDef.getSuccessDefinition() != null) {
//                 ASTSuccessDefinition successDef = flowNodeDef.getSuccessDefinition();
//                 System.out.println("Success Criteria: " + successDef.getSuccessType());
//             }
            
//             // Analyze child type definition
//             if (flowNodeDef.getChildTypeDefinition() != null) {
//                 ASTChildTypeDefinition childTypeDef = flowNodeDef.getChildTypeDefinition();
//                 System.out.println("Child Type: " + childTypeDef.getChildType());
//             }
//         }
        
//         // Count and analyze nodes
//         int flowNodeCount = countFlowNodeDefinitions(rootNode);
//         int serviceCount = countServices(rootNode);
//         int decoratorCount = countDecorators(rootNode);
        
//         System.out.println("\n=== STATISTICS ===");
//         System.out.println("Flow Node Definitions: " + flowNodeCount);
//         System.out.println("Services: " + serviceCount);
//         System.out.println("Decorators: " + decoratorCount);
        
//         // Print tree structure
//         System.out.println("\n=== TREE STRUCTURE ===");
//         printTreeStructure(rootNode, 0);
        
//         System.out.println("\n=== PARSING COMPLETE ===");
//     }
    
//     private static int countFlowNodeDefinitions(ASTRootNode rootNode) {
//         int count = 1; // Root node itself
//         if (rootNode instanceof ASTBTFlowNodeDefinition) {
//             ASTBTFlowNodeDefinition flowNodeDef = (ASTBTFlowNodeDefinition) rootNode;
//             if (flowNodeDef.getBTFlowNodeDefinitionList() != null) {
//                 count += flowNodeDef.getBTFlowNodeDefinitionList().size();
//                 for (ASTBTFlowNodeDefinition nestedFlowNode : flowNodeDef.getBTFlowNodeDefinitionList()) {
//                     count += countNestedFlowNodeDefinitions(nestedFlowNode);
//                 }
//             }
//         }
//         return count;
//     }
    
//     private static int countNestedFlowNodeDefinitions(ASTBTFlowNodeDefinition flowNode) {
//         int count = 0;
//         if (flowNode.getBTFlowNodeDefinitionList() != null) {
//             count += flowNode.getBTFlowNodeDefinitionList().size();
//             for (ASTBTFlowNodeDefinition nestedFlowNode : flowNode.getBTFlowNodeDefinitionList()) {
//                 count += countNestedFlowNodeDefinitions(nestedFlowNode);
//             }
//         }
//         return count;
//     }
    
//     private static int countServices(ASTRootNode rootNode) {
//         int count = 0;
//         if (rootNode instanceof ASTBTFlowNodeDefinition) {
//             ASTBTFlowNodeDefinition flowNodeDef = (ASTBTFlowNodeDefinition) rootNode;
//             if (flowNodeDef.getServiceList() != null) {
//                 count += flowNodeDef.getServiceList().size();
//             }
//             if (flowNodeDef.getBTFlowNodeDefinitionList() != null) {
//                 for (ASTBTFlowNodeDefinition nestedFlowNode : flowNodeDef.getBTFlowNodeDefinitionList()) {
//                     count += countNestedServices(nestedFlowNode);
//                 }
//             }
//         }
//         return count;
//     }
    
//     private static int countNestedServices(ASTBTFlowNodeDefinition flowNode) {
//         int count = 0;
//         if (flowNode.getServiceList() != null) {
//             count += flowNode.getServiceList().size();
//         }
//         if (flowNode.getBTFlowNodeDefinitionList() != null) {
//             for (ASTBTFlowNodeDefinition nestedFlowNode : flowNode.getBTFlowNodeDefinitionList()) {
//                 count += countNestedServices(nestedFlowNode);
//             }
//         }
//         return count;
//     }
    
//     private static int countDecorators(ASTRootNode rootNode) {
//         int count = 0;
//         if (rootNode instanceof ASTBTFlowNodeDefinition) {
//             ASTBTFlowNodeDefinition flowNodeDef = (ASTBTFlowNodeDefinition) rootNode;
//             if (flowNodeDef.getDecoratorList() != null) {
//                 count += flowNodeDef.getDecoratorList().size();
//             }
//             if (flowNodeDef.getBTFlowNodeDefinitionList() != null) {
//                 for (ASTBTFlowNodeDefinition nestedFlowNode : flowNodeDef.getBTFlowNodeDefinitionList()) {
//                     count += countNestedDecorators(nestedFlowNode);
//                 }
//             }
//         }
//         return count;
//     }
    
//     private static int countNestedDecorators(ASTBTFlowNodeDefinition flowNode) {
//         int count = 0;
//         if (flowNode.getDecoratorList() != null) {
//             count += flowNode.getDecoratorList().size();
//         }
//         if (flowNode.getBTFlowNodeDefinitionList() != null) {
//             for (ASTBTFlowNodeDefinition nestedFlowNode : flowNode.getBTFlowNodeDefinitionList()) {
//                 count += countNestedDecorators(nestedFlowNode);
//             }
//         }
//         return count;
//     }
    
//     private static void printTreeStructure(ASTRootNode rootNode, int indentLevel) {
//         String indent = createIndent(indentLevel);
        
//         if (rootNode instanceof ASTBTFlowNodeDefinition) {
//             ASTBTFlowNodeDefinition flowNodeDef = (ASTBTFlowNodeDefinition) rootNode;
//             System.out.println(indent + "RootNode: " + flowNodeDef.getName());
            
//             // Print success definition
//             if (flowNodeDef.getSuccessDefinition() != null) {
//                 ASTSuccessDefinition successDef = flowNodeDef.getSuccessDefinition();
//                 System.out.println(indent + "  ├─ Success: " + successDef.getSuccessType());
//             }
            
//             // Print child type definition
//             if (flowNodeDef.getChildTypeDefinition() != null) {
//                 ASTChildTypeDefinition childTypeDef = flowNodeDef.getChildTypeDefinition();
//                 System.out.println(indent + "  ├─ ChildType: " + childTypeDef.getChildType());
//             }
            
//             // Print services
//             if (flowNodeDef.getServiceList() != null) {
//                 for (ASTService service : flowNodeDef.getServiceList()) {
//                     System.out.println(indent + "  ├─ Service: " + service.getName());
//                 }
//             }
            
//             // Print decorators
//             if (flowNodeDef.getDecoratorList() != null) {
//                 for (ASTDecorator decorator : flowNodeDef.getDecoratorList()) {
//                     System.out.println(indent + "  ├─ Decorator: " + decorator.getName());
//                 }
//             }
            
//             // Print nested flow node definitions
//             if (flowNodeDef.getBTFlowNodeDefinitionList() != null) {
//                 for (ASTBTFlowNodeDefinition nestedFlowNode : flowNodeDef.getBTFlowNodeDefinitionList()) {
//                     printFlowNodeDefinitionStructure(nestedFlowNode, indentLevel + 1);
//                 }
//             }
//         } else {
//             System.out.println(indent + "RootNode: " + rootNode.getClass().getSimpleName());
//         }
//     }
    
//     private static void printFlowNodeDefinitionStructure(ASTBTFlowNodeDefinition flowNode, int indentLevel) {
//         String indent = createIndent(indentLevel);
//         System.out.println(indent + "├─ FlowNode: " + flowNode.getName());
        
//         // Print success definition
//         if (flowNode.getSuccessDefinition() != null) {
//             ASTSuccessDefinition successDef = flowNode.getSuccessDefinition();
//             System.out.println(indent + "  ├─ Success: " + successDef.getSuccessType());
//         }
        
//         // Print child type definition
//         if (flowNode.getChildTypeDefinition() != null) {
//             ASTChildTypeDefinition childTypeDef = flowNode.getChildTypeDefinition();
//             System.out.println(indent + "  ├─ ChildType: " + childTypeDef.getChildType());
//         }
        
//         // Print services
//         if (flowNode.getServiceList() != null) {
//             for (ASTService service : flowNode.getServiceList()) {
//                 System.out.println(indent + "  ├─ Service: " + service.getName());
//             }
//         }
        
//         // Print decorators
//         if (flowNode.getDecoratorList() != null) {
//             for (ASTDecorator decorator : flowNode.getDecoratorList()) {
//                 System.out.println(indent + "  ├─ Decorator: " + decorator.getName());
//             }
//         }
        
//         // Print nested flow node definitions
//         if (flowNode.getBTFlowNodeDefinitionList() != null) {
//             for (ASTBTFlowNodeDefinition nestedFlowNode : flowNode.getBTFlowNodeDefinitionList()) {
//                 printFlowNodeDefinitionStructure(nestedFlowNode, indentLevel + 1);
//             }
//         }
//     }
    
//     // Helper method to create indentation (Java 8 compatible)
//     private static String createIndent(int indentLevel) {
//         StringBuilder indent = new StringBuilder();
//         for (int i = 0; i < indentLevel; i++) {
//             indent.append("  ");
//         }
//         return indent.toString();
//     }
// }
