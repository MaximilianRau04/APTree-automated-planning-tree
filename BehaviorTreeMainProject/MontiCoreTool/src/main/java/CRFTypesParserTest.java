import crftypedef._parser.CRFTypeDefParser;
import crftypedef._ast.ASTWorld;
import crftypedef._ast.ASTPropertyTypeDefinition;
import crftypedef._ast.ASTProperty;
import crftypedef._ast.ASTPropertyType;
import crftypedef._ast.ASTPredicateTypeDefinition;
import crftypedef._ast.ASTState;
import crftypedef._ast.ASTActionTypeDefinition;
import crftypedef._ast.ASTCRFTypeDefNode;
import crftypedef.CRFTypeDefMill;
import de.monticore.types.mcbasictypes._ast.ASTMCPrimitiveType;
import java.util.Optional;
import java.io.*;

public class CRFTypesParserTest {
    
    public static void main(String[] args) {
        try {
            System.out.println("=== CRF TYPES PARSER TEST ===");
            System.out.println("Parsing CRF types model...\n");
            
            // Initialize MontiCore mill for the grammar
            CRFTypeDefMill.init();
            
            // Define the file to parse
            String filePath = "src/test/resources/valid/CRFTypes/CRFTypes.bt";
            
            // Check if file exists
            File testFile = new File(filePath);
            if (!testFile.exists()) {
                System.err.println("✗ ERROR: Test file not found: " + filePath);
                System.err.println("   Current working directory: " + System.getProperty("user.dir"));
                System.err.println("   Please verify the file location.");
                return;
            }
            
            // Create parser instance
            CRFTypeDefParser parser = new CRFTypeDefParser();
            
            // Parse the file
            Optional<ASTWorld> result = parser.parse(filePath);
            
            if (result.isPresent()) {
                ASTWorld world = result.get();
                System.out.println("✓ SUCCESS: Parsed CRF Types model\n");
                
                // Analyze the parsed model
                analyzeWorld(world);
                System.out.println("\n✓ PARSING COMPLETED SUCCESSFULLY");
                
            } else {
                System.out.println("✗ ERROR: Failed to parse " + filePath);
                System.out.println("   Please check the syntax of your CRF types model file.");
            }
            
        } catch (Exception e) {
            System.err.println("✗ EXCEPTION: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    public static void analyzeWorld(ASTWorld world) {
        System.out.println("=== WORLD STRUCTURE ===");
        
        // Count and display PropertyTypeDefinitions
        int propTypeCount = world.getPropertyTypeDefinitionList().size();
        System.out.println("\n📋 Property Type Definitions: " + propTypeCount);
        for (ASTPropertyTypeDefinition propTypeDef : world.getPropertyTypeDefinitionList()) {
            System.out.println("  ├─ " + propTypeDef.getName());
            System.out.println("  │  └─ Super Type: " + propTypeDef.getSuperType());
            System.out.println("  │  └─ Properties: " + propTypeDef.getPropertyList().size());
        }
        
        // Count and display Properties
        int propCount = world.getPropertyList().size();
        System.out.println("\n📌 Properties: " + propCount);
        for (ASTProperty prop : world.getPropertyList()) {
            System.out.println("  ├─ " + prop.getName() + " : " + prop.getType());
        }
        
        // Count and display PredicateTypeDefinitions
        int predTypeCount = world.getPredicateTypeDefinitionList().size();
        System.out.println("\n🔍 Predicate Type Definitions: " + predTypeCount);
        for (ASTPredicateTypeDefinition predTypeDef : world.getPredicateTypeDefinitionList()) {
            System.out.println("  ├─ " + predTypeDef.getName());
            System.out.println("  │  └─ Properties: " + predTypeDef.getPropertyList().size());
        }
        
        // Count and display States
        int stateCount = world.getStateList().size();
        System.out.println("\n⚙️  States: " + stateCount);
        for (ASTState state : world.getStateList()) {
            System.out.println("  ├─ " + state.getStateType() + " " + state.getName());
            System.out.println("  │  └─ Predicates: " + state.getPredicateList().size());
        }
        
        // Count and display ActionTypeDefinitions
        int actionTypeCount = world.getActionTypeDefinitionList().size();
        System.out.println("\n🎯 Action Type Definitions: " + actionTypeCount);
        for (ASTActionTypeDefinition actionTypeDef : world.getActionTypeDefinitionList()) {
            System.out.println("  ├─ " + actionTypeDef.getTypeName());
            System.out.println("  │  ├─ Level: " + actionTypeDef.getActLevel());
            System.out.println("  │  └─ Properties: " + actionTypeDef.getPropertyList().size());
        }
        
        // Summary statistics
        System.out.println("\n=== STATISTICS ===");
        System.out.println("Total Elements: " + (propTypeCount + propCount + predTypeCount + stateCount + actionTypeCount));
        System.out.println("  - Property Type Definitions: " + propTypeCount);
        System.out.println("  - Properties: " + propCount);
        System.out.println("  - Predicate Type Definitions: " + predTypeCount);
        System.out.println("  - States: " + stateCount);
        System.out.println("  - Action Type Definitions: " + actionTypeCount);
    }
    
    /**
     * Helper method to get the type name from a PropertyType.
     * Handles both MCPrimitiveType (boolean, int, etc.) and CustomProperty types.
     */
    private static String getPropertyTypeName(ASTPropertyType propType) {
        if (propType instanceof ASTMCPrimitiveType) {
            ASTMCPrimitiveType primitiveType = (ASTMCPrimitiveType) propType;
            return primitiveType.printType();
        } else {
            // Try to get name via reflection for CustomProperty subtypes
            try {
                var method = propType.getClass().getMethod("getName");
                Object nameObj = method.invoke(propType);
                return nameObj != null ? nameObj.toString() : propType.getClass().getSimpleName();
            } catch (Exception e) {
                // Fallback to class name without AST prefix
                String className = propType.getClass().getSimpleName();
                return className.startsWith("AST") ? className.substring(3) : className;
            }
        }
    }
}
