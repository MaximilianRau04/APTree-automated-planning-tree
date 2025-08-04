/* (c) https://github.com/MontiCore/monticore */
package automata.cocos;

import automata._cocos.AutomataCoCoChecker;

public class AutomataCoCos {
  
  public AutomataCoCoChecker getCheckerForAllCoCos() {
    final AutomataCoCoChecker checker = new AutomataCoCoChecker();
    checker.addCoCo(new AtLeastOneInitialAndFinalState());
    checker.addCoCo(new StateNameStartsWithCapitalLetter());
    checker.addCoCo(new TransitionStatesExist());
    checker.addCoCo(new AutomatonIsDeterministic());
    
    return checker;
  }
}
