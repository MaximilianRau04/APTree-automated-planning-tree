import crftypedef._parser.CRFTypeDefParser;
import crftypedef._ast.ASTWorld;
import crftypedef._ast.ASTActionTypeDefinition;
import crftypedef._ast.ASTProperty;
import crftypedef._ast.ASTPredicateRef;
import crftypedef.CRFTypeDefMill;

import java.util.Optional;
import java.util.List;
import java.io.*;

/**
 * CRFActionTypesParserTest - Parses and displays ActionTypeDefinitions from CRFActionTypes.bt
 */
public class CRFActionTypesParserTest {
    
    private static final String DEFAULT_PATH = "src/test/resources/valid/CRFTypes/CRFActionTypes.bt";
    
    public static void main(String[] args) {
        try {
            System.out.println("=== CRF ACTION TYPES PARSER TEST ===");
            System.out.println("Parsing CRF action types model...\n");
            
            // Initialize MontiCore mill for the grammar
            CRFTypeDefMill.init();
            
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
            CRFTypeDefParser parser = new CRFTypeDefParser();
            
            // Parse the file
            Optional<ASTWorld> result = parser.parse(filePath);
            
            if (result.isPresent()) {
                ASTWorld world = result.get();
                System.out.println("✓ SUCCESS: Parsed file: " + filePath + "\n");
                
                // Analyze the parsed model
                analyzeActionTypes(world);
                
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
     * Analyze and display all ActionTypeDefinitions in the parsed world
     */
    public static void analyzeActionTypes(ASTWorld world) {
        List<ASTActionTypeDefinition> actionTypes = world.getActionTypeDefinitionList();
        
        System.out.println("========================================");
        System.out.println("ACTION TYPE DEFINITIONS: " + actionTypes.size());
        System.out.println("========================================\n");
        
        for (ASTActionTypeDefinition actionDef : actionTypes) {
            displayActionType(actionDef);
        }
    }
    
    /**
     * Display a single ActionTypeDefinition with all its details
     */
    public static void displayActionType(ASTActionTypeDefinition actionDef) {
        String actionName = actionDef.getTypeName();
        String actionLevel = actionDef.getActLevel().name();
        
        System.out.println("┌─────────────────────────────────────");
        System.out.println("│ Action: " + actionName);
        System.out.println("│ Level:  " + actionLevel);
        System.out.println("├─────────────────────────────────────");
        
        // Display parameters
        System.out.println("│ PARAMETERS:");
        List<ASTProperty> params = actionDef.getPropertyList();
        if (params.isEmpty()) {
            System.out.println("│   (none)");
        } else {
            for (ASTProperty param : params) {
                System.out.println("│   - " + param.getName() + " : " + param.getType());
            }
        }
        
        // Display preconditions
        System.out.println("│");
        System.out.println("│ PRECONDITIONS:");
        List<ASTPredicateRef> preconditions = actionDef.getPreconditionsList();
        if (preconditions.isEmpty()) {
            System.out.println("│   (none)");
        } else {
            for (ASTPredicateRef pred : preconditions) {
                System.out.println("│   - " + formatPredicateRef(pred));
            }
        }
        
        // Display effects
        System.out.println("│");
        System.out.println("│ EFFECTS:");
        List<ASTPredicateRef> effects = actionDef.getEffectsList();
        if (effects.isEmpty()) {
            System.out.println("│   (none)");
        } else {
            for (ASTPredicateRef pred : effects) {
                System.out.println("│   - " + formatPredicateRef(pred));
            }
        }
        
        System.out.println("└─────────────────────────────────────\n");
    }
    
    /**
     * Format a PredicateRef as a readable string
     * e.g., "atplace(obj, grabPos)"
     */
    public static String formatPredicateRef(ASTPredicateRef pred) {
        StringBuilder sb = new StringBuilder();
        sb.append(pred.getName()).append("(");
        
        List<String> args = pred.getArgsList();
        for (int i = 0; i < args.size(); i++) {
            if (i > 0) sb.append(", ");
            sb.append(args.get(i));
        }
        
        sb.append(")");
        return sb.toString();
    }
}
