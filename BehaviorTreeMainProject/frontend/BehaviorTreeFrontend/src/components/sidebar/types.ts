import type { ReactNode } from "react";

export interface StructuredItem {
  id: string;
  name: string;
  type: string;
}

export interface TypeProperty {
  id: string;
  name: string;
  valueType: string;
}

export interface ParameterType extends StructuredItem {
  properties: TypeProperty[];
}

export interface ParameterInstance extends StructuredItem {
  typeId: string;
  propertyValues: Record<string, string>;
}

export type DataCategory = string;

export type AppData = Record<DataCategory, StructuredItem[]>;

export interface ModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  category: DataCategory | null;
  index: number | null;
  initialValue: StructuredItem;
}

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

export interface CategoryConfig {
  key: DataCategory;
  title: string;
  addLabel: string;
  defaultItems?: StructuredItem[];
}
