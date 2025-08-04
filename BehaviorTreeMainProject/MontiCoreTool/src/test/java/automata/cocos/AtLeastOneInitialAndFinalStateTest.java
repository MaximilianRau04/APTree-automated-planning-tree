/* (c) https://github.com/MontiCore/monticore */
package automata.cocos;

import automata.AutomataMill;
import automata._ast.ASTAutomaton;
import automata.lang.AbstractTest;
import de.monticore.cocos.helper.Assert;
import de.se_rwth.commons.SourcePosition;
import de.se_rwth.commons.logging.Finding;
import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.antlr.v4.runtime.RecognitionException;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.util.Collection;
import java.util.Collections;

import static org.junit.jupiter.api.Assertions.assertTrue;

public class AtLeastOneInitialAndFinalStateTest extends AbstractTest {

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
    ASTAutomaton automaton = parseModel("src/test/resources/automata/cocos/valid/A.aut");

    AtLeastOneInitialAndFinalState coco = new AtLeastOneInitialAndFinalState();
    coco.check(automaton);

    assertTrue(Log.getFindings().isEmpty());
  }

  @Test
  public void testMissingInitialState() {
    ASTAutomaton automaton = parseModel("src/test/resources/automata/cocos/invalid/MissingInitialState.aut");

    AtLeastOneInitialAndFinalState coco = new AtLeastOneInitialAndFinalState();
    coco.check(automaton);

    Collection<Finding> expectedErrors = Collections.singletonList(
      Finding.error("0xB4114 An automata must have at least one initial and one final state.",
        new SourcePosition(3, 0)));

    Assert.assertErrors(expectedErrors, Log.getFindings());
  }

}
