import de.se_rwth.commons.logging.Log;
import dynamicbtflownode.DynamicBTFlowNodeMill;
import dynamicbtflownode._ast.ASTBehaviorTree;
import dynamicbtflownode._ast.ASTDynamicBTFlowNodeNode;
import dynamicbtflownode._cocos.DynamicBTFlowNodeCoCoChecker;
import CoCos.ConcreteBT.ElementExistsCoCo;
import dynamicbtflownode._symboltable.IDynamicBTFlowNodeArtifactScope;
import dynamicbtflownode._symboltable.IDynamicBTFlowNodeGlobalScope;
import java.nio.file.Path;
import java.nio.file.Paths;

public class APTreeTool {

  public static void main(String[] args) {
    // Standard MontiCore logging setup
    Log.init();
    APTreeTool tool = new APTreeTool();
    String filePath = args.length > 0 ? args[0] : "src/test/resources/valid/behavior_trees/APTree.bt";
    tool.run(filePath);
  }

  public void run(String modelFile) {
    System.out.println("Running APTreeTool on: " + modelFile);

    // 1. Initialize the Mill
    DynamicBTFlowNodeMill.init();
    
    // 2. Configure Global Scope
    DynamicBTFlowNodeMill.globalScope().setSymbolPath(
        new de.monticore.io.paths.MCPath(Paths.get("target", "symbols"))
    );

    try {
        // 3. Parse
        ASTBehaviorTree ast = DynamicBTFlowNodeMill.parser().parseBehaviorTree(modelFile)
             .orElseThrow(() -> new RuntimeException("Parsing failed for file: " + modelFile));
             
        System.out.println("✓ SUCCESS: Syntactically parsed '" + ast.getName() + "'");
    
        // 4. Create Symbol Table
        IDynamicBTFlowNodeGlobalScope gs = DynamicBTFlowNodeMill.globalScope();
        IDynamicBTFlowNodeArtifactScope as = DynamicBTFlowNodeMill.scopesGenitorDelegator().createFromAST(ast);
        as.setEnclosingScope(gs);
        
        // 5. Run CoCo Checks
        DynamicBTFlowNodeCoCoChecker checker = new DynamicBTFlowNodeCoCoChecker();
        // Add custom checks (must register for each node type explicitly to avoid ambiguity)
        ElementExistsCoCo elementCheck = new ElementExistsCoCo();
        checker.addCoCo((concretebt._cocos.ConcreteBTASTPickUpHLCoCo) elementCheck);
       // checker.addCoCo((concretebt._cocos.ConcreteBTASTPlaceHLCoCo) elementCheck);
        
        // Add default CoCos here if any exist in the language definition
        checker.checkAll((ASTDynamicBTFlowNodeNode) ast);
    
        System.out.println("✓ SUCCESS: Model parsed and symbols checked successfully!");

    } catch (Exception e) {
        System.err.println("tool run failed: " + e.getMessage());
        e.printStackTrace();
    }
  }
}
