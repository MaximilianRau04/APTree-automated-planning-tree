import { useMemo, useRef, useState } from "react";
import { createId } from "../../utils/id";
import EditModal from "./EditModal";
import ParameterInstanceModal from "./ParameterInstanceModal.tsx";
import TypeDefinitionModal from "./TypeDefinitionModal";
import type {
  ParameterInstance,
  ParameterType,
  StructuredItem,
} from "./types.ts";
import "./Sidebar.css";
import type {
  AppData,
  DataCategory,
  ModalState,
  SectionProps,
  CategoryConfig,
} from "./types.ts";


const CATEGORY_CONFIG: CategoryConfig[] = [
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
  { key: "actions", title: "Actions", addLabel: "Add Action" },
  {
    key: "actionInstances",
    title: "Action Instances",
    addLabel: "Add Action Instance",
  },
  { key: "nodes", title: "Behavior Tree Nodes", addLabel: "Add Node" },
];

const DEFAULT_DATA: AppData = CATEGORY_CONFIG.reduce<AppData>(
  (acc, section) => {
    const defaults = section.defaultItems ?? [];
    acc[section.key] = defaults.map((item) => ({ ...item }));
    return acc;
  },
  {} as AppData
);

const DEFAULT_TITLES = CATEGORY_CONFIG.reduce<Record<string, string>>(
  (acc, section) => {
    acc[section.key] = section.title;
    return acc;
  },
  {}
);

const ADD_LABELS = CATEGORY_CONFIG.reduce<Record<string, string>>(
  (acc, section) => {
    acc[section.key] = section.addLabel;
    return acc;
  },
  {}
);

const DEFAULT_ORDER = CATEGORY_CONFIG.map((section) => section.key);
const PARAM_TYPES_KEY: DataCategory = "paramTypes";
const PARAM_INSTANCES_KEY: DataCategory = "paramInstances";

interface TypeModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  index: number | null;
  initialValue: ParameterType;
  revision: number;
}

interface ParameterInstanceModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  index: number | null;
  initialValue: ParameterInstance;
  revision: number;
}

const createEmptyStructuredItem = (): StructuredItem => ({
  id: createId("item"),
  name: "",
  type: "",
});

const createEmptyParameterType = (): ParameterType => ({
  ...createEmptyStructuredItem(),
  id: createId("param-type"),
  properties: [],
});

const cloneParameterType = (entry: ParameterType): ParameterType => ({
  ...entry,
  properties: entry.properties.map((property) => ({ ...property })),
});

const createEmptyParameterInstance = (
  parameterType?: ParameterType
): ParameterInstance => ({
  ...createEmptyStructuredItem(),
  id: createId("param-instance"),
  type: parameterType?.name ?? "",
  typeId: parameterType?.id ?? "",
  propertyValues: parameterType
    ? parameterType.properties.reduce<Record<string, string>>((acc, property) => {
        acc[property.id] = "";
        return acc;
      }, {})
    : {},
});

const cloneParameterInstance = (
  entry: ParameterInstance
): ParameterInstance => ({
  ...entry,
  propertyValues: { ...entry.propertyValues },
});

const reconcileInstanceValues = (
  parameterType: ParameterType,
  currentValues: Record<string, string>
): Record<string, string> =>
  parameterType.properties.reduce<Record<string, string>>((acc, property) => {
    acc[property.id] = currentValues[property.id] ?? "";
    return acc;
  }, {});

function SidebarSection({
  title,
  children,
  isOpen = false,
  iconLabel,
  onEdit,
  onDelete,
  disableDelete = false,
}: SectionProps) {
  const [open, setOpen] = useState(isOpen);
  const displayIcon = iconLabel ?? title.charAt(0).toUpperCase();

  const handleToggle = () => setOpen((prev) => !prev);
  const deleteTitle = disableDelete
    ? "Default sections cannot be removed"
    : "Delete section";

  return (
    <div className="sidebar-section">
      <div className="sidebar-section-header">
        <button
          type="button"
          className="sidebar-section-toggle"
          onClick={handleToggle}
          aria-expanded={open}
          aria-label={`${open ? "Collapse" : "Expand"} ${title}`}
        >
          <div className="sidebar-section-header-left">
            <span className="section-icon" aria-hidden>
              {displayIcon}
            </span>
            <strong>{title}</strong>
          </div>
          <span className="toggle-icon" aria-hidden>
            {open ? "▲" : "▼"}
          </span>
        </button>

        {(onEdit || onDelete) && (
          <div className="sidebar-section-actions">
            {onEdit && (
              <button
                type="button"
                className="section-action-btn"
                onClick={onEdit}
                title="Rename section"
                aria-label={`Rename section ${title}`}
              >
                ✎
              </button>
            )}
            {onDelete && (
              <button
                type="button"
                className="section-action-btn delete-action"
                onClick={onDelete}
                title={deleteTitle}
                aria-label={deleteTitle}
                disabled={disableDelete}
              >
                ×
              </button>
            )}
          </div>
        )}
      </div>
      {open && <div className="sidebar-content">{children}</div>}
    </div>
  );
}

export default function Sidebar() {
  const [data, setData] = useState<AppData>(() => ({ ...DEFAULT_DATA }));
  const typeModalRevision = useRef(0);
  const instanceModalRevision = useRef(0);

  const [categoryTitles, setCategoryTitles] = useState<Record<string, string>>(
    () => ({ ...DEFAULT_TITLES })
  );
  const [categoryOrder, setCategoryOrder] = useState<string[]>(() => [
    ...DEFAULT_ORDER,
  ]);
  const [categoryModalOpen, setCategoryModalOpen] = useState(false);
  const [categoryModalMode, setCategoryModalMode] = useState<"add" | "edit">(
    "add"
  );
  const [activeCategoryKey, setActiveCategoryKey] =
    useState<DataCategory | null>(null);
  const [categoryModalValue, setCategoryModalValue] = useState<StructuredItem>(
    () => createEmptyStructuredItem()
  );
  const [searchQueries, setSearchQueries] = useState<Record<string, string>>(
    () => {
      const initial: Record<string, string> = {};
      DEFAULT_ORDER.forEach((key) => {
        initial[key] = "";
      });
      return initial;
    }
  );

  function nextTypeModalRevision() {
    typeModalRevision.current += 1;
    return typeModalRevision.current;
  }

  function nextInstanceModalRevision() {
    instanceModalRevision.current += 1;
    return instanceModalRevision.current;
  }

  const [typeModalState, setTypeModalState] = useState<TypeModalState>(() => ({
    isOpen: false,
    mode: "add",
    index: null,
    initialValue: createEmptyParameterType(),
    revision: 0,
  }));

  const [instanceModalState, setInstanceModalState] = useState<ParameterInstanceModalState>(
    () => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterInstance(),
      revision: 0,
    })
  );

  const [modalState, setModalState] = useState<ModalState>(() => ({
    isOpen: false,
    mode: "add",
    category: null,
    index: null,
    initialValue: createEmptyStructuredItem(),
  }));

  const parameterTypes = useMemo(() => {
    const entries = data[PARAM_TYPES_KEY] as ParameterType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);
  const parameterTypeMap = useMemo(() => {
    const map = new Map<string, ParameterType>();
    parameterTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [parameterTypes]);

  /**
   * Opens the item creation modal for the given category.
   * Initializes the modal with empty default values.
   *
   * @param category The category to which a new item will be added.
   */
  const openAddModal = (category: DataCategory) => {
    if (category === PARAM_TYPES_KEY) {
      setTypeModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyParameterType(),
        revision: nextTypeModalRevision(),
      });
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      if (parameterTypes.length === 0) {
        window.alert("Create a parameter type before adding instances.");
        return;
      }

      const firstType = parameterTypes[0];
      setInstanceModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyParameterInstance(firstType),
        revision: nextInstanceModalRevision(),
      });
      return;
    }

    setModalState({
      isOpen: true,
      mode: "add",
      category,
      index: null,
      initialValue: createEmptyStructuredItem(),
    });
  };

  const handleSearchChange = (category: DataCategory, value: string) => {
    setSearchQueries((prev) => ({ ...prev, [category]: value }));
  };

  /**
   * Opens the item edit modal for a specific entry within a category.
   * Preloads the modal with the item's current values.
   *
   * @param category The category containing the item.
   * @param index Index of the item in the category list.
   * @param currentValue The current item data to prefill the modal.
   */
  const openEditParameterType = (
    index: number,
    currentValue: ParameterType
  ) => {
    setTypeModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: cloneParameterType(currentValue),
      revision: nextTypeModalRevision(),
    });
  };

  const openEditParameterInstance = (
    index: number,
    currentValue: ParameterInstance
  ) => {
    const parameterType = parameterTypeMap.get(currentValue.typeId);
    const initialEntry = cloneParameterInstance(currentValue);

    if (parameterType) {
      initialEntry.type = parameterType.name;
      initialEntry.propertyValues = reconcileInstanceValues(
        parameterType,
        currentValue.propertyValues
      );
    }

    setInstanceModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: initialEntry,
      revision: nextInstanceModalRevision(),
    });
  };

  const openEditModal = (
    category: DataCategory,
    index: number,
    currentValue: StructuredItem
  ) => {
    if (category === PARAM_TYPES_KEY) {
      openEditParameterType(index, currentValue as ParameterType);
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      openEditParameterInstance(index, currentValue as ParameterInstance);
      return;
    }

    setModalState({
      isOpen: true,
      mode: "edit",
      category,
      index,
      initialValue: { ...currentValue },
    });
  };

  /**
   * Closes the item modal without applying changes.
   */
  const closeModal = () => {
    setModalState((prev) => ({ ...prev, isOpen: false }));
  };

  /**
   * Saves data submitted from the item modal.
   * Handles both creation and edit operations.
   *
   * @param value The item data entered in the modal.
   */
  const handleSaveFromModal = (value: StructuredItem) => {
    const categoryKey = modalState.category;
    if (!categoryKey) return;

    setData((prev) => {
      const existingItems = prev[categoryKey] ?? [];
      const nextItems = [...existingItems];
      const normalized: StructuredItem = {
        ...value,
        id: value.id || createId("item"),
      };

      if (modalState.mode === "add") {
        nextItems.push(normalized);
      } else if (modalState.mode === "edit" && modalState.index !== null) {
        nextItems[modalState.index] = normalized;
      }

      return { ...prev, [categoryKey]: nextItems };
    });

    closeModal();
  };

  const closeTypeModal = () => {
    setTypeModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterType(),
      revision: nextTypeModalRevision(),
    });
  };

  const closeInstanceModal = () => {
    setInstanceModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterInstance(),
      revision: nextInstanceModalRevision(),
    });
  };

  const handleSaveParameterType = (value: ParameterType) => {
    const normalized: ParameterType = {
      ...value,
      id: value.id || createId("param-type"),
      name: value.name.trim(),
      type: value.type.trim(),
      properties: value.properties.map((property) => ({
        ...property,
        id: property.id || createId("property"),
        name: property.name.trim(),
        valueType: property.valueType.trim(),
      })),
    };

    setData((prev) => {
      const existingTypes =
        (prev[PARAM_TYPES_KEY] as ParameterType[] | undefined) ?? [];
      const nextTypes = [...existingTypes];

      if (typeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (typeModalState.mode === "edit" && typeModalState.index !== null) {
        nextTypes[typeModalState.index] = normalized;
      }

      const existingInstances =
        (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
      const nextInstances = existingInstances.map((instance) => {
        if (instance.typeId !== normalized.id) {
          return instance;
        }

        return {
          ...instance,
          type: normalized.name,
          propertyValues: reconcileInstanceValues(
            normalized,
            instance.propertyValues
          ),
        };
      });

      return {
        ...prev,
        [PARAM_TYPES_KEY]: nextTypes,
        [PARAM_INSTANCES_KEY]: nextInstances,
      };
    });

    closeTypeModal();
  };

  const handleSaveParameterInstance = (value: ParameterInstance) => {
    const parameterType = parameterTypeMap.get(value.typeId);
    if (!parameterType) {
      window.alert("Select a valid parameter type.");
      return;
    }

    const sanitizedValues = parameterType.properties.reduce<Record<string, string>>(
      (acc, property) => {
        const rawValue = value.propertyValues?.[property.id] ?? "";
        acc[property.id] = rawValue.trim();
        return acc;
      },
      {}
    );

    const normalized: ParameterInstance = {
      ...value,
      id: value.id || createId("param-instance"),
      name: value.name.trim(),
      type: parameterType.name,
      typeId: parameterType.id,
      propertyValues: sanitizedValues,
    };

    setData((prev) => {
      const existing =
        (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
      const next = [...existing];

      if (instanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        instanceModalState.mode === "edit" &&
        instanceModalState.index !== null
      ) {
        next[instanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [PARAM_INSTANCES_KEY]: next,
      };
    });

    closeInstanceModal();
  };

  /**
   * Deletes an item from a category after user confirmation.
   *
   * @param category The category from which the item will be removed.
   * @param index Index of the item to delete.
   */
  const handleDeleteItem = (category: DataCategory, index: number) => {
    if (!window.confirm("Delete this item?")) {
      return;
    }

    const removedEntry = (data[category] ?? [])[index];

    setData((prev) => {
      const existingItems = prev[category] ?? [];
      const nextItems = existingItems.filter((_, i) => i !== index);
      const nextData: AppData = { ...prev, [category]: nextItems };

      if (category === PARAM_TYPES_KEY && removedEntry) {
        const removedType = removedEntry as ParameterType;
        const existingInstances =
          (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
        nextData[PARAM_INSTANCES_KEY] = existingInstances.filter(
          (instance) => instance.typeId !== removedType.id
        );
      }

      return nextData;
    });

    if (
      category === PARAM_INSTANCES_KEY &&
      instanceModalState.isOpen &&
      instanceModalState.index === index
    ) {
      closeInstanceModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      typeModalState.isOpen &&
      typeModalState.index === index
    ) {
      closeTypeModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      removedEntry &&
      instanceModalState.isOpen &&
      instanceModalState.initialValue.typeId ===
        (removedEntry as ParameterType).id
    ) {
      closeInstanceModal();
    }
  };

  /**
   * Renders the list of items for a given category, including edit/delete controls.
   *
   * @param category The category whose items should be rendered.
   * @returns A JSX structure containing the item list.
   */
  const renderList = (category: DataCategory) => {
    const isParameterInstanceCategory = category === PARAM_INSTANCES_KEY;
    const query = (searchQueries[category] ?? "").trim().toLowerCase();
    const rawItems = data[category] ?? [];

    const matchesSearch = (item: StructuredItem) => {
      if (!query) {
        return true;
      }

      const base = `${item.name} ${item.type}`.toLowerCase();
      if (base.includes(query)) {
        return true;
      }

      if (category === PARAM_TYPES_KEY) {
        const typeItem = item as ParameterType;
        return typeItem.properties.some((property) =>
          `${property.name} ${property.valueType}`.toLowerCase().includes(query)
        );
      }

      if (category === PARAM_INSTANCES_KEY) {
        const instance = item as ParameterInstance;
        const linkedType = parameterTypeMap.get(instance.typeId);
        const typeName = (linkedType?.name ?? item.type).toLowerCase();
        if (typeName.includes(query)) {
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
            return combined.includes(query);
          }
        );
      }

      return false;
    };

    const items = query ? rawItems.filter(matchesSearch) : rawItems;

    return (
      <div className="item-list-container">
        {items.map((item, index) => {
          const badgeLabel = isParameterInstanceCategory
            ? (parameterTypeMap.get((item as ParameterInstance).typeId)?.name || item.type)
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
                  onClick={() => openEditModal(category, index, item)}
                  title="Edit"
                  type="button"
                >
                  ✎
                </button>
                <button
                  className="icon-btn delete-btn"
                  onClick={() => handleDeleteItem(category, index)}
                  title="Delete"
                  type="button"
                >
                  ×
                </button>
              </div>
            </div>
          );
        })}

        {items.length === 0 && (
          <p className="empty-copy">
            {query
              ? "No matching items."
              : isParameterInstanceCategory && parameterTypes.length === 0
              ? "Create a parameter type before adding instances."
              : "No items defined."}
          </p>
        )}
      </div>
    );
  };

  /**
   * Opens the modal for creating a new category/section.
   */
  const openCategoryModal = () => {
    setCategoryModalMode("add");
    setActiveCategoryKey(null);
    setCategoryModalValue(createEmptyStructuredItem());
    setCategoryModalOpen(true);
  };

  /**
   * Opens the modal to rename an existing category.
   *
   * @param categoryKey The key of the category to rename.
   */
  const openRenameCategoryModal = (categoryKey: DataCategory) => {
    const currentTitle = categoryTitles[categoryKey] ?? categoryKey;
    setCategoryModalMode("edit");
    setActiveCategoryKey(categoryKey);
    setCategoryModalValue({
      ...createEmptyStructuredItem(),
      name: currentTitle,
    });
    setCategoryModalOpen(true);
  };

  /**
   * Closes the category modal and resets all temporary modal state.
   */
  const closeCategoryModal = () => {
    setCategoryModalOpen(false);
    setCategoryModalMode("add");
    setActiveCategoryKey(null);
    setCategoryModalValue(createEmptyStructuredItem());
  };

  /**
   * Generates a unique category key based on a display label.
   * Ensures that the resulting key does not collide with existing ones.
   *
   * @param label The display name from which the base key is derived.
   * @returns A unique, sanitized category identifier.
   */
  const createCategoryKey = (label: string) => {
    const baseKey = label
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-+|-+$/g, "");

    const candidateBase = baseKey || "category";
    let candidate = candidateBase;
    let suffix = 1;
    const existingKeys = new Set([...categoryOrder, ...Object.keys(data)]);

    while (existingKeys.has(candidate)) {
      candidate = `${candidateBase}-${suffix++}`;
    }

    return candidate;
  };

  /**
   * Handles saving a new or renamed category.
   * Creates a new category when in "add" mode, or updates the title when in "edit" mode.
   *
   * @param value Structured input containing the category name.
   */
  const handleSaveCategory = (value: StructuredItem) => {
    const displayName = value.name.trim();
    if (!displayName) {
      return;
    }

    if (categoryModalMode === "add") {
      const newKey = createCategoryKey(displayName);
      setData((prev) => ({ ...prev, [newKey]: [] }));
      setCategoryTitles((prev) => ({ ...prev, [newKey]: displayName }));
      setCategoryOrder((prev) => [...prev, newKey]);
      setSearchQueries((prev) => ({ ...prev, [newKey]: "" }));
    } else if (categoryModalMode === "edit" && activeCategoryKey) {
      setCategoryTitles((prev) => ({
        ...prev,
        [activeCategoryKey]: displayName,
      }));
    }

    closeCategoryModal();
  };

  /**
   * Deletes an entire category/section along with all contained items.
   * Requests user confirmation before proceeding.
   *
   * @param categoryKey The key of the category to delete.
   */
  const handleDeleteCategory = (categoryKey: DataCategory) => {
    const displayName = categoryTitles[categoryKey] ?? categoryKey;
    if (
      !window.confirm(`Delete section "${displayName}" and all of its items?`)
    ) {
      return;
    }

    setCategoryOrder((prev) => prev.filter((key) => key !== categoryKey));
    setCategoryTitles((prev) => {
      const nextTitles = { ...prev };
      delete nextTitles[categoryKey];
      return nextTitles;
    });
    setData((prev) => {
      const nextData: AppData = { ...prev };
      delete nextData[categoryKey];
      return nextData;
    });
    setSearchQueries((prev) => {
      const nextQueries = { ...prev };
      delete nextQueries[categoryKey];
      return nextQueries;
    });
    setModalState((prev) =>
      prev.category === categoryKey
        ? {
            isOpen: false,
            mode: "add",
            category: null,
            index: null,
            initialValue: createEmptyStructuredItem(),
          }
        : prev
    );

    if (categoryKey === PARAM_TYPES_KEY) {
      closeTypeModal();
    }

    if (categoryKey === PARAM_INSTANCES_KEY) {
      closeInstanceModal();
    }

    if (activeCategoryKey === categoryKey) {
      closeCategoryModal();
    }
  };

  return (
    <div className="sidebar">
      <div className="sidebar-title">
        <span className="sidebar-title-text">AI Planner</span>
        <button
          className="add-category-button"
          onClick={openCategoryModal}
          type="button"
        >
          + Add Section
        </button>
      </div>

      <TypeDefinitionModal
        key={`${typeModalState.mode}-${typeModalState.index ?? "new"}-${typeModalState.initialValue.id}-${typeModalState.revision}`}
        isOpen={typeModalState.isOpen}
        mode={typeModalState.mode}
        title={
          typeModalState.mode === "add"
            ? "Add Parameter Type"
            : "Edit Parameter Type"
        }
        initialValue={typeModalState.initialValue}
        onClose={closeTypeModal}
        onSave={handleSaveParameterType}
      />

      <ParameterInstanceModal
        key={`${instanceModalState.mode}-${instanceModalState.index ?? "new"}-${instanceModalState.initialValue.id}-${instanceModalState.revision}`}
        isOpen={instanceModalState.isOpen}
        mode={instanceModalState.mode}
        title={
          instanceModalState.mode === "add"
            ? "Add Parameter Instance"
            : "Edit Parameter Instance"
        }
        initialValue={instanceModalState.initialValue}
        parameterTypes={parameterTypes}
        onClose={closeInstanceModal}
        onSave={handleSaveParameterInstance}
      />

      <EditModal
        key={`${modalState.mode}-${modalState.index}`}
        isOpen={modalState.isOpen}
        title={modalState.mode === "add" ? "Add Item" : "Edit Item"}
        initialValue={modalState.initialValue}
        onClose={closeModal}
        onSave={handleSaveFromModal}
        nameLabel="Display Name"
        namePlaceholder="e.g., target_entity"
      />

      <EditModal
        key={`${categoryModalMode}-${activeCategoryKey ?? "new"}`}
        isOpen={categoryModalOpen}
        title={categoryModalMode === "add" ? "Add Section" : "Rename Section"}
        initialValue={categoryModalValue}
        onClose={closeCategoryModal}
        onSave={handleSaveCategory}
        hideTypeField
        nameLabel="Section Title"
        namePlaceholder="e.g., Sensors"
        helperText={
          categoryModalMode === "add"
            ? "Sections group related planner data. Use a short, descriptive title."
            : undefined
        }
        saveLabel={categoryModalMode === "add" ? "Create Section" : "Save"}
      />

      {categoryOrder.map((categoryKey, index) => {
        const displayTitle = categoryTitles[categoryKey] ?? categoryKey;
        const iconLabel = displayTitle.charAt(0).toUpperCase();
        const buttonLabel = ADD_LABELS[categoryKey] ?? "Add Item";
        const searchQuery = searchQueries[categoryKey] ?? "";

        return (
          <SidebarSection
            key={categoryKey}
            title={displayTitle}
            isOpen={index === 0}
            iconLabel={iconLabel}
            onEdit={() => openRenameCategoryModal(categoryKey)}
            onDelete={() => handleDeleteCategory(categoryKey)}
          >
            <button
              className="add-button"
              onClick={() => openAddModal(categoryKey)}
              type="button"
            >
              + {buttonLabel}
            </button>
            <div className="section-search">
              <input
                type="search"
                className="section-search-input"
                value={searchQuery}
                onChange={(event) =>
                  handleSearchChange(categoryKey, event.target.value)
                }
                placeholder="Search..."
                aria-label={`Search ${displayTitle}`}
              />
            </div>
            {renderList(categoryKey)}
          </SidebarSection>
        );
      })}
    </div>
  );
}
