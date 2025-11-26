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

/**
 * Sidebar Section Component
 * @param param0
 * @returns The Sidebar Section TSX element
 */
function SidebarSection({ title, children, isOpen = false }: SectionProps) {
  const [open, setOpen] = useState(isOpen);

  return (
    <div className="sidebar-section">
      <div className="sidebar-section-header" onClick={() => setOpen(!open)}>
        <strong>{title}</strong>
        <span className="toggle-icon">{open ? "▲" : "▼"}</span>
      </div>
      {open && <div className="sidebar-content">{children}</div>}
    </div>
  );
}

export default function Sidebar() {
  const [data, setData] = useState<AppData>({
    variables: [
      { name: "health", type: "Integer" },
      { name: "target", type: "Agent" },
    ],
    paramTypes: [],
    paramInstances: [],
    predTypes: [],
    predInstances: [],
    actions: [],
    actionInstances: [],
    nodes: [],
  });

  // modal state
  const [modalState, setModalState] = useState<ModalState>({
    isOpen: false,
    mode: "add",
    category: null,
    index: null,
    initialValue: { name: "", type: "" },
  });

  /**
   * opens the add modal for a specific category
   * @param category
   */
  const openAddModal = (category: DataCategory) => {
    setModalState({
      isOpen: true,
      mode: "add",
      category,
      index: null,
      initialValue: { name: "", type: "String" },
    });
  };

  /**
   * opens the edit modal for a specific category and index
   * @param category
   * @param index
   * @param currentValue
   */
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

  /**
   * handles saving from the modal
   * @param value The new or updated StructuredItem to save
   * @returns modified data state
   */
  const handleSaveFromModal = (value: StructuredItem) => {
    if (!modalState.category) return;

    setData((prev) => {
      const categoryList = [...prev[modalState.category!]];

      if (modalState.mode === "add") {
        categoryList.push(value);
      } else if (modalState.mode === "edit" && modalState.index !== null) {
        categoryList[modalState.index] = value;
      }

      return { ...prev, [modalState.category!]: categoryList };
    });

    closeModal();
  };

  /**
   * handles deleting an item
   * @param category The category from which to delete an item
   * @param index The index of the item to delete
   */
  const handleDelete = (category: DataCategory, index: number) => {
    if (window.confirm("Delete this item?")) {
      setData((prev) => ({
        ...prev,
        [category]: prev[category].filter((_, i) => i !== index),
      }));
    }
  };

  const renderList = (category: DataCategory) => (
    <div className="item-list-container">
      {data[category].map((item, index) => (
        <div key={index} className="list-item-box">
          <span className="list-item-text">
            {item.name} <span style={{ color: "#888" }}>:</span>{" "}
            <span className="type-badge">{item.type}</span>
          </span>

          <div className="list-item-actions">
            <button
              className="icon-btn edit-btn"
              onClick={() => openEditModal(category, index, item)}
              title="Edit"
            >
              ✎
            </button>
            <button
              className="icon-btn delete-btn"
              onClick={() => handleDelete(category, index)}
              title="Delete"
            >
              ×
            </button>
          </div>
        </div>
      ))}
      {data[category].length === 0 && (
        <p
          style={{
            opacity: 0.5,
            padding: "5px 8px",
            fontSize: "0.85em",
            fontStyle: "italic",
            margin: 0,
          }}
        >
          No items defined.
        </p>
      )}
    </div>
  );

  return (
    <div className="sidebar">
      <h2 className="sidebar-title">AI Planner</h2>

      <EditModal
        key={`${modalState.mode}-${modalState.index}`}
        isOpen={modalState.isOpen}
        title={modalState.mode === "add" ? "Add Item" : "Edit Item"}
        initialValue={modalState.initialValue}
        onClose={closeModal}
        onSave={handleSaveFromModal}
      />

      <SidebarSection title="Blackboard Variables" isOpen={true}>
        <button
          className="add-button"
          onClick={() => openAddModal("variables")}
        >
          + Add Variable
        </button>
        {renderList("variables")}
      </SidebarSection>

      <SidebarSection title="Parameter Types">
        <button
          className="add-button"
          onClick={() => openAddModal("paramTypes")}
        >
          + Add Parameter Type
        </button>
        {renderList("paramTypes")}
      </SidebarSection>

      <SidebarSection title="Parameter Instances">
        <button
          className="add-button"
          onClick={() => openAddModal("paramInstances")}
        >
          + Add Parameter Instance
        </button>
        {renderList("paramInstances")}
      </SidebarSection>

      <SidebarSection title="Predicate Types">
        <button
          className="add-button"
          onClick={() => openAddModal("predTypes")}
        >
          + Add Predicate Type
        </button>
        {renderList("predTypes")}
      </SidebarSection>

      <SidebarSection title="Predicate Instances">
        <button
          className="add-button"
          onClick={() => openAddModal("predInstances")}
        >
          + Add Predicate Instance
        </button>
        {renderList("predInstances")}
      </SidebarSection>

      <SidebarSection title="Actions">
        <button className="add-button" onClick={() => openAddModal("actions")}>
          + Add Action
        </button>
        {renderList("actions")}
      </SidebarSection>

      <SidebarSection title="Action Instances">
        <button
          className="add-button"
          onClick={() => openAddModal("actionInstances")}
        >
          + Add Action Instance
        </button>
        {renderList("actionInstances")}
      </SidebarSection>

      <SidebarSection title="Behavior Tree Nodes">
        <button className="add-button" onClick={() => openAddModal("nodes")}>
          + Add Node
        </button>
        {renderList("nodes")}
      </SidebarSection>
    </div>
  );
}
