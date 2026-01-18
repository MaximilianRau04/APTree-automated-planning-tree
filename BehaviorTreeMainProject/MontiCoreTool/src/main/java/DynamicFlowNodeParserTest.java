import dynamicbtflownode._parser.DynamicBTFlowNodeParser;
import dynamicbtflownode._ast.ASTDynamicFlowNode;
import dynamicbtflownode._ast.ASTGraphNode;
import dynamicbtflownode._ast.ASTNodeGraph;
import dynamicbtflownode._ast.ASTRelation;
import dynamicbtflownode.DynamicBTFlowNodeMill;
import behaviortree._ast.ASTService;
import behaviortree._ast.ASTBTNode;

import java.util.Optional;
import java.util.List;
import java.io.*;

/**
 * DynamicFlowNodeParserTest - Parses and displays DynamicFlowNode models
 */
public class DynamicFlowNodeParserTest {
    
    private static final String DEFAULT_PATH = "src/test/resources/valid/DynamicFlowNode/DynamicFlowNode.bt";
    
    public static void main(String[] args) {
        try {
            System.out.println("=== DYNAMIC FLOW NODE PARSER TEST ===");
            System.out.println("Parsing DynamicFlowNode model...\n");
            
            // Initialize MontiCore mill for the grammar
            DynamicBTFlowNodeMill.init();
            
            // Define the file to parse
            String filePath = args.length > 0 ? args[0] : DEFAULT_PATH;
            
            // Check if file exists
            File testFile = new File(filePath);
            if (!testFile.exists()) {
                System.err.println("✗ ERROR: Test file not found: " + filePath);
                System.err.println("   Current working directory: " + System.getProperty("user.dir"));
                return;
            }
            
            // Create parser instance
            DynamicBTFlowNodeParser parser = new DynamicBTFlowNodeParser();
            
            // Parse the file - use parseDynamicFlowNode for the specific rule
            Optional<ASTDynamicFlowNode> result = parser.parseDynamicFlowNode(filePath);
            
            if (result.isPresent()) {
                ASTDynamicFlowNode flowNode = result.get();
                System.out.println("✓ SUCCESS: Parsed file: " + filePath + "\n");
                
                // Analyze the parsed model
                analyzeFlowNode(flowNode);
                
                System.out.println("\n✓ PARSING COMPLETED SUCCESSFULLY");
            } else {
                System.err.println("✗ FAILED: Could not parse model");
                if (parser.hasErrors()) {
                    System.err.println("   Parser errors occurred.");
                }
            }
            
        } catch (Exception e) {
            System.err.println("✗ ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    /**
     * Analyze and display the parsed DynamicFlowNode
     */
    public static void analyzeFlowNode(ASTDynamicFlowNode flowNode) {
        System.out.println("========================================");
        System.out.println("DYNAMIC FLOW NODE");
        System.out.println("========================================\n");
        
        // Display success criteria
        System.out.println("Success Criteria: " + flowNode.getSuccri().name());
        
        // Display child type
        System.out.println("Child Type: " + flowNode.getChildType().name());
        
        // Display services
        System.out.println("\n--- SERVICES ---");
        List<ASTService> services = flowNode.getServiceList();
        if (services.isEmpty()) {
            System.out.println("  (none)");
        } else {
            for (ASTService service : services) {
                System.out.println("  - " + service.getName());
            }
        }
        
        // Display node graph
        ASTNodeGraph nodeGraph = flowNode.getNodeGraph();
        System.out.println("\n--- NODE GRAPH: " + nodeGraph.getName() + " ---");
        
        // Display Nodes AND Relations in the graph
        System.out.println("\n  Nodes & Relations:");
        List<ASTGraphNode> graphNodes = nodeGraph.getNodesList();
        
        if (graphNodes.isEmpty()) {
             System.out.println("    (none)");
        } else {
            for (ASTGraphNode gNode : graphNodes) {
                System.out.println("    - Node: " + gNode.getNode().getName());
                for (ASTRelation rel : gNode.getSuccessorsList()) {
                    System.out.println("      -> Relation: [" + rel.getTemptype().name() + "] --> " + rel.getTarget());
                }
            }
        }
        
        System.out.println("\n========================================");
    }
}
