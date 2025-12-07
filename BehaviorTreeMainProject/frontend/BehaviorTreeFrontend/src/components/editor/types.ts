import type {
  DataCategory,
  FlowSuccessType,
  PredicateInstance,
} from "../sidebar/utils/types";
import type { DragEntityKind } from "./dragTypes";
import type { PredicateType } from "../sidebar/utils/types";
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
  isNegated?: boolean;
  successType?: FlowSuccessType;
  preconditions?: PredicateInstance[];
  effects?: PredicateInstance[];
}

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
}
