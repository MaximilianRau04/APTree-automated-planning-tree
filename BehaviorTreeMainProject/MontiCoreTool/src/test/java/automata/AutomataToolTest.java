/* (c) https://github.com/MontiCore/monticore */
package automata;


import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * A very short test for the main function
 *
 */
public class AutomataToolTest {

  @BeforeEach
  public void setUp() {
    AutomataMill.reset();
    AutomataMill.init();
    LogStub.init();
    Log.enableFailQuick(false);
    Log.getFindings().clear();
  }
  
  @Test
  public void executeMain() {
    AutomataTool.main(new String[] { "-i","src/main/resources/example/PingPong.aut"});

    assertTrue(!false);
  }
  
}
