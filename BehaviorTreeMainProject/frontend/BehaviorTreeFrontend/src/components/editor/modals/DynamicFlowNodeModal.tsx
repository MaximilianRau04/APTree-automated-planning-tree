import { useMemo, useState } from "react";
import type {
  CanvasNode,
  DynamicTemporalRelation,
  TemporalType,
} from "../types";
import { FLOW_SUCCESS_TYPES, type FlowSuccessType } from "../../sidebar/utils/types";
import { GRAMMAR_CONSTRAINTS, TEMPORAL_TYPES } from "../../../generated/aptreeGrammar";

interface DynamicFlowNodeModalProps {
  isOpen: boolean;
  node: CanvasNode | null;
  nodes: CanvasNode[];
  connections: Array<{ sourceNodeId: string; targetNodeId: string }>;
  draft: {
    successType: FlowSuccessType;
    childType: "ALLACTION" | "ALLFLOW";
    nodeGraphName: string;
    temporalRelations: DynamicTemporalRelation[];
  } | null;
  onChangeDraft: (patch: Partial<{
    successType: FlowSuccessType;
    childType: "ALLACTION" | "ALLFLOW";
    nodeGraphName: string;
    temporalRelations: DynamicTemporalRelation[];
  }>) => void;
  onClose: () => void;
  onSave: () => void;
}

function isAttachmentNode(node: CanvasNode) {
  return node.category === "decorators" || node.category === "services";
}

function isActionNode(node: CanvasNode) {
  return node.kind === "actionType" || node.kind === "actionInstance";
}

export default function DynamicFlowNodeModal({
  isOpen,
  node,
  nodes,
  connections,
  draft,
  onChangeDraft,
  onClose,
  onSave,
}: DynamicFlowNodeModalProps) {
  const children = useMemo(() => {
    if (!node) {
      return [];
    }

    const childIds = connections
      .filter((c) => c.sourceNodeId === node.id)
      .map((c) => c.targetNodeId);

    const byId = new Map(nodes.map((n) => [n.id, n] as const));
    return childIds
      .map((id) => byId.get(id))
      .filter((entry): entry is CanvasNode => Boolean(entry))
      .filter((entry) => !isAttachmentNode(entry));
  }, [connections, node, nodes]);

  // Only restrict relation endpoints if the grammar says NodeGraph uses action nodes.
  const relationNodes = useMemo(() => {
    if (GRAMMAR_CONSTRAINTS.nodeGraph.relationNodeKind === "action") {
      return children.filter(isActionNode);
    }
    return children;
  }, [children]);

  const childOptions = useMemo(
    () => relationNodes.map((c) => ({ id: c.id, label: c.name })),
    [relationNodes]
  );

  const [newFromId, setNewFromId] = useState<string>("");
  const [newToId, setNewToId] = useState<string>("");
  const [newTemporalType, setNewTemporalType] = useState<TemporalType>("BEFORE");

  const allowedChildIdSet = useMemo(
    () => new Set(relationNodes.map((c) => c.id)),
    [relationNodes]
  );

  const visibleRelations = useMemo(() => {
    const temporalRelations = draft?.temporalRelations ?? [];
    return temporalRelations.filter(
      (rel) => allowedChildIdSet.has(rel.fromNodeId) && allowedChildIdSet.has(rel.toNodeId)
    );
  }, [allowedChildIdSet, draft?.temporalRelations]);

  const relationLabel = useMemo(() => {
    const byId = new Map(relationNodes.map((c) => [c.id, c.name] as const));
    return (rel: DynamicTemporalRelation) => {
      const from = byId.get(rel.fromNodeId) ?? rel.fromNodeId;
      const to = byId.get(rel.toNodeId) ?? rel.toNodeId;
      return `${from} --[${rel.temporalType}]--> ${to}`;
    };
  }, [relationNodes]);

  if (!isOpen || !node || !draft) {
    return null;
  }

  const isNodeGraphNameValid = Boolean(draft.nodeGraphName.trim());

  const canAddRelation =
    newFromId &&
    newToId &&
    newFromId !== newToId &&
    allowedChildIdSet.has(newFromId) &&
    allowedChildIdSet.has(newToId);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content type-modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{`Root Node (${GRAMMAR_CONSTRAINTS.behaviorTreeRootNonterminal})`}</h3>
          <button className="modal-close-btn" onClick={onClose}>
            &times;
          </button>
        </div>

        <form
          className="modal-form type-modal-form"
          onSubmit={(event) => {
            event.preventDefault();

            if (!isNodeGraphNameValid) {
              return;
            }

            onChangeDraft({ temporalRelations: visibleRelations });
            onSave();
          }}
        >
          <div className="form-group">
            <label className="modal-label" htmlFor="dynamic-success">
              SuccessCriteria
            </label>
            <select
              id="dynamic-success"
              className="modal-select"
              value={draft.successType}
              onChange={(e) =>
                onChangeDraft({
                  successType: e.target.value as FlowSuccessType,
                })
              }
            >
              {FLOW_SUCCESS_TYPES.map((entry) => (
                <option key={entry} value={entry}>
                  {entry}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label className="modal-label" htmlFor="dynamic-childtype">
              ChildType
            </label>
            <select
              id="dynamic-childtype"
              className="modal-select"
              value={draft.childType}
              onChange={(e) =>
                onChangeDraft({
                  childType: e.target.value as "ALLACTION" | "ALLFLOW",
                })
              }
            >
              <option value="ALLACTION">ALLACTION</option>
              <option value="ALLFLOW">ALLFLOW</option>
            </select>
            <p className="category-modal-text">
              Connections from this node are restricted by ChildType.
            </p>
          </div>

          <div className="form-group">
            <label className="modal-label" htmlFor="dynamic-graphname">
              Nodegraph Name
            </label>
            <input
              id="dynamic-graphname"
              className="modal-input"
              value={draft.nodeGraphName}
              onChange={(e) => onChangeDraft({ nodeGraphName: e.target.value })}
              placeholder="e.g., MainGraph"
            />
            {!isNodeGraphNameValid ? (
              <p className="category-modal-text">
                Nodegraph Name is required by DynamicBTFlowNode.mc4.
              </p>
            ) : null}
          </div>

          <div className="form-group">
            <label className="modal-label">Temporal Relations (NodeGraph)</label>
            {childOptions.length === 0 ? (
              <p className="category-modal-text">
                {GRAMMAR_CONSTRAINTS.nodeGraph.relationNodeKind === "action"
                  ? "Add action children (connect actions from this node) to create relations."
                  : "Add child nodes (connect from this node) to create relations."}
              </p>
            ) : (
              <>
                <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
                  <select
                    className="modal-select"
                    value={newFromId}
                    onChange={(e) => setNewFromId(e.target.value)}
                    style={{ flex: "1 1 160px", minWidth: 160 }}
                  >
                    <option value="" disabled>
                      From...
                    </option>
                    {childOptions.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.label}
                      </option>
                    ))}
                  </select>

                  <select
                    className="modal-select"
                    value={newTemporalType}
                    onChange={(e) => setNewTemporalType(e.target.value as TemporalType)}
                    style={{ flex: "1 1 160px", minWidth: 160 }}
                  >
                    {TEMPORAL_TYPES.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </select>

                  <select
                    className="modal-select"
                    value={newToId}
                    onChange={(e) => setNewToId(e.target.value)}
                    style={{ flex: "1 1 160px", minWidth: 160 }}
                  >
                    <option value="" disabled>
                      To...
                    </option>
                    {childOptions.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.label}
                      </option>
                    ))}
                  </select>

                  <button
                    type="button"
                    className="btn-save"
                    disabled={!canAddRelation}
                    onClick={() => {
                      if (!canAddRelation) {
                        return;
                      }
                      onChangeDraft({
                        temporalRelations: [
                          ...(draft.temporalRelations ?? []),
                          {
                            fromNodeId: newFromId,
                            toNodeId: newToId,
                            temporalType: newTemporalType,
                          },
                        ],
                      });
                      setNewFromId("");
                      setNewToId("");
                    }}
                    style={{ padding: "10px 16px" }}
                  >
                    Add
                  </button>
                </div>

                {visibleRelations.length ? (
                  <div style={{ marginTop: 10, display: "flex", flexDirection: "column", gap: 8 }}>
                    {visibleRelations.map((rel, idx) => (
                      <div
                        key={`${rel.fromNodeId}-${rel.temporalType}-${rel.toNodeId}-${idx}`}
                        style={{
                          display: "flex",
                          justifyContent: "space-between",
                          alignItems: "center",
                          gap: 12,
                          padding: "10px 12px",
                          border: "1px solid var(--border-subtle)",
                          borderRadius: "var(--radius-sm)",
                          background: "var(--surface-panel)",
                        }}
                      >
                        <span style={{ color: "var(--text-secondary)", fontSize: "0.92rem" }}>
                          {relationLabel(rel)}
                        </span>
                        <button
                          type="button"
                          className="btn-cancel"
                          onClick={() => {
                            onChangeDraft({
                              temporalRelations: (draft.temporalRelations ?? []).filter(
                                (_r, i) => i !== idx
                              ),
                            });
                          }}
                          style={{ padding: "8px 12px" }}
                        >
                          Remove
                        </button>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="category-modal-text" style={{ marginTop: 10 }}>
                    No relations yet.
                  </p>
                )}
              </>
            )}
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-save" disabled={!isNodeGraphNameValid}>
              Save
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
