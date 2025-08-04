/* (c) https://github.com/MontiCore/monticore */
package automata.prettyprint;

import automata.AutomataMill;
import automata._ast.ASTAutomaton;
import automata._prettyprint.AutomataFullPrettyPrinter;
import automata.lang.AbstractTest;
import de.monticore.prettyprint.IndentPrinter;
import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class AutomataPrettyPrinterTest extends AbstractTest {

  @BeforeEach
  public void setUp() {
    AutomataMill.reset();
    AutomataMill.init();
    LogStub.init();
    Log.enableFailQuick(false);
    Log.getFindings().clear();
  }

  @Test
  public void testPrettyPrinter() {
    ASTAutomaton automaton = parseModel("src/test/resources/automata/prettyprinter/valid/A.aut");
    AutomataFullPrettyPrinter pp = new AutomataFullPrettyPrinter(new IndentPrinter());
    String printed = pp.prettyprint(automaton);
    ASTAutomaton reparsed = parseStringModel(printed);
    String reprinted = pp.prettyprint(reparsed);
    assertEquals(printed, reprinted);
  }

}
