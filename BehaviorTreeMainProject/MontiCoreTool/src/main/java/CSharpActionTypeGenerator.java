import crf._parser.CRFParser;
import crf._ast.ASTAllowedType;
import crf._ast.ASTAction;
import crf._ast.ASTParameterInstance;
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
            
            // Test if the file exists
            String testFilePath = "src/test/resources/valid/crf/test_crf.txt";
            File testFile = new File(testFilePath);
            System.out.println("Testing file path: " + testFile.getAbsolutePath());
            System.out.println("File exists: " + testFile.exists());
            
            CRFParser parser = new CRFParser();
            System.out.println("CRFParser created successfully");
            
            System.out.println("Attempting to parse: " + testFilePath);
            Optional<ASTAllowedType> result = parser.parse(testFilePath);
            
            System.out.println("Parse result is present: " + result.isPresent());
            
            if (result.isPresent()) {
                ASTAllowedType ast = result.get();
                System.out.println("AST obtained successfully");
                generateCSharpClasses(ast);
                System.out.println("C# action type classes generated successfully!");
            } else {
                System.out.println("Failed to parse CRF model");
                System.out.println("Trying alternative paths...");
                
                // Try alternative paths
                String[] alternativePaths = {
                    "MontiCoreTool/src/test/resources/valid/crf/test_crf.txt",
                    "../MontiCoreTool/src/test/resources/valid/crf/test_crf.txt",
                    "test_crf.txt"
                };
                
                for (String altPath : alternativePaths) {
                    System.out.println("Trying: " + altPath);
                    File altFile = new File(altPath);
                    System.out.println("  File exists: " + altFile.exists());
                    if (altFile.exists()) {
                        try {
                            result = parser.parse(altPath);
                            if (result.isPresent()) {
                                System.out.println("Successfully parsed: " + altPath);
                                ASTAllowedType ast = result.get();
                                generateCSharpClasses(ast);
                                System.out.println("C# action type classes generated successfully!");
                                return;
                            }
                        } catch (Exception e) {
                            System.out.println("  Failed to parse " + altPath + ": " + e.getMessage());
                        }
                    }
                }
            }
            
        } catch (Exception e) {
            System.err.println("ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }
    
    public static void generateCSharpClasses(ASTAllowedType ast) throws IOException {
        // Clean the output directory first
        cleanOutputDirectory();
        
        // Ensure output directory exists
        Files.createDirectories(Paths.get(OUTPUT_DIR));
        
        System.out.println("Debug: Checking AST for Action nodes...");
        System.out.println("Debug: Action list is null? " + (ast.getActionList() == null));
        
        if (ast.getActionList() != null) {
            System.out.println("Debug: Found " + ast.getActionList().size() + " Action nodes");
            for (ASTAction action : ast.getActionList()) {
                System.out.println("Debug: Processing Action: " + action.getName());
                generateActionTypeClass(action, ast);
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
            if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
                for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                    String paramName = param.getName(0);
                    String paramTypeName = param.getName(1);
                    String csharpType = getBasicTypeFromName(paramTypeName);
                    writer.print(", " + csharpType + " " + paramName);
                }
            }
            writer.println(")");
            writer.println("            : base(actionType, instanceName, blackboard)");
            writer.println("        {");
            // Set all properties directly
            if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
                for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                    String paramName = param.getName(0);
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
            
            // Generate OnTick_NodeLogic method
            writer.println("        protected override bool OnTick_NodeLogic(float InDeltaTime)");
            writer.println("        {");
            writer.println("            // TODO: Implement action logic for " + className);
            writer.println("            // Access parameters via properties: obj, rob, loc, tool, etc.");
            writer.println("            return SetStatusAndCalculateReturnvalue(EBTNodeResult.Succeeded);");
            writer.println("        }");
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
        if (action.getActionParametersBlock() != null && action.getActionParametersBlock().getParameterInstanceList() != null) {
            for (ASTParameterInstance param : action.getActionParametersBlock().getParameterInstanceList()) {
                String paramName = param.getName(0);
                String paramTypeName = param.getName(1);
                String csharpType = getBasicTypeFromName(paramTypeName);
                
                writer.println("        // Parameter: " + paramName + " of type " + paramTypeName);
                writer.println("        public " + csharpType + " " + paramName + " { get; private set; }");
                writer.println();
            }
        }
    }
    
    private static String getBasicTypeFromName(String typeName) {
        // Return the original type name as-is, but capitalize it for C# convention
        if (typeName == null || typeName.isEmpty()) {
            return "string";
        }
        
        // Capitalize the first letter for C# class naming convention
        return typeName.substring(0, 1).toUpperCase() + typeName.substring(1);
    }
    
    private static void generatePredicateInstantiationCode(PrintWriter writer, String actionName, boolean isPrecondition, ASTAllowedType ast) throws IOException {
        String[] predicateStrings = getPredicateStringsFromAST(actionName, isPrecondition, ast);
        String stateVarName = isPrecondition ? "preconditions" : "effects";
        
        for (int i = 0; i < predicateStrings.length; i++) {
            String predicateString = predicateStrings[i];
            String predicateCode = generatePredicateInstanceCode(predicateString, actionName);
            if (predicateCode != null) {
                writer.println("            " + stateVarName + ".AddPredicate(new FastName(\"" + actionName + "_" + (isPrecondition ? "pre" : "eff") + "_" + i + "\"), " + predicateCode + ");");
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
        // Read predicate instances directly from the model file
        return getPredicateStringsFromModelFile(actionName, isPrecondition);
    }
    
    private static String[] getPredicateStringsFromModelFile(String actionName, boolean isPrecondition) {
        try {
            String modelFilePath = "src/test/resources/valid/crf/test_crf.txt";
            File modelFile = new File(modelFilePath);
            if (!modelFile.exists()) {
                System.err.println("Model file not found: " + modelFilePath);
                return new String[0];
            }
            
            List<String> predicates = new ArrayList<>();
            boolean inAction = false;
            boolean inPrecondition = false;
            boolean inEffect = false;
            
            try (BufferedReader reader = new BufferedReader(new FileReader(modelFile))) {
                String line;
                while ((line = reader.readLine()) != null) {
                    line = line.trim();
                    
                    // Check if we're entering the target action
                    if (line.startsWith("Action " + actionName + " {")) {
                        inAction = true;
                        continue;
                    }
                    
                    // If we're in the target action, look for precondition and effect blocks
                    if (inAction) {
                        if (line.equals("precondition {")) {
                            inPrecondition = true;
                            inEffect = false;
                            continue;
                        } else if (line.equals("effect {")) {
                            inPrecondition = false;
                            inEffect = true;
                            continue;
                        } else if (line.equals("}") && (inPrecondition || inEffect)) {
                            // End of precondition or effect block
                            inPrecondition = false;
                            inEffect = false;
                            continue;
                        } else if (line.equals("}") && inAction) {
                            // End of action block
                            inAction = false;
                            break;
                        }
                        
                        // Collect predicate instances
                        if ((isPrecondition && inPrecondition) || (!isPrecondition && inEffect)) {
                            if (line.startsWith("PredicateInstance:")) {
                                predicates.add(line);
                            }
                        }
                    }
                }
            }
            
            System.out.println("Found " + predicates.size() + " predicates for " + actionName + " (" + (isPrecondition ? "precondition" : "effect") + ")");
            return predicates.toArray(new String[0]);
            
        } catch (IOException e) {
            System.err.println("Error reading model file: " + e.getMessage());
            return new String[0];
        }
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
