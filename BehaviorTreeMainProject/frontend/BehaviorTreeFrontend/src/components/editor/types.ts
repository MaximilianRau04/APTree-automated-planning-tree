import type { DataCategory } from "../sidebar/utils/types";
import type { DragEntityKind } from "./dragTypes";

export interface CanvasNode {
  id: string;
  sourceId: string;
  name: string;
  typeLabel: string;
  category: DataCategory;
  kind: DragEntityKind;
  x: number;
  y: number;
  isNegated?: boolean;
}
