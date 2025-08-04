/* (c) https://github.com/MontiCore/monticore */
package automata.cocos;

import automata._ast.ASTTransition;
import automata._cocos.AutomataASTTransitionCoCo;
import de.se_rwth.commons.logging.Log;

import java.util.HashSet;
import java.util.Set;
import java.util.Map;
import java.util.HashMap;

public class AutomatonIsDeterministic implements AutomataASTTransitionCoCo {

   protected Map<String, Set<String>> transitions = new HashMap<>();

  @Override
  public void check(ASTTransition node) {
    String from = node.getFrom();
    String input = node.getInput();
    if(!transitions.containsKey(from)) {
      Set<String> inputs = new HashSet<>();
      inputs.add(input);
      transitions.put(from, inputs);
    } else {
      if(!transitions.get(from).contains(input)) {
        transitions.get(from).add(input);
      } else {
        Log.error("0x14708 Automaton must be deterministic");
      }
    }
  }
}
