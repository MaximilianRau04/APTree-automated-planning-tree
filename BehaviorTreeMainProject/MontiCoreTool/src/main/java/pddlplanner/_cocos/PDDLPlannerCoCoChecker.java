package pddlplanner._cocos;

import pddlplanner._ast.ASTPlannerDefinition;

/**
 * Checker that coordinates all Context Conditions for PDDLPlanner grammar
 */
public class PDDLPlannerCoCoChecker extends PDDLPlannerCoCoCheckerTOP {
    
    /**
     * Get a checker with all standard CoCos registered
     */
    public static PDDLPlannerCoCoChecker getCheckerForAllCoCos() {
        PDDLPlannerCoCoChecker checker = new PDDLPlannerCoCoChecker();
        
        // Register all CoCos
        checker.addCoCo(new DomainFileNameCoCo());
        
        return checker;
    }
}
