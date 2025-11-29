import { createId } from "../../../utils/id";
import type { ParameterInstance, ParameterType, StructuredItem } from "./types";

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
 * deep clones a parameter type including its properties.
 * @param entry parameter type that should be cloned
 * @returns cloned parameter type
 */
export const cloneParameterType = (entry: ParameterType): ParameterType => ({
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
 * aligns stored parameter-instance values with the latest parameter-type schema.
 * @param parameterType parameter type describing the expected property ids
 * @param currentValues map containing the current property values
 * @returns reconciled property value map containing all expected keys
 */
export const reconcileInstanceValues = (
  parameterType: ParameterType,
  currentValues: Record<string, string>
): Record<string, string> =>
  parameterType.properties.reduce<Record<string, string>>((acc, property) => {
    acc[property.id] = currentValues[property.id] ?? "";
    return acc;
  }, {});

/**
 * generates a unique identifier for a parameter property.
 * @returns freshly generated property id
 */
export const generatePropertyId = () => createId("property");
