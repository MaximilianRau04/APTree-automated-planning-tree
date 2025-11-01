import pddlplanner._parser.PDDLPlannerParser;
import pddlplanner._ast.ASTPlannerDefinition;
import pddlplanner._ast.ASTFilesSection;
import pddlplanner._ast.ASTSettingsSection;
import pddlplanner._ast.ASTFileAssignment;
import pddlplanner._ast.ASTSettingAssignment;
import pddlplanner._ast.ASTDomainAssignment;
import pddlplanner._ast.ASTProblemAssignment;
import pddlplanner._ast.ASTPlannerAssignment;
import pddlplanner._ast.ASTConfigurationAssignment;
import pddlplanner._ast.ASTPlannerNameAssignment;
import pddlplanner._cocos.PDDLPlannerCoCoChecker;
import de.se_rwth.commons.logging.Log;
import java.util.Optional;
import java.io.*;

/**
 * Test class for PDDL Planner grammar parsing
 */
public class PDDLPlannerTest {
    
    public static void main(String[] args) {
        try {
            System.out.println("=== PDDL PLANNER PARSER TEST ===");
            System.out.println("Parsing PDDLPlanner.txt...");
            
            // Define the file to parse
            String filePath = "src/test/resources/valid/crf/PDDLPlanner.txt";
            
            // Check if file exists
            File testFile = new File(filePath);
            if (!testFile.exists()) {
                System.err.println("ERROR: Test file not found: " + filePath);
                System.err.println("Current working directory: " + System.getProperty("user.dir"));
                return;
            }
            
            // Create parser instance
            PDDLPlannerParser parser = new PDDLPlannerParser();
            
            // Parse the file
            Optional<ASTPlannerDefinition> result = parser.parse(filePath);
            
            if (result.isPresent()) {
                ASTPlannerDefinition plannerDef = result.get();
                System.out.println("SUCCESS: Parsed PDDL planner: " + plannerDef.getName());
                
                // Analyze the parsed planner definition
                analyzePlannerDefinition(plannerDef);
                
                // Run Context Conditions
                System.out.println("\n=== RUNNING CONTEXT CONDITIONS ===");
                PDDLPlannerCoCoChecker cocoChecker = PDDLPlannerCoCoChecker.getCheckerForAllCoCos();
                cocoChecker.checkAll(plannerDef);
                
                // Check for errors
                if (Log.getErrorCount() > 0) {
                    System.err.println("\n❌ VALIDATION FAILED: " + Log.getErrorCount() + " error(s) found");
                } else {
                    System.out.println("\n✓ VALIDATION PASSED: All context conditions satisfied");
                }
                
            } else {
                System.out.println("ERROR: Failed to parse " + filePath);
                System.out.println("Please check the grammar and test file for syntax errors.");
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    /**
     * Analyze the parsed PDDL planner definition
     */
    public static void analyzePlannerDefinition(ASTPlannerDefinition plannerDef) {
        System.out.println("\n=== PDDL PLANNER ANALYSIS ===");
        System.out.println("Planner Name: " + plannerDef.getName());
        
        // Analyze files section
        ASTFilesSection filesSection = plannerDef.getFilesSection();
        System.out.println("\n📁 Files Section:");
        
        for (ASTFileAssignment fileAssignment : filesSection.getFileAssignmentList()) {
            if (fileAssignment.isPresentDomainAssignment()) {
                ASTDomainAssignment domainAssignment = fileAssignment.getDomainAssignment();
                System.out.println("  Domain: " + domainAssignment.getFilePath().getSTRING_VALUE());
            }
            if (fileAssignment.isPresentProblemAssignment()) {
                ASTProblemAssignment problemAssignment = fileAssignment.getProblemAssignment();
                System.out.println("  Problem: " + problemAssignment.getFilePath().getSTRING_VALUE());
            }
            if (fileAssignment.isPresentPlannerAssignment()) {
                ASTPlannerAssignment plannerAssignment = fileAssignment.getPlannerAssignment();
                System.out.println("  Planner: " + plannerAssignment.getFilePath().getSTRING_VALUE());
            }
        }
        
        // Analyze settings section
        ASTSettingsSection settingsSection = plannerDef.getSettingsSection();
        System.out.println("\n⚙️ Settings Section:");
        
        for (ASTSettingAssignment settingAssignment : settingsSection.getSettingAssignmentList()) {
            if (settingAssignment.isPresentConfigurationAssignment()) {
                ASTConfigurationAssignment configAssignment = settingAssignment.getConfigurationAssignment();
                System.out.println("  Configuration: " + configAssignment.getName());
            }
            if (settingAssignment.isPresentPlannerNameAssignment()) {
                ASTPlannerNameAssignment plannerNameAssignment = settingAssignment.getPlannerNameAssignment();
                System.out.println("  Planner Name: " + plannerNameAssignment.getName());
            }
        }
        
        System.out.println("\n=== PARSING COMPLETE ===");
    }
}
