import { useCallback, useMemo, useRef, useState } from "react";
import type { MouseEvent } from "react";
import {
  DRAG_DATA_FORMAT,
  isSidebarDrag,
  type DraggedSidebarItem,
} from "./dragTypes";
import type { CanvasNode, EditorCanvasProps } from "./types";
import "./EditorCanvas.css";
import type {
  PredicateInstance,
  PredicateType,
} from "../sidebar/utils/types";

/**
 * builds a lookup table so predicate instances can resolve their type metadata quickly.
 */
function createPredicateTypeMap(predicateTypes?: PredicateType[]) {
  return new Map(predicateTypes?.map((type) => [type.id, type]) ?? []);
}

/**
 * Creates a short, human friendly description for a predicate instance.
 * Includes resolved property names when the predicate type definition is available.
 */
function formatPredicateSummary(
  predicate: PredicateInstance,
  predicateTypeMap: Map<string, PredicateType>
): string {
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

/** returns true when the node represents an action (type or instance). */
function isActionNode(node: CanvasNode) {
  return node.kind === "actionType" || node.kind === "actionInstance";
}

/**
 * renders the central editor canvas and accepts sidebar items via drag-and-drop.
 * @param props rendered nodes and drop callback supplied by the parent
 * @returns interactive canvas surface populated with dropped nodes
 */
export default function EditorCanvas({
  nodes,
  connections = [],
  onDropNode,
  onMoveNode,
  onRemoveNode,
  onAddConnection,
  onRemoveConnection,
  onAddActionPrecondition,
  onAddActionEffect,
  onCycleFlowSuccessType,
  onEditActionPredicate,
  onRemoveActionPredicate,
  predicateTypes,
}: EditorCanvasProps) {
  const [isActive, setIsActive] = useState(false);
  const [connectingFrom, setConnectingFrom] = useState<{ nodeId: string; port: 'top' | 'right' | 'bottom' | 'left' } | null>(null);
  const [tempConnectionEnd, setTempConnectionEnd] = useState<{ x: number; y: number } | null>(null);
  const [hoveredPort, setHoveredPort] = useState<{ nodeId: string; port: 'top' | 'right' | 'bottom' | 'left' } | null>(null);
  const dragOffset = useRef<{ x: number; y: number }>({ x: 0, y: 0 });
  const canvasRef = useRef<HTMLDivElement>(null);
  const nodeRefs = useRef<Map<string, HTMLDivElement>>(new Map());
  const predicateTypeMap = useMemo(
    () => createPredicateTypeMap(predicateTypes),
    [predicateTypes]
  );

  /** prevents drag interference and delegates predicate edit/remove requests. */
  const handlePredicateAction = (
    event: MouseEvent<HTMLButtonElement>,
    handler:
      | EditorCanvasProps["onEditActionPredicate"]
      | EditorCanvasProps["onRemoveActionPredicate"],
    nodeId: string,
    predicateId: string,
    collection: "precondition" | "effect"
  ) => {
    event.stopPropagation();

    if (!handler) {
      return;
    }

    handler(nodeId, predicateId, collection);
  };

  /** renders a predicate collection (preconditions or effects) for a node. */
  const renderPredicateCollection = (
    nodeId: string,
    items: PredicateInstance[],
    collection: "precondition" | "effect"
  ) => {
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
                  {onEditActionPredicate ? (
                    <button
                      type="button"
                      className="canvas-node-state-btn"
                      onMouseDown={(event) => {
                        event.stopPropagation();
                      }}
                      onClick={(event) =>
                        handlePredicateAction(
                          event,
                          onEditActionPredicate,
                          nodeId,
                          predicate.id,
                          collection
                        )
                      }
                      title={`Edit ${collection}`}
                      aria-label={`Edit ${collection}`}
                    >
                      E
                    </button>
                  ) : null}
                  {onRemoveActionPredicate ? (
                    <button
                      type="button"
                      className="canvas-node-state-btn"
                      onMouseDown={(event) => {
                        event.stopPropagation();
                      }}
                      onClick={(event) =>
                        handlePredicateAction(
                          event,
                          onRemoveActionPredicate,
                          nodeId,
                          predicate.id,
                          collection
                        )
                      }
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
  };

  /** handles drag enter events on the canvas area. */
  const handleDragEnter: React.DragEventHandler<HTMLDivElement> = (event) => {
    if (!isSidebarDrag(event.dataTransfer.types)) {
      return;
    }

    event.preventDefault();
    setIsActive(true);
  };

  /** handles drag over events on the canvas area. */
  const handleDragOver: React.DragEventHandler<HTMLDivElement> = (event) => {
    if (!isSidebarDrag(event.dataTransfer.types)) {
      return;
    }

    event.preventDefault();
    event.dataTransfer.dropEffect = "copy";
  };

  /** handles drag leave events on the canvas area. */
  const handleDragLeave: React.DragEventHandler<HTMLDivElement> = (event) => {
    if (event.currentTarget.contains(event.relatedTarget as Node)) {
      return;
    }

    setIsActive(false);
  };

  /** handles drop events on the canvas area. */
  const handleDrop: React.DragEventHandler<HTMLDivElement> = (event) => {
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
      const canvasRect = event.currentTarget.getBoundingClientRect();
      onDropNode(payload, {
        x: event.clientX - canvasRect.left,
        y: event.clientY - canvasRect.top,
      });
    } catch (error) {
      console.error("Failed to parse sidebar drag payload", error);
    }
  };

  const setNodeRef = useCallback((nodeId: string, element: HTMLDivElement | null) => {
    if (element) {
      nodeRefs.current.set(nodeId, element);
    } else {
      nodeRefs.current.delete(nodeId);
    }
  }, []);

  /**
   * calculates the port position for a given node and port side.
   */
  const getPortPosition = useCallback((nodeId: string, port: 'top' | 'right' | 'bottom' | 'left'): { x: number; y: number } | null => {
    const node = nodes.find(n => n.id === nodeId);
    if (!node) {
      return null;
    }

    // Minimal offsets - arrows directly at node edge
    const horizontalOffset = 90; // Distance to left/right edge
    const verticalOffset = 58; // Distance to top/bottom edge
    
    switch (port) {
      case 'top':
        return { x: node.x, y: node.y - verticalOffset };
      case 'right':
        return { x: node.x + horizontalOffset, y: node.y };
      case 'bottom':
        return { x: node.x, y: node.y + verticalOffset };
      case 'left':
        return { x: node.x - horizontalOffset, y: node.y };
    }
  }, [nodes]);

  /**
   * handles starting a connection from a node's port.
   */
  const handlePortMouseDown = useCallback((nodeId: string, port: 'top' | 'right' | 'bottom' | 'left', event: MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    event.preventDefault();
    setConnectingFrom({ nodeId, port });
  }, []);

  /**
   * handles mouse entering a port for snap effect.
   */
  const handlePortMouseEnter = useCallback((nodeId: string, port: 'top' | 'right' | 'bottom' | 'left') => {
    if (connectingFrom) {
      setHoveredPort({ nodeId, port });
    }
  }, [connectingFrom]);

  /**
   * handles mouse leaving a port.
   */
  const handlePortMouseLeave = useCallback(() => {
    setHoveredPort(null);
  }, []);

  /**
   * handles mouse movement during connection creation.
   */
  const handleCanvasMouseMove = useCallback((event: React.MouseEvent<HTMLDivElement>) => {
    if (connectingFrom && canvasRef.current) {
      const rect = canvasRef.current.getBoundingClientRect();
      
      // Snap to hovered port if available
      if (hoveredPort) {
        const portPos = getPortPosition(hoveredPort.nodeId, hoveredPort.port);
        if (portPos) {
          setTempConnectionEnd(portPos);
          return;
        }
      }
      
      setTempConnectionEnd({
        x: event.clientX - rect.left,
        y: event.clientY - rect.top,
      });
    }
  }, [connectingFrom, hoveredPort, getPortPosition]);

  /**
   * handles mouse up to complete or cancel connection.
   */
  const handleCanvasMouseUp = useCallback(() => {
    if (connectingFrom && hoveredPort && onAddConnection) {
      // Complete connection if dropped on a valid port
      if (connectingFrom.nodeId !== hoveredPort.nodeId) {
        onAddConnection(
          connectingFrom.nodeId,
          hoveredPort.nodeId,
          connectingFrom.port,
          hoveredPort.port
        );
      }
    }
    
    setConnectingFrom(null);
    setTempConnectionEnd(null);
    setHoveredPort(null);
  }, [connectingFrom, hoveredPort, onAddConnection]);

  /**
   * renders a straight arrow between two points.
   */
  const renderConnection = (
    start: { x: number; y: number },
    end: { x: number; y: number },
    id?: string,
    isTemp = false
  ) => {
    // Calculate angle for arrowhead
    const angle = Math.atan2(end.y - start.y, end.x - start.x);
    const arrowSize = 10;
    
    // Arrowhead points
    const arrowPoint1 = {
      x: end.x - arrowSize * Math.cos(angle - Math.PI / 6),
      y: end.y - arrowSize * Math.sin(angle - Math.PI / 6),
    };
    const arrowPoint2 = {
      x: end.x - arrowSize * Math.cos(angle + Math.PI / 6),
      y: end.y - arrowSize * Math.sin(angle + Math.PI / 6),
    };

    return (
      <g key={id || "temp"}>
        {/* Main line */}
        <line
          x1={start.x}
          y1={start.y}
          x2={end.x}
          y2={end.y}
          className={`canvas-connection${isTemp ? " is-temp" : ""}`}
          strokeWidth="2"
        />
        {/* Arrowhead */}
        <polygon
          points={`${end.x},${end.y} ${arrowPoint1.x},${arrowPoint1.y} ${arrowPoint2.x},${arrowPoint2.y}`}
          className={`canvas-connection-arrow${isTemp ? " is-temp" : ""}`}
        />
        {/* Delete button */}
        {!isTemp && onRemoveConnection && id && (
          <text
            x={(start.x + end.x) / 2}
            y={(start.y + end.y) / 2}
            className="canvas-connection-remove"
            textAnchor="middle"
            dominantBaseline="central"
            style={{ cursor: 'pointer', pointerEvents: 'all' }}
            onClick={(event) => {
              event.stopPropagation();
              onRemoveConnection(id);
            }}
          >
            ×
          </text>
        )}
      </g>
    );
  };

  return (
    <div
      ref={canvasRef}
      className={`editor-canvas${isActive ? " is-active" : ""}`}
      onDragEnter={handleDragEnter}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      onMouseMove={handleCanvasMouseMove}
      onMouseUp={handleCanvasMouseUp}
      role="presentation"
    >
      {/* SVG layer for connections */}
      <svg className="canvas-connections-layer">
        {/* Render existing connections */}
        {connections.map((connection) => {
          // Use stored port information, fallback to right/left
          const sourcePort = connection.sourcePort || 'right';
          const targetPort = connection.targetPort || 'left';
          const start = getPortPosition(connection.sourceNodeId, sourcePort);
          const end = getPortPosition(connection.targetNodeId, targetPort);
          
          if (!start || !end) {
            return null;
          }

          return renderConnection(start, end, connection.id);
        })}

        {/* Render temporary connection being drawn */}
        {connectingFrom && tempConnectionEnd && (() => {
          const start = getPortPosition(connectingFrom.nodeId, connectingFrom.port);
          if (!start) {
            return null;
          }
          return renderConnection(start, tempConnectionEnd, undefined, true);
        })()}
      </svg>

      {nodes.length === 0
        ? null
        : nodes.map((node) => {
            const preconditions = node.preconditions ?? [];
            const effects = node.effects ?? [];

            return (
              <div
                key={node.id}
                className={`canvas-node canvas-node-${node.kind}`}
                style={{ left: node.x, top: node.y }}
                draggable
                ref={(element) => setNodeRef(node.id, element)}
                data-node-id={node.id}
                onDragStart={(event) => {
                  if (!onMoveNode) {
                    event.preventDefault();
                    return;
                  }

                  const rect = event.currentTarget.getBoundingClientRect();
                  dragOffset.current = {
                    x: event.clientX - rect.left,
                    y: event.clientY - rect.top,
                  };
                  event.dataTransfer.setData("text/plain", node.name);
                  event.dataTransfer.effectAllowed = "move";
                }}
                onDrag={(event) => {
                  if (!onMoveNode) {
                    return;
                  }

                  const canvas = event.currentTarget.parentElement;
                  if (!canvas) {
                    return;
                  }

                  const canvasRect = canvas.getBoundingClientRect();
                  const nodeRect = event.currentTarget.getBoundingClientRect();

                  const nextPosition = {
                    x:
                      event.clientX - canvasRect.left - dragOffset.current.x +
                      nodeRect.width / 2,
                    y:
                      event.clientY - canvasRect.top - dragOffset.current.y +
                      nodeRect.height / 2,
                  };

                  onMoveNode(node.id, nextPosition);
                }}
                onDragEnd={(event) => {
                  if (!onMoveNode) {
                    return;
                  }

                  const canvas = event.currentTarget.parentElement;
                  if (!canvas) {
                    return;
                  }

                  const canvasRect = canvas.getBoundingClientRect();
                  const nextPosition = {
                    x:
                      event.clientX - canvasRect.left - dragOffset.current.x +
                      event.currentTarget.offsetWidth / 2,
                    y:
                      event.clientY - canvasRect.top - dragOffset.current.y +
                      event.currentTarget.offsetHeight / 2,
                  };

                  onMoveNode(node.id, nextPosition);
                }}
              >
                {node.successType ? (
                  onCycleFlowSuccessType ? (
                    <button
                      type="button"
                      className="canvas-node-success"
                      onMouseDown={(event) => event.stopPropagation()}
                      onClick={(event) => {
                        event.stopPropagation();
                        onCycleFlowSuccessType(node.id);
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
                {onRemoveNode ? (
                  <button
                    type="button"
                    className="canvas-node-remove"
                    onMouseDown={(event) => {
                      event.stopPropagation();
                    }}
                    onClick={(event) => {
                      event.stopPropagation();
                      onRemoveNode(node.id);
                    }}
                    aria-label={`Remove ${node.name}`}
                  >
                    X
                  </button>
                ) : null}
                <span className="canvas-node-label">{node.name}</span>
                <span className="canvas-node-meta">{node.typeLabel}</span>
                {node.isNegated ? (
                  <span className="canvas-node-badge" aria-label="Negated predicate">
                    NOT
                  </span>
                ) : null}

                {onAddConnection && (
                  <>
                    <button
                      type="button"
                      className={`canvas-node-port canvas-node-port-top${
                        hoveredPort?.nodeId === node.id && hoveredPort?.port === 'top' ? ' is-hovered' : ''
                      }`}
                      onMouseDown={(event) => handlePortMouseDown(node.id, 'top', event)}
                      onMouseEnter={() => handlePortMouseEnter(node.id, 'top')}
                      onMouseLeave={handlePortMouseLeave}
                      title="Connection port"
                      aria-label="Top connection port"
                    >
                      <span className="canvas-node-port-dot" />
                    </button>
                    {/* Right port */}
                    <button
                      type="button"
                      className={`canvas-node-port canvas-node-port-right${
                        hoveredPort?.nodeId === node.id && hoveredPort?.port === 'right' ? ' is-hovered' : ''
                      }`}
                      onMouseDown={(event) => handlePortMouseDown(node.id, 'right', event)}
                      onMouseEnter={() => handlePortMouseEnter(node.id, 'right')}
                      onMouseLeave={handlePortMouseLeave}
                      title="Connection port"
                      aria-label="Right connection port"
                    >
                      <span className="canvas-node-port-dot" />
                    </button>
                    {/* Bottom port */}
                    <button
                      type="button"
                      className={`canvas-node-port canvas-node-port-bottom${
                        hoveredPort?.nodeId === node.id && hoveredPort?.port === 'bottom' ? ' is-hovered' : ''
                      }`}
                      onMouseDown={(event) => handlePortMouseDown(node.id, 'bottom', event)}
                      onMouseEnter={() => handlePortMouseEnter(node.id, 'bottom')}
                      onMouseLeave={handlePortMouseLeave}
                      title="Connection port"
                      aria-label="Bottom connection port"
                    >
                      <span className="canvas-node-port-dot" />
                    </button>
                    {/* Left port */}
                    <button
                      type="button"
                      className={`canvas-node-port canvas-node-port-left${
                        hoveredPort?.nodeId === node.id && hoveredPort?.port === 'left' ? ' is-hovered' : ''
                      }`}
                      onMouseDown={(event) => handlePortMouseDown(node.id, 'left', event)}
                      onMouseEnter={() => handlePortMouseEnter(node.id, 'left')}
                      onMouseLeave={handlePortMouseLeave}
                      title="Connection port"
                      aria-label="Left connection port"
                    >
                      <span className="canvas-node-port-dot" />
                    </button>
                  </>
                )}

                {isActionNode(node) ? (
                  <div className="canvas-node-state">
                    {(onAddActionPrecondition || onAddActionEffect) && (
                      <div className="canvas-node-actions">
                        {onAddActionPrecondition ? (
                          <button
                            type="button"
                            className="canvas-node-action-btn"
                            onMouseDown={(event) => {
                              event.stopPropagation();
                            }}
                            onClick={(event) => {
                              event.stopPropagation();
                              onAddActionPrecondition(node.id);
                            }}
                            title="Add precondition"
                            aria-label="Add precondition"
                          >
                            Preconditions +
                          </button>
                        ) : null}
                        {onAddActionEffect ? (
                          <button
                            type="button"
                            className="canvas-node-action-btn"
                            onMouseDown={(event) => {
                              event.stopPropagation();
                            }}
                            onClick={(event) => {
                              event.stopPropagation();
                              onAddActionEffect(node.id);
                            }}
                            title="Add effect"
                            aria-label="Add effect"
                          >
                            Effects +
                          </button>
                        ) : null}
                      </div>
                    )}

                    {renderPredicateCollection(node.id, preconditions, "precondition")}
                    {renderPredicateCollection(node.id, effects, "effect")}
                  </div>
                ) : null}
              </div>
            );
          })}
    </div>
  );
}
