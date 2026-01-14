import { useCallback, useEffect, useState } from "react";
import { useReactFlow, useViewport } from "reactflow";
import type { HierarchySeparator } from "./types";

export default function HierarchyLevelSeparators({
  separators,
  onMove,
  onRemove,
}: {
  separators: HierarchySeparator[];
  onMove?: (id: string, y: number) => void;
  onRemove?: (id: string) => void;
}) {
  const { screenToFlowPosition } = useReactFlow();
  const viewport = useViewport();
  const [draggingId, setDraggingId] = useState<string | null>(null);
  const [hoveredId, setHoveredId] = useState<string | null>(null);

  /**
   * handles mouse down event on a separator line to initiate dragging.
   */
  const handleMouseDown = useCallback((id: string, event: React.MouseEvent) => {
    event.preventDefault();
    event.stopPropagation();
    setDraggingId(id);
  }, []);

  /**
   * handles mouse move event to update the position of the dragged separator.
   */
  const handleMouseMove = useCallback(
    (event: MouseEvent) => {
      if (!draggingId || !onMove) return;

      const flowPos = screenToFlowPosition({ x: event.clientX, y: event.clientY });
      onMove(draggingId, flowPos.y);
    },
    [draggingId, onMove, screenToFlowPosition]
  );

  /**
   * handles mouse up event to end dragging.
   */
  const handleMouseUp = useCallback(() => {
    setDraggingId(null);
  }, []);

  useEffect(() => {
    if (draggingId !== null) {
      window.addEventListener("mousemove", handleMouseMove);
      window.addEventListener("mouseup", handleMouseUp);
      return () => {
        window.removeEventListener("mousemove", handleMouseMove);
        window.removeEventListener("mouseup", handleMouseUp);
      };
    }
  }, [draggingId, handleMouseMove, handleMouseUp]);

  return (
    <>
      <svg
        style={{
          position: "absolute",
          top: 0,
          left: 0,
          width: "100%",
          height: "100%",
          pointerEvents: "none",
          zIndex: 0,
        }}
      >
        {separators.map((separator) => {
          const screenY = separator.y * viewport.zoom + viewport.y;
          const isDragging = draggingId === separator.id;
          const isHovered = hoveredId === separator.id;

          return (
            <g key={separator.id}>
              <line
                x1={0}
                y1={screenY}
                x2="100%"
                y2={screenY}
                stroke={
                  isDragging
                    ? "rgba(99, 102, 241, 0.8)"
                    : isHovered
                      ? "rgba(255, 255, 255, 0.6)"
                      : "rgba(255, 255, 255, 0.3)"
                }
                strokeWidth={isDragging ? 3 : 2}
                strokeDasharray="10 5"
              />
              {separator.label && (
                <text
                  x={20}
                  y={screenY - 10}
                  fill={
                    isDragging
                      ? "rgba(99, 102, 241, 1)"
                      : "rgba(255, 255, 255, 0.6)"
                  }
                  fontSize="14"
                  fontWeight="500"
                  style={{ pointerEvents: "none" }}
                >
                  {separator.label}
                </text>
              )}
            </g>
          );
        })}
      </svg>

      {separators.map((separator) => {
        const screenY = separator.y * viewport.zoom + viewport.y;
        const isHovered = hoveredId === separator.id;
        return (
          <div key={separator.id}>
            <div
              onMouseEnter={() => setHoveredId(separator.id)}
              onMouseLeave={() => setHoveredId(null)}
              style={{
                position: "absolute",
                top: screenY - 12,
                left: 0,
                width: "100%",
                height: 24,
                zIndex: 100,
                pointerEvents: "all",
              }}
            >
              <div
                onMouseDown={(e) => handleMouseDown(separator.id, e)}
                style={{
                  position: "absolute",
                  top: 2,
                  left: 0,
                  width: "100%",
                  height: 20,
                  cursor: draggingId === separator.id ? "grabbing" : "ns-resize",
                  zIndex: 100,
                }}
                title="Drag to move, click × to remove"
              />
              {isHovered && onRemove && (
                <button
                  type="button"
                  onMouseDown={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                  }}
                  onClick={(e) => {
                    e.stopPropagation();
                    onRemove(separator.id);
                  }}
                  style={{
                    position: "absolute",
                    top: 0,
                    right: 20,
                    width: 24,
                    height: 24,
                    borderRadius: "50%",
                    border: "1px solid rgba(239, 68, 68, 0.7)",
                    background: "rgba(239, 68, 68, 0.15)",
                    color: "#ef4444",
                    fontSize: "16px",
                    fontWeight: 700,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    cursor: "pointer",
                    zIndex: 101,
                    transition: "all 0.2s",
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = "rgba(239, 68, 68, 0.25)";
                    e.currentTarget.style.transform = "scale(1.1)";
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = "rgba(239, 68, 68, 0.15)";
                    e.currentTarget.style.transform = "scale(1)";
                  }}
                  aria-label="Remove separator"
                >
                  ×
                </button>
              )}
            </div>
          </div>
        );
      })}
    </>
  );
}
