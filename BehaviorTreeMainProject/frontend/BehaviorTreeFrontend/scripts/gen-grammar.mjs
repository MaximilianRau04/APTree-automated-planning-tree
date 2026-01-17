import fs from "node:fs";
import path from "node:path";

const here = path.dirname(new URL(import.meta.url).pathname);
const frontendRoot = path.resolve(here, "..");
const repoRoot = path.resolve(frontendRoot, "..", "..");

const grammarsDir = path.resolve(
  repoRoot,
  "MontiCoreTool",
  "src",
  "main",
  "grammars"
);

const grammarFiles = [
  "BehaviorTree.mc4",
  "CRFTypeDef.mc4",
  "ConcreteBT.mc4",
  "PlanningService.mc4",
  "DynamicBTFlowNode.mc4",
].map((file) => path.resolve(grammarsDir, file));

const dynamicGrammarFile = grammarFiles.find((file) => file.endsWith("DynamicBTFlowNode.mc4"));
if (!dynamicGrammarFile) {
  throw new Error("DynamicBTFlowNode.mc4 path resolution failed");
}

const generatedTs = path.resolve(
  frontendRoot,
  "src",
  "generated",
  "aptreeGrammar.ts"
);

const generatedJson = path.resolve(
  frontendRoot,
  "src",
  "generated",
  "aptreeGrammar.json"
);

const generatedBasicTypesTs = path.resolve(
  frontendRoot,
  "src",
  "generated",
  "basicTypes.ts"
);

const generatedSidebarTemplatesTs = path.resolve(
  frontendRoot,
  "src",
  "generated",
  "sidebarTemplates.ts"
);

const generatedSidebarCategoriesTs = path.resolve(
  frontendRoot,
  "src",
  "generated",
  "sidebarCategories.ts"
);

function statMtimeMs(filePath) {
  try {
    return fs.statSync(filePath).mtimeMs;
  } catch {
    return null;
  }
}

const grammarMtimes = grammarFiles
  .map((file) => ({ file, mtime: statMtimeMs(file) }))
  .filter((entry) => entry.mtime != null);

if (grammarMtimes.length === 0) {
  console.error("No grammar files found under:", grammarsDir);
  process.exit(1);
}

const latestGrammarMtime = Math.max(...grammarMtimes.map((entry) => entry.mtime));

const scriptFile = path.resolve(here, "gen-grammar.mjs");
const scriptMtime = statMtimeMs(scriptFile) ?? latestGrammarMtime;
const latestInputMtime = Math.max(latestGrammarMtime, scriptMtime);

const outputFiles = [
  generatedTs,
  generatedJson,
  generatedBasicTypesTs,
  generatedSidebarTemplatesTs,
  generatedSidebarCategoriesTs,
];

const oldestOutputMtime = Math.min(
  ...outputFiles.map((file) => statMtimeMs(file) ?? -1)
);

const needsGen = oldestOutputMtime < 0 || latestInputMtime > oldestOutputMtime;

if (!needsGen) {
  console.log(
    "Grammar artifacts up-to-date:",
    path.relative(frontendRoot, generatedTs)
  );
  process.exit(0);
}

console.log("Generating frontend grammar artifacts from MontiCore grammar file...");

const dynamicGrammarText = fs.readFileSync(dynamicGrammarFile, "utf8");
const grammarTexts = new Map(
  grammarFiles
    .map((file) => {
      try {
        return [file, fs.readFileSync(file, "utf8")];
      } catch {
        return null;
      }
    })
    .filter(Boolean)
);

/**
 * extractEnum extracts the string values of a TypeScript enum from the grammar text.
 * @param {string} enumName 
 * @returns {string[]}
 */
function extractEnum(enumName) {
  const enumRe = new RegExp(`enum\\s+${enumName}\\s*=\\s*([^;]+);`, "m");
  const match = dynamicGrammarText.match(enumRe);
  if (!match) {
    throw new Error(`Enum not found: ${enumName}`);
  }
  const body = match[1];
  const values = Array.from(body.matchAll(/"([^"]+)"/g)).map((m) => m[1]);
  if (!values.length) {
    throw new Error(`Enum ${enumName} had no values`);
  }
  return values;
}

/**
 * extractRootNonterminal extracts the root nonterminal name from the BehaviorTree rule.
 * @returns {string} 
 */
function extractRootNonterminal() {
  const match = dynamicGrammarText.match(
    /BehaviorTree\s*=.*?(?:"root"|root)\s+root:([A-Za-z_][A-Za-z0-9_]*)/s
  );
  if (!match) {
    throw new Error("Could not parse BehaviorTree root nonterminal");
  }
  return match[1];
}

/**
 * extractGraphNodeKeyword extracts the GraphNode keyword literal.
 * @returns {string} 
 */
function extractGraphNodeKeyword() {
  const match = dynamicGrammarText.match(/GraphNode\s*=\s*"([^"]+)"/m);
  if (!match) {
    throw new Error("Could not parse GraphNode keyword literal");
  }
  return match[1];
}

/**
 * converts a string to kebab-case.
 * @param {*} value 
 * @returns 
 */
function toKebabCase(value) {
  return value
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .replace(/_/g, "-")
    .trim()
    .toLowerCase();
}

/**
 * creates a slug from a label string.
 * @param {*} value 
 * @returns 
 */
function slugFromLabel(value) {
  return value
    .trim()
    .replace(/[^A-Za-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .toLowerCase();
}

function extractQuotedStrings(statement) {
  return Array.from(statement.matchAll(/"([^"]+)"/g)).map((m) => m[1]);
}

/**
 * finds all grammar statements that extend any of the given base names.
 * @param {*} text 
 * @param {*} baseNames 
 * @returns 
 */
function findStatementsExtending(text, baseNames) {
  const results = [];
  const startRe = /^\s*([A-Za-z_][A-Za-z0-9_]*)\s+extends\s+([A-Za-z_][A-Za-z0-9_]*)\s*=/gm;
  let match;
  while ((match = startRe.exec(text))) {
    const name = match[1];
    const base = match[2];
    if (!baseNames.includes(base)) {
      continue;
    }
    const start = match.index;
    const end = text.indexOf(";", start);
    if (end < 0) {
      continue;
    }
    const statement = text.slice(start, end + 1);
    results.push({ name, base, statement });
  }
  return results;
}

/**
 * extracts a section of text between start and end markers.
 * @param {*} text 
 * @param {*} startMarker 
 * @param {*} endMarker 
 * @returns {string|null}
 */
function extractSection(text, startMarker, endMarker) {
  const startIndex = text.indexOf(startMarker);
  if (startIndex < 0) {
    return null;
  }
  const endIndex = text.indexOf(endMarker, startIndex + startMarker.length);
  if (endIndex < 0) {
    return null;
  }
  return text.slice(startIndex + startMarker.length, endIndex);
}

/**
 * extracts argument definitions from a grammar statement.
 * @param {*} statement 
 * @returns 
 */
function extractArgs(statement) {
  const args = [];
  const argRe = /\b([A-Za-z_][A-Za-z0-9_]*)\s*:\s*Name\s*@\s*([A-Za-z_][A-Za-z0-9_]*)/g;
  let match;
  while ((match = argRe.exec(statement))) {
    const name = match[1];
    if (name === "subtreeAnnotation") {
      continue;
    }
    args.push({ name, type: match[2] });
  }
  return args;
}

/**
 * builds a predicate or action type definition from grammar info.
 * @param {*} param0 
 * @returns {object}
 */
function buildTypeDefinition({ kind, name, label, args }) {
  const baseType = kind === "predicate" ? "Predicate" : "Action";
  const normalizedKind = kind === "predicate" ? "predtype" : "actiontype";
  const id = `${normalizedKind}-${slugFromLabel(label || name) || toKebabCase(name)}`;
  const properties = args.map((arg) => ({
    id: `${id}-${slugFromLabel(arg.name)}`,
    name: arg.name,
    valueType: arg.type,
  }));
  return {
    id,
    name: label || name,
    type: baseType,
    description: `Generated from grammar (${name}).`,
    properties,
  };
}

/**
 * collects all property type names from the grammar text.
 * @param {*} text 
 * @returns {Set<string>}
 */
function collectPropertyTypes(text) {
  const typeNames = new Set();
  const symbolRe = /^\s*(?:abstract\s+)?symbol\s+([A-Za-z_][A-Za-z0-9_]*)\s+extends\s+([A-Za-z_][A-Za-z0-9_]*)/gm;
  let match;
  while ((match = symbolRe.exec(text))) {
    typeNames.add(match[1]);
    typeNames.add(match[2]);
  }
  const implementsRe = /^\s*([A-Za-z_][A-Za-z0-9_]*)\s+implements\s+PropertyType\b/gm;
  while ((match = implementsRe.exec(text))) {
    typeNames.add(match[1]);
  }
  return typeNames;
}

const successCriteria = extractEnum("SuccessCriteria");
const temporalTypes = extractEnum("TemporalType");
const childTypes = extractEnum("ChildType");
const behaviorTreeRootNonterminal = extractRootNonterminal();
const graphNodeKeyword = extractGraphNodeKeyword();

// Minimal mapping between grammar nonterminals and editor templates.
const rootSourceId = behaviorTreeRootNonterminal
  ? toKebabCase(behaviorTreeRootNonterminal)
  : null;
const relationNodeKind = graphNodeKeyword === "action" ? "action" : null;

fs.mkdirSync(path.dirname(generatedTs), { recursive: true });

const now = new Date().toISOString();
const ts = `// AUTO-GENERATED by scripts/gen-grammar.mjs\n` +
  `// Source: ${path.relative(frontendRoot, dynamicGrammarFile)}\n` +
  `// Generated at: ${now}\n` +
  `// DO NOT EDIT MANUALLY\n\n` +
  `export const SUCCESS_CRITERIA = ${JSON.stringify(successCriteria)} as const;\n` +
  `export type SuccessCriteria = (typeof SUCCESS_CRITERIA)[number];\n\n` +
  `export const TEMPORAL_TYPES = ${JSON.stringify(temporalTypes)} as const;\n` +
  `export type TemporalType = (typeof TEMPORAL_TYPES)[number];\n\n` +
  `export const CHILD_TYPES = ${JSON.stringify(childTypes)} as const;\n` +
  `export type ChildType = (typeof CHILD_TYPES)[number];\n\n` +
  `export const GRAMMAR_CONSTRAINTS = {\n` +
  `  behaviorTreeRootNonterminal: ${JSON.stringify(behaviorTreeRootNonterminal)},\n` +
  `  rootNode: { category: "flowNodes", sourceId: ${rootSourceId ? JSON.stringify(rootSourceId) : "null"} },\n` +
  `  nodeGraph: { graphNodeKeyword: ${JSON.stringify(graphNodeKeyword)}, relationNodeKind: ${relationNodeKind ? JSON.stringify(relationNodeKind) : "null"} },\n` +
  `} as const;\n`;

fs.writeFileSync(generatedTs, ts);

const json = {
  generatedAt: now,
  source: path.relative(frontendRoot, dynamicGrammarFile),
  enums: {
    SuccessCriteria: successCriteria,
    TemporalType: temporalTypes,
    ChildType: childTypes,
  },
  constraints: {
    behaviorTreeRootNonterminal,
    rootNode: { category: "flowNodes", sourceId: rootSourceId },
    nodeGraph: { graphNodeKeyword, relationNodeKind },
  },
};

fs.writeFileSync(generatedJson, JSON.stringify(json, null, 2) + "\n");

// sidebar category definitions
const sidebarCategoryDefinitions = [
  { key: "variables", title: "Blackboard", addLabel: "Add Variable" },
  { key: "nodes", title: "Behavior Tree Nodes", addLabel: "Add Behavior Node" },
  { key: "decorators", title: "Decorator Templates", addLabel: "Add Decorator" },
  { key: "services", title: "Service Templates", addLabel: "Add Service" },
  { key: "paramTypes", title: "Parameter Types", addLabel: "Add Parameter Type" },
  { key: "paramInstances", title: "Parameter Instances", addLabel: "Add Parameter Instance" },
  { key: "predTypes", title: "Predicate Types", addLabel: "Add Predicate Type" },
  { key: "predInstances", title: "Predicate Instances", addLabel: "Add Predicate Instance" },
  { key: "actions", title: "Action Types", addLabel: "Add Action Type" },
];

// Sidebar categories that support dragging items onto the canvas.
const draggableNodeCategories = ["actionInstances"];

const sidebarCategoriesTs = `// AUTO-GENERATED by scripts/gen-grammar.mjs\n` +
  `// Sources: ${grammarFiles.map((f) => path.basename(f)).join(", ")}\n` +
  `// Generated at: ${now}\n` +
  `// DO NOT EDIT MANUALLY\n\n` +
  `export const GENERATED_SIDEBAR_CATEGORY_DEFINITIONS = ${JSON.stringify(sidebarCategoryDefinitions, null, 2)} as const;\n\n` +
  `export const GENERATED_DRAGGABLE_NODE_CATEGORIES = ${JSON.stringify(draggableNodeCategories, null, 2)} as const;\n`;

fs.writeFileSync(generatedSidebarCategoriesTs, sidebarCategoriesTs);

// basic type definitions
const primitiveTypes = ["String", "Double", "Integer", "Boolean"];
const propertyTypes = new Set();
for (const [filePath, text] of grammarTexts.entries()) {
  if (filePath.endsWith("CRFTypeDef.mc4") || filePath.endsWith("ConcreteBT.mc4")) {
    for (const entry of collectPropertyTypes(text)) {
      propertyTypes.add(entry);
    }
  }
}

// Drop internal helper types that are not useful as user-facing base types.
propertyTypes.delete("PropertyType");
propertyTypes.delete("CustomProperty");
propertyTypes.delete("MCPrimitiveType");

const basicTypeOptions = Array.from(new Set([...propertyTypes, ...primitiveTypes]))
  .filter(Boolean)
  .sort((a, b) => a.localeCompare(b));

const basicTypesTs = `// AUTO-GENERATED by scripts/gen-grammar.mjs\n` +
  `// Sources: ${grammarFiles.map((f) => path.basename(f)).join(", ")}\n` +
  `// Generated at: ${now}\n` +
  `// DO NOT EDIT MANUALLY\n\n` +
  `export const BASIC_TYPE_OPTIONS = ${JSON.stringify(basicTypeOptions)} as const;\n` +
  `export type BasicTypeOption = (typeof BASIC_TYPE_OPTIONS)[number];\n`;

fs.writeFileSync(generatedBasicTypesTs, basicTypesTs);

const flowStatements = [];
const decoratorStatements = [];
const serviceStatements = [];

for (const text of grammarTexts.values()) {
  flowStatements.push(...findStatementsExtending(text, ["FlowNode"]));
  decoratorStatements.push(...findStatementsExtending(text, ["Decorator"]));
  serviceStatements.push(...findStatementsExtending(text, ["Service", "PService"]));
}

/**
 * buildTemplate builds a sidebar template option from a grammar statement.
 * @param {*} param0 
 * @param {*} kind 
 * @returns {object}
 */
function buildTemplate({ name, base, statement }, kind) {
  const quoted = extractQuotedStrings(statement);
  const keyword = quoted[0] ?? name;
  const secondary = quoted[1] ?? null;
  const rawLabel =
    secondary && keyword.toLowerCase() === "action"
      ? secondary
      : keyword.toLowerCase() === "decorator" || keyword.toLowerCase() === "service"
        ? name
        : keyword;
  const label = rawLabel;
  const id = slugFromLabel(label || name || `${kind}-${base}`) || toKebabCase(name);

  const typeLabel =
    kind === "flow" ? "Flow Node" : kind === "decorator" ? "Decorator" : "Service";

  const description = `Generated from grammar (${name} extends ${base}).`;

  const option = {
    id,
    label,
    typeLabel,
    description,
    kind,
  };

  if (kind === "flow") {
    const defaultSuccessType =
      /fallback/i.test(label) ? "ANY" : /sequence/i.test(label) ? "ALL" : "ALL";
    return { ...option, defaultSuccessType };
  }

  return option;
}

/**
 * Generate sidebar template options from grammar statements.
 */
const generatedFlowNodeOptions = flowStatements
  .filter((entry) => entry.name !== behaviorTreeRootNonterminal)
  .map((entry) => buildTemplate(entry, "flow"))
  // Ensure the grammar root node is always present as a template.
  .concat(
    rootSourceId
      ? [
          {
            id: rootSourceId,
            label: `Root Node (${behaviorTreeRootNonterminal})`,
            typeLabel: "Flow Node",
            description: `Generated grammar root node template (${behaviorTreeRootNonterminal}).`,
            kind: "flow",
            defaultSuccessType: "ALL",
          },
        ]
      : []
  )
  // de-dup by id
  .filter((option, index, arr) => arr.findIndex((o) => o.id === option.id) === index)
  .sort((a, b) => a.label.localeCompare(b.label));

const generatedDecoratorNodeOptions = decoratorStatements
  .map((entry) => buildTemplate(entry, "decorator"))
  .filter((option, index, arr) => arr.findIndex((o) => o.id === option.id) === index)
  .sort((a, b) => a.label.localeCompare(b.label));

const generatedServiceNodeOptions = serviceStatements
  .map((entry) => buildTemplate(entry, "service"))
  .filter((option, index, arr) => arr.findIndex((o) => o.id === option.id) === index)
  .sort((a, b) => a.label.localeCompare(b.label));

// Predicate + Action type definitions 
const concreteText = Array.from(grammarTexts.entries()).find(([filePath]) =>
  filePath.endsWith("ConcreteBT.mc4")
)?.[1];

const predicateTypes = [];
const actionTypes = [];

if (concreteText) {
  const predicateSection = extractSection(
    concreteText,
    "// === GENERATED PREDICATE RULES (DO NOT EDIT BELOW) ===",
    "// === END GENERATED PREDICATE RULES ==="
  );

  if (predicateSection) {
    const predRe = /\b([A-Za-z_][A-Za-z0-9_]*)\s+extends\s+Predicate\s*=\s*([^;]+);/g;
    for (const match of predicateSection.matchAll(predRe)) {
      const name = match[1];
      const statement = match[2];
      const quoted = extractQuotedStrings(statement);
      const label = quoted[0] ?? name;
      const args = extractArgs(statement);
      predicateTypes.push(buildTypeDefinition({ kind: "predicate", name, label, args }));
    }
  }

  const actionSection = extractSection(
    concreteText,
    "// === GENERATED ACTION RULES (DO NOT EDIT BELOW) ===",
    "// === END GENERATED ACTION RULES ==="
  );

  if (actionSection) {
    const actionRe = /\b([A-Za-z_][A-Za-z0-9_]*)\s+extends\s+PActionNode\s*=\s*([^;]+);/gs;
    for (const match of actionSection.matchAll(actionRe)) {
      const name = match[1];
      const statement = match[2];
      const quoted = extractQuotedStrings(statement);
      const label = quoted[1] ?? quoted[0] ?? name;
      const args = extractArgs(statement);
      actionTypes.push(buildTypeDefinition({ kind: "action", name, label, args }));
    }
  }
}

predicateTypes.sort((a, b) => a.name.localeCompare(b.name));
actionTypes.sort((a, b) => a.name.localeCompare(b.name));

const sidebarTemplatesTs = `// AUTO-GENERATED by scripts/gen-grammar.mjs\n` +
  `// Sources: ${grammarFiles.map((f) => path.basename(f)).join(", ")}\n` +
  `// Generated at: ${now}\n` +
  `// DO NOT EDIT MANUALLY\n\n` +
  `export const GENERATED_FLOW_NODE_OPTIONS = ${JSON.stringify(generatedFlowNodeOptions, null, 2)} as const;\n\n` +
  `export const GENERATED_DECORATOR_NODE_OPTIONS = ${JSON.stringify(generatedDecoratorNodeOptions, null, 2)} as const;\n\n` +
  `export const GENERATED_SERVICE_NODE_OPTIONS = ${JSON.stringify(generatedServiceNodeOptions, null, 2)} as const;\n\n` +
  `export const GENERATED_PREDICATE_TYPES = ${JSON.stringify(predicateTypes, null, 2)} as const;\n\n` +
  `export const GENERATED_ACTION_TYPES = ${JSON.stringify(actionTypes, null, 2)} as const;\n`;

fs.writeFileSync(generatedSidebarTemplatesTs, sidebarTemplatesTs);

console.log("Generated:", path.relative(frontendRoot, generatedTs));
console.log("Generated:", path.relative(frontendRoot, generatedJson));
console.log("Generated:", path.relative(frontendRoot, generatedBasicTypesTs));
console.log("Generated:", path.relative(frontendRoot, generatedSidebarTemplatesTs));
console.log("Generated:", path.relative(frontendRoot, generatedSidebarCategoriesTs));
