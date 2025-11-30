import type { AppData, CategoryConfig, DataCategory } from "./types";

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
  { key: "nodes", title: "Behavior Tree Nodes", addLabel: "Add Node" },
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

/** sidebar key pointing to parameter-type entries. */
export const PARAM_TYPES_KEY: DataCategory = "paramTypes";
/** sidebar key pointing to parameter-instance entries. */
export const PARAM_INSTANCES_KEY: DataCategory = "paramInstances";
/** sidebar key pointing to predicate-type entries. */
export const PREDICATE_TYPES_KEY: DataCategory = "predTypes";
/** sidebar key pointing to predicate-instance entries. */
export const PREDICATE_INSTANCES_KEY: DataCategory = "predInstances";
/** sidebar key pointing to action-type entries. */
export const ACTION_TYPES_KEY: DataCategory = "actions";
/** sidebar key pointing to action-instance entries. */
export const ACTION_INSTANCES_KEY: DataCategory = "actionInstances";
