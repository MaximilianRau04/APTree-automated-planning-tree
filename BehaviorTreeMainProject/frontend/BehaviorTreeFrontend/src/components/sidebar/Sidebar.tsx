import { useState } from "react";
import EditModal from "./EditModal";
import type { StructuredItem } from "./types.ts";
import "./Sidebar.css";
import type {
  AppData,
  DataCategory,
  ModalState,
  SectionProps,
} from "./types.ts";

interface CategoryConfig {
  key: DataCategory;
  title: string;
  addLabel: string;
  defaultItems?: StructuredItem[];
}

const CATEGORY_CONFIG: CategoryConfig[] = [
  {
    key: "variables",
    title: "Blackboard Variables",
    addLabel: "Add Variable",
    defaultItems: [
      { name: "health", type: "Integer" },
      { name: "target", type: "Agent" },
    ],
  },
  { key: "paramTypes", title: "Parameter Types", addLabel: "Add Parameter Type" },
  {
    key: "paramInstances",
    title: "Parameter Instances",
    addLabel: "Add Parameter Instance",
  },
  { key: "predTypes", title: "Predicate Types", addLabel: "Add Predicate Type" },
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

const DEFAULT_DATA: AppData = CATEGORY_CONFIG.reduce<AppData>((acc, section) => {
  acc[section.key] = section.defaultItems ? [...section.defaultItems] : [];
  return acc;
}, {} as AppData);

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
const BASE_SECTION_KEYS = new Set(DEFAULT_ORDER);

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
  const [activeCategoryKey, setActiveCategoryKey] = useState<DataCategory | null>(
    null
  );
  const [categoryModalValue, setCategoryModalValue] = useState<StructuredItem>({
    name: "",
    type: "",
  });

  const [modalState, setModalState] = useState<ModalState>({
    isOpen: false,
    mode: "add",
    category: null,
    index: null,
    initialValue: { name: "", type: "" },
  });

  const openAddModal = (category: DataCategory) => {
    setModalState({
      isOpen: true,
      mode: "add",
      category,
      index: null,
      initialValue: { name: "", type: "" },
    });
  };

  const openEditModal = (
    category: DataCategory,
    index: number,
    currentValue: StructuredItem
  ) => {
    setModalState({
      isOpen: true,
      mode: "edit",
      category,
      index,
      initialValue: currentValue,
    });
  };

  const closeModal = () => {
    setModalState((prev) => ({ ...prev, isOpen: false }));
  };

  const handleSaveFromModal = (value: StructuredItem) => {
    const categoryKey = modalState.category;
    if (!categoryKey) return;

    setData((prev) => {
      const existingItems = prev[categoryKey] ?? [];
      const nextItems = [...existingItems];

      if (modalState.mode === "add") {
        nextItems.push(value);
      } else if (modalState.mode === "edit" && modalState.index !== null) {
        nextItems[modalState.index] = value;
      }

      return { ...prev, [categoryKey]: nextItems };
    });

    closeModal();
  };

  const handleDeleteItem = (category: DataCategory, index: number) => {
    if (!window.confirm("Delete this item?")) {
      return;
    }

    setData((prev) => ({
      ...prev,
      [category]: (prev[category] ?? []).filter((_, i) => i !== index),
    }));
  };

  const renderList = (category: DataCategory) => {
    const items = data[category] ?? [];

    return (
      <div className="item-list-container">
        {items.map((item, index) => (
          <div key={`${item.name}-${index}`} className="list-item-box">
            <span className="list-item-text">
              {item.name}
              <span className="list-item-separator" aria-hidden>
                :
              </span>
              <span className="type-badge">{item.type}</span>
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
        ))}

        {items.length === 0 && <p className="empty-copy">No items defined.</p>}
      </div>
    );
  };

  const openCategoryModal = () => {
    setCategoryModalMode("add");
    setActiveCategoryKey(null);
    setCategoryModalValue({ name: "", type: "" });
    setCategoryModalOpen(true);
  };

  const openRenameCategoryModal = (categoryKey: DataCategory) => {
    const currentTitle = categoryTitles[categoryKey] ?? categoryKey;
    setCategoryModalMode("edit");
    setActiveCategoryKey(categoryKey);
    setCategoryModalValue({ name: currentTitle, type: "" });
    setCategoryModalOpen(true);
  };

  const closeCategoryModal = () => {
    setCategoryModalOpen(false);
    setCategoryModalMode("add");
    setActiveCategoryKey(null);
    setCategoryModalValue({ name: "", type: "" });
  };

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
    } else if (categoryModalMode === "edit" && activeCategoryKey) {
      setCategoryTitles((prev) => ({
        ...prev,
        [activeCategoryKey]: displayName,
      }));
    }

    closeCategoryModal();
  };

  const handleDeleteCategory = (categoryKey: DataCategory) => {
    const displayName = categoryTitles[categoryKey] ?? categoryKey;
    if (
      !window.confirm(
        `Delete section "${displayName}" and all of its items?`
      )
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
    setModalState((prev) =>
      prev.category === categoryKey
        ? {
            isOpen: false,
            mode: "add",
            category: null,
            index: null,
            initialValue: { name: "", type: "" },
          }
        : prev
    );

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
        title={
          categoryModalMode === "add" ? "Add Section" : "Rename Section"
        }
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
        const disableDelete = BASE_SECTION_KEYS.has(categoryKey);

        return (
          <SidebarSection
            key={categoryKey}
            title={displayTitle}
            isOpen={index === 0}
            iconLabel={iconLabel}
            onEdit={() => openRenameCategoryModal(categoryKey)}
            onDelete={() => handleDeleteCategory(categoryKey)}
            disableDelete={disableDelete}
          >
            <button
              className="add-button"
              onClick={() => openAddModal(categoryKey)}
              type="button"
            >
              + {buttonLabel}
            </button>
            {renderList(categoryKey)}
          </SidebarSection>
        );
      })}
    </div>
  );
}
