/* (c) https://github.com/MontiCore/monticore */
package automata.cocos;

import automata.AutomataMill;
import automata._ast.ASTAutomaton;
import automata._cocos.AutomataCoCoChecker;
import automata._symboltable.AutomataScopesGenitor;
import automata._visitor.AutomataTraverser;
import automata.lang.AbstractTest;
import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.antlr.v4.runtime.RecognitionException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

public class AutomatonIsDeterministicTest extends AbstractTest {

  @BeforeEach
  public void setUp() throws RecognitionException {
    AutomataMill.reset();
    AutomataMill.init();
    LogStub.init();
    Log.enableFailQuick(false);
    Log.getFindings().clear();
  }

  @Test
  public void testValid() {
    ASTAutomaton ast = parseModel("src/test/resources/automata/cocos/valid/A.aut");
    createSymTab(ast);
    AutomataCoCoChecker checker = new AutomataCoCos().getCheckerForAllCoCos();
    checker.checkAll(ast);
    assertTrue(Log.getFindings().isEmpty());
  }

  @Test
  public void testInvalid() {
    ASTAutomaton ast = parseModel("src/test/resources/automata/cocos/invalid/NonDeterministicAutomaton.aut");
    createSymTab(ast);
    AutomataCoCoChecker checker = new AutomataCoCos().getCheckerForAllCoCos();
    checker.checkAll(ast);
    assertEquals(1, Log.getFindings().size());
    assertTrue(Log.getFindings().get(0).getMsg().startsWith("0x14708"));
  }

  private static void createSymTab(ASTAutomaton ast) {
    AutomataScopesGenitor genitor = new AutomataScopesGenitor();
    AutomataTraverser traverser = AutomataMill.traverser();
    traverser.setAutomataHandler(genitor);
    traverser.add4Automata(genitor);
    genitor.createFromAST(ast);
  }
}
