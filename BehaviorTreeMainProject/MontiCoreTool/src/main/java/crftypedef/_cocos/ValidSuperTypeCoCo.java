package crftypedef._cocos;

import crftypedef._ast.ASTPropertyTypeDefinition;
import de.se_rwth.commons.logging.Log;

import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

/**
 * CoCo that validates the superType in PropertyTypeDefinition.
 * Ensures that the superType is one of the valid CustomProperty types:
 * Element, Location, Tool, Agent, Layer, Module
 */
public class ValidSuperTypeCoCo implements CRFTypeDefASTPropertyTypeDefinitionCoCo {

    // Valid custom property types
    private static final Set<String> VALID_SUPER_TYPES = new HashSet<>(Arrays.asList(
        "Element",
        "Location", 
        "Tool",
        "Agent",
        "Layer",
        "Module"
    ));

    @Override
    public void check(ASTPropertyTypeDefinition node) {
        String superType = node.getSuperType();
        
        if (!VALID_SUPER_TYPES.contains(superType)) {
            Log.error(String.format(
                "0xCRF01: Invalid superType '%s' in PropertyTypeDefinition '%s'. " +
                "Valid types are: %s",
                superType,
                node.getName(),
                String.join(", ", VALID_SUPER_TYPES)
            ), node.get_SourcePositionStart());
        }
    }
}
