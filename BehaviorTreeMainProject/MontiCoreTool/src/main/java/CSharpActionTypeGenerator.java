import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTAction;
import crf._ast.ASTArgument;
import java.util.Optional;
import java.io.*;
import java.nio.file.*;
import java.util.Map;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.ArrayList;

public class CSharpActionTypeGenerator {
    
    private static final String OUTPUT_DIR = "C:/Users/sherk/Documents/BehaviorTreeMainProject/BehaviorTreeMainProject/src/ModelLoader/ActionTypes";
    
    public static void main(String[] args) {
        try {
            System.out.println("=== CSharpActionTypeGenerator Starting ===");
            System.out.println("Current working directory: " + System.getProperty("user.dir"));
            System.out.println("Generating C# Action Type Classes...");
            
            // Define the files to process
            String[] filesToProcess = {
                "src/test/resources/valid/crf/PDDLActions.txt",
                "src/test/resources/valid/crf/GOAPActions.txt"
            };
            
            CRFParser parser = new CRFParser();
            System.out.println("CRFParser created successfully");
            
            java.util.Set<String> processedActions = new java.util.HashSet<>();
            
            // Clean the output directory first
            cleanOutputDirectory();
            
            // Ensure output directory exists
            Files.createDirectories(Paths.get(OUTPUT_DIR));
            
            // Process each file
            for (String filePath : filesToProcess) {
                System.out.println("Processing file: " + filePath);
                File testFile = new File(filePath);
                System.out.println("File exists: " + testFile.exists());
                
                if (testFile.exists()) {
                    Optional<ASTAllowedType> result = parser.parse(filePath);
                    
                    if (result.isPresent()) {
                        ASTAllowedType ast = result.get();
                        System.out.println("AST obtained successfully from " + filePath);
                        generateCSharpClassesFromAST(ast, processedActions);
                        System.out.println("SUCCESS: Processed " + filePath);
                    } else {
                        System.out.println("WARNING: Failed to parse " + filePath);
                    }
                } else {
                    System.out.println("WARNING: File not found: " + filePath);
                }
            }
            
            System.out.println("SUCCESS: C# action type classes generated successfully!");
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    public static void generateCSharpClassesFromAST(ASTAllowedType ast, java.util.Set<String> processedActions) throws IOException {
        System.out.println("Debug: Checking AST for Action nodes...");
        System.out.println("Debug: Action list is null? " + (ast.getActionList() == null));
        
        if (ast.getActionList() != null) {
            System.out.println("Debug: Found " + ast.getActionList().size() + " Action nodes");
            for (ASTAction action : ast.getActionList()) {
                String actionName = action.getName();
                if (!processedActions.contains(actionName)) {
                    System.out.println("Debug: Processing Action: " + actionName);
                    generateActionTypeClass(action, ast);
                    processedActions.add(actionName);
                    System.out.println("Generated action class: " + actionName);
                } else {
                    System.out.println("Skipped duplicate action: " + actionName);
                }
            }
        } else {
            System.out.println("Debug: No Action nodes found in AST");
        }
    }
    
    private static void cleanOutputDirectory() throws IOException {
        Path outputPath = Paths.get(OUTPUT_DIR);
        
        if (Files.exists(outputPath)) {
            System.out.println("Cleaning output directory: " + OUTPUT_DIR);
            
            // Delete all .cs files in the directory
            try (DirectoryStream<Path> stream = Files.newDirectoryStream(outputPath, "*.cs")) {
                for (Path file : stream) {
                    Files.delete(file);
                    System.out.println("Deleted: " + file.getFileName());
                }
            }
        } else {
            System.out.println("Output directory does not exist, will be created: " + OUTPUT_DIR);
        }
    }
    
    public static void generateActionTypeClass(ASTAction action, ASTAllowedType ast) throws IOException {
        String className = capitalizeFirst(action.getName());
        String fileName = className + ".cs";
        String filePath = OUTPUT_DIR + "/" + fileName;
        
        try (PrintWriter writer = new PrintWriter(new FileWriter(filePath, false))) { // false = overwrite mode
            // Generate the C# class
            writer.println("using System;");
            writer.println("using System.Collections.Generic;");
            writer.println("using ModelLoader.ParameterTypes;");
            writer.println("using ModelLoader.PredicateTypes;");
            writer.println();
            writer.println("namespace BehaviorTreeMainProject");
            writer.println("{");
            writer.println("    public class " + className + " : GenericBTAction");
            writer.println("    {");
            
            // Generate parameter properties
            generateParameterProperties(writer, action);
            
            // Generate State fields for preconditions and effects
            writer.println("        // Preconditions and Effects as State objects");
            writer.println("        private State preconditions;");
            writer.println("        private State effects;");
            writer.println();
            
            // Generate constructor with all properties as parameters
            writer.print("        public " + className + "(string actionType, string instanceName, Blackboard<FastName> blackboard");
            if (action.getParametersBlock() != null && action.getParametersBlock().isPresentPropertyList()) {
                for (crf._ast.ASTProperty param : action.getParametersBlock().getPropertyList().getPropertyList()) {
                    String paramName = param.getName();
                    String csharpType = getBasicTypeName(param.getBasicType());
                    writer.print(", " + csharpType + " " + paramName);
                }
            }
            writer.println(")");
            writer.println("            : base(actionType, instanceName, blackboard)");
            writer.println("        {");
            // Set all properties directly
            if (action.getParametersBlock() != null && action.getParametersBlock().isPresentPropertyList()) {
                for (crf._ast.ASTProperty param : action.getParametersBlock().getPropertyList().getPropertyList()) {
                    String paramName = param.getName();
                    writer.println("            this." + paramName + " = " + paramName + ";");
                }
            }
            writer.println("            InitializePredicates();");
            writer.println("        }");
            writer.println();
            
            // Generate InitializePredicates method
            writer.println("        private void InitializePredicates()");
            writer.println("        {");
            writer.println("            // Initialize preconditions");
            writer.println("            preconditions = new State(StateType.Precondition, new FastName(\"" + action.getName() + "_preconditions\"));");
            generatePredicateInstantiationCode(writer, action.getName(), true, ast);
            writer.println();
            writer.println("            // Initialize effects");
            writer.println("            effects = new State(StateType.Effect, new FastName(\"" + action.getName() + "_effects\"));");
            generatePredicateInstantiationCode(writer, action.getName(), false, ast);
            writer.println("        }");
            writer.println();
            
            // Override the abstract properties to return the instantiated States
            writer.println("        protected override State Preconditions => preconditions;");
            writer.println("        protected override State Effects => effects;");
            writer.println();
            
            // ExecuteActionLogic method generation removed - will use base class implementation
            writer.println("    }");
            writer.println("}");
            
            System.out.println("Generated: " + fileName);
        }
    }
    
    private static String capitalizeFirst(String str) {
        if (str == null || str.isEmpty()) {
            return str;
        }
        return str.substring(0, 1).toUpperCase() + str.substring(1);
    }
    
    private static void generateParameterProperties(PrintWriter writer, ASTAction action) throws IOException {
        if (action.getParametersBlock() != null && action.getParametersBlock().isPresentPropertyList()) {
            for (crf._ast.ASTProperty param : action.getParametersBlock().getPropertyList().getPropertyList()) {
                String paramName = param.getName();
                String csharpType = getBasicTypeName(param.getBasicType());
                
                writer.println("        // Parameter: " + paramName + " of type " + csharpType);
                writer.println("        public " + csharpType + " " + paramName + " { get; private set; }");
                writer.println();
            }
        }
    }
    
    private static String getBasicTypeName(Object basicType) {
        // Check if the basicType is an ASTBasicType and use the named alternative methods
        if (basicType instanceof crf._ast.ASTBasicType) {
            crf._ast.ASTBasicType astBasicType = (crf._ast.ASTBasicType) basicType;
            
            // Use the named alternative methods to determine the type
            if (astBasicType.isPresentElement()) {
                return "Element";
            } else if (astBasicType.isPresentAgent()) {
                return "Agent";
            } else if (astBasicType.isPresentLocation()) {
                return "Location";
            } else if (astBasicType.isPresentLayer()) {
                return "Layer";
            } else if (astBasicType.isPresentModule()) {
                return "Module";
            } else if (astBasicType.isPresentTool()) {
                return "Tool";
            } else if (astBasicType.isPresentString()) {
                return "string";
            } else if (astBasicType.isPresentDouble()) {
                return "double";
            } else if (astBasicType.isPresentInteger()) {
                return "int";
            } else if (astBasicType.isPresentBoolean()) {
                return "bool";
            }
        }
        
        // Fallback to string
        return "string";
    }
    
    private static void generatePredicateInstantiationCode(PrintWriter writer, String actionName, boolean isPrecondition, ASTAllowedType ast) throws IOException {
        String[] predicateStrings = getPredicateStringsFromAST(actionName, isPrecondition, ast);
        String stateVarName = isPrecondition ? "preconditions" : "effects";
        
        System.out.println("DEBUG: Found " + predicateStrings.length + " predicates for " + actionName + " (" + (isPrecondition ? "precondition" : "effect") + ")");
        
        for (int i = 0; i < predicateStrings.length; i++) {
            String predicateString = predicateStrings[i];
            System.out.println("DEBUG: Processing predicate: " + predicateString);
            String predicateCode = generatePredicateInstanceCode(predicateString, actionName);
            if (predicateCode != null) {
                System.out.println("DEBUG: Generated code: " + predicateCode);
                writer.println("            " + stateVarName + ".AddPredicate(new FastName(\"" + actionName + "_" + (isPrecondition ? "pre" : "eff") + "_" + i + "\"), " + predicateCode + ");");
            } else {
                System.out.println("DEBUG: Failed to generate code for predicate: " + predicateString);
            }
        }
    }
    
    private static String generatePredicateInstanceCode(String predicateString, String actionName) {
        // Parse the predicate string to extract name and parameters
        String predicateName = extractPredicateName(predicateString);
        Map<String, String> parameters = parsePredicateParameters(predicateString);
        
        // Generate direct predicate instantiation
        return generateDirectPredicateInstance(predicateName, parameters);
    }
    
    private static String generateDirectPredicateInstance(String predicateName, Map<String, String> parameters) {
        // Remove isNegated from parameters as it's handled separately
        boolean isNegated = Boolean.parseBoolean(parameters.getOrDefault("isNegated", "false"));
        parameters.remove("isNegated");
        
        // Build constructor parameters dynamically
        StringBuilder constructorParams = new StringBuilder();
        boolean first = true;
        
        for (Map.Entry<String, String> entry : parameters.entrySet()) {
            if (!first) {
                constructorParams.append(", ");
            }
            // Use the parameter value (which should be the action parameter name)
            constructorParams.append(entry.getValue());
            first = false;
        }
        
        // Add isNegated parameter
        if (!first) {
            constructorParams.append(", ");
        }
        constructorParams.append(isNegated);
        
        // Generate the predicate instantiation using the capitalized predicate name
        String capitalizedPredicateName = capitalizeFirst(predicateName);
        return "new " + capitalizedPredicateName + "(" + constructorParams.toString() + ")";
    }
    
    private static String extractPredicateName(String predicateString) {
        // Extract predicate name from "PredicateInstance: predicateName(...)"
        int colonIndex = predicateString.indexOf(':');
        int parenIndex = predicateString.indexOf('(');
        if (colonIndex >= 0 && parenIndex >= 0 && parenIndex > colonIndex) {
            return predicateString.substring(colonIndex + 1, parenIndex).trim();
        }
        return "unknown";
    }
    
    private static Map<String, String> parsePredicateParameters(String predicateString) {
        Map<String, String> parameters = new LinkedHashMap<>();
        
        // Extract the part between parentheses
        int startIndex = predicateString.indexOf('(');
        int endIndex = predicateString.lastIndexOf(')');
        
        if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex) {
            String argsString = predicateString.substring(startIndex + 1, endIndex);
            String[] args = argsString.split(",");
            
            for (String arg : args) {
                String trimmedArg = arg.trim();
                if (trimmedArg.contains("=")) {
                    // Use regex to split on = with optional whitespace
                    String[] parts = trimmedArg.split("\\s*=\\s*");
                    if (parts.length == 2) {
                        String paramName = parts[0].trim();
                        String paramValue = parts[1].trim();
                        parameters.put(paramName, paramValue);
                        System.out.println("DEBUG: Parsed parameter - " + paramName + " = " + paramValue);
                    }
                }
            }
        }
        
        return parameters;
    }
    
    private static String[] getPredicateStringsFromAST(String actionName, boolean isPrecondition, ASTAllowedType ast) {
        // For now, skip AST extraction since it has incomplete data and go directly to file parsing
        // This is more reliable until we can fix the AST parsing issues
        System.out.println("DEBUG: Skipping AST extraction due to incomplete data, using file parsing for " + actionName);
        return getPredicateStringsFromModelFile(actionName, isPrecondition);
    }
    
    private static String[] extractPredicatesFromASTAction(ASTAction action, boolean isPrecondition) {
        List<String> predicates = new ArrayList<>();
        
        try {
            System.out.println("DEBUG: Extracting predicates for action: " + action.getName() + " (isPrecondition: " + isPrecondition + ")");
            
            if (isPrecondition) {
                System.out.println("DEBUG: Precondition state is null? " + (action.getPreconditionState() == null));
                if (action.getPreconditionState() != null) {
                    System.out.println("DEBUG: Precondition state predicate list size: " + action.getPreconditionState().getPredicateInstanceDefList().size());
                    // Extract predicates from precondition state
                    for (crf._ast.ASTPredicateInstanceDef predicateDef : action.getPreconditionState().getPredicateInstanceDefList()) {
                        System.out.println("DEBUG: Processing precondition predicate: " + predicateDef.getName());
                        String predicateString = convertPredicateDefToString(predicateDef);
                        if (predicateString != null) {
                            predicates.add(predicateString);
                        }
                    }
                }
            } else {
                System.out.println("DEBUG: Effect state is null? " + (action.getEffectState() == null));
                if (action.getEffectState() != null) {
                    System.out.println("DEBUG: Effect state predicate list size: " + action.getEffectState().getPredicateInstanceDefList().size());
                    // Extract predicates from effect state
                    for (crf._ast.ASTPredicateInstanceDef predicateDef : action.getEffectState().getPredicateInstanceDefList()) {
                        System.out.println("DEBUG: Processing effect predicate: " + predicateDef.getName());
                        String predicateString = convertPredicateDefToString(predicateDef);
                        if (predicateString != null) {
                            predicates.add(predicateString);
                        }
                    }
                }
            }
        } catch (Exception e) {
            System.err.println("Error extracting predicates from AST: " + e.getMessage());
            e.printStackTrace();
        }
        
        System.out.println("DEBUG: Extracted " + predicates.size() + " predicates from AST");
        return predicates.toArray(new String[0]);
    }
    
    private static String convertPredicateDefToString(crf._ast.ASTPredicateInstanceDef predicateDef) {
        try {
            StringBuilder sb = new StringBuilder();
            String predicateName = predicateDef.getName();
            System.out.println("DEBUG: Converting predicate: " + predicateName);
            
            sb.append("PredicateInstance: ").append(predicateName).append("(");
            
            // Add predicate arguments
            boolean first = true;
            if (predicateDef.isPresentArgumentList()) {
                for (crf._ast.ASTArgument arg : predicateDef.getArgumentList().getArgumentList()) {
                    if (!first) {
                        sb.append(", ");
                    }
                    
                    // Get argument name and value
                    String argName = arg.getName();
                    String argValue = getValueAsString(arg.getValue());
                    
                    System.out.println("DEBUG: Argument - " + argName + " = " + argValue);
                    
                    sb.append(argName).append(" = ").append(argValue);
                    first = false;
                }
            }
            
            // Add isNegated value - it's directly accessible in the PredicateInstanceDef
            if (!first) {
                sb.append(", ");
            }
            
            // Get the boolean value token
            String negatedValue = predicateDef.getBOOLEAN_VALUE();
            System.out.println("DEBUG: isNegated value: " + negatedValue);
            sb.append("isNegated = ").append(negatedValue);
            
            sb.append(")");
            String result = sb.toString();
            System.out.println("DEBUG: Generated predicate string: " + result);
            return result;
        } catch (Exception e) {
            System.err.println("Error converting predicate def to string: " + e.getMessage());
            e.printStackTrace();
            return null;
        }
    }
    
    private static String getValueAsString(crf._ast.ASTValue value) {
        // ASTValue can be Name, INTEGER_VALUE, DOUBLE_VALUE, STRING_VALUE, or BOOLEAN_VALUE
        // We'll get the name which should work for all types
        return value.getName();
    }
    
    private static String[] getPredicateStringsFromModelFile(String actionName, boolean isPrecondition) {
        // Define the files to search in
        String[] filesToSearch = {
            "src/test/resources/valid/crf/PDDLActions.txt",
            "src/test/resources/valid/crf/GOAPActions.txt"
        };
        
        for (String modelFilePath : filesToSearch) {
            try {
                File modelFile = new File(modelFilePath);
                if (!modelFile.exists()) {
                    System.err.println("Model file not found: " + modelFilePath);
                    continue;
                }
            
            List<String> predicates = new ArrayList<>();
            boolean inAction = false;
            boolean inPrecondition = false;
            boolean inEffect = false;
            int actionBraceLevel = 0;
            
            try (BufferedReader reader = new BufferedReader(new FileReader(modelFile))) {
                String line;
                while ((line = reader.readLine()) != null) {
                    line = line.trim();
                    
                                         // Check if we're entering the target action
                     if (line.startsWith("Action " + actionName + " {")) {
                         System.out.println("DEBUG: Found action " + actionName + " in " + modelFilePath);
                         inAction = true;
                         actionBraceLevel = 1;
                         continue;
                     }
                     
                     // Debug output for action state
                     if (inAction) {
                         System.out.println("DEBUG: In action " + actionName + " - Line: '" + line + "' - BraceLevel: " + actionBraceLevel + " - InPrecondition: " + inPrecondition + " - InEffect: " + inEffect);
                     }
                    
                    // If we're in the target action, look for precondition and effect blocks
                    if (inAction) {
                        // Check for precondition and effect blocks first
                        if (line.equals("precondition {")) {
                            System.out.println("DEBUG: Entering precondition block for " + actionName);
                            inPrecondition = true;
                            inEffect = false;
                            continue;
                        } else if (line.equals("effect {")) {
                            System.out.println("DEBUG: Entering effect block for " + actionName);
                            inPrecondition = false;
                            inEffect = true;
                            continue;
                        } else if (line.equals("}") && (inPrecondition || inEffect)) {
                            // End of precondition or effect block
                            System.out.println("DEBUG: Exiting " + (inPrecondition ? "precondition" : "effect") + " block for " + actionName);
                            inPrecondition = false;
                            inEffect = false;
                            continue;
                        }
                        
                        // Count braces within the action (after checking for blocks)
                        if (line.contains("{")) actionBraceLevel++;
                        if (line.contains("}")) actionBraceLevel--;
                        
                        // Check for end of action block
                        if (actionBraceLevel <= 0 && inAction) {
                            // End of action block
                            System.out.println("DEBUG: Exiting action block for " + actionName + " (brace level: " + actionBraceLevel + ")");
                            inAction = false;
                            break;
                        }
                        
                        // Collect predicate instances
                        if ((isPrecondition && inPrecondition) || (!isPrecondition && inEffect)) {
                            if (line.startsWith("PredicateInstance:")) {
                                System.out.println("DEBUG: Found predicate in " + modelFilePath + ": " + line);
                                predicates.add(line);
                            }
                        }
                    }
                }
            }
            
                System.out.println("Found " + predicates.size() + " predicates for " + actionName + " (" + (isPrecondition ? "precondition" : "effect") + ") in " + modelFilePath);
                if (!predicates.isEmpty()) {
                    return predicates.toArray(new String[0]);
                }
            } catch (IOException e) {
                System.err.println("Error reading model file " + modelFilePath + ": " + e.getMessage());
            }
        }
        
        System.out.println("No predicates found for " + actionName + " in any file");
        return new String[0];
    }
    
    private static String[] getPredicateStringsForAction(String actionName, boolean isPrecondition) {
        // Hardcoded mapping based on the test file predicates
        switch (actionName.toLowerCase()) {
            case "pickup":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: isAt(myObject = pickedObject, location = loc, isNegated = false)",
                        "PredicateInstance: atAgent(agent = rob, location = loc, isNegated = false)",
                        "PredicateInstance: hasTool(agent = rob, tool = robTool, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: holding(agent = rob, myObject = pickedObject, isNegated = false)",
                        "PredicateInstance: atAgent(agent = rob, location = loc, isNegated = false)"
                    };
                }
            case "equipe":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: empty(client = client, isNegated = false)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: hasTool(agent = client, tool = too, isNegated = false)",
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = false)"
                    };
                }
            case "deequip":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: hasTool(agent = client, tool = too, isNegated = false)",
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: empty(client = client, isNegated = false)",
                        "PredicateInstance: hasTool(agent = client, tool = too, isNegated = true)",
                        "PredicateInstance: positionfree(pos = ep, isNegated = true)"
                    };
                }
            case "grab":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = obj, place = grabPos, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = true)",
                        "PredicateInstance: positionfree(pos = grabPos, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)",
                        "PredicateInstance: stacked(myObject = obj, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj, place = grabPos, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj, isNegated = true)",
                        "PredicateInstance: positionfree(pos = grabPos, isNegated = false)"
                    };
                }
            case "place":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj, isNegated = true)",
                        "PredicateInstance: positionfree(pos = placePos, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = obj, place = placePos, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = obj, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)",
                        "PredicateInstance: positionfree(pos = placePos, isNegated = true)"
                    };
                }
            case "stackhl":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: holding(agent = client, myObject = obj1, isNegated = false)",
                        "PredicateInstance: hasTool(agent = client, tool = vg, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj2, place = pr, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj1, place = pr, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: ontop(myObject1 = obj1, myObject2 = obj2, isNegated = false)",
                        "PredicateInstance: stacked(myObject = obj1, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = obj1, isNegated = true)",
                        "PredicateInstance: atplace(myObject = obj1, place = pr, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj2, isNegated = true)",
                        "PredicateInstance: clear(myObject = obj1, isNegated = false)",
                        "PredicateInstance: allset(lay = lay, mod = mod, isNegated = false)"
                    };
                }
            case "stackonmultiple":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: allset(lay = lay, mod = mod, isNegated = false)",
                        "PredicateInstance: hasTool(agent = client, tool = vg, isNegated = false)",
                        "PredicateInstance: holding(agent = client, myObject = plate, isNegated = false)",
                        "PredicateInstance: atplace(myObject = plate, place = pos, isNegated = true)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: atplace(myObject = plate, place = pos, isNegated = false)"
                    };
                }
            case "gluing":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: hasTool(agent = client, tool = gg, isNegated = false)",
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: atplace(myObject = obj, place = pos, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: glued(myObject = obj, isNegated = false)"
                    };
                }
            case "nailing":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: empty(client = client, isNegated = true)",
                        "PredicateInstance: hasTool(agent = client, tool = ng, isNegated = false)",
                        "PredicateInstance: atplace(myObject = obj, place = pos, isNegated = false)",
                        "PredicateInstance: clear(myObject = obj, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: nailed(myObject = obj, isNegated = false)"
                    };
                }
            case "dummyaction":
                if (isPrecondition) {
                    return new String[] {
                        "PredicateInstance: isAt(myObject = testParam, location = testLocation, isNegated = false)",
                        "PredicateInstance: atAgent(agent = testRobot, location = testLocation, isNegated = false)"
                    };
                } else {
                    return new String[] {
                        "PredicateInstance: holding(agent = testRobot, myObject = testParam, isNegated = false)",
                        "PredicateInstance: clear(myObject = testParam, isNegated = true)"
                    };
                }
            default:
                return new String[] {
                    "PredicateInstance: unknown(unknown = unknown, isNegated = false)"
                };
        }
    }
}
