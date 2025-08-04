/* (c) https://github.com/MontiCore/monticore */
package automata.symboltable;

import automata.AutomataMill;
import automata.AutomataTool;
import automata._ast.ASTAutomaton;
import automata._symboltable.AutomataScopesGenitor;
import automata._symboltable.IAutomataGlobalScope;
import automata._symboltable.IAutomataScope;
import automata._visitor.AutomataTraverser;
import de.monticore.io.paths.MCPath;
import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.nio.file.Paths;

import static org.junit.jupiter.api.Assertions.assertTrue;

public class AutomatonSymbolTableCreatorTest {

  private IAutomataGlobalScope globalScope;

  @BeforeEach
  public void setup() {

    AutomataMill.reset();
    AutomataMill.init();
    LogStub.init();
    Log.enableFailQuick(false);
    Log.getFindings().clear();

    final MCPath symbolPath =
            new MCPath(Paths.get("src/test/resources/automata/symboltable"));

    globalScope = AutomataMill.globalScope();
    globalScope.clear();
    globalScope.setSymbolPath(symbolPath);
    
    Log.enableFailQuick(false);
  }

  @Test
  public void testAutomatonSymbolTableCreation2(){
    ASTAutomaton ast = new AutomataTool()
            .parse("src/test/resources/automata/symboltable/PingPong.aut");
    AutomataScopesGenitor genitor = AutomataMill.scopesGenitor();
    AutomataTraverser traverser = AutomataMill.traverser();
    traverser.setAutomataHandler(genitor);
    traverser.add4Automata(genitor);
    genitor.putOnStack(globalScope);
    IAutomataScope artifact = genitor.createFromAST(ast);

    IAutomataScope s = artifact.getSubScopes().stream().findAny().get();
    assertTrue(s.resolveState("NoGame").isPresent());
    assertTrue(s.resolveState("PingPong.NoGame").isPresent());
    assertTrue(s.resolveAutomaton("PingPong").isPresent());

    assertTrue(globalScope.resolveAutomaton("PingPong").isPresent());
    assertTrue(globalScope.resolveState("PingPong.NoGame").isPresent());

    assertTrue(artifact.resolveAutomaton("PingPong").isPresent());
    assertTrue(artifact.resolveState("PingPong.NoGame").isPresent());

  }

}
