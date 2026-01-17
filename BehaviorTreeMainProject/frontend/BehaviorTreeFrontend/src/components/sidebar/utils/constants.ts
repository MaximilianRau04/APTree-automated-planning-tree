import type {
  AppData,
  BehaviorNodeOption,
  CategoryConfig,
  DataCategory,
  DecoratorNodeOption,
  FlowNodeOption,
  ServiceNodeOption,
  StructuredItem,
} from "./types";
import {
  GENERATED_ACTION_TYPES,
  GENERATED_DECORATOR_NODE_OPTIONS,
  GENERATED_FLOW_NODE_OPTIONS,
  GENERATED_PREDICATE_TYPES,
  GENERATED_SERVICE_NODE_OPTIONS,
} from "../../../generated/sidebarTemplates";
import {
  GENERATED_DRAGGABLE_NODE_CATEGORIES,
  GENERATED_SIDEBAR_CATEGORY_DEFINITIONS,
} from "../../../generated/sidebarCategories";

/** canonical flow-node definitions displayed in the BT node wizard. */
export const FLOW_NODE_OPTIONS: FlowNodeOption[] =
  GENERATED_FLOW_NODE_OPTIONS as unknown as FlowNodeOption[];

/** canonical decorator-node definitions made available to users. */
export const DECORATOR_NODE_OPTIONS: DecoratorNodeOption[] =
  GENERATED_DECORATOR_NODE_OPTIONS as unknown as DecoratorNodeOption[];

/** canonical service-node definitions exposed in the sidebar. */
export const SERVICE_NODE_OPTIONS: ServiceNodeOption[] =
  GENERATED_SERVICE_NODE_OPTIONS as unknown as ServiceNodeOption[];

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

export const BLACKBOARD_KEY: DataCategory = "variables";
export const BT_NODES_KEY: DataCategory = "nodes";
export const DECORATOR_NODES_KEY: DataCategory = "decorators";
export const SERVICE_NODES_KEY: DataCategory = "services";

const mapBehaviorOptionToItem = (
  option: BehaviorNodeOption
): StructuredItem => ({
  id: option.id,
  name: option.label,
  type: option.typeLabel,
  description: option.description ?? "",
});

/**
 * central configuration describing each sidebar category including labels and defaults.
 * @returns ordered category configuration list consumed across the sidebar
 */
const DEFAULT_ITEMS_BY_CATEGORY: Partial<Record<DataCategory, StructuredItem[]>> = {
  [DECORATOR_NODES_KEY]: DECORATOR_NODE_OPTIONS.map(mapBehaviorOptionToItem),
  [SERVICE_NODES_KEY]: SERVICE_NODE_OPTIONS.map(mapBehaviorOptionToItem),
  predTypes: GENERATED_PREDICATE_TYPES as unknown as StructuredItem[],
  actions: GENERATED_ACTION_TYPES as unknown as StructuredItem[],
};

export const CATEGORY_CONFIG: CategoryConfig[] =
  GENERATED_SIDEBAR_CATEGORY_DEFINITIONS.map((definition) => {
    const defaultItems = DEFAULT_ITEMS_BY_CATEGORY[definition.key];
    return {
      key: definition.key,
      title: definition.title,
      addLabel: definition.addLabel,
      ...(defaultItems ? { defaultItems } : {}),
    };
  });

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
export const DRAGGABLE_NODE_CATEGORIES: readonly DataCategory[] =
  GENERATED_DRAGGABLE_NODE_CATEGORIES as unknown as DataCategory[];
