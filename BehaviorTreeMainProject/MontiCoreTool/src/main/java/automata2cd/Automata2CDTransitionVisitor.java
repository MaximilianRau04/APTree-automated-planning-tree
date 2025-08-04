/* (c) https://github.com/MontiCore/monticore */
package automata2cd;

import automata._ast.ASTTransition;
import automata._visitor.AutomataVisitor2;
import de.monticore.cd.methodtemplates.CD4C;
import de.monticore.cd4code.CD4CodeMill;
import de.monticore.cdbasis.CDBasisMill;
import de.monticore.cdbasis._ast.ASTCDClass;
import de.monticore.types.mcbasictypes._ast.ASTMCQualifiedType;
import de.monticore.types.mcbasictypes._ast.ASTMCReturnType;
import de.se_rwth.commons.Splitters;

import java.util.*;

public abstract class Automata2CDTransitionVisitor implements AutomataVisitor2 {
  
  /**
   * The main class
   */
  protected final ASTCDClass automataClass;
  
  /**
   * Mapping of the state implementation classes for every state
   */
  protected final Map<String, ASTCDClass> stateToClassMap;
  
  protected final ASTMCReturnType voidReturnType;
  protected final ASTCDClass stateSuperClass;
  protected final CD4C cd4C;
  protected final Set<String> stimuli = new HashSet<>();
  
  public Automata2CDTransitionVisitor(ASTCDClass automataClass,
                                Map<String, ASTCDClass> stateToClassMap,
                                ASTCDClass stateSuperClass) {
    this.automataClass = automataClass;
    this.stateToClassMap = stateToClassMap;
    this.stateSuperClass = stateSuperClass;
    this.cd4C = CD4C.getInstance();
    
    this.voidReturnType = CDBasisMill.mCReturnTypeBuilder().setMCVoidType(CDBasisMill.mCVoidTypeBuilder().build()).build();
  }
  
  protected ASTMCQualifiedType qualifiedType(String qname) {
    return qualifiedType(Splitters.DOT.splitToList(qname));
  }
  
  protected ASTMCQualifiedType qualifiedType(List<String> partsList) {
    return CD4CodeMill.mCQualifiedTypeBuilder()
      .setMCQualifiedName(CD4CodeMill.mCQualifiedNameBuilder().setPartsList(partsList).build()).build();
  }
  
  protected Optional<ASTTransition> transition = Optional.empty();
  
  @Override
  public void visit(ASTTransition node) {
    this.transition = Optional.of(node);
  }
  
  @Override
  public void endVisit(ASTTransition node) {
    this.transition = Optional.empty();
  }

}
