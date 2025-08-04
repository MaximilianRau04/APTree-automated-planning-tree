/* (c) https://github.com/MontiCore/monticore */
package automata2cd;

import automata.AutomataMill;
import automata._ast.ASTAutomaton;
import automata._visitor.AutomataTraverser;
import de.monticore.cd4code.CD4CodeMill;
import de.monticore.generating.templateengine.GlobalExtensionManagement;

public class Automata2CDConverter {
  
  public Automata2CDData doConvert(ASTAutomaton astAutomaton, GlobalExtensionManagement glex) {
    
    // Phase 1: Work on states
    Automata2CDStateVisitor phase1Visitor = new Automata2CDStateVisitor(glex);
    AutomataTraverser traverser = AutomataMill.inheritanceTraverser();
    traverser.add4Automata(phase1Visitor);
  
    // we use the CD4Code language for the CD (and now switch to it)
    CD4CodeMill.init();
  
    traverser.handle(astAutomaton);
  
    // Phase 2: Work with transitions
    Automata2CDUMLTransitionVisitor phase2Visitor = new Automata2CDUMLTransitionVisitor(phase1Visitor.getScClass(),
      phase1Visitor.getStateToClassMap(),
      phase1Visitor.getStateSuperClass());
    traverser = AutomataMill.inheritanceTraverser();
    traverser.add4Automata(phase2Visitor);
    traverser.handle(astAutomaton);
  
    // voila
    return new Automata2CDData(phase1Visitor.getCdCompilationUnit(), phase1Visitor.getScClass(),
      phase1Visitor.getStateSuperClass(),
      phase1Visitor.getStateToClassMap().values());
  }
  
}
