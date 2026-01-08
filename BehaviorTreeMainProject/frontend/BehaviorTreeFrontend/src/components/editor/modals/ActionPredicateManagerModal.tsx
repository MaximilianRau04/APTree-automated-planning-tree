import { useMemo, useState } from "react";
import type { PredicateInstance, PredicateType } from "../../sidebar/utils/types";
import "./ActionPredicateManagerModal.css";

type ActionPredicateCollection = "precondition" | "effect";

type ViewMode = "assigned" | "available";

/**
 * format the property entries for display in the details pane
 * @param predicate  
 * @param predicateTypeMap  
 * @returns formatted property entries 
 */
const formatPropertyEntries = (
  predicate: PredicateInstance,
  predicateTypeMap: Map<string, PredicateType>
): Array<{ label: string; value: string }> => {
  const type = predicateTypeMap.get(predicate.typeId);
  const entries = Object.entries(predicate.propertyValues ?? {});

  return entries
    .map(([propertyId, value]) => {
      const propertyName = type?.properties.find(
        (property) => property.id === propertyId
      )?.name;
      return {
        label: propertyName ? propertyName : propertyId,
        value,
      };
    })
    .filter((entry) => entry.label.trim().length > 0);
};

interface ActionPredicateManagerModalProps {
  isOpen: boolean;
  nodeName: string;
  nodeTypeLabel: string;
  collection: ActionPredicateCollection;
  assigned: PredicateInstance[];
  available: PredicateInstance[];
  predicateTypeMap: Map<string, PredicateType>;
  onClose: () => void;
  onAdd: (predicateId: string) => void;
  onRemove: (predicateId: string) => void;
  onEdit?: (predicateId: string) => void;
  onCreateNew?: () => void;
}

export default function ActionPredicateManagerModal({
  isOpen,
  nodeName,
  nodeTypeLabel,
  collection,
  assigned,
  available,
  predicateTypeMap,
  onClose,
  onAdd,
  onRemove,
  onEdit,
  onCreateNew,
}: ActionPredicateManagerModalProps) {
  const [view, setView] = useState<ViewMode>("assigned");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [wasOpen, setWasOpen] = useState(false);

  if (isOpen && !wasOpen) {
    setWasOpen(true);
    setView("assigned");
    if (!selectedId || !assigned.some((p) => p.id === selectedId)) {
      setSelectedId(assigned[0]?.id ?? null);
    }
  } else if (!isOpen && wasOpen) {
    setWasOpen(false);
  }

  const titlePrefix = collection === "effect" ? "Effects" : "Preconditions";

  const selectedPredicate = useMemo(() => {
    const pool = view === "available" ? available : assigned;
    if (!selectedId) {
      return pool[0] ?? null;
    }
    return pool.find((p) => p.id === selectedId) ?? pool[0] ?? null;
  }, [assigned, available, selectedId, view]);

  const selectedPropertyEntries = useMemo(() => {
    if (!selectedPredicate) {
      return [];
    }
    return formatPropertyEntries(selectedPredicate, predicateTypeMap);
  }, [predicateTypeMap, selectedPredicate]);

  const listItems = view === "available" ? available : assigned;

  if (!isOpen) {
    return null;
  }

  const emptyListLabel =
    view === "available"
      ? "No predicate instances available. Create predicate instances in the sidebar first."
      : `No ${collection === "effect" ? "effects" : "preconditions"} attached yet.`;

  const canAddSelected =
    view === "available" && selectedPredicate ? true : false;

  return (
    <div className="modal-overlay action-predicate-overlay" role="dialog" aria-modal="true">
      <div
        className="modal-content action-predicate-modal"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="modal-header">
          <div className="action-predicate-header">
            <h3 className="action-predicate-title">{titlePrefix}</h3>
            <div className="action-predicate-subtitle">
              <span className="action-predicate-node">{nodeName}</span>
              <span className="action-predicate-node-type">{nodeTypeLabel}</span>
            </div>
          </div>
          <button
            type="button"
            className="modal-close-btn"
            onClick={onClose}
            aria-label="Close dialog"
          >
            ×
          </button>
        </div>

        <div className="modal-form action-predicate-body">
          <div className="action-predicate-left">
            <div className="action-predicate-toolbar">
              <div className="action-predicate-meta" aria-hidden="true" />
              {view === "assigned" ? (
                <button
                  type="button"
                  className="btn-save action-predicate-add-btn"
                  onClick={() => {
                    setView("available");
                    setSelectedId((prev) => {
                      if (prev && available.some((p) => p.id === prev)) {
                        return prev;
                      }
                      return available[0]?.id ?? null;
                    });
                  }}
                >
                  Add…
                </button>
              ) : (
                <button
                  type="button"
                  className="btn-cancel action-predicate-back-btn"
                  onClick={() => {
                    setView("assigned");
                    setSelectedId((prev) => {
                      if (prev && assigned.some((p) => p.id === prev)) {
                        return prev;
                      }
                      return assigned[0]?.id ?? null;
                    });
                  }}
                >
                  Back
                </button>
              )}
            </div>

            <div className="action-predicate-list" role="list">
              {listItems.length === 0 ? (
                <div className="action-predicate-empty">
                  <p className="canvas-node-state-empty">{emptyListLabel}</p>
                  {view === "available" && onCreateNew ? (
                    <button
                      type="button"
                      className="btn-save"
                      onClick={onCreateNew}
                    >
                      Create New…
                    </button>
                  ) : null}
                </div>
              ) : (
                listItems.map((predicate) => {
                  const isSelected = predicate.id === selectedPredicate?.id;
                  const displayName = predicate.name || predicate.type;
                  const negationPrefix = predicate.isNegated ? "NOT " : "";

                  return (
                    <div
                      key={predicate.id}
                      className={`action-predicate-list-item${isSelected ? " is-selected" : ""}`}
                      role="button"
                      tabIndex={0}
                      onClick={() => setSelectedId(predicate.id)}
                      onKeyDown={(event) => {
                        if (event.key === "Enter" || event.key === " ") {
                          event.preventDefault();
                          setSelectedId(predicate.id);
                        }
                      }}
                    >
                      <div className="action-predicate-list-body">
                        <div className="action-predicate-list-title">
                          {negationPrefix}
                          {displayName}
                        </div>
                        <div className="action-predicate-list-subtitle">{predicate.type}</div>
                      </div>

                      {view === "assigned" ? (
                        <button
                          type="button"
                          className="canvas-node-state-btn action-predicate-remove"
                          onClick={(event) => {
                            event.stopPropagation();
                            onRemove(predicate.id);
                          }}
                          title={`Remove ${collection}`}
                          aria-label={`Remove ${collection}`}
                        >
                          X
                        </button>
                      ) : null}
                    </div>
                  );
                })
              )}
            </div>

            {view === "available" ? (
              <div className="action-predicate-footer">
                {onCreateNew ? (
                  <button type="button" className="btn-cancel" onClick={onCreateNew}>
                    Create New…
                  </button>
                ) : (
                  <span />
                )}
                <button
                  type="button"
                  className="btn-cancel"
                  onClick={() => {
                    setView("assigned");
                    setSelectedId(assigned[0]?.id ?? null);
                  }}
                >
                  Cancel
                </button>
                <button
                  type="button"
                  className="btn-save"
                  disabled={!canAddSelected || !selectedPredicate}
                  onClick={() => {
                    if (!selectedPredicate) {
                      return;
                    }
                    onAdd(selectedPredicate.id);
                    setView("assigned");
                    setSelectedId(selectedPredicate.id);
                  }}
                >
                  Add Selected
                </button>
              </div>
            ) : null}
          </div>

          <div className="action-predicate-right" aria-label="Predicate details">
            {selectedPredicate ? (
              <>
                <div className="action-predicate-detail-header">
                  <div className="action-predicate-detail-title">
                    {selectedPredicate.isNegated ? "NOT " : ""}
                    {selectedPredicate.name || selectedPredicate.type}
                  </div>
                  {view === "assigned" && onEdit ? (
                    <button
                      type="button"
                      className="canvas-node-state-btn"
                      onClick={() => onEdit(selectedPredicate.id)}
                      title={`Edit ${collection}`}
                      aria-label={`Edit ${collection}`}
                    >
                      ✎
                    </button>
                  ) : null}
                </div>

                <div className="action-predicate-detail-meta">
                  <div>
                    <span className="action-predicate-detail-label">Type</span>
                    <div className="action-predicate-detail-value">
                      {selectedPredicate.type || "Unknown"}
                    </div>
                  </div>
                  <div>
                    <span className="action-predicate-detail-label">Negated</span>
                    <div className="action-predicate-detail-value">
                      {selectedPredicate.isNegated ? "Yes" : "No"}
                    </div>
                  </div>
                </div>

                <div className="action-predicate-detail-section">
                  <span className="action-predicate-detail-label">Arguments</span>
                  {selectedPropertyEntries.length === 0 ? (
                    <div className="action-predicate-detail-empty">No arguments.</div>
                  ) : (
                    <div className="action-predicate-args">
                      {selectedPropertyEntries.map((entry) => (
                        <div key={entry.label} className="action-predicate-arg-row">
                          <span className="action-predicate-arg-key">{entry.label}</span>
                          <span className="action-predicate-arg-value">{entry.value}</span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </>
            ) : (
              <div className="action-predicate-detail-empty">Select an entry to see details.</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}