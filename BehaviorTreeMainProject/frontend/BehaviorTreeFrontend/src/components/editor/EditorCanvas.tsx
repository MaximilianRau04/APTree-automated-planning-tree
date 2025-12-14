import { useCallback, useMemo, useRef, useState } from "react";
import type { CSSProperties } from "react";
import {
  Background,
  BackgroundVariant,
  BaseEdge,
  EdgeLabelRenderer,
  getSmoothStepPath,
  Handle,
  MarkerType,
  NodeResizer,
  type Connection,
  ConnectionLineType,
  type Edge as FlowEdge,
  type EdgeProps,
  type EdgeTypes,
  ReactFlow,
  ReactFlowProvider,
  type Node as FlowNode,
  type NodeProps,
  type NodeTypes,
  Position,
  useReactFlow,
} from "reactflow";
import "reactflow/dist/style.css";
import {
  DRAG_DATA_FORMAT,
  isSidebarDrag,
  type DraggedSidebarItem,
} from "./dragTypes";
import type { CanvasNode, EditorCanvasProps } from "./types";
import {
  DEFAULT_CANVAS_NODE_HEIGHT,
  DEFAULT_CANVAS_NODE_WIDTH,
} from "./types";
import type {
  ActionInstance,
  ActionType,
  PredicateInstance,
  PredicateType,
} from "../sidebar/utils/types";
import {
  DECORATOR_NODES_KEY,
  FLOW_NODES_KEY,
  SERVICE_NODES_KEY,
} from "../sidebar/utils/constants";
import "./EditorCanvas.css";

type PortSide = "top" | "right" | "bottom" | "left";

const resolveNumericOffset = (value: string | number | undefined): number => {
  if (typeof value === "number") {
    return value;
  }

  if (typeof value === "string") {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  return 0;
};

interface BehaviorNodeData {
  node: CanvasNode;
  predicateTypeMap: Map<string, PredicateType>;
  actionTypeMap: Map<string, ActionType>;
  actionInstanceMap: Map<string, ActionInstance>;
  onRemoveNode?: (nodeId: string) => void;
  onCycleFlowSuccessType?: (nodeId: string) => void;
  onAddActionPrecondition?: (nodeId: string) => void;
  onAddActionEffect?: (nodeId: string) => void;
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
  onResizeNode?: (nodeId: string, size: { width: number; height: number }) => void;
  onMoveNode?: (nodeId: string, position: { x: number; y: number }) => void;
}

interface BehaviorEdgeData {
  onRemoveConnection?: (connectionId: string) => void;
}

const portPositions: Record<PortSide, Position> = {
  top: Position.Top,
  right: Position.Right,
  bottom: Position.Bottom,
  left: Position.Left,
};

const PORT_STYLES: Record<PortSide, CSSProperties> = {
  top: { top: -6, left: "50%", transform: "translate(-50%, 0)" },
  right: { right: -6, top: "50%", transform: "translate(0, -50%)" },
  bottom: { bottom: -6, left: "50%", transform: "translate(-50%, 0)" },
  left: { left: -6, top: "50%", transform: "translate(0, -50%)" },
};

const SOURCE_HANDLE_STYLES: Record<PortSide, CSSProperties> = {
  top: { top: -16, left: "50%", transform: "translate(-50%, 0)" },
  right: { right: -16, top: "50%", transform: "translate(0, -50%)" },
  bottom: { bottom: -16, left: "50%", transform: "translate(-50%, 0)" },
  left: { left: -16, top: "50%", transform: "translate(0, -50%)" },
};

const TARGET_HANDLE_STYLES: Record<PortSide, CSSProperties> = {
  top: { top: -8, left: "50%", transform: "translate(-50%, 0)" },
  right: { right: -8, top: "50%", transform: "translate(0, -50%)" },
  bottom: { bottom: -8, left: "50%", transform: "translate(-50%, 0)" },
  left: { left: -8, top: "50%", transform: "translate(0, -50%)" },
};

const PARAM_BOX_HEIGHT = 24;
const PARAM_BOX_GAP = 6;
const PARAM_STACK_CLEARANCE = 18;

/**
 * resolves the port side from a handle ID string.
 * @param handleId the handle ID string from react-flow 
 * @param fallback the fallback port side if none can be resolved 
 * @returns the resolved port side 
 */
function resolvePortFromHandle(
  handleId: string | null | undefined,
  fallback: PortSide
): PortSide {
  if (!handleId) {
    return fallback;
  }

  const match = handleId.match(/(top|right|bottom|left)$/);
  if (!match) {
    return fallback;
  }

  return match[1] as PortSide;
}

/**
 * creates a map of predicate type ID to predicate type for easy lookup.
 * @param predicateTypes optional array of predicate types 
 * @returns map of predicate type ID to predicate type 
 */
function createPredicateTypeMap(predicateTypes?: PredicateType[]) {
  return new Map(predicateTypes?.map((type) => [type.id, type]) ?? []);
}

/**
 * formats a predicate instance into a summary string for display.
 * @param predicate the predicate instance to format 
 * @param predicateTypeMap map of predicate type ID to predicate type 
 * @returns formatted summary string 
 */
function formatPredicateSummary(
  predicate: PredicateInstance,
  predicateTypeMap: Map<string, PredicateType>
) {
  const type = predicateTypeMap.get(predicate.typeId);
  const entries = Object.entries(predicate.propertyValues ?? {});

  if (entries.length === 0) {
    return "";
  }

  return entries
    .map(([propertyId, value]) => {
      const propertyName = type?.properties.find(
        (property) => property.id === propertyId
      )?.name;
      return propertyName ? `${propertyName}: ${value}` : `${propertyId}: ${value}`;
    })
    .join(", ");
}

/**
 * checks if a canvas node is an action node.
 * @param node the canvas node to check 
 * @returns true if the node is an action node, false otherwise 
 */
function isActionNode(node: CanvasNode) {
  return node.kind === "actionType" || node.kind === "actionInstance";
}

/**
 * presents a collection of predicates within a behavior node.
 * @param param0 component props 
 * @returns JSX element or null if no items 
 */
function PredicateCollection({
  nodeId,
  items,
  collection,
  predicateTypeMap,
  onEdit,
  onRemove,
}: {
  nodeId: string;
  items: PredicateInstance[];
  collection: "precondition" | "effect";
  predicateTypeMap: Map<string, PredicateType>;
  onEdit?: BehaviorNodeData["onEditActionPredicate"];
  onRemove?: BehaviorNodeData["onRemoveActionPredicate"];
}) {
  if (items.length === 0) {
    return null;
  }

  return (
    <div className="canvas-node-state-group">
      <div className="canvas-node-state-list">
        {items.map((predicate) => {
          const summary = formatPredicateSummary(predicate, predicateTypeMap);

          return (
            <div key={predicate.id} className="canvas-node-state-item">
              <div className="canvas-node-state-body">
                <span className="canvas-node-state-name">
                  {predicate.isNegated ? "NOT " : ""}
                  {predicate.name || predicate.type}
                </span>
                <span className="canvas-node-state-meta">{predicate.type}</span>
                {summary ? (
                  <span className="canvas-node-state-args">{summary}</span>
                ) : null}
              </div>
              <div className="canvas-node-state-actions">
                {onEdit ? (
                  <button
                    type="button"
                    className="canvas-node-state-btn"
                    onMouseDown={(event) => event.stopPropagation()}
                    onClick={(event) => {
                      event.stopPropagation();
                      onEdit(nodeId, predicate.id, collection);
                    }}
                    title={`Edit ${collection}`}
                    aria-label={`Edit ${collection}`}
                  >
                    E
                  </button>
                ) : null}
                {onRemove ? (
                  <button
                    type="button"
                    className="canvas-node-state-btn"
                    onMouseDown={(event) => event.stopPropagation()}
                    onClick={(event) => {
                      event.stopPropagation();
                      onRemove(nodeId, predicate.id, collection);
                    }}
                    title={`Remove ${collection}`}
                    aria-label={`Remove ${collection}`}
                  >
                    X
                  </button>
                ) : null}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

/**
 * behavior tree node component for the editor canvas.
 * @param param0 component props 
 * @returns JSX element 
 */
function BehaviorTreeNode({ id, data, selected }: NodeProps<BehaviorNodeData>) {
  const { node } = data;
  const preconditions = node.preconditions ?? [];
  const effects = node.effects ?? [];
  const isAction = isActionNode(node);
  const actionInstance =
    isAction && node.kind === "actionInstance"
      ? data.actionInstanceMap.get(node.sourceId)
      : undefined;

  const resolvedActionTypeId = isAction
    ? node.kind === "actionType"
      ? node.sourceId
      : actionInstance?.typeId ?? node.typeId
    : undefined;

  const actionTypeDefinition =
    resolvedActionTypeId && isAction
      ? data.actionTypeMap.get(resolvedActionTypeId)
      : undefined;

  const actionParameterSummaries =
    isAction && actionTypeDefinition
      ? actionTypeDefinition.properties
          .map((property, index) => {
            const fallbackId = `${resolvedActionTypeId ?? "action"}-${index}`;
            const propertyKey = property.id || fallbackId;
            const name = property.name?.trim() || property.id || `param_${index + 1}`;
            const value = actionInstance?.propertyValues?.[property.id];
            const label = value && value.trim().length > 0 ? `${name}: ${value}` : name;
            return label.trim().length > 0
              ? { id: propertyKey, label }
              : null;
          })
          .filter((entry): entry is { id: string; label: string } => Boolean(entry))
      : [];

  const showActionParams = actionParameterSummaries.length > 0;
  const paramStackHeight = showActionParams
    ? actionParameterSummaries.length * PARAM_BOX_HEIGHT +
      (actionParameterSummaries.length - 1) * PARAM_BOX_GAP
    : 0;
  const paramClearance = showActionParams
    ? paramStackHeight + PARAM_STACK_CLEARANCE
    : 0;

  const nodeClasses = ["canvas-node", `canvas-node-${node.kind}`];

  if (node.category === FLOW_NODES_KEY) {
    nodeClasses.push("canvas-node-flow");
  } else if (node.category === DECORATOR_NODES_KEY) {
    nodeClasses.push("canvas-node-decorator");
  } else if (node.category === SERVICE_NODES_KEY) {
    nodeClasses.push("canvas-node-service");
  }

  if (isAction) {
    nodeClasses.push("canvas-node-action");
  }

  const portStyleOverrides: Partial<Record<PortSide, CSSProperties>> = {};
  const sourceHandleOverrides: Partial<Record<PortSide, CSSProperties>> = {};
  const targetHandleOverrides: Partial<Record<PortSide, CSSProperties>> = {};

  if (isAction) {
    portStyleOverrides.left = { left: 8 };
    portStyleOverrides.right = { right: 8 };
    sourceHandleOverrides.left = { left: -12 };
    sourceHandleOverrides.right = { right: -12 };
    targetHandleOverrides.left = { left: -4 };
    targetHandleOverrides.right = { right: -4 };

    portStyleOverrides.top = {
      top: resolveNumericOffset(PORT_STYLES.top.top) - paramClearance,
    };
    sourceHandleOverrides.top = {
      top: resolveNumericOffset(SOURCE_HANDLE_STYLES.top.top) - paramClearance,
    };
    targetHandleOverrides.top = {
      top: resolveNumericOffset(TARGET_HANDLE_STYLES.top.top) - paramClearance,
    };
  }

  return (
    <div className={nodeClasses.join(" ")}>
      <NodeResizer
        isVisible={selected}
        minWidth={180}
        minHeight={120}
        onResizeEnd={(_event, params) => {
          if (!data.onResizeNode && !data.onMoveNode) {
            return;
          }

          const previousWidth = data.node.width ?? DEFAULT_CANVAS_NODE_WIDTH;
          const previousHeight = data.node.height ?? DEFAULT_CANVAS_NODE_HEIGHT;
          const nextWidth = params.width ?? previousWidth;
          const nextHeight = params.height ?? previousHeight;

          const previousTopLeft = {
            x: data.node.x - previousWidth / 2,
            y: data.node.y - previousHeight / 2,
          };

          const nextTopLeft = {
            x: params.x ?? previousTopLeft.x,
            y: params.y ?? previousTopLeft.y,
          };

          data.onResizeNode?.(id, {
            width: nextWidth,
            height: nextHeight,
          });

          data.onMoveNode?.(id, {
            x: nextTopLeft.x + nextWidth / 2,
            y: nextTopLeft.y + nextHeight / 2,
          });
        }}
        handleClassName="canvas-node-resizer-handle"
        lineClassName="canvas-node-resizer-line"
      />

      {showActionParams ? (
        <div
          className="canvas-node-params"
          aria-label="Action parameters"
        >
          {actionParameterSummaries.map((entry) => (
            <span key={entry.id} className="canvas-node-params-chip">
              {entry.label}
            </span>
          ))}
        </div>
      ) : null}

      {node.successType ? (
        data.onCycleFlowSuccessType ? (
          <button
            type="button"
            className="canvas-node-success"
            onMouseDown={(event) => event.stopPropagation()}
            onClick={(event) => {
              event.stopPropagation();
              data.onCycleFlowSuccessType?.(id);
            }}
            title="Click to cycle success type"
            aria-label={`Success type ${node.successType}. Click to cycle.`}
          >
            {node.successType}
          </button>
        ) : (
          <span className="canvas-node-success" aria-hidden="true">
            {node.successType}
          </span>
        )
      ) : null}

      {data.onRemoveNode ? (
        <button
          type="button"
          className="canvas-node-remove"
          onMouseDown={(event) => event.stopPropagation()}
          onClick={(event) => {
            event.stopPropagation();
            data.onRemoveNode?.(id);
          }}
          aria-label={`Remove ${node.name}`}
        >
          ×
        </button>
      ) : null}

      <span className="canvas-node-label">{node.name}</span>
      <span className="canvas-node-meta">{node.typeLabel}</span>
      {node.isNegated ? (
        <span className="canvas-node-badge" aria-label="Negated predicate">
          NOT
        </span>
      ) : null}

      {isAction ? (
        <div className="canvas-node-state">
          {(data.onAddActionPrecondition || data.onAddActionEffect) && (
            <div className="canvas-node-actions">
              {data.onAddActionPrecondition ? (
                <button
                  type="button"
                  className="canvas-node-action-btn"
                  onMouseDown={(event) => event.stopPropagation()}
                  onClick={(event) => {
                    event.stopPropagation();
                    data.onAddActionPrecondition?.(id);
                  }}
                  title="Add precondition"
                  aria-label="Add precondition"
                >
                  Preconditions +
                </button>
              ) : null}
              {data.onAddActionEffect ? (
                <button
                  type="button"
                  className="canvas-node-action-btn"
                  onMouseDown={(event) => event.stopPropagation()}
                  onClick={(event) => {
                    event.stopPropagation();
                    data.onAddActionEffect?.(id);
                  }}
                  title="Add effect"
                  aria-label="Add effect"
                >
                  Effects +
                </button>
              ) : null}
            </div>
          )}

          <PredicateCollection
            nodeId={id}
            items={preconditions}
            predicateTypeMap={data.predicateTypeMap}
            collection="precondition"
            onEdit={data.onEditActionPredicate}
            onRemove={data.onRemoveActionPredicate}
          />
          <PredicateCollection
            nodeId={id}
            items={effects}
            predicateTypeMap={data.predicateTypeMap}
            collection="effect"
            onEdit={data.onEditActionPredicate}
            onRemove={data.onRemoveActionPredicate}
          />
        </div>
      ) : null}

      {(Object.keys(portPositions) as PortSide[]).map((side) => (
        <span
          key={`port-${side}`}
          className={`canvas-node-port canvas-node-port-${side}`}
          style={{ ...PORT_STYLES[side], ...(portStyleOverrides[side] ?? {}) }}
        />
      ))}
      {(Object.keys(portPositions) as PortSide[]).map((side) => (
        <Handle
          key={`source-${side}`}
          type="source"
          position={portPositions[side]}
          id={`source-${side}`}
          className="canvas-node-handle canvas-node-handle-hitbox canvas-node-handle-source"
          style={{
            ...SOURCE_HANDLE_STYLES[side],
            ...(sourceHandleOverrides[side] ?? {}),
          }}
          isConnectableEnd={false}
        />
      ))}
      {(Object.keys(portPositions) as PortSide[]).map((side) => (
        <Handle
          key={`target-${side}`}
          type="target"
          position={portPositions[side]}
          id={`target-${side}`}
          className="canvas-node-handle canvas-node-handle-hitbox canvas-node-handle-target"
          style={{
            ...TARGET_HANDLE_STYLES[side],
            ...(targetHandleOverrides[side] ?? {}),
          }}
          isConnectableStart={false}
        />
      ))}
    </div>
  );
}

/**
 * behavior tree edge component for the editor canvas.
 * @param param0 component props 
 * @returns JSX element 
 */
function BehaviorEdge({
  id,
  sourceX,
  sourceY,
  targetX,
  targetY,
  sourcePosition,
  targetPosition,
  markerEnd,
  style,
  data,
}: EdgeProps<BehaviorEdgeData>) {
  const [edgePath, midX, midY] = getSmoothStepPath({
    sourceX,
    sourceY,
    targetX,
    targetY,
    sourcePosition,
    targetPosition,
  });

  return (
    <>
      <BaseEdge
        id={id}
        path={edgePath}
        markerEnd={markerEnd}
        style={{
          stroke: "#fff",
          strokeWidth: 2,
          ...style,
        }}
      />
      {data?.onRemoveConnection ? (
        <EdgeLabelRenderer>
          <button
            type="button"
            className="canvas-connection-remove-btn"
            style={{
              position: "absolute",
              transform: `translate(-50%, -50%) translate(${midX}px, ${midY}px)`,
              pointerEvents: "all",
            }}
            onClick={(event) => {
              event.stopPropagation();
              data.onRemoveConnection?.(id);
            }}
            aria-label="Remove connection"
          >
            ×
          </button>
        </EdgeLabelRenderer>
      ) : null}
    </>
  );
}

const nodeTypes: NodeTypes = { btNode: BehaviorTreeNode };
const edgeTypes: EdgeTypes = { btEdge: BehaviorEdge };

/**
 * editor canvas component for behavior tree nodes and connections.
 * @param props component props 
 * @returns JSX element 
 */
function EditorCanvasInner(props: EditorCanvasProps) {
  const {
    nodes,
    connections = [],
    onDropNode,
    onMoveNode,
    onResizeNode,
    onRemoveNode,
    onAddConnection,
    onRemoveConnection,
    onAddActionPrecondition,
    onAddActionEffect,
    onCycleFlowSuccessType,
    onEditActionPredicate,
    onRemoveActionPredicate,
    predicateTypes,
    actionTypes,
    actionInstances,
  } = props;

  const wrapperRef = useRef<HTMLDivElement>(null);
  const { project } = useReactFlow();
  const [isActive, setIsActive] = useState(false);

  const predicateTypeMap = useMemo(
    () => createPredicateTypeMap(predicateTypes),
    [predicateTypes]
  );

  const actionTypeMap = useMemo(() => {
    const entries = actionTypes ?? [];
    return new Map(entries.map((type) => [type.id, type] as const));
  }, [actionTypes]);

  const actionInstanceMap = useMemo(() => {
    const entries = actionInstances ?? [];
    return new Map(entries.map((instance) => [instance.id, instance] as const));
  }, [actionInstances]);

  const flowNodes = useMemo<FlowNode<BehaviorNodeData>[]>(
    () =>
      nodes.map((node) => {
        const width = node.width ?? DEFAULT_CANVAS_NODE_WIDTH;
        const height = node.height ?? DEFAULT_CANVAS_NODE_HEIGHT;

        return {
          id: node.id,
          type: "btNode" as const,
          position: { x: node.x - width / 2, y: node.y - height / 2 },
          data: {
            node,
            predicateTypeMap,
            actionTypeMap,
            actionInstanceMap,
            onRemoveNode,
            onCycleFlowSuccessType,
            onAddActionPrecondition,
            onAddActionEffect,
            onEditActionPredicate,
            onRemoveActionPredicate,
            onResizeNode,
            onMoveNode,
          },
          width,
          height,
        } satisfies FlowNode<BehaviorNodeData>;
      }),
    [
      nodes,
      predicateTypeMap,
      actionTypeMap,
      actionInstanceMap,
      onRemoveNode,
      onCycleFlowSuccessType,
      onAddActionPrecondition,
      onAddActionEffect,
      onEditActionPredicate,
      onRemoveActionPredicate,
      onResizeNode,
      onMoveNode,
    ]
  );

  /**
   * maps canvas connections to react-flow edges.
   * @returns array of react-flow edges
   */
  const flowEdges = useMemo<FlowEdge<BehaviorEdgeData>[]>(
    () =>
      connections.map((connection) => ({
        id: connection.id,
        source: connection.sourceNodeId,
        target: connection.targetNodeId,
        sourceHandle: connection.sourcePort
          ? `source-${connection.sourcePort}`
          : undefined,
        targetHandle: connection.targetPort
          ? `target-${connection.targetPort}`
          : undefined,
        type: "btEdge" as const,
        animated: false,
        data: {
          onRemoveConnection,
        },
        markerEnd: {
          type: MarkerType.ArrowClosed,
          color: "#fff",
          width: 16,
          height: 16,
        },
      })),
    [connections, onRemoveConnection]
  );

  /**
   * handles drag over events on the canvas.
   * @param event drag event
   */
  const handleDragOver: React.DragEventHandler<HTMLDivElement> = useCallback(
    (event) => {
      if (!isSidebarDrag(event.dataTransfer.types)) {
        return;
      }

      event.preventDefault();
      event.dataTransfer.dropEffect = "copy";
      setIsActive(true);
    },
    []
  );

  /**
   * handles drag leave events on the canvas.
   * @param event drag event
   */
  const handleDragLeave: React.DragEventHandler<HTMLDivElement> = useCallback(
    (event) => {
      const nextTarget = event.relatedTarget;
      if (
        nextTarget instanceof Element &&
        wrapperRef.current?.contains(nextTarget)
      ) {
        return;
      }

      setIsActive(false);
    },
    []
  );

  /**
   * handles drop events on the canvas.
   * @param event drop event
   */
  const handleDrop: React.DragEventHandler<HTMLDivElement> = useCallback(
    (event) => {
      if (!isSidebarDrag(event.dataTransfer.types)) {
        return;
      }

      event.preventDefault();
      setIsActive(false);

      const rawPayload = event.dataTransfer.getData(DRAG_DATA_FORMAT);
      if (!rawPayload) {
        return;
      }

      try {
        const payload = JSON.parse(rawPayload) as DraggedSidebarItem;
        const bounds = wrapperRef.current?.getBoundingClientRect();
        if (!bounds) {
          return;
        }

        const position = project({
          x: event.clientX - bounds.left,
          y: event.clientY - bounds.top,
        });

        onDropNode(payload, position);
      } catch (error) {
        console.error("Failed to parse sidebar drag payload", error);
      }
    },
    [onDropNode, project]
  );

  /**
   * handles connection events on the canvas.
   * @param connection connection data from react-flow
   */
  const handleConnect = useCallback(
    (connection: Connection) => {
      if (!onAddConnection || !connection.source || !connection.target) {
        return;
      }

      const sourcePort = resolvePortFromHandle(connection.sourceHandle, "right");
      const targetPort = resolvePortFromHandle(connection.targetHandle, "left");
      onAddConnection(
        connection.source,
        connection.target,
        sourcePort,
        targetPort
      );
    },
    [onAddConnection]
  );

  /**
   * handles node drag events on the canvas.
   * @param event mouse event
   * @param node dragged node data
   */
  const handleNodeDrag = useCallback(
    (_event: React.MouseEvent, node: FlowNode<BehaviorNodeData>) => {
      const width = node.width ?? DEFAULT_CANVAS_NODE_WIDTH;
      const height = node.height ?? DEFAULT_CANVAS_NODE_HEIGHT;
      onMoveNode?.(node.id, {
        x: node.position.x + width / 2,
        y: node.position.y + height / 2,
      });
    },
    [onMoveNode]
  );

  /**
   * handles node drag stop events on the canvas.
   * @param event mouse event
   * @param node dragged node data
   */
  const handleNodeDragStop = useCallback(
    (_event: React.MouseEvent, node: FlowNode<BehaviorNodeData>) => {
      const width = node.width ?? DEFAULT_CANVAS_NODE_WIDTH;
      const height = node.height ?? DEFAULT_CANVAS_NODE_HEIGHT;
      onMoveNode?.(node.id, {
        x: node.position.x + width / 2,
        y: node.position.y + height / 2,
      });
    },
    [onMoveNode]
  );

  return (
    <div
      ref={wrapperRef}
      className={`editor-canvas${isActive ? " is-active" : ""}`}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
    >
      <ReactFlow
        nodes={flowNodes}
        edges={flowEdges}
        nodeTypes={nodeTypes}
        edgeTypes={edgeTypes}
        onConnect={handleConnect}
        onNodeDrag={handleNodeDrag}
        onNodeDragStop={handleNodeDragStop}
        connectionLineType={ConnectionLineType.SmoothStep}
        proOptions={{ hideAttribution: true }}
        panOnDrag
        fitView
        nodesDraggable
        nodesConnectable
        nodesFocusable
        elementsSelectable
      >
        <Background
          variant={BackgroundVariant.Cross}
          gap={32}
          size={2}
          color="rgba(99, 102, 241, 0.25)"
        />
      </ReactFlow>
    </div>
  );
}

export default function EditorCanvas(props: EditorCanvasProps) {
  return (
    <ReactFlowProvider>
      <EditorCanvasInner {...props} />
    </ReactFlowProvider>
  );
}
