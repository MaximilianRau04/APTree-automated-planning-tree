import dynamicbtflownode._parser.DynamicBTFlowNodeParser;
import dynamicbtflownode._ast.ASTBehaviorTree;
import dynamicbtflownode._ast.ASTDynamicFlowNode;
import dynamicbtflownode._ast.ASTNodeGraph;
import dynamicbtflownode._ast.ASTRelation;
import dynamicbtflownode.DynamicBTFlowNodeMill;
import behaviortree._ast.ASTFlowNode;
import behaviortree._ast.ASTSequence;
import behaviortree._ast.ASTFallback;
import behaviortree._ast.ASTService;
import behaviortree._ast.ASTActionNode;
import behaviortree._ast.ASTBTNode;
import behaviortree._ast.ASTDecorator;
import concretebt._ast.*;
import java.nio.file.Paths;
import java.nio.file.Path;

import java.util.Optional;
import java.util.List;
import java.io.*;

/**
 * APTreeParserTest - Parser for the APTree behavior tree model
 * 
 * This parser handles the complete APTree.bt model which includes:
 * - BehaviorTree root structure
 * - Sequence and Fallback flow nodes
 * - DynamicFlowNodes with Services
 * - NodeGraphs containing Actions and Relations
 * - Temporal relations (MEETS, BEFORE, OVERLAPS, etc.)
 */
public class APTreeParserTest {
    
    private static final String DEFAULT_PATH = "src/test/resources/valid/behavior_trees/APTree.bt";
    private static int totalNodes = 0;
    private static int totalRelations = 0;
    private static int totalActions = 0;
    
    public static void main(String[] args) {
        try {
            System.out.println("╔══════════════════════════════════════════════════════════════╗");
            System.out.println("║           AP TREE PARSER - Behavior Tree Analysis            ║");
            System.out.println("╚══════════════════════════════════════════════════════════════╝");
            System.out.println();
            
            // Initialize MontiCore mill for the grammar
            DynamicBTFlowNodeMill.init();

            // Add model path for serialized symbols (autoload via <name>.sym)
            // Default output from InstanceSymbolsGenerator is target/symbols
            Path symDir = Paths.get("target", "symbols");
            DynamicBTFlowNodeMill.globalScope().getSymbolPath().addEntry(symDir.toAbsolutePath());
            
            // Define the file to parse
            String filePath = args.length > 0 ? args[0] : DEFAULT_PATH;
            
            // Check if file exists
            File testFile = new File(filePath);
            if (!testFile.exists()) {
                printError("Test file not found: " + filePath);
                System.err.println("   Current working directory: " + System.getProperty("user.dir"));
                return;
            }
            
            System.out.println("📂 Parsing file: " + filePath);
            System.out.println("🔎 Symbol path entry: " + symDir.toAbsolutePath());
            System.out.println();
            
            // Create parser instance
            DynamicBTFlowNodeParser parser = new DynamicBTFlowNodeParser();
            
            // Parse the BehaviorTree
            Optional<ASTBehaviorTree> result = parser.parseBehaviorTree(filePath);
            
            if (result.isPresent()) {
                ASTBehaviorTree behaviorTree = result.get();
                printSuccess("Successfully parsed BehaviorTree: '" + behaviorTree.getName() + "'");
                System.out.println();
                
                // Analyze the complete tree structure
                analyzeBehaviorTree(behaviorTree);
                
                // Validate all referenced symbols are resolved (detect undefined instances)
                System.out.println();
                validateSymbolResolution(behaviorTree);
                
                // Print summary
                printSummary();
                
            } else {
                printError("Failed to parse BehaviorTree model");
                if (parser.hasErrors()) {
                    System.err.println("   Parser reported errors. Check syntax.");
                }
            }
            
        } catch (Exception e) {
            printError("Exception occurred: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    /**
     * Main analysis entry point for the BehaviorTree
     */
    public static void analyzeBehaviorTree(ASTBehaviorTree behaviorTree) {
        printSection("BEHAVIOR TREE STRUCTURE");
        
        System.out.println("  Tree Name: " + behaviorTree.getName());
        
        // Get root node
        ASTFlowNode rootNode = behaviorTree.getRoot();
        if (rootNode == null) {
            printWarning("No root FlowNode found in BehaviorTree");
            return;
        }
        
        System.out.println("  Root Type: " + getNodeTypeName(rootNode));
        System.out.println();
        
        printSection("TREE HIERARCHY");
        analyzeFlowNode(rootNode, 1);
    }
    
    /**
     * Recursively analyze FlowNode and its children
     */
    public static void analyzeFlowNode(ASTFlowNode node, int depth) {
        String indent = createIndent(depth);
        String nodeType = getNodeTypeName(node);
        totalNodes++;
        
        // Print node header
        System.out.println(indent + "┌─ " + nodeType);
        
        // Handle DynamicFlowNode specifically
        if (node instanceof ASTDynamicFlowNode) {
            analyzeDynamicFlowNode((ASTDynamicFlowNode) node, depth);
        }
        // Handle Sequence
        else if (node instanceof ASTSequence) {
            analyzeSequence((ASTSequence) node, depth);
        }
        // Handle Fallback
        else if (node instanceof ASTFallback) {
            analyzeFallback((ASTFallback) node, depth);
        }
        
        System.out.println(indent + "└─────────────────────────────");
    }
    
    /**
     * Analyze DynamicFlowNode with its services, criteria, and node graph
     */
    public static void analyzeDynamicFlowNode(ASTDynamicFlowNode flowNode, int depth) {
        String indent = createIndent(depth);
        
        // Success criteria
        System.out.println(indent + "│ Success Criteria: " + flowNode.getSuccri().name());
        
        // Child type
        System.out.println(indent + "│ Child Type: " + flowNode.getChildType().name());
        
        // Services
        List<ASTService> services = flowNode.getServiceList();
        if (!services.isEmpty()) {
            System.out.println(indent + "│ Services:");
            for (ASTService service : services) {
                System.out.println(indent + "│   • " + service.getName());
            }
        }
        
        // NodeGraph
        ASTNodeGraph nodeGraph = flowNode.getNodeGraph();
        if (nodeGraph != null) {
            analyzeNodeGraph(nodeGraph, depth);
        }
        
        // Nested FlowNodes (children)
        analyzeNestedFlowNodes(flowNode, depth);
    }
    
    /**
     * Analyze Sequence node
     */
    public static void analyzeSequence(ASTSequence sequence, int depth) {
        String indent = createIndent(depth);
        System.out.println(indent + "│ [Sequential execution - all children must succeed]");
        
        // Check for nested flow nodes
        for (ASTFlowNode child : sequence.getFlowNodeList()) {
            analyzeFlowNode(child, depth + 1);
        }
        
        // Check for action nodes
        for (ASTActionNode action : sequence.getActionNodeList()) {
            analyzeActionNode(action, depth);
        }
    }
    
    /**
     * Analyze Fallback node
     */
    public static void analyzeFallback(ASTFallback fallback, int depth) {
        String indent = createIndent(depth);
        System.out.println(indent + "│ [Fallback execution - first success wins]");
        
        // Check for nested flow nodes
        for (ASTFlowNode child : fallback.getFlowNodeList()) {
            analyzeFlowNode(child, depth + 1);
        }
        
        // Check for action nodes
        for (ASTActionNode action : fallback.getActionNodeList()) {
            analyzeActionNode(action, depth);
        }
    }
    
    /**
     * Analyze NodeGraph containing actions and relations
     */
    public static void analyzeNodeGraph(ASTNodeGraph nodeGraph, int depth) {
        String indent = createIndent(depth);
        
        System.out.println(indent + "│");
        System.out.println(indent + "│ ╔═ NodeGraph: " + nodeGraph.getName() + " ═╗");
        
        // Actions in the graph
        List<ASTBTNode> nodes = nodeGraph.getBTNodeList();
        if (!nodes.isEmpty()) {
            System.out.println(indent + "│ ║ Actions:");
            for (ASTBTNode node : nodes) {
                totalActions++;
                String actionInfo = formatActionNode(node);
                System.out.println(indent + "│ ║   → " + actionInfo);
            }
        }
        
        // Relations (temporal constraints)
        List<ASTRelation> relations = nodeGraph.getRelationList();
        if (!relations.isEmpty()) {
            System.out.println(indent + "│ ║ Relations:");
            for (ASTRelation rel : relations) {
                totalRelations++;
                String relStr = String.format("%s --[%s]--> %s",
                    rel.getSource(),
                    rel.getTemptype().name(),
                    rel.getTarget());
                System.out.println(indent + "│ ║   ⟿ " + relStr);
            }
        }
        
        System.out.println(indent + "│ ╚══════════════════════════════╝");
    }
    
    /**
     * Analyze nested FlowNodes within a DynamicFlowNode
     */
    public static void analyzeNestedFlowNodes(ASTDynamicFlowNode flowNode, int depth) {
        // Check for nested FlowNodes
        List<ASTFlowNode> nestedFlowNodes = flowNode.getFlowNodeList();
        if (nestedFlowNodes != null && !nestedFlowNodes.isEmpty()) {
            for (ASTFlowNode nested : nestedFlowNodes) {
                analyzeFlowNode(nested, depth + 1);
            }
        }
    }
    
    /**
     * Analyze a single ActionNode
     */
    public static void analyzeActionNode(ASTActionNode action, int depth) {
        String indent = createIndent(depth);
        totalActions++;
        
        String actionInfo = formatActionNode(action);
        System.out.println(indent + "│   ◆ Action: " + actionInfo);
        
        // Check for decorators
        for (ASTDecorator decorator : action.getDecoratorList()) {
            System.out.println(indent + "│       ↳ Decorator: " + decorator.getName());
        }
        
        // Check for services
        for (ASTService service : action.getServiceList()) {
            System.out.println(indent + "│       ↳ Service: " + service.getName());
        }
    }
    
    /**
     * Format action node information
     */
    private static String formatActionNode(ASTBTNode node) {
        String typeName = getNodeTypeName(node);
        String name = node.getName();
        
        // Check if it's a PickUpHL or PlaceHL action with parameters
        if (node instanceof ASTPickUpHL) {
            ASTPickUpHL pickup = (ASTPickUpHL) node;
            return String.format("PickUpHL %s (obj: %s, pos: %s, agent: %s)",
                name, pickup.getObj(), pickup.getGrabPos(), pickup.getClient());
        }
        
        return typeName + " " + name;
    }
    
    /**
     * Get clean node type name
     */
    private static String getNodeTypeName(Object node) {
        String fullName = node.getClass().getSimpleName();
        return fullName.startsWith("AST") ? fullName.substring(3) : fullName;
    }
    
    /**
     * Create indentation string
     */
    private static String createIndent(int level) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < level; i++) {
            sb.append("  ");
        }
        return sb.toString();
    }
    
    /**
     * Print section header
     */
    private static void printSection(String title) {
        System.out.println("┌──────────────────────────────────────────────────┐");
        System.out.println("│ " + title + spaces(48 - title.length()) + " │");
        System.out.println("└──────────────────────────────────────────────────┘");
    }
    
    /**
     * Print summary statistics
     */
    private static void printSummary() {
        System.out.println();
        System.out.println("╔══════════════════════════════════════════════════════════════╗");
        System.out.println("║                    PARSING SUMMARY                           ║");
        System.out.println("╠══════════════════════════════════════════════════════════════╣");
        System.out.println("║  Total Flow Nodes:    " + String.format("%-38d", totalNodes) + " ║");
        System.out.println("║  Total Actions:       " + String.format("%-38d", totalActions) + " ║");
        System.out.println("║  Total Relations:     " + String.format("%-38d", totalRelations) + " ║");
        System.out.println("╠══════════════════════════════════════════════════════════════╣");
        System.out.println("║  ✓ PARSING COMPLETED SUCCESSFULLY                            ║");
        System.out.println("╚══════════════════════════════════════════════════════════════╝");
    }

    /**
     * Validate that all typed references (@Element, @Location, @Agent, @Robot) are resolved.
     * This detects instances that are referenced but not defined in CRFConcreteInstances.bt.
     */
    private static void validateSymbolResolution(ASTBehaviorTree behaviorTree) {
        printSection("SYMBOL RESOLUTION VALIDATION");
        
        int unresolvedCount = 0;
        int resolvedCount = 0;
        
        // Traverse all PickUpHL actions and validate their symbol references
        unresolvedCount += validateActionSymbols(behaviorTree, resolvedCount);
        
        if (unresolvedCount == 0) {
            System.out.println("✓ All " + resolvedCount + " symbol references resolved successfully.");
        } else {
            System.out.println("✗ Found " + unresolvedCount + " UNRESOLVED symbol references!");
            System.out.println("  These instances are used but NOT defined in CRFConcreteInstances.bt");
        }
    }

    /**
     * Recursively validate PickUpHL and PlaceHL action nodes
     */
    private static int validateActionSymbols(ASTBehaviorTree behaviorTree, int resolved) {
        int unresolved = 0;
        
        // Validate root FlowNode
        if (behaviorTree.getRoot() != null) {
            unresolved += validateFlowNodeActions(behaviorTree.getRoot());
        }
        
        return unresolved;
    }

    /**
     * Recursively check all FlowNodes for unresolved action symbols
     */
    private static int validateFlowNodeActions(ASTFlowNode node) {
        int unresolved = 0;
        
        if (node instanceof ASTDynamicFlowNode) {
            ASTDynamicFlowNode dyn = (ASTDynamicFlowNode) node;
            
            // Check NodeGraph actions
            if (dyn.getNodeGraph() != null) {
                unresolved += validateNodeGraphActions(dyn.getNodeGraph());
            }
            
            // Recurse into nested flow nodes
            for (ASTFlowNode nested : dyn.getFlowNodeList()) {
                unresolved += validateFlowNodeActions(nested);
            }
        } else if (node instanceof ASTSequence) {
            ASTSequence seq = (ASTSequence) node;
            for (ASTFlowNode child : seq.getFlowNodeList()) {
                unresolved += validateFlowNodeActions(child);
            }
        } else if (node instanceof ASTFallback) {
            ASTFallback fb = (ASTFallback) node;
            for (ASTFlowNode child : fb.getFlowNodeList()) {
                unresolved += validateFlowNodeActions(child);
            }
        }
        
        return unresolved;
    }

    /**
     * Validate all action nodes in a NodeGraph
     */
    private static int validateNodeGraphActions(ASTNodeGraph nodeGraph) {
        int unresolved = 0;
        
        for (ASTBTNode node : nodeGraph.getBTNodeList()) {
            if (node instanceof ASTPickUpHL) {
                ASTPickUpHL pickup = (ASTPickUpHL) node;
                
                // Check if obj (Element) symbol is resolved
                if (!pickup.isPresentObjSymbol()) {
                    System.out.println("  ✗ PickUpHL '" + pickup.getName() + "': undefined Element '" + pickup.getObj() + "'");
                    unresolved++;
                } else {
                    System.out.println("  ✓ PickUpHL '" + pickup.getName() + "': Element '" + pickup.getObj() + "' resolved");
                }
                
                // Check if grabPos (Location) symbol is resolved
                if (!pickup.isPresentGrabPosSymbol()) {
                    System.out.println("  ✗ PickUpHL '" + pickup.getName() + "': undefined Location '" + pickup.getGrabPos() + "'");
                    unresolved++;
                }
                
                // Check if client (Robot) symbol is resolved
                if (!pickup.isPresentClientSymbol()) {
                    System.out.println("  ✗ PickUpHL '" + pickup.getName() + "': undefined Robot '" + pickup.getClient() + "'");
                    unresolved++;
                }
            }
        }
        
        return unresolved;
    }
    
    private static void printSuccess(String msg) {
        System.out.println("✓ SUCCESS: " + msg);
    }
    
    private static void printError(String msg) {
        System.err.println("✗ ERROR: " + msg);
    }
    
    private static void printWarning(String msg) {
        System.out.println("⚠ WARNING: " + msg);
    }
    
    private static String spaces(int count) {
        return count > 0 ? String.format("%" + count + "s", "") : "";
    }
}
