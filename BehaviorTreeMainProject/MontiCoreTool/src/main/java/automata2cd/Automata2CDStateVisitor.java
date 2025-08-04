/* (c) https://github.com/MontiCore/monticore */
package automata2cd;

import automata._ast.ASTAutomaton;
import automata._ast.ASTState;
import automata._ast.ASTTransition;
import automata._visitor.AutomataVisitor2;
import de.monticore.cd.methodtemplates.CD4C;
import de.monticore.cd4code.CD4CodeMill;
import de.monticore.cdbasis.CDBasisMill;
import de.monticore.cdbasis._ast.*;
import de.monticore.cdinterfaceandenum._ast.ASTCDEnum;
import de.monticore.cdinterfaceandenum._ast.ASTCDEnumConstant;
import de.monticore.generating.templateengine.GlobalExtensionManagement;
import de.monticore.types.mcbasictypes._ast.ASTMCQualifiedType;
import de.monticore.umlmodifier.UMLModifierMill;
import de.se_rwth.commons.Splitters;
import de.se_rwth.commons.logging.Log;

import java.util.*;
import java.util.stream.Collectors;

public class Automata2CDStateVisitor implements AutomataVisitor2 {
  
  public final static String ERROR_CODE = "0xDC012";
  
  protected ASTAutomaton astAutomaton;
  
  protected ASTCDCompilationUnit cdCompilationUnit;
  
  protected ASTCDPackage cdPackage;

  protected ASTCDClass automataClass;
  /**
   * The super class for all state implementations
   */
  protected ASTCDClass stateSuperClass;
  /**
   * Mapping of the state implementation classes for every state
   */
  protected final Map<String, ASTCDClass> stateToClassMap = new HashMap<>();
  /**
   * Code template reference
   */
  protected final CD4C cd4C;
  
  protected final GlobalExtensionManagement glex;
  
  /**
   * Name of the initial state
   */
  protected String initialState = "";
  
  public Automata2CDStateVisitor(GlobalExtensionManagement glex) {
    this.cd4C = CD4C.getInstance();
    this.glex = glex;
  }
  
  @Override
  public void visit(ASTAutomaton astAutomaton) {
    this.astAutomaton = astAutomaton;
  
    // Add a CDDefinition for every automaton
    ASTCDDefinition astcdDefinition = CDBasisMill.cDDefinitionBuilder().setName(astAutomaton.getName())
      .setModifier(UMLModifierMill.modifierBuilder().build()).build();
  
    cdPackage = CDBasisMill.cDPackageBuilder()
      .setMCQualifiedName(CDBasisMill.mCQualifiedNameBuilder()
        .setPartsList(Arrays.asList(astAutomaton.getName().toLowerCase()))
        .build())
      .build();
    
    astcdDefinition.addCDElement(cdPackage);
    
    ASTCDCompilationUnitBuilder cdCompilationUnitBuilder = CDBasisMill.cDCompilationUnitBuilder();
    cdCompilationUnitBuilder.setCDDefinition(astcdDefinition);
  
    cdCompilationUnit = cdCompilationUnitBuilder.build();
    this.stateSuperClass = createStateSuperClass();
    
    // Main class, names equally to the Automaton
    automataClass = CDBasisMill.cDClassBuilder().setName(astAutomaton.getName())
      .setModifier(CDBasisMill.modifierBuilder().PUBLIC().build()).build();
    cdPackage.addCDElement(automataClass);
    
    // replace the template to add a setState method
    this.cd4C.addMethod(this.automataClass, "automaton2cd.StateSetStateMethod");
    this.cd4C.addAttribute(this.automataClass, "protected "+getStateSuperClass().getName()+" state");
  
    ASTCDEnum stimuli = CD4CodeMill.cDEnumBuilder()
      .setModifier(CDBasisMill.modifierBuilder().PUBLIC().build())
      .setName("Stimuli")
      .addAllCDEnumConstants(createConstants(astAutomaton.getTransitionList()))
      .build();
    cdPackage.addCDElement(stimuli);
    
    cd4C.addMethod(automataClass, "automaton2cd.Run",
      stimuli.getCDEnumConstantList().stream().map(ASTCDEnumConstant::getName).collect(Collectors.toList()));
  }
  
  @Override
  public void endVisit(ASTAutomaton astAutomaton) {
    // Generate the constructor of the class
    // It inits all state attribute and the initial state
    this.cd4C.addConstructor(this.automataClass, "automaton2cd.StateInitConstructor",
      automataClass.getName(),
      this.stateToClassMap.keySet(),
      this.initialState);
    automataClass.getCDAttributeList().forEach(a ->
      this.cd4C.addMethods(this.automataClass, a, true, false));
  }
  
  @Override
  public void visit(ASTState state) {
    if (state.getName().equals("state")) {
      Log.error(ERROR_CODE + "State is named \"state\", which interferes with the attribute for the currently selected state");
    }
    if (initialState.isEmpty() || state.isInitial()) {
      initialState = state.getName();
    }
    
    // A class extending StateClass for this state
    ASTCDClass stateClass = CDBasisMill.cDClassBuilder().setName(state.getName())
      .setModifier(CDBasisMill.modifierBuilder().PUBLIC().build())
      .setCDExtendUsage(CDBasisMill.cDExtendUsageBuilder().addSuperclass(qualifiedType("StateClass")).build())
      .build();
    
    cd4C.addConstructor(stateClass, "automaton2cd.StateConstructor", state.getName(), state.isFinal());
  
    // Add the StateClass to the CD and mapping
    this.cdPackage.addCDElement(stateClass);
    this.stateToClassMap.put(state.getName(), stateClass);
    
    // Add reference to this in the main class, in form of an attribute
    String name = " "+state.getName();
    cd4C.addAttribute(automataClass, "protected"+name+name.substring(0,2).toLowerCase()+name.substring(2));
  }

  
  public ASTCDCompilationUnit getCdCompilationUnit() {
    return cdCompilationUnit;
  }
  
  public ASTCDClass getScClass() {
    return automataClass;
  }
  
  public Map<String, ASTCDClass> getStateToClassMap() {
    return stateToClassMap;
  }
  
  public ASTCDClass getStateSuperClass() {
    return stateSuperClass;
  }
  
  // Support methods
  protected ASTMCQualifiedType qualifiedType(String qname) {
    return qualifiedType(Splitters.DOT.splitToList(qname));
  }
  
  protected ASTMCQualifiedType qualifiedType(List<String> partsList) {
    return CD4CodeMill.mCQualifiedTypeBuilder()
      .setMCQualifiedName(CD4CodeMill.mCQualifiedNameBuilder().setPartsList(partsList).build()).build();
  }
  
  protected ASTCDClass createStateSuperClass() {
    ASTCDClass astClass = CDBasisMill.cDClassBuilder()
      .setModifier(CDBasisMill.modifierBuilder().PUBLIC().ABSTRACT().build())
      .setName("StateClass").build();
    cdPackage.addCDElement(astClass);
    cd4C.addAttribute(astClass, true, false, "protected boolean isFinal");
    return astClass;
  }
  
  protected List<ASTCDEnumConstant> createConstants(List<ASTTransition> transitionList) {
    return transitionList.stream()
      .map(ASTTransition::getInput).distinct()
      .map(s -> CD4CodeMill.cD4CodeEnumConstantBuilder().setName(s).build())
      .collect(Collectors.toList());
  }
}
