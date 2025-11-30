import EditModal from "./modals/EditModal";
import ParameterInstanceModal, {
  ActionInstanceModal,
  PredicateInstanceModal,
} from "./modals/InstanceModal";
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
    closeParameterInstanceModal,
    closePredicateInstanceModal,
    closeActionInstanceModal,
    closeModal,
    closeParameterTypeModal,
    closePredicateTypeModal,
    closeActionTypeModal,
    getItemsForCategory,
    handleDeleteCategory,
    handleDeleteItem,
    handleSaveCategory,
    handleSaveFromModal,
    handleSaveParameterInstance,
    handleSavePredicateInstance,
    handleSaveActionInstance,
    handleSaveParameterType,
    handleSavePredicateType,
    handleSaveActionType,
    handleSearchChange,
    parameterInstanceModalState,
    predicateInstanceModalState,
    actionInstanceModalState,
    modalState,
    openAddModal,
    openCategoryModal,
    openEditModal,
    openRenameCategoryModal,
    parameterTypeMap,
    parameterTypes,
    predicateTypeMap,
    predicateTypes,
    actionTypeMap,
    actionTypes,
    searchQueries,
    parameterTypeModalState,
    predicateTypeModalState,
    actionTypeModalState,
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
        key={`${parameterTypeModalState.mode}-${
          parameterTypeModalState.index ?? "new"
        }-${parameterTypeModalState.initialValue.id}-${
          parameterTypeModalState.revision
        }`}
        isOpen={parameterTypeModalState.isOpen}
        mode={parameterTypeModalState.mode}
        title={
          parameterTypeModalState.mode === "add"
            ? "Add Parameter Type"
            : "Edit Parameter Type"
        }
        initialValue={parameterTypeModalState.initialValue}
        onClose={closeParameterTypeModal}
        onSave={handleSaveParameterType}
      />

      <TypeDefinitionModal
        key={`${predicateTypeModalState.mode}-${
          predicateTypeModalState.index ?? "new"
        }-${predicateTypeModalState.initialValue.id}-${
          predicateTypeModalState.revision
        }`}
        isOpen={predicateTypeModalState.isOpen}
        mode={predicateTypeModalState.mode}
        title={
          predicateTypeModalState.mode === "add"
            ? "Add Predicate Type"
            : "Edit Predicate Type"
        }
        initialValue={predicateTypeModalState.initialValue}
        onClose={closePredicateTypeModal}
        onSave={handleSavePredicateType}
        nameLabel="Predicate Type Name"
        namePlaceholder="e.g., is_reachable"
        baseTypeLabel="Predicate Base Type"
        baseTypePlaceholder="Select a base type..."
        propertyLabel="Predicate Properties"
        propertyNamePlaceholder="e.g., target_location"
        propertyTypePlaceholder="e.g., string"
      />

      <TypeDefinitionModal
        key={`${actionTypeModalState.mode}-${
          actionTypeModalState.index ?? "new"
        }-${actionTypeModalState.initialValue.id}-${
          actionTypeModalState.revision
        }`}
        isOpen={actionTypeModalState.isOpen}
        mode={actionTypeModalState.mode}
        title={
          actionTypeModalState.mode === "add"
            ? "Add Action Type"
            : "Edit Action Type"
        }
        initialValue={actionTypeModalState.initialValue}
        onClose={closeActionTypeModal}
        onSave={handleSaveActionType}
        nameLabel="Action Type Name"
        namePlaceholder="e.g., pick_up"
        baseTypeLabel="Action Base Type"
        baseTypePlaceholder="Select a base type..."
        propertyLabel="Action Properties"
        propertyNamePlaceholder="e.g., required_tool"
        propertyTypePlaceholder="e.g., Tool"
      />

      <ParameterInstanceModal
        key={`${parameterInstanceModalState.mode}-${
          parameterInstanceModalState.index ?? "new"
        }-${parameterInstanceModalState.initialValue.id}-${
          parameterInstanceModalState.revision
        }`}
        isOpen={parameterInstanceModalState.isOpen}
        mode={parameterInstanceModalState.mode}
        title={
          parameterInstanceModalState.mode === "add"
            ? "Add Parameter Instance"
            : "Edit Parameter Instance"
        }
        initialValue={parameterInstanceModalState.initialValue}
        typeDefinitions={parameterTypes}
        onClose={closeParameterInstanceModal}
        onSave={handleSaveParameterInstance}
        namePlaceholder="e.g., selected_tool"
        typeLabel="Parameter Type"
        typePlaceholder="Select a parameter type..."
        propertyValuesLabel="Property Values"
        propertyEmptyMessage="Define a parameter type to provide property values."
        baseTypePrefixLabel="Base type"
        createButtonLabel="Create Instance"
        saveButtonLabel="Save Changes"
      />

      <PredicateInstanceModal
        key={`${predicateInstanceModalState.mode}-${
          predicateInstanceModalState.index ?? "new"
        }-${predicateInstanceModalState.initialValue.id}-${
          predicateInstanceModalState.revision
        }`}
        isOpen={predicateInstanceModalState.isOpen}
        mode={predicateInstanceModalState.mode}
        title={
          predicateInstanceModalState.mode === "add"
            ? "Add Predicate Instance"
            : "Edit Predicate Instance"
        }
        initialValue={predicateInstanceModalState.initialValue}
        typeDefinitions={predicateTypes}
        onClose={closePredicateInstanceModal}
        onSave={handleSavePredicateInstance}
      />

      <ActionInstanceModal
        key={`${actionInstanceModalState.mode}-${
          actionInstanceModalState.index ?? "new"
        }-${actionInstanceModalState.initialValue.id}-${
          actionInstanceModalState.revision
        }`}
        isOpen={actionInstanceModalState.isOpen}
        mode={actionInstanceModalState.mode}
        title={
          actionInstanceModalState.mode === "add"
            ? "Add Action Instance"
            : "Edit Action Instance"
        }
        initialValue={actionInstanceModalState.initialValue}
        typeDefinitions={actionTypes}
        onClose={closeActionInstanceModal}
        onSave={handleSaveActionInstance}
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
              predicateTypes={predicateTypes}
              predicateTypeMap={predicateTypeMap}
              actionTypes={actionTypes}
              actionTypeMap={actionTypeMap}
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
