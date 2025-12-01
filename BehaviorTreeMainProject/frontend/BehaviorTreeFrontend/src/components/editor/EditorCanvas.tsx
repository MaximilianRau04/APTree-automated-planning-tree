import { useMemo, useRef, useState } from "react";
import { DRAG_DATA_FORMAT, isSidebarDrag, type DraggedSidebarItem } from "./dragTypes";
import type { CanvasNode } from "./types";
import "./EditorCanvas.css";

export interface EditorCanvasProps {
  nodes: CanvasNode[];
  onDropNode: (
    item: DraggedSidebarItem,
    position: { x: number; y: number }
  ) => void;
  onMoveNode?: (
    nodeId: string,
    position: { x: number; y: number }
  ) => void;
}

/**
 * renders the central editor canvas and accepts sidebar items via drag-and-drop.
 * @param props rendered nodes and drop callback supplied by the parent
 * @returns interactive canvas surface populated with dropped nodes
 */
export default function EditorCanvas({
  nodes,
  onDropNode,
  onMoveNode,
}: EditorCanvasProps) {
  const [isActive, setIsActive] = useState(false);
  const dragOffset = useRef<{ x: number; y: number }>({ x: 0, y: 0 });

  const emptyStateCopy = useMemo(
    () => "Drag types or instances from the sidebar to start planning.",
    []
  );

  /**
   * handles drag enter events on the canvas area.
   * @param event DragEvent<HTMLDivElement>
   * @returns void
   */
  const handleDragEnter: React.DragEventHandler<HTMLDivElement> = (event) => {
    if (!isSidebarDrag(event.dataTransfer.types)) {
      return;
    }

    event.preventDefault();
    setIsActive(true);
  };

  /**
   * handles drag over events on the canvas area.
   * @param event DragEvent<HTMLDivElement>
   * @returns void
   */
  const handleDragOver: React.DragEventHandler<HTMLDivElement> = (event) => {
    if (!isSidebarDrag(event.dataTransfer.types)) {
      return;
    }

    event.preventDefault();
    event.dataTransfer.dropEffect = "copy";
  };

  /**
   * handles drag leave events on the canvas area.
   * @param event DragEvent<HTMLDivElement>
   * @returns void
   */
  const handleDragLeave: React.DragEventHandler<HTMLDivElement> = (event) => {
    if (event.currentTarget.contains(event.relatedTarget as Node)) {
      return;
    }

    setIsActive(false);
  };

  /**
   * handles drop events on the canvas area.
   * @param event DragEvent<HTMLDivElement>
   * @returns void
   */
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

  return (
    <div
      className={`editor-canvas${isActive ? " is-active" : ""}`}
      onDragEnter={handleDragEnter}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      role="presentation"
    >
      {nodes.length === 0 ? (
        <div className="canvas-placeholder">{emptyStateCopy}</div>
      ) : (
        nodes.map((node) => (
          <div
            key={node.id}
            className={`canvas-node canvas-node-${node.kind}`}
            style={{ left: node.x, top: node.y }}
            draggable
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
            <span className="canvas-node-label">{node.name}</span>
            <span className="canvas-node-meta">{node.typeLabel}</span>
            {node.isNegated ? (
              <span className="canvas-node-badge" aria-label="Negated predicate">
                NOT
              </span>
            ) : null}
          </div>
        ))
      )}
    </div>
  );
}
