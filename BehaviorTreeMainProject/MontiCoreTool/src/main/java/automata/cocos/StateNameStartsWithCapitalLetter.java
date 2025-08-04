/* (c) https://github.com/MontiCore/monticore */
package automata.cocos;

import automata._ast.ASTState;
import automata._cocos.AutomataASTStateCoCo;
import de.se_rwth.commons.logging.Log;

public class StateNameStartsWithCapitalLetter implements AutomataASTStateCoCo {

  @Override
  public void check(ASTState state) {
    String stateName = state.getName();
    boolean startsWithUpperCase = Character.isUpperCase(stateName.charAt(0));

    if (!startsWithUpperCase) {
      // Issue warning...
      Log.warn(String.format("0xB4002 State name '%s' should start with a capital letter.", stateName), state.get_SourcePositionStart());
    }
  }

}
