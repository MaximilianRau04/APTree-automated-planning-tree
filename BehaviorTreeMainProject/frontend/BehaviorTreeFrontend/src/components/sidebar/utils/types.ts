import type { ReactNode } from "react";

export const FLOW_SUCCESS_TYPES = [
  "ALL",
  "ANY",
  "COUNT",
  "PERCENTAGE",
  "SIGNAL",
] as const;

export type FlowSuccessType = (typeof FLOW_SUCCESS_TYPES)[number];

/** represents a base entity displayed within sidebar lists. */
export interface StructuredItem {
  id: string;
  name: string;
  type: string;
}

/** describes a parameter-type property, defining the schema fields. */
export interface TypeProperty {
  id: string;
  name: string;
  valueType: string;
}

/** extends a structured item with its parameter-type properties. */
export interface ParameterType extends StructuredItem {
  properties: TypeProperty[];
}

/** represents a concrete parameter instance populated by users. */
export interface ParameterInstance extends StructuredItem {
  typeId: string;
  propertyValues: Record<string, string>;
}

/** extends parameter-type declarations for predicate definitions. */
export type PredicateType = ParameterType;

/** concrete predicate instance including negation flag. */
export interface PredicateInstance extends ParameterInstance {
  isNegated: boolean;
}

/** extends parameter-type declarations for action definitions. */
export type ActionType = ParameterType;

/** concrete action instance referencing an action type. */
export type ActionInstance = ParameterInstance;

export type DataCategory = string;

export type AppData = Record<DataCategory, StructuredItem[]>;

/** captures runtime state for the add/edit category modal. */
export interface CategoryModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  activeKey: DataCategory | null;
  value: StructuredItem;
}

/** tracks shared modal state for the simple edit modal. */
export interface ModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  category: DataCategory | null;
  index: number | null;
  initialValue: StructuredItem;
}

/** component props configuring a collapsible sidebar section. */
export interface SectionProps {
  title: string;
  children: ReactNode;
  isOpen?: boolean;
  iconLabel?: string;
  onEdit?: () => void;
  onDelete?: () => void;
  disableDelete?: boolean;
}

/** Props for the edit modal component. */
export interface EditModalProps {
  isOpen: boolean;
  title: string;
  initialValue: StructuredItem;
  onClose: () => void;
  onSave: (value: StructuredItem) => void;
  hideTypeField?: boolean;
  nameLabel?: string;
  namePlaceholder?: string;
  helperText?: string;
  saveLabel?: string;
}

/** defines configuration metadata for each sidebar category. */
export interface CategoryConfig {
  key: DataCategory;
  title: string;
  addLabel: string;
  defaultItems?: StructuredItem[];
}

/** modal state shared by the type-definition modal. */
export interface TypeModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  index: number | null;
  initialValue: ParameterType;
  revision: number;
}

/** modal state governing the parameter-instance modal. */
export interface ParameterInstanceModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  index: number | null;
  initialValue: ParameterInstance;
  revision: number;
}

/** modal state governing predicate-type editing. */
export type PredicateTypeModalState = Omit<TypeModalState, "initialValue"> & {
  initialValue: PredicateType;
};

/** modal state governing predicate-instance editing. */
export interface PredicateInstanceModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  index: number | null;
  initialValue: PredicateInstance;
  revision: number;
}

/** modal state governing action-type editing. */
export type ActionTypeModalState = Omit<TypeModalState, "initialValue"> & {
  initialValue: ActionType;
};

/** modal state governing action-instance editing. */
export interface ActionInstanceModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  index: number | null;
  initialValue: ActionInstance;
  revision: number;
}

/** maps categories to their current search-filter query strings. */
export type SearchQueries = Record<DataCategory, string>;

/** props consumed by the generic instance modal component. */
export interface TypedInstanceModalProps<
  TInstance extends ParameterInstance = ParameterInstance
> {
  isOpen: boolean;
  mode: "add" | "edit";
  title: string;
  initialValue: TInstance;
  typeDefinitions: ParameterType[];
  onClose: () => void;
  onSave: (value: TInstance) => void;
  nameLabel?: string;
  namePlaceholder?: string;
  typeLabel?: string;
  typePlaceholder?: string;
  propertyValuesLabel?: string;
  propertyEmptyMessage?: string;
  createButtonLabel?: string;
  saveButtonLabel?: string;
  baseTypePrefixLabel?: string;
  enableNegationToggle?: boolean;
  negationLabel?: string;
  validatePropertyValue?: (
    value: string,
    property: TypeProperty
  ) => boolean;
  propertyValidationHint?: string;
}

export type ParameterInstanceModalProps = TypedInstanceModalProps<ParameterInstance>;

export type PredicateInstanceModalProps = TypedInstanceModalProps<PredicateInstance>;

export type ActionInstanceModalProps = TypedInstanceModalProps<ActionInstance>;

/** props consumed by the type-definition modal component. */
export interface TypeDefinitionModalProps {
  isOpen: boolean;
  mode: "add" | "edit";
  title: string;
  initialValue: ParameterType;
  onClose: () => void;
  onSave: (value: ParameterType) => void;
  nameLabel?: string;
  namePlaceholder?: string;
  baseTypeLabel?: string;
  baseTypePlaceholder?: string;
  showBaseTypeField?: boolean;
  propertyLabel?: string;
  propertyNamePlaceholder?: string;
  propertyTypePlaceholder?: string;
  propertyHelperText?: string;
  baseTypeOptions?: string[];
  fixedBaseTypeValue?: string;
}

/** public api returned by the useSidebarManager hook. */
export interface SidebarManager {
  addLabelFor: (category: DataCategory) => string;
  categoryModal: CategoryModalState;
  categoryOrder: string[];
  categoryTitles: Record<string, string>;
  closeCategoryModal: () => void;
  closeParameterInstanceModal: () => void;
  closePredicateInstanceModal: () => void;
  closeActionInstanceModal: () => void;
  closeModal: () => void;
  closeParameterTypeModal: () => void;
  closePredicateTypeModal: () => void;
  closeActionTypeModal: () => void;
  getItemsForCategory: (category: DataCategory) => StructuredItem[];
  handleDeleteCategory: (categoryKey: DataCategory) => void;
  handleDeleteItem: (category: DataCategory, index: number) => void;
  handleSaveCategory: (value: StructuredItem) => void;
  handleSaveFromModal: (value: StructuredItem) => void;
  handleSaveParameterInstance: (value: ParameterInstance) => void;
  handleSavePredicateInstance: (value: PredicateInstance) => void;
  handleSaveActionInstance: (value: ActionInstance) => void;
  handleSaveParameterType: (value: ParameterType) => void;
  handleSavePredicateType: (value: PredicateType) => void;
  handleSaveActionType: (value: ActionType) => void;
  handleSearchChange: (category: DataCategory, value: string) => void;
  parameterInstanceModalState: ParameterInstanceModalState;
  predicateInstanceModalState: PredicateInstanceModalState;
  actionInstanceModalState: ActionInstanceModalState;
  modalState: ModalState;
  openAddModal: (category: DataCategory) => void;
  openCategoryModal: () => void;
  openEditModal: (
    category: DataCategory,
    index: number,
    currentValue: StructuredItem
  ) => void;
  openRenameCategoryModal: (category: DataCategory) => void;
  parameterTypeMap: Map<string, ParameterType>;
  parameterTypes: ParameterType[];
  predicateTypeMap: Map<string, PredicateType>;
  predicateTypes: PredicateType[];
  actionTypeMap: Map<string, ActionType>;
  actionTypes: ActionType[];
  searchQueries: SearchQueries;
  parameterTypeModalState: TypeModalState;
  predicateTypeModalState: PredicateTypeModalState;
  actionTypeModalState: ActionTypeModalState;
}

/** props consumed by the category item list component. */
export interface CategoryItemListProps {
  category: DataCategory;
  items: StructuredItem[];
  parameterTypes: ParameterType[];
  parameterTypeMap: Map<string, ParameterType>;
  predicateTypes: PredicateType[];
  predicateTypeMap: Map<string, PredicateType>;
  actionTypes: ActionType[];
  actionTypeMap: Map<string, ActionType>;
  searchQuery: string;
  onEdit: (
    category: DataCategory,
    index: number,
    item: StructuredItem
  ) => void;
  onDelete: (category: DataCategory, index: number) => void;
}

export type BehaviorNodeKind = "flow" | "decorator" | "service";

/** describes a selectable behavior-tree node template surfaced in the sidebar. */
export interface BehaviorNodeOption {
  id: string;
  label: string;
  typeLabel: string;
  description?: string;
  kind: BehaviorNodeKind;
  defaultSuccessType?: FlowSuccessType;
}

/** convenience aliases for the specific node kinds. */
export type FlowNodeOption = BehaviorNodeOption & { kind: "flow" };
export type DecoratorNodeOption = BehaviorNodeOption & { kind: "decorator" };
export type ServiceNodeOption = BehaviorNodeOption & { kind: "service" };
