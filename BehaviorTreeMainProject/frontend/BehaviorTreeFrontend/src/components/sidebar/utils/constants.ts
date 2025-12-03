import type {
  AppData,
  BehaviorNodeOption,
  CategoryConfig,
  DataCategory,
  DecoratorNodeOption,
  FlowNodeOption,
  ServiceNodeOption,
} from "./types";

/** canonical flow-node definitions displayed in the BT node wizard. */
export const FLOW_NODE_OPTIONS: FlowNodeOption[] = [
  {
    id: "sequence",
    label: "Sequence",
    typeLabel: "Flow Node",
    description:
      "Execute children in order and fail on the first child that fails.",
    kind: "flow",
  },
  {
    id: "selector",
    label: "Selector",
    typeLabel: "Flow Node",
    description:
      "Pick the first child that succeeds, falling back to the next on failure.",
    kind: "flow",
  },
  {
    id: "parallel",
    label: "Parallel",
    typeLabel: "Flow Node",
    description: "Run all children simultaneously and gather their results.",
    kind: "flow",
  },
];

/** canonical decorator-node definitions made available to users. */
export const DECORATOR_NODE_OPTIONS: DecoratorNodeOption[] = [
  {
    id: "inverter",
    label: "Inverter",
    typeLabel: "Decorator",
    description:
      "Flip the child node result from success to failure and vice versa.",
    kind: "decorator",
  },
  {
    id: "repeat-until-success",
    label: "Repeat Until Success",
    typeLabel: "Decorator",
    description:
      "Retry the child node until it succeeds or reaches a retry limit.",
    kind: "decorator",
  },
  {
    id: "cooldown",
    label: "Cooldown",
    typeLabel: "Decorator",
    description:
      "Ensure the child node executes only after a specified cooldown period.",
    kind: "decorator",
  },
];

/** canonical service-node definitions exposed in the sidebar. */
export const SERVICE_NODE_OPTIONS: ServiceNodeOption[] = [
  {
    id: "sensing-service",
    label: "Sensing Service",
    typeLabel: "Service",
    description:
      "Run periodic sensor checks alongside the behavior tree branch.",
    kind: "service",
  },
  {
    id: "blackboard-sync",
    label: "Blackboard Sync",
    typeLabel: "Service",
    description:
      "Continuously synchronize key values into the blackboard while active.",
    kind: "service",
  },
];

/** combined behavior-node catalog leveraged by the sidebar and canvas. */
export const BEHAVIOR_NODE_OPTIONS: BehaviorNodeOption[] = [
  ...FLOW_NODE_OPTIONS,
  ...DECORATOR_NODE_OPTIONS,
  ...SERVICE_NODE_OPTIONS,
];

/** constant-time lookup table for behavior node templates by id. */
export const BEHAVIOR_NODE_OPTION_MAP = new Map<string, BehaviorNodeOption>(
  BEHAVIOR_NODE_OPTIONS.map((option) => [option.id, option])
);

export const BT_NODES_KEY: DataCategory = "nodes";
export const DECORATOR_NODES_KEY: DataCategory = "decorators";
export const SERVICE_NODES_KEY: DataCategory = "services";

/**
 * central configuration describing each sidebar category including labels and defaults.
 * @returns ordered category configuration list consumed across the sidebar
 */
export const CATEGORY_CONFIG: CategoryConfig[] = [
  {
    key: "variables",
    title: "Blackboard Variables",
    addLabel: "Add Variable",
    defaultItems: [
      { id: "variable-health", name: "health", type: "Integer" },
      { id: "variable-target", name: "target", type: "Agent" },
    ],
  },
  {
    key: BT_NODES_KEY,
    title: "Behavior Tree Nodes",
    addLabel: "Add Behavior Node",
  },
  {
    key: "paramTypes",
    title: "Parameter Types",
    addLabel: "Add Parameter Type",
  },
  {
    key: "paramInstances",
    title: "Parameter Instances",
    addLabel: "Add Parameter Instance",
  },
  {
    key: "predTypes",
    title: "Predicate Types",
    addLabel: "Add Predicate Type",
  },
  {
    key: "predInstances",
    title: "Predicate Instances",
    addLabel: "Add Predicate Instance",
  },
  { key: "actions", title: "Action Types", addLabel: "Add Action Type" },
  {
    key: "actionInstances",
    title: "Action Instances",
    addLabel: "Add Action Instance",
  },
];

/**
 * provides default data entries mapped by category, cloning template defaults where available.
 * @returns hydrated data map keyed by category identifiers
 */
export const DEFAULT_DATA: AppData = CATEGORY_CONFIG.reduce<AppData>(
  (acc, section) => {
    const defaults = section.defaultItems ?? [];
    acc[section.key] = defaults.map((item) => ({ ...item }));
    return acc;
  },
  {} as AppData
);

/**
 * maps each category key to its display title for quick lookup.
 * @returns immutable dictionary mapping category to title
 */
export const DEFAULT_TITLES = CATEGORY_CONFIG.reduce<Record<string, string>>(
  (acc, section) => {
    acc[section.key] = section.title;
    return acc;
  },
  {}
);

/**
 * maps category keys to their associated "add" button labels.
 * @returns dictionary of add button captions
 */
export const ADD_LABELS = CATEGORY_CONFIG.reduce<Record<string, string>>(
  (acc, section) => {
    acc[section.key] = section.addLabel;
    return acc;
  },
  {}
);

/**
 * lists the default rendering order of categories in the sidebar.
 * @returns array of category keys sorted for initial render
 */
export const DEFAULT_ORDER = CATEGORY_CONFIG.map((section) => section.key);

export const PARAM_TYPES_KEY: DataCategory = "paramTypes";
export const PARAM_INSTANCES_KEY: DataCategory = "paramInstances";
export const PREDICATE_TYPES_KEY: DataCategory = "predTypes";
export const PREDICATE_INSTANCES_KEY: DataCategory = "predInstances";
export const ACTION_TYPES_KEY: DataCategory = "actions";
export const ACTION_INSTANCES_KEY: DataCategory = "actionInstances";
export const FLOW_NODES_KEY: DataCategory = "flowNodes";
export const DRAGGABLE_NODE_CATEGORIES: readonly DataCategory[] = [
  ACTION_INSTANCES_KEY,
];
