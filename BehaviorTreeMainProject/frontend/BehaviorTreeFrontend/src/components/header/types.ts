export interface HeaderProps {
  theme: "light" | "dark";
  onToggleTheme: () => void;
  onImportParameterTypes: (file: File) => void;
  onImportPredicateTypes: (file: File) => void;
  onImportActionTypes: (file: File) => void;
  onExportParameterTypesTxt: () => void;
  onExportPredicateTypesTxt: () => void;
  onExportActionTypesTxt: () => void;
  onImportParameterInstances: (file: File) => void;
  onImportPredicateInstances: (file: File) => void;
  onImportActionInstances: (file: File) => void;
  onExportTypesAndInstances: (scope?: TypesAndInstancesJsonImportScope) => void;
  onImportTypesAndInstances: (file: File, scope?: TypesAndInstancesJsonImportScope) => void;
  onExportCanvasGraph: () => void;
  onImportCanvasGraph: (file: File) => void;
}

export type TypesAndInstancesJsonImportScope =
  | "full"
  | "paramTypes"
  | "paramInstances"
  | "predTypes"
  | "predInstances"
  | "actions"
  | "actionInstances";

export interface DropdownActionItem {
  kind?: "action";
  label: string;
  hint?: string;
  onSelect?: () => void;
  disabled?: boolean;
}

export interface DropdownFileItem {
  kind: "file";
  label: string;
  hint?: string;
  accept?: string;
  onFileSelect: (file: File) => void;
}

export interface DropdownDividerItem {
  kind: "divider";
}

export interface DropdownLabelItem {
  kind: "label";
  label: string;
}

export type DropdownMenuItem =
  | string
  | DropdownActionItem
  | DropdownFileItem
  | DropdownDividerItem
  | DropdownLabelItem;

export type NormalizedDropdownItem =
  | (DropdownActionItem & { kind: "action" })
  | DropdownFileItem
  | DropdownDividerItem
  | DropdownLabelItem;

export interface DropdownProps {
  title: string;
  items: DropdownMenuItem[];
}
