import type { ReactNode } from "react";

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

/** maps categories to their current search-filter query strings. */
export type SearchQueries = Record<DataCategory, string>;

/** props consumed by the parameter-instance modal component. */
export interface ParameterInstanceModalProps {
  isOpen: boolean;
  mode: "add" | "edit";
  title: string;
  initialValue: ParameterInstance;
  parameterTypes: ParameterType[];
  onClose: () => void;
  onSave: (value: ParameterInstance) => void;
}

/** props consumed by the type-definition modal component. */
export interface TypeDefinitionModalProps {
  isOpen: boolean;
  mode: "add" | "edit";
  title: string;
  initialValue: ParameterType;
  onClose: () => void;
  onSave: (value: ParameterType) => void;
}

/** public api returned by the useSidebarManager hook. */
export interface SidebarManager {
  addLabelFor: (category: DataCategory) => string;
  categoryModal: CategoryModalState;
  categoryOrder: string[];
  categoryTitles: Record<string, string>;
  closeCategoryModal: () => void;
  closeInstanceModal: () => void;
  closeModal: () => void;
  closeTypeModal: () => void;
  getItemsForCategory: (category: DataCategory) => StructuredItem[];
  handleDeleteCategory: (categoryKey: DataCategory) => void;
  handleDeleteItem: (category: DataCategory, index: number) => void;
  handleSaveCategory: (value: StructuredItem) => void;
  handleSaveFromModal: (value: StructuredItem) => void;
  handleSaveParameterInstance: (value: ParameterInstance) => void;
  handleSaveParameterType: (value: ParameterType) => void;
  handleSearchChange: (category: DataCategory, value: string) => void;
  instanceModalState: ParameterInstanceModalState;
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
  searchQueries: SearchQueries;
  typeModalState: TypeModalState;
}

/** props consumed by the category item list component. */
export interface CategoryItemListProps {
  category: DataCategory;
  items: StructuredItem[];
  parameterTypes: ParameterType[];
  parameterTypeMap: Map<string, ParameterType>;
  searchQuery: string;
  onEdit: (
    category: DataCategory,
    index: number,
    item: StructuredItem
  ) => void;
  onDelete: (category: DataCategory, index: number) => void;
}
