/* (c) https://github.com/MontiCore/monticore */
package automata;

import automata._ast.ASTAutomaton;
import automata._cocos.AutomataCoCoChecker;
import automata._prettyprint.AutomataFullPrettyPrinter;
import automata._symboltable.AutomataScopesGenitor;
import automata._symboltable.IAutomataArtifactScope;
import automata._symboltable.IAutomataGlobalScope;
import automata._visitor.AutomataTraverser;
import automata.cocos.AtLeastOneInitialAndFinalState;
import automata.cocos.AutomataCoCos;
import automata.cocos.StateNameStartsWithCapitalLetter;
import automata.cocos.TransitionStatesExist;
import automata.visitors.CountStates;
import automata2cd.Automata2CDConverter;
import de.monticore.cd.codegen.CDGenerator;
import de.monticore.cd.codegen.CdUtilsPrinter;
import de.monticore.generating.GeneratorSetup;
import de.monticore.generating.templateengine.GlobalExtensionManagement;
import de.monticore.generating.templateengine.TemplateController;
import de.monticore.generating.templateengine.TemplateHookPoint;
import de.monticore.prettyprint.IndentPrinter;
import de.se_rwth.commons.logging.Log;
import org.apache.commons.cli.*;
import org.apache.commons.lang3.StringUtils;

import java.io.File;
import java.util.Arrays;
import java.util.List;

import static de.se_rwth.commons.Names.getPathFromPackage;

public class AutomataTool extends AutomataToolTOP {
  
  @Override
  public void run(String[] args) {
    Options options = initOptions();

    try {
      // create CLI parser and parse input options from command line
      CommandLineParser cliParser = new DefaultParser();
      CommandLine cmd =  cliParser.parse(options, args);
  
      // help: when --help
      if (cmd.hasOption("h")) {
        printHelp(options);
        // do not continue, when help is printed
        return;
      }
      
      // if -i input is missing: also print help and stop
      if (!cmd.hasOption("i")) {
        printHelp(options);
        // do not continue, when help is printed
        return;
      }
      
      // -option developer logging
      if (cmd.hasOption("d")) {
        Log.initDEBUG();
      } else {
        Log.init();
      }
  
      // parse input file, which is now available
      // (only returns if successful)
      ASTAutomaton astAutomaton = parse(cmd.getOptionValue("i"));
      Log.info(cmd.getOptionValue("i") + " parsed successfully!", AutomataTool.class.getName());

      IAutomataArtifactScope modelTopScope = createSymbolTable(astAutomaton);

      // execute default context conditions
      runDefaultCoCos(astAutomaton);

      // execute a custom set of context conditions
      AutomataCoCoChecker customCoCos = new AutomataCoCoChecker();
      customCoCos.addCoCo(new StateNameStartsWithCapitalLetter());
      customCoCos.checkAll(astAutomaton);

      // store artifact scope
      String symFile = "target/symbols/" + getPathFromPackage(modelTopScope.getFullName()) + ".autsym";
      storeSymbols(modelTopScope, symFile);

      // analyze the model with a visitor
      CountStates cs = new CountStates();
      AutomataTraverser t = AutomataMill.traverser();
      t.add4Automata(cs);
      astAutomaton.accept(t);
      Log.info("The model contains " + cs.getCount() + " states.", AutomataTool.class.getName());


      // -option pretty print
      if (cmd.hasOption("pp")) {
        String path = cmd.getOptionValue("pp", StringUtils.EMPTY);
        prettyPrint(astAutomaton, path);
      }
  
      // -option reports
      if (cmd.hasOption("r")) {
        String path = cmd.getOptionValue("r", StringUtils.EMPTY);
        report(astAutomaton, path);
      }
  
      String outputDir = cmd.hasOption("o")
        ? cmd.getOptionValue("o")
        : "target/gen-test/"+astAutomaton.getName();
      generateCD(astAutomaton,outputDir);
      
    } catch (ParseException e) {
      Log.error("0xA7105 Could not process parameters: "+e.getMessage());
    }
  }
  
  
  /**
   * Create the symbol table from the parsed AST.
   *
   * @param node Input AST.
   * @return The symbol table created from the AST.
   */
  @Override
  public IAutomataArtifactScope createSymbolTable(ASTAutomaton node) {
    IAutomataGlobalScope gs = AutomataMill.globalScope();
    gs.clear();
    
    AutomataScopesGenitor genitor = AutomataMill.scopesGenitor();
    AutomataTraverser traverser = AutomataMill.traverser();
    traverser.setAutomataHandler(genitor);
    traverser.add4Automata(genitor);
    genitor.putOnStack(gs);
    
    IAutomataArtifactScope scope = genitor.createFromAST(node);
    gs.addSubScope(scope);
    return scope;
  }
  
  @Override
  public void prettyPrint(ASTAutomaton ast, String file) {
    AutomataFullPrettyPrinter printer = new AutomataFullPrettyPrinter(new IndentPrinter(),true);
    String result = printer.prettyprint(ast);
    Log.info("Pretty printing the parsed automata into console:", AutomataTool.class.getName());
    System.out.println(result);
  }
  
  
  /**
   * Run the default context conditions {@link AtLeastOneInitialAndFinalState},
   * {@link TransitionStatesExist}, and
   * {@link StateNameStartsWithCapitalLetter}.
   *
   * @param ast The input AST.
   */
  @Override
  public void runDefaultCoCos(ASTAutomaton ast) {
    new AutomataCoCos().getCheckerForAllCoCos().checkAll(ast);
  }
  
  public void generateCD(ASTAutomaton ast, String outputDir) {
    
    GeneratorSetup setup = new GeneratorSetup();
    GlobalExtensionManagement glex = new GlobalExtensionManagement();
    setup.setGlex(glex);
    glex.setGlobalValue("cdPrinter", new CdUtilsPrinter());
    
    if (!outputDir.isEmpty()){
      File targetDir = new File(outputDir);
      setup.setOutputDirectory(targetDir);
    }
    
    String configTemplate = "automaton2cd.Automaton2CD";
    TemplateController tc = setup.getNewTemplateController(configTemplate);
    CDGenerator generator = new CDGenerator(setup);
    TemplateHookPoint hpp = new TemplateHookPoint(configTemplate);
    List<Object> configTemplateArgs;
    // select the conversion variant:
    Automata2CDConverter converter = new Automata2CDConverter();
    configTemplateArgs = Arrays.asList(glex, converter, setup.getHandcodedPath(), generator);
    
    hpp.processValue(tc, ast, configTemplateArgs);
  }

  @Override
  public Options addAdditionalOptions(Options options) {
    options.addOption(new Option("o","output",true,"Sets the output path"));
    return options;
  }
}
