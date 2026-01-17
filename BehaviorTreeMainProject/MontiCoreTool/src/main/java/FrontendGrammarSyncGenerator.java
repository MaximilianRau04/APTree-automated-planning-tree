import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.time.Instant;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Generates frontend TypeScript constants/types from MontiCore grammars.
 *
 * This intentionally keeps the output small and stable:
 * - enums (SuccessCriteria, TemporalType, ChildType)
 * - key constraints (BehaviorTree root nonterminal, GraphNode keyword)
 */
public class FrontendGrammarSyncGenerator {

  private static final String DEFAULT_GRAMMAR_RELATIVE_PATH = "src/main/grammars/DynamicBTFlowNode.mc4";

  private static final Pattern ENUM_PATTERN = Pattern.compile(
      "enum\\s+(\\w+)\\s*=\\s*([^;]+);",
      Pattern.MULTILINE
  );

  private static final Pattern QUOTED_TOKEN_PATTERN = Pattern.compile("\"([^\"]+)\"");

  private static final Pattern ROOT_NONTERMINAL_PATTERN = Pattern.compile(
      "BehaviorTree\\s*=.*?root\\s+root:([A-Za-z_][A-Za-z0-9_]*)",
      Pattern.DOTALL
  );

  private static final Pattern GRAPHNODE_KEYWORD_PATTERN = Pattern.compile(
      "GraphNode\\s*=\\s*\\\"([^\\\"]+)\\\"",
      Pattern.MULTILINE
  );

  public static void main(String[] args) throws Exception {
    final Path montiCoreToolDir = Paths.get(System.getProperty("user.dir")).toAbsolutePath().normalize();

    final Path grammarPath = args.length > 0
        ? montiCoreToolDir.resolve(args[0]).normalize()
        : montiCoreToolDir.resolve(DEFAULT_GRAMMAR_RELATIVE_PATH).normalize();

    if (!Files.exists(grammarPath)) {
      throw new IllegalStateException("Grammar file not found: " + grammarPath);
    }

    final String grammarText = Files.readString(grammarPath, StandardCharsets.UTF_8);

    final Map<String, List<String>> enums = parseEnums(grammarText);
    final List<String> successCriteria = requireEnum(enums, "SuccessCriteria");
    final List<String> temporalTypes = requireEnum(enums, "TemporalType");
    final List<String> childTypes = requireEnum(enums, "ChildType");

    final String rootNonterminal = parseRootNonterminal(grammarText);
    final String graphNodeKeyword = parseGraphNodeKeyword(grammarText);

    // Keep mapping small and explicit. Frontend uses sourceId "dynamic-flow-node" for the grammar nonterminal DynamicFlowNode.
    final String rootSourceId = "DynamicFlowNode".equals(rootNonterminal) ? "dynamic-flow-node" : null;

    final Path frontendGeneratedDir = montiCoreToolDir
        .resolve("../frontend/BehaviorTreeFrontend/src/generated")
        .normalize();

    Files.createDirectories(frontendGeneratedDir);

    final Path tsOut = frontendGeneratedDir.resolve("aptreeGrammar.ts");
    final Path jsonOut = frontendGeneratedDir.resolve("aptreeGrammar.json");

    writeTypeScript(tsOut, grammarPath, successCriteria, temporalTypes, childTypes, rootNonterminal, rootSourceId, graphNodeKeyword);
    writeJson(jsonOut, grammarPath, successCriteria, temporalTypes, childTypes, rootNonterminal, rootSourceId, graphNodeKeyword);

    System.out.println("Generated: " + tsOut);
    System.out.println("Generated: " + jsonOut);
  }

  private static Map<String, List<String>> parseEnums(String text) {
    final Map<String, List<String>> out = new LinkedHashMap<>();

    final Matcher matcher = ENUM_PATTERN.matcher(text);
    while (matcher.find()) {
      final String name = matcher.group(1);
      final String body = matcher.group(2);

      final List<String> values = new ArrayList<>();
      final Matcher tokenMatcher = QUOTED_TOKEN_PATTERN.matcher(body);
      while (tokenMatcher.find()) {
        values.add(tokenMatcher.group(1));
      }

      if (!values.isEmpty()) {
        out.put(name, values);
      }
    }

    return out;
  }

  private static List<String> requireEnum(Map<String, List<String>> enums, String name) {
    final List<String> values = enums.get(name);
    if (values == null || values.isEmpty()) {
      throw new IllegalStateException("Required enum not found or empty: " + name);
    }
    return values;
  }

  private static String parseRootNonterminal(String text) {
    final Matcher matcher = ROOT_NONTERMINAL_PATTERN.matcher(text);
    if (!matcher.find()) {
      throw new IllegalStateException("Could not parse BehaviorTree root nonterminal.");
    }
    return matcher.group(1);
  }

  private static String parseGraphNodeKeyword(String text) {
    final Matcher matcher = GRAPHNODE_KEYWORD_PATTERN.matcher(text);
    if (!matcher.find()) {
      throw new IllegalStateException("Could not parse GraphNode keyword literal.");
    }
    return matcher.group(1);
  }

  private static void writeTypeScript(
      Path out,
      Path grammarPath,
      List<String> successCriteria,
      List<String> temporalTypes,
      List<String> childTypes,
      String rootNonterminal,
      String rootSourceId,
      String graphNodeKeyword
  ) throws IOException {

    final String header = "// AUTO-GENERATED by MontiCoreTool (FrontendGrammarSyncGenerator)\n" +
        "// Source: " + grammarPath.getFileName() + "\n" +
        "// Generated at: " + Instant.now().toString() + "\n" +
        "// DO NOT EDIT MANUALLY\n\n";

    final StringBuilder sb = new StringBuilder();
    sb.append(header);

    sb.append("export const SUCCESS_CRITERIA = ").append(asTsConstArray(successCriteria)).append(";\n");
    sb.append("export type SuccessCriteria = (typeof SUCCESS_CRITERIA)[number];\n\n");

    sb.append("export const TEMPORAL_TYPES = ").append(asTsConstArray(temporalTypes)).append(";\n");
    sb.append("export type TemporalType = (typeof TEMPORAL_TYPES)[number];\n\n");

    sb.append("export const CHILD_TYPES = ").append(asTsConstArray(childTypes)).append(";\n");
    sb.append("export type ChildType = (typeof CHILD_TYPES)[number];\n\n");

    sb.append("export const GRAMMAR_CONSTRAINTS = {");
    sb.append("\n  behaviorTreeRootNonterminal: ").append(quote(rootNonterminal)).append(",");
    sb.append("\n  rootNode: {");
    sb.append("\n    category: \"flowNodes\",");
    sb.append("\n    sourceId: ").append(rootSourceId == null ? "null" : quote(rootSourceId)).append(",");
    sb.append("\n  },");
    sb.append("\n  nodeGraph: {");
    sb.append("\n    graphNodeKeyword: ").append(quote(graphNodeKeyword)).append(",");
    sb.append("\n    relationNodeKind: ").append(Objects.equals(graphNodeKeyword, "action") ? "\"action\"" : "null").append(",");
    sb.append("\n  },");
    sb.append("\n} as const;\n");

    Files.writeString(out, sb.toString(), StandardCharsets.UTF_8);
  }

  private static void writeJson(
      Path out,
      Path grammarPath,
      List<String> successCriteria,
      List<String> temporalTypes,
      List<String> childTypes,
      String rootNonterminal,
      String rootSourceId,
      String graphNodeKeyword
  ) throws IOException {
    final String json = "{\n" +
        "  \"generatedAt\": \"" + Instant.now().toString() + "\",\n" +
        "  \"sourceGrammar\": \"" + grammarPath.getFileName().toString() + "\",\n" +
        "  \"enums\": {\n" +
        "    \"SuccessCriteria\": " + asJsonArray(successCriteria) + ",\n" +
        "    \"TemporalType\": " + asJsonArray(temporalTypes) + ",\n" +
        "    \"ChildType\": " + asJsonArray(childTypes) + "\n" +
        "  },\n" +
        "  \"constraints\": {\n" +
        "    \"behaviorTreeRootNonterminal\": \"" + escapeJson(rootNonterminal) + "\",\n" +
        "    \"rootNode\": {\n" +
        "      \"category\": \"flowNodes\",\n" +
        "      \"sourceId\": " + (rootSourceId == null ? "null" : ("\"" + escapeJson(rootSourceId) + "\"")) + "\n" +
        "    },\n" +
        "    \"nodeGraph\": {\n" +
        "      \"graphNodeKeyword\": \"" + escapeJson(graphNodeKeyword) + "\",\n" +
        "      \"relationNodeKind\": " + (Objects.equals(graphNodeKeyword, "action") ? "\"action\"" : "null") + "\n" +
        "    }\n" +
        "  }\n" +
        "}\n";

    Files.writeString(out, json, StandardCharsets.UTF_8);
  }

  private static String asTsConstArray(List<String> values) {
    final StringBuilder sb = new StringBuilder();
    sb.append("[");
    for (int i = 0; i < values.size(); i++) {
      if (i > 0) sb.append(", ");
      sb.append(quote(values.get(i)));
    }
    sb.append("] as const");
    return sb.toString();
  }

  private static String asJsonArray(List<String> values) {
    final StringBuilder sb = new StringBuilder();
    sb.append("[");
    for (int i = 0; i < values.size(); i++) {
      if (i > 0) sb.append(", ");
      sb.append("\"").append(escapeJson(values.get(i))).append("\"");
    }
    sb.append("]");
    return sb.toString();
  }

  private static String quote(String value) {
    return "\"" + value.replace("\\", "\\\\").replace("\"", "\\\"") + "\"";
  }

  private static String escapeJson(String value) {
    return value
        .replace("\\", "\\\\")
        .replace("\"", "\\\"")
        .replace("\n", "\\n")
        .replace("\r", "\\r")
        .replace("\t", "\\t");
  }
}
