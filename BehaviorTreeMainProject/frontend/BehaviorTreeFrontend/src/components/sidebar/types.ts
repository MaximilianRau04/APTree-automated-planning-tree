import type { ReactNode } from "react";

export interface StructuredItem {
  name: string;
  type: string;
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
