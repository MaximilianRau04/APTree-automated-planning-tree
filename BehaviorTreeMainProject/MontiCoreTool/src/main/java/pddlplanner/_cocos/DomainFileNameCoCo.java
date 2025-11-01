package pddlplanner._cocos;

import pddlplanner._ast.ASTPlannerDefinition;
import pddlplanner._ast.ASTFileAssignment;
import de.se_rwth.commons.logging.Log;

/**
 * Context Condition: Validates that domain file names contain the word "domain"
 * (case-insensitive). Examples: domain.pddl, domainML.pddl, DomainHL.pddl
 */
public class DomainFileNameCoCo implements PDDLPlannerASTPlannerDefinitionCoCo {
    
    @Override
    public void check(ASTPlannerDefinition node) {
        try {
            System.out.println("CoCo: Validating domain file names...");
            
            // Check all file assignments
            if (node.getFilesSection() != null && node.getFilesSection().getFileAssignmentList() != null) {
                for (ASTFileAssignment fileAssignment : node.getFilesSection().getFileAssignmentList()) {
                    
                    // Check if this is a domain assignment
                    if (fileAssignment.isPresentDomainAssignment()) {
                        String domainPath = fileAssignment.getDomainAssignment().getFilePath().getSTRING_VALUE();
                        
                        // Remove quotes
                        domainPath = cleanFilePath(domainPath);
                        
                        // Extract filename from path
                        String fileName = extractFileName(domainPath);
                        
                        System.out.println("  Checking domain file: " + fileName);
                        
                        // Check if filename contains "domain" (case-insensitive)
                        if (!fileName.toLowerCase().contains("domain")) {
                            Log.error("0xPDDL001 Domain file name must contain 'domain'. Found: '" + fileName + "'", 
                                     fileAssignment.get_SourcePositionStart());
                            System.err.println("  ERROR: Domain file '" + fileName + "' does not contain 'domain'");
                        } else {
                            System.out.println("  ✓ Valid domain file name: " + fileName);
                        }
                    }
                }
            }
            
        } catch (Exception e) {
            Log.warn("0xPDDL002 Failed to validate domain file names: " + e.getMessage(), 
                    node.get_SourcePositionStart());
            System.err.println("Warning: Domain file name CoCo validation failed: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    /**
     * Extract filename from a file path
     * Examples: "path/to/domainML.pddl" -> "domainML.pddl"
     *           "C:\\domains\\Domain.pddl" -> "Domain.pddl"
     */
    private String extractFileName(String path) {
        // Handle both forward and backward slashes
        int lastSlash = Math.max(path.lastIndexOf('/'), path.lastIndexOf('\\'));
        
        if (lastSlash >= 0 && lastSlash < path.length() - 1) {
            return path.substring(lastSlash + 1);
        }
        
        return path; // No path separator, it's just a filename
    }
    
    /**
     * Remove surrounding quotes from file path
     */
    private String cleanFilePath(String path) {
        if (path.startsWith("\"") && path.endsWith("\"")) {
            return path.substring(1, path.length() - 1);
        }
        return path;
    }
}
