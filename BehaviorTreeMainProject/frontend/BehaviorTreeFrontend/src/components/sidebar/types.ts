export type DataCategory =
  | "variables"
  | "paramTypes"
  | "paramInstances"
  | "predTypes"
  | "predInstances"
  | "actions"
  | "actionInstances"
  | "nodes";

export interface AppData {
  variables: StructuredItem[];
  paramTypes: StructuredItem[];
  paramInstances: StructuredItem[];
  predTypes: StructuredItem[];
  predInstances: StructuredItem[];
  actions: StructuredItem[];
  actionInstances: StructuredItem[];
  nodes: StructuredItem[];
}

export interface ModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  category: DataCategory | null;
  index: number | null;
  initialValue: StructuredItem;
}

export interface SectionProps {
  title: string;
  children: React.ReactNode;
  isOpen?: boolean;
}

export interface StructuredItem {
  name: string;
  type: string;
}
