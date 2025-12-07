import { useState } from "react";
import EditModal from "./modals/EditModal";
import ParameterInstanceModal, {
  ActionInstanceModal,
  PredicateInstanceModal,
} from "./modals/InstanceModal";
import TypeDefinitionModal from "./modals/TypeDefinitionModal";
import BtNodeWizardModal, { type WizardStage } from "./modals/BtNodeWizardModal";
import "./Sidebar.css";
import { CategoryItemList } from "./components/CategoryItemList";
import SidebarSection from "./components/SidebarSection";
import { useSidebarManager } from "./useSidebarLogic";
import {
  ACTION_INSTANCES_KEY,
  ACTION_TYPES_KEY,
  BT_NODES_KEY,
  DECORATOR_NODE_OPTIONS,
  FLOW_NODE_OPTIONS,
  SERVICE_NODE_OPTIONS,
} from "./utils/constants";
import type { BehaviorNodeOption } from "./utils/types";

interface SidebarProps {
  onCreateBehaviorNode?: (option: BehaviorNodeOption) => void;
}

/**
 * assembles the full planner sidebar, wiring state-driven modals and section content together.
 * @returns sidebar layout containing sections, search inputs, and supporting modals
 */
export default function Sidebar({ onCreateBehaviorNode }: SidebarProps) {
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

  const [isBtNodeWizardOpen, setBtNodeWizardOpen] = useState(false);
  const [wizardHighlightStage, setWizardHighlightStage] = useState<WizardStage | null>(null);
  const flowOptions = FLOW_NODE_OPTIONS;
  const decoratorOptions = DECORATOR_NODE_OPTIONS;
  const serviceOptions = SERVICE_NODE_OPTIONS;

  const buildStatefulModalKey = (
    mode: "add" | "edit",
    index: number | null,
    id: string,
    revision: number
  ) => `${mode}-${index ?? "new"}-${id}-${revision}`;

  const categoryModalTitle =
    categoryModal.mode === "add" ? "Add Section" : "Rename Section";
  const categoryModalHelper =
    categoryModal.mode === "add"
      ? "Sections group related planner data. Use a short, descriptive title."
      : undefined;
  const categoryModalSaveLabel =
    categoryModal.mode === "add" ? "Create Section" : "Save";

  const openBtNodeWizard = (highlightStage: WizardStage | null = null) => {
    setWizardHighlightStage(highlightStage === "root" ? null : highlightStage);
    setBtNodeWizardOpen(true);
  };

  const closeBtNodeWizard = () => {
    setWizardHighlightStage(null);
    setBtNodeWizardOpen(false);
  };

  const handleWizardSelectBehaviorOption = (option: BehaviorNodeOption) => {
    onCreateBehaviorNode?.(option);
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

      <BtNodeWizardModal
        isOpen={isBtNodeWizardOpen}
        flowOptions={flowOptions}
        decoratorOptions={decoratorOptions}
        serviceOptions={serviceOptions}
        onClose={closeBtNodeWizard}
        onSelectActionType={() => openAddModal(ACTION_TYPES_KEY)}
        onSelectActionInstance={() => openAddModal(ACTION_INSTANCES_KEY)}
        onSelectBehaviorOption={handleWizardSelectBehaviorOption}
        emphasizedStage={
          wizardHighlightStage && wizardHighlightStage !== "root"
            ? wizardHighlightStage
            : null
        }
      />

      <TypeDefinitionModal
        key={buildStatefulModalKey(
          parameterTypeModalState.mode,
          parameterTypeModalState.index,
          parameterTypeModalState.initialValue.id,
          parameterTypeModalState.revision
        )}
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
        baseTypeLabel="Basic Type"
        baseTypePlaceholder="Select basic type..."
        propertyLabel="Parameter Properties"
        propertyNamePlaceholder="parameter name"
        propertyTypePlaceholder="Select basic type..."
      />

      <TypeDefinitionModal
        key={buildStatefulModalKey(
          predicateTypeModalState.mode,
          predicateTypeModalState.index,
          predicateTypeModalState.initialValue.id,
          predicateTypeModalState.revision
        )}
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
        propertyLabel="Predicate Parameters"
        propertyNamePlaceholder="predicate parameter name"
        propertyTypePlaceholder="Select basic type..."
        fixedBaseTypeValue="predicate"
      />

      <TypeDefinitionModal
        key={buildStatefulModalKey(
          actionTypeModalState.mode,
          actionTypeModalState.index,
          actionTypeModalState.initialValue.id,
          actionTypeModalState.revision
        )}
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
        propertyLabel="Action Parameters"
        propertyNamePlaceholder="action parameter name"
        propertyTypePlaceholder="Select basic type..."
        fixedBaseTypeValue="GenericBTAction"
      />

      <ParameterInstanceModal
        key={buildStatefulModalKey(
          parameterInstanceModalState.mode,
          parameterInstanceModalState.index,
          parameterInstanceModalState.initialValue.id,
          parameterInstanceModalState.revision
        )}
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
        key={buildStatefulModalKey(
          predicateInstanceModalState.mode,
          predicateInstanceModalState.index,
          predicateInstanceModalState.initialValue.id,
          predicateInstanceModalState.revision
        )}
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
        key={buildStatefulModalKey(
          actionInstanceModalState.mode,
          actionInstanceModalState.index,
          actionInstanceModalState.initialValue.id,
          actionInstanceModalState.revision
        )}
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

      {categoryOrder.map((categoryKey) => {
        const displayTitle = categoryTitles[categoryKey] ?? categoryKey;
        const iconLabel = displayTitle.charAt(0).toUpperCase();
        const buttonLabel = addLabelFor(categoryKey);
        const searchQuery = searchQueries[categoryKey] ?? "";
        const items = getItemsForCategory(categoryKey);
        const isBehaviorNodeCategory = categoryKey === BT_NODES_KEY;
        const isActionCategory =
          categoryKey === ACTION_TYPES_KEY || categoryKey === ACTION_INSTANCES_KEY;

        return (
          <SidebarSection
            key={categoryKey}
            title={displayTitle}
            isOpen={false}
            iconLabel={iconLabel}
            onEdit={() => openRenameCategoryModal(categoryKey)}
            onDelete={() => handleDeleteCategory(categoryKey)}
          >
            {isBehaviorNodeCategory ? (
              <button
                className="add-button"
                onClick={() => openBtNodeWizard(null)}
                type="button"
              >
                + {buttonLabel}
              </button>
            ) : isActionCategory ? (
              <button
                className="add-button"
                onClick={() => openBtNodeWizard("action")}
                type="button"
              >
                + Open BT Node Wizard
              </button>
            ) : (
              <button
                className="add-button"
                onClick={() => openAddModal(categoryKey)}
                type="button"
              >
                + {buttonLabel}
              </button>
            )}
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
