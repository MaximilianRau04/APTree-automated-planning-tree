import type {
  CategoryItemListProps,
  DataCategory,
  ParameterInstance,
  ParameterType,
  StructuredItem,
} from "../utils/types";
import { PARAM_INSTANCES_KEY, PARAM_TYPES_KEY } from "../utils/constants";

/**
 * checks if an item matches the search query for the given category.
 * @param category active category used to determine additional fields to inspect
 * @param query normalized search query string
 * @param item structured item currently being evaluated
 * @param parameterTypeMap lookup table for parameter types keyed by their id
 * @returns true when the item contains the query in its main fields or nested data
 */
const matchesSearch = (
  category: DataCategory,
  query: string,
  item: StructuredItem,
  parameterTypeMap: Map<string, ParameterType>
): boolean => {
  if (!query) {
    return true;
  }

  const lowerQuery = query.toLowerCase();
  const base = `${item.name} ${item.type}`.toLowerCase();
  if (base.includes(lowerQuery)) {
    return true;
  }

  if (category === PARAM_TYPES_KEY) {
    const typeItem = item as ParameterType;
    return typeItem.properties.some((property) =>
      `${property.name} ${property.valueType}`
        .toLowerCase()
        .includes(lowerQuery)
    );
  }

  if (category === PARAM_INSTANCES_KEY) {
    const instance = item as ParameterInstance;
    const linkedType = parameterTypeMap.get(instance.typeId);
    const typeName = (linkedType?.name ?? item.type).toLowerCase();
    if (typeName.includes(lowerQuery)) {
      return true;
    }

    return Object.entries(instance.propertyValues ?? {}).some(
      ([propertyId, propertyValue]) => {
        const propertyLabel = linkedType?.properties.find(
          (property) => property.id === propertyId
        )?.name;
        const combined = `${propertyLabel ?? ""} ${propertyValue ?? ""}`
          .toLowerCase()
          .trim();
        return combined.includes(lowerQuery);
      }
    );
  }

  return false;
};

/**
 * renders the filtered list of items inside a sidebar section and exposes edit/delete affordances.
 * @param props composite props for the rendered list and callbacks
 * @returns sidebar list markup including empty-state messaging
 */
export function CategoryItemList({
  category,
  items,
  parameterTypes,
  parameterTypeMap,
  searchQuery,
  onEdit,
  onDelete,
}: CategoryItemListProps) {
  const trimmedQuery = searchQuery.trim();
  const isParameterInstanceCategory = category === PARAM_INSTANCES_KEY;
  const filteredItems = trimmedQuery
    ? items.filter((item) =>
        matchesSearch(category, trimmedQuery, item, parameterTypeMap)
      )
    : items;

  return (
    <div className="item-list-container">
      {filteredItems.map((item, index) => {
        const badgeLabel = isParameterInstanceCategory
          ? parameterTypeMap.get((item as ParameterInstance).typeId)?.name ||
            item.type
          : item.type;

        return (
          <div
            key={item.id || `${item.name}-${index}`}
            className="list-item-box"
          >
            <span className="list-item-text">
              <span className="item-name">{item.name}</span>
              <span className="list-item-separator" aria-hidden>
                :
              </span>
              <span className="type-badge">
                <span className="type-badge-primary">{badgeLabel}</span>
              </span>
            </span>

            <div className="list-item-actions">
              <button
                className="icon-btn edit-btn"
                onClick={() => onEdit(category, index, item)}
                title="Edit"
                type="button"
              >
                ✎
              </button>
              <button
                className="icon-btn delete-btn"
                onClick={() => onDelete(category, index)}
                title="Delete"
                type="button"
              >
                ×
              </button>
            </div>
          </div>
        );
      })}

      {filteredItems.length === 0 && (
        <p className="empty-copy">
          {trimmedQuery
            ? "No matching items."
            : isParameterInstanceCategory && parameterTypes.length === 0
            ? "Create a parameter type before adding instances."
            : "No items defined."}
        </p>
      )}
    </div>
  );
}
