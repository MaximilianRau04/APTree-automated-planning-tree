package CoCos.ConcreteBT;

import concretebt._ast.ASTPickUpHL;
import concretebt._ast.ASTPlaceHL;
import concretebt._cocos.ConcreteBTASTPickUpHLCoCo;
import concretebt._cocos.ConcreteBTASTPlaceHLCoCo;
import de.se_rwth.commons.logging.Log;
import crftypedef._symboltable.ElementSymbol;

import java.util.Optional;

public class ElementExistsCoCo implements ConcreteBTASTPickUpHLCoCo, ConcreteBTASTPlaceHLCoCo {

@Override
public void check(ASTPickUpHL node) {
    // 1. Get the raw string name. 
    // In MontiCore, if Name@Element is used, getObj() returns the String name.
    String elementName = node.getObj(); 

    // 2. Check if the string is empty or null (just in case)
    if (elementName == null || elementName.isEmpty()) {
        return;
    }

    // 3. Manually trigger resolution through the scope
    // Use the string 'elementName' to find the 'ElementSymbol'
    Optional<ElementSymbol> symbol = node.getEnclosingScope().resolveElement(elementName);

    if (!symbol.isPresent()) {
        Log.error("0xA001 Error: Element '" + elementName + "' is not defined!", 
                  node.get_SourcePositionStart());
    }
}
    @Override
    public void check(ASTPlaceHL node) {
        String elementName = node.getObj();
        Optional<ElementSymbol> symbol = node.getEnclosingScope().resolveElement(elementName);
        
        // We avoid calling node.getName() directly in the error message if it might be empty
        // causing 0xA7003. Since isPresentName() is missing, we assume it's mandatory but
        // potentially problematic in this specific generated state.
        
        if (!symbol.isPresent()) {
            Log.error("0xA002 Error: Element '" + elementName + "' used in PlaceHL action is not defined! Definition of the element (e.g. Beam, Plate) is missing.", node.get_SourcePositionStart());
        }
    }
}
