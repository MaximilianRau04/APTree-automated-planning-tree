/* (c) https://github.com/MontiCore/monticore */
package automata.symboltable;

import automata.AutomataMill;
import automata.AutomataTool;
import automata._ast.ASTAutomaton;
import automata._symboltable.*;
import automata._visitor.AutomataTraverser;
import de.monticore.io.paths.MCPath;
import de.se_rwth.commons.logging.Log;
import de.se_rwth.commons.logging.LogStub;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.nio.file.Paths;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;

public class AutomatonSymbolSerializationTest {

  @BeforeEach
  public void setUp() {
    AutomataMill.reset();
    AutomataMill.init();
    LogStub.init();
    Log.enableFailQuick(false);
    Log.getFindings().clear();
  }

  private IAutomataArtifactScope scope;

  public void setup (String model) {
    ASTAutomaton ast = new AutomataTool().parse("src/test/resources/" + model);

    MCPath mp = new MCPath(Paths.get("src/test/resources/" + model).getParent());
    IAutomataGlobalScope globalScope = AutomataMill.globalScope();
    globalScope.setSymbolPath(mp);
    AutomataScopesGenitor genitor = AutomataMill.scopesGenitor();
    AutomataTraverser traverser = AutomataMill.traverser();
    traverser.setAutomataHandler(genitor);
    traverser.add4Automata(genitor);
    genitor.putOnStack(globalScope);
    scope = genitor.createFromAST(ast);
  }

  @Test
  public void testSerializationOfAutomatonScope () {
    setup("automata/symboltable/PingPong.aut");
    final AutomatonSymbol automatonSymbol = scope.resolveAutomaton("PingPong").orElse(null);
    assertNotNull(automatonSymbol);

    AutomataSymbols2Json symbols2Json = new AutomataSymbols2Json();
    String serialized = symbols2Json.serialize(scope);
    System.out.println(serialized);
    assertTrue(serialized.length() > 0);

    IAutomataScope deserialized = symbols2Json.deserialize(serialized);
    assertNotNull(deserialized);
    assertEquals(1, deserialized.getLocalAutomatonSymbols().size());
    AutomatonSymbol autSymbol = (AutomatonSymbol) deserialized.getLocalAutomatonSymbols().toArray()[0];
    assertEquals("PingPong", autSymbol.getName());
    assertEquals(1, deserialized.getSubScopes().size());
  }

  @Test
  public void testLoadSymbol () {
    MCPath mp = new MCPath(Paths.get("src/test/resources"));
    IAutomataGlobalScope gs = AutomataMill.globalScope();
    gs.clear();
    gs.setSymbolPath(mp);
    Optional<AutomatonSymbol> resolvedAutomaton = gs.resolveAutomaton("automata.serialization.Ping2Pong2");
    assertTrue(resolvedAutomaton.isPresent());
  }

  @Test
  public void testStoreSymbols () {
    String path = "automata/cocos/valid/A.aut";
    AutomataTool.main(new String[] {"-i","src/test/resources/" + path});
    // Note: usually, the stored symbols will be in a file located in the folder structure that
    // matches the package structure. However, the automata language does not defines packages for
    // automata.
    assertTrue(new File("target/symbols/A.autsym").exists());
  }

  @Test
  public void testDoorModel () {
    String path = "automata/symboltable/Door.aut";
    String symbolPath = "target/symbols/Door.autsym";
    AutomataTool.main(new String[] {"-i","src/test/resources/" + path});
    Log.enableFailQuick(false);
    assertTrue(new File(symbolPath).exists());

    AutomataSymbols2Json symbols2Json = new AutomataSymbols2Json();
    IAutomataArtifactScope artifactScope = symbols2Json.load(symbolPath);
    assertEquals("Door", artifactScope.getName());
    assertEquals("", artifactScope.getPackageName());
    assertEquals(1, artifactScope.getSymbolsSize());
    assertEquals(1, artifactScope.getSubScopes().size());
    assertTrue(null == artifactScope.getEnclosingScope());

    AutomatonSymbol automatonSymbol = artifactScope.getLocalAutomatonSymbols().get(0);
    assertEquals("Door", automatonSymbol.getName());
    assertTrue(null != automatonSymbol.getSpannedScope());
    assertEquals(artifactScope, automatonSymbol.getEnclosingScope());

    IAutomataScope automatonScope = automatonSymbol.getSpannedScope();
    assertEquals("Door", automatonScope.getName());
    assertEquals(5, automatonScope.getSymbolsSize());
    assertEquals(0, automatonScope.getSubScopes().size());
    assertEquals(artifactScope, automatonSymbol.getEnclosingScope());

    StateSymbol close = automatonScope.getLocalStateSymbols().get(0);
    assertEquals("Close", close.getName());
    assertEquals(automatonScope, close.getEnclosingScope());

    StateSymbol closing = automatonScope.getLocalStateSymbols().get(1);
    assertEquals("Closing", closing.getName());
    assertEquals(automatonScope, closing.getEnclosingScope());

    StateSymbol closed = automatonScope.getLocalStateSymbols().get(2);
    assertEquals("Closed", closed.getName());
    assertEquals(automatonScope, closed.getEnclosingScope());

    StateSymbol opening = automatonScope.getLocalStateSymbols().get(3);
    assertEquals("Opening", opening.getName());
    assertEquals(automatonScope, opening.getEnclosingScope());

    StateSymbol open = automatonScope.getLocalStateSymbols().get(4);
    assertEquals("Open", open.getName());
    assertEquals(automatonScope, open.getEnclosingScope());

  }

}
