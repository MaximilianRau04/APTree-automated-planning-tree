import type {
  ActionInstance,
  ActionType,
  DataCategory,
  FlowSuccessType,
  PredicateInstance,
  PredicateType,
} from "../sidebar/utils/types";
import type { DragEntityKind } from "./dragTypes";
import type { DraggedSidebarItem } from "./dragTypes";

/** serialized node information stored by the canvas component. */
export interface CanvasNode {
  id: string;
  sourceId: string;
  name: string;
  typeLabel: string;
  category: DataCategory;
  kind: DragEntityKind;
  x: number;
  y: number;
  width?: number;
  height?: number;
  isNegated?: boolean;
  successType?: FlowSuccessType;
  preconditions?: PredicateInstance[];
  effects?: PredicateInstance[];
  typeId?: string;
}

export const DEFAULT_CANVAS_NODE_WIDTH = 240;
export const DEFAULT_CANVAS_NODE_HEIGHT = 180;

/** represents a connection between two nodes. */
export interface NodeConnection {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
  sourcePort?: 'top' | 'right' | 'bottom' | 'left';
  targetPort?: 'top' | 'right' | 'bottom' | 'left';
}

/** contract for the editor canvas so the parent app can control interactions. */
export interface EditorCanvasProps {
  nodes: CanvasNode[];
  connections?: NodeConnection[];
  onDropNode: (
    item: DraggedSidebarItem,
    position: { x: number; y: number }
  ) => void;
  onMoveNode?: (
    nodeId: string,
    position: { x: number; y: number }
  ) => void;
  onResizeNode?: (
    nodeId: string,
    size: { width: number; height: number }
  ) => void;
  onRemoveNode?: (nodeId: string) => void;
  onAddConnection?: (
    sourceNodeId: string,
    targetNodeId: string,
    sourcePort: 'top' | 'right' | 'bottom' | 'left',
    targetPort: 'top' | 'right' | 'bottom' | 'left'
  ) => void;
  onRemoveConnection?: (connectionId: string) => void;
  onAddActionPrecondition?: (nodeId: string) => void;
  onAddActionEffect?: (nodeId: string) => void;
  onCycleFlowSuccessType?: (nodeId: string) => void;
  onEditActionPredicate?: (
    nodeId: string,
    predicateId: string,
    collection: "precondition" | "effect"
  ) => void;
  onRemoveActionPredicate?: (
    nodeId: string,
    predicateId: string,
    collection: "precondition" | "effect"
  ) => void;
  predicateTypes?: PredicateType[];
  actionTypes?: ActionType[];
  actionInstances?: ActionInstance[];
}
