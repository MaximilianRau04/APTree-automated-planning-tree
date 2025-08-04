/* (c) https://github.com/MontiCore/monticore */
package automata.cocos;

import automata.AutomataMill;
import automata._ast.ASTAutomaton;
import automata._ast.ASTTransition;
import automata._cocos.AutomataCoCoChecker;
import automata._symboltable.AutomataScopesGenitor;
import automata._symboltable.IAutomataGlobalScope;
import automata._visitor.AutomataTraverser;
import automata.lang.AbstractTest;
import de.monticore.cocos.helper.Assert;
import de.monticore.io.paths.MCPath;
import de.se_rwth.commons.SourcePosition;
import de.se_rwth.commons.logging.Finding;
import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.antlr.v4.runtime.RecognitionException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.nio.file.Paths;
import java.util.Collection;
import java.util.Collections;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class TransitionStatesExistTest extends AbstractTest {

  private IAutomataGlobalScope globalScope;

  @BeforeEach
  public void setUp() throws RecognitionException {
    AutomataMill.reset();
    AutomataMill.init();
    LogStub.init();
    Log.enableFailQuick(false);
    Log.getFindings().clear();
    globalScope = AutomataMill.globalScope();
    globalScope.clear();
  }
  
  @Test
  public void testValid() {
    globalScope.setSymbolPath(new MCPath(Paths.get("src/test/resources/automata/cocos/valid")));

    ASTAutomaton ast = parseModel("src/test/resources/automata/cocos/valid/A.aut");
    AutomataScopesGenitor genitor = AutomataMill.scopesGenitor();
    AutomataTraverser traverser = AutomataMill.traverser();
    traverser.setAutomataHandler(genitor);
    traverser.add4Automata(genitor);
    genitor.putOnStack(globalScope);
    genitor.createFromAST(ast);
    
    AutomataCoCoChecker checker = new AutomataCoCos().getCheckerForAllCoCos();
    checker.checkAll(ast);
    
    //The error log would be empty if automaton A was created from AST before being resolved
    assertEquals(0,Log.getErrorCount());
  }
  
  @Test
  public void testNotExistingTransitionSource() {
    globalScope.setSymbolPath(new MCPath(Paths.get("src/test/resources/automata/cocos/invalid")));
    ASTAutomaton ast = parseModel("src/test/resources/automata/cocos/invalid/NotExistingTransitionSource.aut");
    AutomataScopesGenitor genitor = AutomataMill.scopesGenitor();
    AutomataTraverser traverser = AutomataMill.traverser();
    traverser.setAutomataHandler(genitor);
    traverser.add4Automata(genitor);
    genitor.putOnStack(globalScope);
    genitor.createFromAST(ast);

    ASTTransition transition = ast.getTransitionList().get(0);
    
    TransitionStatesExist coco = new TransitionStatesExist();
    coco.check(transition);
    
    Collection<Finding> expectedErrors = Collections.singletonList(
      Finding.error("0xB4003 The source state of the transition does not exist.",
        new SourcePosition(6, 2)));
    
    Assert.assertErrors(expectedErrors, Log.getFindings());
  }
  
}
