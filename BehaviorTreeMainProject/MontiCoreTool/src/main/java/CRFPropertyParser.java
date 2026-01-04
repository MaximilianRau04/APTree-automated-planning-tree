import crftypedef._parser.CRFTypeDefParser;
import crftypedef._ast.ASTWorld;
import crftypedef._ast.ASTPropertyTypeDefinition;
import crftypedef._ast.ASTProperty;
import crftypedef.CRFTypeDefMill;

import java.io.*;
import java.nio.file.*;
import java.util.Optional;
import java.util.List;
import java.util.ArrayList;

/**
 * CRFPropertyParser - Reads CRFTypes model and generates grammar rules for ConcreteBT.mc4
 * 
 * For each PropertyTypeDefinition like:
 *   define Beam as Element {
 *     lenght: DOUBLE_VALUE
 *     color: Name
 *   }
 * 
 * Generates a grammar rule:
 *   Beam extends Element = "Beam" name:Name "(" lenght:DOUBLE_VALUE color:Name ")";
 */
public class CRFPropertyParser {

    private static final String CRFTYPES_PATH = "src/test/resources/valid/CRFTypes/CRFPropertyTypes.bt";
    private static final String CONCRETE_BT_PATH = "src/main/grammars/ConcreteBT.mc4";
    
    public static void main(String[] args) {
        try {
            System.out.println("=== CRF PROPERTY PARSER ===");
            System.out.println("Generating grammar rules from CRFTypes model...\n");
            
            // Initialize MontiCore mill
            CRFTypeDefMill.init();
            
            // Parse the CRFTypes model
            String modelPath = args.length > 0 ? args[0] : CRFTYPES_PATH;
            List<String> grammarRules = parseAndGenerateRules(modelPath);
            
            if (grammarRules.isEmpty()) {
                System.out.println("No PropertyTypeDefinitions found to generate.");
                return;
            }
            
            // Display generated rules
            System.out.println("=== GENERATED GRAMMAR RULES ===");
            for (String rule : grammarRules) {
                System.out.println(rule);
            }
            
            // Write to ConcreteBT.mc4
            String grammarPath = args.length > 1 ? args[1] : CONCRETE_BT_PATH;
            writeRulesToGrammar(grammarRules, grammarPath);
            
            System.out.println("\n✓ Grammar rules successfully written to " + grammarPath);
            
        } catch (Exception e) {
            System.err.println("✗ ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    /**
     * Parse the CRFTypes model and generate grammar rules for each PropertyTypeDefinition
     */
    public static List<String> parseAndGenerateRules(String modelPath) throws IOException {
        List<String> rules = new ArrayList<>();
        
        // Check if file exists
        File modelFile = new File(modelPath);
        if (!modelFile.exists()) {
            throw new FileNotFoundException("Model file not found: " + modelPath);
        }
        
        // Create parser and parse
        CRFTypeDefParser parser = new CRFTypeDefParser();
        Optional<ASTWorld> result = parser.parse(modelPath);
        
        if (!result.isPresent()) {
            throw new RuntimeException("Failed to parse model: " + modelPath);
        }
        
        ASTWorld world = result.get();
        System.out.println("✓ Parsed model: " + modelPath);
        System.out.println("  Found " + world.getPropertyTypeDefinitionList().size() + " PropertyTypeDefinitions\n");
        
        // Generate a rule for each PropertyTypeDefinition
        for (ASTPropertyTypeDefinition propTypeDef : world.getPropertyTypeDefinitionList()) {
            String rule = generateGrammarRule(propTypeDef);
            rules.add(rule);
        }
        
        return rules;
    }
    
    /**
     * Generate a grammar rule from a PropertyTypeDefinition
     * 
     * Input:
     *   define Beam as Element {
     *     lenght: DOUBLE_VALUE
     *     color: Name
     *   }
     * 
     * Output:
     *   symbol Beam extends Element = "Beam" name:Name "(" lenght:DOUBLE_VALUE color:Name ")";
     */
    public static String generateGrammarRule(ASTPropertyTypeDefinition propTypeDef) {
        String typeName = propTypeDef.getName();
        String superType = propTypeDef.getSuperType();
        List<ASTProperty> properties = propTypeDef.getPropertyList();
        
        StringBuilder rule = new StringBuilder();
        
        // Start: symbol TypeName extends SuperType = "TypeName" name:Name "("
        rule.append("symbol ")
            .append(typeName)
            .append(" extends ")
            .append(superType)
            .append(" = \"")
            .append(typeName)
            .append("\" name:Name \"(\"");
        
        // Add properties: propName:propType
        for (int i = 0; i < properties.size(); i++) {
            ASTProperty prop = properties.get(i);
            String propName = prop.getName();
            String propType = prop.getType();
            
            rule.append(" ")
                .append(propName)
                .append(":")
                .append(propType);
        }
        
        // End: ")";
        rule.append(" \")\";");
        
        return rule.toString();
    }
    
    /**
     * Write the generated rules to the ConcreteBT.mc4 grammar file
     * Inserts the rules between the grammar's curly brackets
     */
    public static void writeRulesToGrammar(List<String> rules, String grammarPath) throws IOException {
        Path path = Paths.get(grammarPath);
        
        if (!Files.exists(path)) {
            throw new FileNotFoundException("Grammar file not found: " + grammarPath);
        }
        
        // Read the current grammar content
        String content = new String(Files.readAllBytes(path));
        
        // Find the position to insert (before the closing brace, after existing content)
        // Look for a marker comment or insert before the last }
        String marker = "// === GENERATED RULES (DO NOT EDIT BELOW) ===";
        String endMarker = "// === END GENERATED RULES ===";
        
        // Build the rules block
        StringBuilder rulesBlock = new StringBuilder();
        rulesBlock.append("\n\n").append(marker).append("\n");
        for (String rule : rules) {
            rulesBlock.append(rule).append("\n");
        }
        rulesBlock.append(endMarker).append("\n");
        
        // Check if we already have generated rules - replace them
        if (content.contains(marker)) {
            int startIdx = content.indexOf(marker);
            int endIdx = content.indexOf(endMarker);
            if (endIdx > startIdx) {
                endIdx += endMarker.length();
                content = content.substring(0, startIdx) + rulesBlock.toString().trim() + content.substring(endIdx);
            }
        } else {
            // Insert before the last closing brace
            int lastBrace = content.lastIndexOf('}');
            if (lastBrace == -1) {
                throw new RuntimeException("Invalid grammar file: no closing brace found");
            }
            content = content.substring(0, lastBrace) + rulesBlock.toString() + content.substring(lastBrace);
        }
        
        // Write back
        Files.write(path, content.getBytes());
    }
}
