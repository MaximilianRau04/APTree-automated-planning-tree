import { createId } from "../../../utils/id";
import type {
  ActionInstance,
  ActionType,
  ParameterInstance,
  ParameterType,
  PredicateInstance,
  PredicateType,
  StructuredItem,
} from "./types";

/**
 * generates a unique identifier for generic structured items.
 * @returns freshly generated item id
 */
export const generateItemId = () => createId("item");

/**
 * creates an empty structured item scaffold.
 * @returns blank structured item ready for user input
 */
export const createEmptyStructuredItem = (): StructuredItem => ({
  id: generateItemId(),
  name: "",
  type: "",
});

/**
 * creates an empty parameter-type definition with no properties.
 * @returns blank parameter type descriptor
 */
export const createEmptyParameterType = (): ParameterType => ({
  ...createEmptyStructuredItem(),
  id: createId("param-type"),
  properties: [],
});

/**
 * creates an empty predicate-type definition with no properties.
 * @returns blank predicate type descriptor
 */
export const createEmptyPredicateType = (): PredicateType => ({
  ...createEmptyStructuredItem(),
  id: createId("predicate-type"),
  properties: [],
});

/**
 * creates an empty action-type definition with no properties.
 * @returns blank action type descriptor
 */
export const createEmptyActionType = (): ActionType => ({
  ...createEmptyStructuredItem(),
  id: createId("action-type"),
  properties: [],
});

/**
 * deep clones a parameter type including its properties.
 * @param entry parameter type that should be cloned
 * @returns cloned parameter type
 */
export const cloneParameterType = (entry: ParameterType): ParameterType => ({
  ...entry,
  properties: entry.properties.map((property) => ({ ...property })),
});

/**
 * deep clones a predicate type including its properties.
 * @param entry predicate type that should be cloned
 * @returns cloned predicate type
 */
export const clonePredicateType = (entry: PredicateType): PredicateType => ({
  ...entry,
  properties: entry.properties.map((property) => ({ ...property })),
});

/**
 * deep clones an action type including its properties.
 * @param entry action type that should be cloned
 * @returns cloned action type
 */
export const cloneActionType = (entry: ActionType): ActionType => ({
  ...entry,
  properties: entry.properties.map((property) => ({ ...property })),
});

/**
 * creates an empty parameter instance optionally seeded from a parameter type.
 * @param parameterType optional type whose schema populates the instance
 * @returns blank parameter instance structure
 */
export const createEmptyParameterInstance = (
  parameterType?: ParameterType
): ParameterInstance => ({
  ...createEmptyStructuredItem(),
  id: createId("param-instance"),
  type: parameterType?.name ?? "",
  typeId: parameterType?.id ?? "",
  propertyValues: parameterType
    ? parameterType.properties.reduce<Record<string, string>>(
        (acc, property) => {
          acc[property.id] = "";
          return acc;
        },
        {}
      )
    : {},
});

/**
 * creates an empty predicate instance optionally seeded from a predicate type.
 * @param predicateType optional type describing the expected predicate structure
 * @returns blank predicate instance structure
 */
export const createEmptyPredicateInstance = (
  predicateType?: PredicateType
): PredicateInstance => ({
  ...createEmptyStructuredItem(),
  id: createId("predicate-instance"),
  type: predicateType?.name ?? "",
  typeId: predicateType?.id ?? "",
  propertyValues: predicateType
    ? predicateType.properties.reduce<Record<string, string>>(
        (acc, property) => {
          acc[property.id] = "";
          return acc;
        },
        {}
      )
    : {},
  isNegated: false,
});

/**
 * creates an empty action instance optionally seeded from an action type.
 * @param actionType optional type describing the expected action structure
 * @returns blank action instance structure
 */
export const createEmptyActionInstance = (
  actionType?: ActionType
): ActionInstance => ({
  ...createEmptyStructuredItem(),
  id: createId("action-instance"),
  type: actionType?.name ?? "",
  typeId: actionType?.id ?? "",
  propertyValues: actionType
    ? actionType.properties.reduce<Record<string, string>>(
        (acc, property) => {
          acc[property.id] = "";
          return acc;
        },
        {}
      )
    : {},
});

/**
 * deep clones a parameter instance including its property values map.
 * @param entry parameter instance to clone
 * @returns cloned parameter instance
 */
export const cloneParameterInstance = (
  entry: ParameterInstance
): ParameterInstance => ({
  ...entry,
  propertyValues: { ...entry.propertyValues },
});

/**
 * deep clones a predicate instance including its property values map.
 * @param entry predicate instance to clone
 * @returns cloned predicate instance
 */
export const clonePredicateInstance = (
  entry: PredicateInstance
): PredicateInstance => ({
  ...entry,
  propertyValues: { ...entry.propertyValues },
  isNegated: entry.isNegated,
});

/**
 * deep clones an action instance including its property values map.
 * @param entry action instance to clone
 * @returns cloned action instance
 */
export const cloneActionInstance = (entry: ActionInstance): ActionInstance => ({
  ...entry,
  propertyValues: { ...entry.propertyValues },
});

/**
 * aligns stored parameter-instance values with the latest parameter-type schema.
 * @param parameterType parameter type describing the expected property ids
 * @param currentValues map containing the current property values
 * @returns reconciled property value map containing all expected keys
 */
export const reconcileInstanceValues = (
  definition: ParameterType | PredicateType | ActionType,
  currentValues: Record<string, string>
): Record<string, string> =>
  definition.properties.reduce<Record<string, string>>((acc, property) => {
    acc[property.id] = currentValues[property.id] ?? "";
    return acc;
  }, {});

/**
 * generates a unique identifier for a parameter property.
 * @returns freshly generated property id
 */
export const generatePropertyId = () => createId("property");
