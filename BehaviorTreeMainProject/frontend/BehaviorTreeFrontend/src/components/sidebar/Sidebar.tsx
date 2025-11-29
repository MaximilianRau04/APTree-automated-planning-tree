import EditModal from "./modals/EditModal";
import ParameterInstanceModal from "./modals/ParameterInstanceModal";
import TypeDefinitionModal from "./modals/TypeDefinitionModal";
import "./Sidebar.css";
import { CategoryItemList } from "./components/CategoryItemList";
import SidebarSection from "./components/SidebarSection";
import { useSidebarManager } from "./useSidebarLogic";

/**
 * assembles the full planner sidebar, wiring state-driven modals and section content together.
 * @returns sidebar layout containing sections, search inputs, and supporting modals
 */
export default function Sidebar() {
  const {
    addLabelFor,
    categoryModal,
    categoryOrder,
    categoryTitles,
    closeCategoryModal,
    closeInstanceModal,
    closeModal,
    closeTypeModal,
    getItemsForCategory,
    handleDeleteCategory,
    handleDeleteItem,
    handleSaveCategory,
    handleSaveFromModal,
    handleSaveParameterInstance,
    handleSaveParameterType,
    handleSearchChange,
    instanceModalState,
    modalState,
    openAddModal,
    openCategoryModal,
    openEditModal,
    openRenameCategoryModal,
    parameterTypeMap,
    parameterTypes,
    searchQueries,
    typeModalState,
  } = useSidebarManager();

  const categoryModalTitle =
    categoryModal.mode === "add" ? "Add Section" : "Rename Section";
  const categoryModalHelper =
    categoryModal.mode === "add"
      ? "Sections group related planner data. Use a short, descriptive title."
      : undefined;
  const categoryModalSaveLabel =
    categoryModal.mode === "add" ? "Create Section" : "Save";

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
        key={`${typeModalState.mode}-${typeModalState.index ?? "new"}-${
          typeModalState.initialValue.id
        }-${typeModalState.revision}`}
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
        key={`${instanceModalState.mode}-${instanceModalState.index ?? "new"}-${
          instanceModalState.initialValue.id
        }-${instanceModalState.revision}`}
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
        key={`${categoryModal.mode}-${categoryModal.activeKey ?? "new"}`}
        isOpen={categoryModal.isOpen}
        title={categoryModalTitle}
        initialValue={categoryModal.value}
        onClose={closeCategoryModal}
        onSave={handleSaveCategory}
        hideTypeField
        nameLabel="Section Title"
        namePlaceholder="e.g., Sensors"
        helperText={categoryModalHelper}
        saveLabel={categoryModalSaveLabel}
      />

      {categoryOrder.map((categoryKey, index) => {
        const displayTitle = categoryTitles[categoryKey] ?? categoryKey;
        const iconLabel = displayTitle.charAt(0).toUpperCase();
        const buttonLabel = addLabelFor(categoryKey);
        const searchQuery = searchQueries[categoryKey] ?? "";
        const items = getItemsForCategory(categoryKey);

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
            <CategoryItemList
              category={categoryKey}
              items={items}
              parameterTypes={parameterTypes}
              parameterTypeMap={parameterTypeMap}
              searchQuery={searchQuery}
              onEdit={openEditModal}
              onDelete={handleDeleteItem}
            />
          </SidebarSection>
        );
      })}
    </div>
  );
}
