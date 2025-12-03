import type { DragEvent } from "react";
import type {
  ActionInstance,
  ActionType,
  CategoryItemListProps,
  DataCategory,
  ParameterInstance,
  ParameterType,
  PredicateInstance,
  PredicateType,
  StructuredItem,
} from "../utils/types";
import {
  ACTION_INSTANCES_KEY,
  ACTION_TYPES_KEY,
  BT_NODES_KEY,
  PARAM_INSTANCES_KEY,
  PARAM_TYPES_KEY,
  PREDICATE_INSTANCES_KEY,
  PREDICATE_TYPES_KEY,
  DRAGGABLE_NODE_CATEGORIES,
} from "../utils/constants";
import {
  DRAG_DATA_FORMAT,
  resolveDragEntityKind,
  type DraggedSidebarItem,
} from "../../editor/dragTypes";

/**
 * checks if an item matches the search query for the given category.
 * @param category active category used to determine additional fields to inspect
 * @param query normalized search query string
 * @param item structured item currently being evaluated
 * @param parameterTypeMap lookup table for parameter types keyed by their id
 * @param predicateTypeMap lookup table for predicate types keyed by their id
 * @param actionTypeMap lookup table for action types keyed by their id
 * @returns true when the item contains the query in its main fields or nested data
 */
const matchesSearch = (
  category: DataCategory,
  query: string,
  item: StructuredItem,
  parameterTypeMap: Map<string, ParameterType>,
  predicateTypeMap: Map<string, PredicateType>,
  actionTypeMap: Map<string, ActionType>
): boolean => {
  if (!query) {
    return true;
  }

  const lowerQuery = query.toLowerCase();
  const base = `${item.name} ${item.type}`.toLowerCase();
  if (base.includes(lowerQuery)) {
    return true;
  }

  if (
    category === PARAM_TYPES_KEY ||
    category === PREDICATE_TYPES_KEY ||
    category === ACTION_TYPES_KEY
  ) {
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

  if (category === PREDICATE_INSTANCES_KEY) {
    const instance = item as PredicateInstance;
    const linkedType = predicateTypeMap.get(instance.typeId);
    const typeName = (linkedType?.name ?? item.type).toLowerCase();
    if (typeName.includes(lowerQuery)) {
      return true;
    }

    if (instance.isNegated) {
      const negationTerms = ["negated", "not", "!"];
      if (
        negationTerms.some(
          (term) => term.includes(lowerQuery) || lowerQuery.includes(term)
        )
      ) {
        return true;
      }
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

  if (category === ACTION_INSTANCES_KEY) {
    const instance = item as ActionInstance;
    const linkedType = actionTypeMap.get(instance.typeId);
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
  predicateTypes,
  predicateTypeMap,
  actionTypes,
  actionTypeMap,
  searchQuery,
  onEdit,
  onDelete,
}: CategoryItemListProps) {
  const trimmedQuery = searchQuery.trim();
  const isParameterInstanceCategory = category === PARAM_INSTANCES_KEY;
  const isPredicateInstanceCategory = category === PREDICATE_INSTANCES_KEY;
  const isActionInstanceCategory = category === ACTION_INSTANCES_KEY;
  const isBehaviorNodeCategory = category === BT_NODES_KEY;
  const isDraggableCategory = DRAGGABLE_NODE_CATEGORIES.includes(category);
  const filteredItems = trimmedQuery
    ? items.filter((item) =>
        matchesSearch(
          category,
          trimmedQuery,
          item,
          parameterTypeMap,
          predicateTypeMap,
          actionTypeMap
        )
      )
    : items;

  return (
    <div className="item-list-container">
      {filteredItems.map((item, index) => {
        let badgeLabel = item.type;
        let dragTypeId: string | undefined;
        let dragIsNegated: boolean | undefined;

        if (isParameterInstanceCategory) {
          const parameterInstance = item as ParameterInstance;
          badgeLabel =
            parameterTypeMap.get(parameterInstance.typeId)?.name || item.type;
          dragTypeId = parameterInstance.typeId;
        } else if (isPredicateInstanceCategory) {
          const predicateInstance = item as PredicateInstance;
          const typeName =
            predicateTypeMap.get(predicateInstance.typeId)?.name || item.type;
          badgeLabel = predicateInstance.isNegated
            ? `NOT ${typeName}`
            : typeName;
          dragTypeId = predicateInstance.typeId;
          dragIsNegated = predicateInstance.isNegated;
        } else if (isActionInstanceCategory) {
          const actionInstance = item as ActionInstance;
          badgeLabel =
            actionTypeMap.get(actionInstance.typeId)?.name || item.type;
          dragTypeId = actionInstance.typeId;
        }

        let dragPayload: DraggedSidebarItem | null = null;

        if (isDraggableCategory) {
          dragPayload = {
            id: item.id,
            name: item.name,
            type: badgeLabel,
            category,
            kind: resolveDragEntityKind(category),
          };

          if (dragTypeId) {
            dragPayload.typeId = dragTypeId;
          }

          if (typeof dragIsNegated === "boolean") {
            dragPayload.isNegated = dragIsNegated;
          }
        }

        const handleDragStart = (event: DragEvent<HTMLSpanElement>) => {
          if (!dragPayload) {
            return;
          }

          const serializedPayload = JSON.stringify(dragPayload);
          event.dataTransfer.setData("text/plain", item.name);
          event.dataTransfer.setData(DRAG_DATA_FORMAT, serializedPayload);
          event.dataTransfer.effectAllowed = "copy";
        };

        const handleDragEnd = (event: DragEvent<HTMLSpanElement>) => {
          if (!dragPayload) {
            return;
          }

          const dataTransfer = event.dataTransfer;
          if (dataTransfer) {
            dataTransfer.clearData(DRAG_DATA_FORMAT);
          }
        };

        return (
          <div
            key={item.id || `${item.name}-${index}`}
            className="list-item-box"
          >
            <span
              className="list-item-text"
              draggable={isDraggableCategory}
              onDragStart={handleDragStart}
              onDragEnd={handleDragEnd}
              data-drag-kind={dragPayload?.kind}
            >
              <span className="item-name">{item.name}</span>
              <span className="item-meta">
                <span className="list-item-separator" aria-hidden>
                  :
                </span>
                <span className="type-badge">
                  <span className="type-badge-primary">{badgeLabel}</span>
                </span>
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
            : isPredicateInstanceCategory && predicateTypes.length === 0
            ? "Create a predicate type before adding instances."
            : isActionInstanceCategory && actionTypes.length === 0
            ? "Create an action type before adding instances."
            : isBehaviorNodeCategory
            ? "Use the Add Behavior Node wizard to create new flow, decorator, or service templates."
            : "No items defined."}
        </p>
      )}
    </div>
  );
}
