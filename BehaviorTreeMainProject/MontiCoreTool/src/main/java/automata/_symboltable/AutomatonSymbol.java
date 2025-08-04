/* (c) https://github.com/MontiCore/monticore */
package automata._symboltable;

import de.monticore.symboltable.ISymbol;

import java.util.Collection;
import java.util.Optional;

public class AutomatonSymbol extends AutomatonSymbolTOP {

  protected String template;

  public AutomatonSymbol(final String name) {
    super(name);
    template = "";
  }

  /**
   * Factory method for creating the scope spanned by this symbol.
   *
   * @param name The name of the state.
   * @return the (newly) created scope spanned by this symbol.
   */
  public Optional<StateSymbol> getState(final String name) {
    return getSpannedScope().resolveStateLocally(name);
  }

  public Collection<StateSymbol> getStates() {
    return ISymbol.sortSymbolsByPosition(getSpannedScope().getLocalStateSymbols());
  }

  /**
   * @return template
   */
  public String getTemplate() {
    return this.template;
  }

  /**
   * @param template the template to set
   */
  public void setTemplate(String template) {
    this.template = template;
  }

}
