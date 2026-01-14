import { useCallback, useMemo, useState } from "react";
import "./App.css";
import Header from "./components/header/Header.tsx";
import Sidebar from "./components/sidebar/Sidebar.tsx";
import { useSidebarManager } from "./components/sidebar/useSidebarLogic";
import EditorCanvas from "./components/editor/EditorCanvas.tsx";
import type {
  ActionParameterDetail,
} from "./components/editor/types";
import {
  DEFAULT_CANVAS_NODE_HEIGHT,
  DEFAULT_CANVAS_NODE_WIDTH,
} from "./components/editor/types";
import type { DraggedSidebarItem } from "./components/editor/dragTypes";
import { createId } from "./utils/id";
import { createBehaviorNode } from "./components/editor/flowNodeFactory";
import { reconcileInstanceValues } from "./components/sidebar/utils/helpers";
import {
  ACTION_INSTANCES_KEY,
  BEHAVIOR_NODE_OPTION_MAP,
  BT_NODES_KEY,
} from "./components/sidebar/utils/constants";
import { PredicateInstanceModal } from "./components/sidebar/modals/InstanceModal";
import ActionParameterDetailsModal from "./components/editor/modals/ActionParameterDetailsModal.tsx";
import ActionPredicateManagerModal from "./components/editor/modals/ActionPredicateManagerModal.tsx";
import DynamicFlowNodeModal from "./components/editor/modals/DynamicFlowNodeModal";
import {
  clonePredicateInstance,
  createEmptyPredicateInstance,
} from "./components/sidebar/utils/helpers";
import { FLOW_SUCCESS_TYPES } from "./components/sidebar/utils/types";
import type {
  ActionInstance,
  BehaviorNodeOption,
  PredicateInstance,
  FlowSuccessType,
} from "./components/sidebar/utils/types";
import { PREDICATE_TYPE_CATALOG } from "./constants/predicateCatalog";
import { PREDICATE_INSTANCES_KEY } from "./components/sidebar/utils/constants";
import { useThemePreference } from "./hooks/useThemePreference";
import {
  createEmptyGraphs,
  HIERARCHY_TO_LEVEL,
  mergeConnections,
  mergeNodesWithHierarchy,
  parseCanvasGraphImport,
  type CanvasGraph,
  type CanvasLevel,
  type ExportedCanvasGraphsV1,
} from "./utils/canvasGraphs";

type ActionPredicateCollection = "precondition" | "effect";
type PredicateCollectionKey = "preconditions" | "effects";

const COLLECTION_KEY_MAP: Record<
  ActionPredicateCollection,
  PredicateCollectionKey
> = {
  precondition: "preconditions",
  effect: "effects",
};

interface ActionPredicateModalState {
  isOpen: boolean;
  mode: "add" | "edit";
  level: CanvasLevel | null;
  nodeId: string | null;
  collection: ActionPredicateCollection | null;
  initialValue: PredicateInstance;
  revision: number;
}

interface ActionPredicateManagerState {
  isOpen: boolean;
  level: CanvasLevel | null;
  nodeId: string | null;
  collection: ActionPredicateCollection | null;
  revision: number;
}

type PendingManagerReopen = {
  level: CanvasLevel;
  nodeId: string;
  collection: ActionPredicateCollection;
} | null;

const createInitialPredicateModalState = (): ActionPredicateModalState => ({
  isOpen: false,
  mode: "add",
  level: null,
  nodeId: null,
  collection: null,
  initialValue: createEmptyPredicateInstance(),
  revision: 0,
});

const createInitialPredicateManagerState = (): ActionPredicateManagerState => ({
  isOpen: false,
  level: null,
  nodeId: null,
  collection: null,
  revision: 0,
});

/**
 * application root component.
 * @returns main application element
 */
function App() {
  const { theme, toggleTheme } = useThemePreference();
  const [activeLevel, setActiveLevel] = useState<CanvasLevel>("high");
  const [graphs, setGraphs] = useState<Record<CanvasLevel, CanvasGraph>>(() =>
    createEmptyGraphs()
  );
  const [separators, setSeparators] = useState<Array<{ id: string; y: number; label?: string }>>([]);
  const [predicateModalState, setPredicateModalState] =
    useState<ActionPredicateModalState>(createInitialPredicateModalState);
  const [predicateManagerState, setPredicateManagerState] =
    useState<ActionPredicateManagerState>(createInitialPredicateManagerState);
  const [, setPendingManagerReopen] = useState<PendingManagerReopen>(null);
  const [parameterDetail, setParameterDetail] =
    useState<ActionParameterDetail | null>(null);

  const [dynamicFlowNodeState, setDynamicFlowNodeState] = useState<{
    isOpen: boolean;
    level: CanvasLevel | null;
    nodeId: string | null;
    draft: {
      successType: FlowSuccessType;
      childType: "ALLACTION" | "ALLFLOW";
      nodeGraphName: string;
      temporalRelations: Array<{
        fromNodeId: string;
        toNodeId: string;
        temporalType: "MEETS" | "BEFORE" | "AFTER" | "OVERLAPS" | "DURING";
      }>;
    } | null;
  }>({ isOpen: false, level: null, nodeId: null, draft: null });
  const sidebarManager = useSidebarManager();
  const {
    importParameterInstancesFromText,
    importPredicateInstancesFromText,
    importActionInstancesFromText,
    actionTypes,
    getItemsForCategory,
    openEditModal,
  } = sidebarManager;

  const rawActionInstances = useMemo(
    () => getItemsForCategory(ACTION_INSTANCES_KEY) as ActionInstance[],
    [getItemsForCategory]
  );

  const predicateInstances = useMemo(
    () => getItemsForCategory(PREDICATE_INSTANCES_KEY) as PredicateInstance[],
    [getItemsForCategory]
  );

  const actionInstances = useMemo(() => {
    if (!rawActionInstances.length) {
      return rawActionInstances;
    }

    const typeMap = new Map(
      (actionTypes ?? []).map((type) => [type.id, type] as const)
    );

    let hasChanges = false;
    const reconciled = rawActionInstances.map((instance) => {
      const definition = typeMap.get(instance.typeId);
      if (!definition) {
        if (
          !instance.propertyValues ||
          Object.keys(instance.propertyValues).length === 0
        ) {
          return instance;
        }

        hasChanges = true;
        return { ...instance, propertyValues: {} };
      }

      const nextValues = reconcileInstanceValues(
        definition,
        instance.propertyValues ?? {}
      );

      const hasSameKeys =
        Object.keys(nextValues).length ===
          Object.keys(instance.propertyValues ?? {}).length &&
        Object.entries(nextValues).every(
          ([key, value]) => instance.propertyValues?.[key] === value
        );

      if (hasSameKeys) {
        return instance;
      }

      hasChanges = true;
      return {
        ...instance,
        propertyValues: nextValues,
      };
    });

    return hasChanges ? reconciled : rawActionInstances;
  }, [rawActionInstances, actionTypes]);

  const mergedNodes = useMemo(() => mergeNodesWithHierarchy(graphs), [graphs]);
  const mergedConnections = useMemo(() => mergeConnections(graphs), [graphs]);

  const rootNodeIdsByHierarchyLevel = useMemo(() => {
    return {
      1: graphs.high.rootNodeId ?? null,
      2: graphs.mid.rootNodeId ?? null,
      3: graphs.low.rootNodeId ?? null,
    } as Record<number, string | null>;
  }, [graphs.high.rootNodeId, graphs.low.rootNodeId, graphs.mid.rootNodeId]);

  /**
   * resets the predicate modal state to its initial configuration.
   */
  const resetPredicateModalState = useCallback(() => {
    setPredicateModalState((prev) => ({
      ...createInitialPredicateModalState(),
      revision: prev.revision + 1,
    }));
  }, []);

  /**
   * closes the action predicate modal.
   */
  const closeActionPredicateModal = useCallback(() => {
    resetPredicateModalState();
    setPendingManagerReopen((prev) => {
      if (!prev) {
        return null;
      }
      setPredicateManagerState((managerPrev) => ({
        isOpen: true,
        level: prev.level,
        nodeId: prev.nodeId,
        collection: prev.collection,
        revision: managerPrev.revision + 1,
      }));
      return null;
    });
  }, [resetPredicateModalState]);

  /**
   * closes the action predicate manager modal.
   */
  const closeActionPredicateManager = useCallback(() => {
    setPredicateManagerState((prev) => ({
      ...createInitialPredicateManagerState(),
      revision: prev.revision + 1,
    }));
  }, []);

  /**
   * opens the action predicate modal with the provided configuration.
   * @param config modal configuration
   */
  const openActionPredicateModal = useCallback(
    (config: {
      mode: "add" | "edit";
      level: CanvasLevel;
      nodeId: string;
      collection: ActionPredicateCollection;
      predicate?: PredicateInstance;
    }) => {
      setPredicateModalState((prev) => ({
        isOpen: true,
        mode: config.mode,
        level: config.level,
        nodeId: config.nodeId,
        collection: config.collection,
        initialValue: config.predicate
          ? clonePredicateInstance(config.predicate)
          : createEmptyPredicateInstance(),
        revision: prev.revision + 1,
      }));
    },
    []
  );

  /**
   * opens the action predicate manager modal with the provided configuration.
   * @param config modal configuration
   */
  const openActionPredicateManager = useCallback(
    (config: {
      level: CanvasLevel;
      nodeId: string;
      collection: ActionPredicateCollection;
    }) => {
      setPredicateManagerState((prev) => ({
        isOpen: true,
        level: config.level,
        nodeId: config.nodeId,
        collection: config.collection,
        revision: prev.revision + 1,
      }));
    },
    []
  );

  /**
   * shows the action parameter detail modal with the provided detail.
   * @param detail action parameter detail to display
   */
  const handleShowActionParameterDetail = useCallback(
    (detail: ActionParameterDetail) => {
      setParameterDetail(detail);
    },
    []
  );

  /**
   * closes the action parameter detail modal.
   */
  const handleCloseActionParameterDetail = useCallback(() => {
    setParameterDetail(null);
  }, []);

  // Theme logic extracted to useThemePreference

  /**
   * handles importing instances from a file using the provided importer function.
   */
  const handleImportFromFile = useCallback(
    (
      file: File,
      importer: (text: string) => {
        processed: number;
        imported: number;
        skipped: number;
        errors: string[];
      },
      label: string
    ) => {
      const reader = new FileReader();
      reader.onload = () => {
        const text = typeof reader.result === "string" ? reader.result : "";
        const summary = importer(text);
        if (summary.processed === 0) {
          window.alert(`No ${label} found in the file.`);
          return;
        }

        const base = `${summary.imported} of ${summary.processed} ${label} imported.`;
        const skippedNote =
          summary.skipped > 0
            ? `\n${summary.skipped} lines were skipped.`
            : "";
        const errorNote =
          summary.errors.length > 0
            ? `\nErrors:\n- ${summary.errors.join("\n- ")}`
            : "";
        window.alert(`${base}${skippedNote}${errorNote}`.trim());
      };
      reader.onerror = () => {
        window.alert(
          `Import for ${label} failed: ${
            reader.error?.message ?? "Unknown error"
          }`
        );
      };
      reader.readAsText(file);
    },
    []
  );

  /**
   * handles importing parameter instances from a file.
   */
  const handleImportParameterInstancesFile = useCallback(
    (file: File) =>
      handleImportFromFile(
        file,
        importParameterInstancesFromText,
        "Parameter Instances"
      ),
    [handleImportFromFile, importParameterInstancesFromText]
  );

  /**
   * handles importing predicate instances from a file.
   */
  const handleImportPredicateInstancesFile = useCallback(
    (file: File) =>
      handleImportFromFile(
        file,
        importPredicateInstancesFromText,
        "Predicate Instances"
      ),
    [handleImportFromFile, importPredicateInstancesFromText]
  );

  /**
   * handles importing action instances from a file.
   */
  const handleImportActionInstancesFile = useCallback(
    (file: File) =>
      handleImportFromFile(
        file,
        importActionInstancesFromText,
        "Action Instances"
      ),
    [handleImportFromFile, importActionInstancesFromText]
  );

  /**
   * handles exporting the current canvas graphs to a JSON file.
   */
  const handleExportCanvasGraph = useCallback(() => {
    const graphsToValidate = Object.entries(graphs) as Array<
      [CanvasLevel, CanvasGraph]
    >;
    for (const [level, graph] of graphsToValidate) {
      const hasAnyNodes = graph.nodes.length > 0;
      if (!hasAnyNodes) {
        continue;
      }

      const rootId = graph.rootNodeId ?? null;
      const root = rootId ? graph.nodes.find((n) => n.id === rootId) : null;

      if (!rootId || !root || root.category !== "flowNodes" || root.sourceId !== "dynamic-flow-node") {
        window.alert(
          `Export blocked: Graph '${level}' must have a Dynamic Flow Node root (DynamicBTFlowNode.mc4). Use the crown icon on a Dynamic Flow Node.`
        );
        return;
      }

      const dynamicFlowNodes = graph.nodes.filter(
        (n) => n.category === "flowNodes" && n.sourceId === "dynamic-flow-node"
      );
      const byId = new Map(graph.nodes.map((n) => [n.id, n] as const));
      const isAttachment = (n: (typeof graph.nodes)[number]) =>
        n.category === "decorators" || n.category === "services";
      const isAction = (n: (typeof graph.nodes)[number]) =>
        n.kind === "actionType" || n.kind === "actionInstance";

      for (const node of dynamicFlowNodes) {
        if (!node.nodeGraphName || !node.nodeGraphName.trim()) {
          window.alert(
            `Export blocked: Dynamic Flow Node '${node.name}' in graph '${level}' is missing Nodegraph Name.`
          );
          return;
        }

        const childIds = graph.connections
          .filter((c) => c.sourceNodeId === node.id)
          .map((c) => c.targetNodeId);

        const children = childIds
          .map((id) => byId.get(id))
          .filter((entry): entry is (typeof graph.nodes)[number] => Boolean(entry))
          .filter((entry) => !isAttachment(entry));

        const actionChildIdSet = new Set(
          children.filter(isAction).map((child) => child.id)
        );

        const relations = node.temporalRelations ?? [];
        const hasInvalidRelation = relations.some(
          (rel) =>
            !actionChildIdSet.has(rel.fromNodeId) ||
            !actionChildIdSet.has(rel.toNodeId)
        );

        if (hasInvalidRelation) {
          window.alert(
            `Export blocked: Dynamic Flow Node '${node.name}' in graph '${level}' has temporal relations that reference non-action children. (DynamicBTFlowNode.mc4: Nodegraph uses 'action' graph nodes.)`
          );
          return;
        }
      }
    }

    const payload: ExportedCanvasGraphsV1 = {
      version: 1,
      exportedAt: new Date().toISOString(),
      activeLevel,
      graphs,
    };

    const json = JSON.stringify(payload, null, 2);
    const blob = new Blob([json], { type: "application/json" });
    const url = URL.createObjectURL(blob);

    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = "aptree-canvas-graphs.json";
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();

    URL.revokeObjectURL(url);
  }, [activeLevel, graphs]);

  /**
   * handles importing canvas graphs from a file.
   */
  const handleImportCanvasGraphFile = useCallback(
    async (file: File) => {
      try {
        const text = await file.text();
        const parsedResult = parseCanvasGraphImport(text);

        if (parsedResult.migrationNotices.length > 0) {
          window.alert(parsedResult.migrationNotices.join("\n"));
        }

        setGraphs(parsedResult.graphs);
        if (parsedResult.activeLevel) {
          setActiveLevel(parsedResult.activeLevel);
        }

        setParameterDetail(null);
        setPredicateModalState(createInitialPredicateModalState());
        setPredicateManagerState(createInitialPredicateManagerState());
      } catch (error) {
        window.alert(
          `Import failed: ${error instanceof Error ? error.message : "Unknown error"}`
        );
      }
    },
    []
  );

  /**
   * sets the root node for the specified hierarchy level.
   */
  const handleSetRootNode = useCallback(
    (nodeId: string, hierarchyLevel?: number) => {
      const level: CanvasLevel = hierarchyLevel
        ? HIERARCHY_TO_LEVEL[hierarchyLevel] ?? activeLevel
        : activeLevel;

      setGraphs((prev) => {
        const graph = prev[level];
        const candidate = graph.nodes.find((n) => n.id === nodeId);
        if (
          !candidate ||
          candidate.category !== "flowNodes" ||
          candidate.sourceId !== "dynamic-flow-node"
        ) {
          window.alert("Root must be a Dynamic Flow Node (DynamicBTFlowNode.mc4).");
          return prev;
        }

        const nextRoot = graph.rootNodeId === nodeId ? null : nodeId;

        return {
          ...prev,
          [level]: {
            ...graph,
            rootNodeId: nextRoot,
          },
        };
      });
    },
    [activeLevel]
  );

  /**
   * opens the appropriate sidebar edit modal for the node's source item, if available.
   */
  const handleEditNodeFromCanvas = useCallback(
    (nodeId: string) => {
      const locateNode = (): { level: CanvasLevel; node: (typeof graphs)[CanvasLevel]["nodes"][number] } | null => {
        const levels: CanvasLevel[] = ["high", "mid", "low"];
        for (const level of levels) {
          const found = graphs[level].nodes.find((entry) => entry.id === nodeId);
          if (found) {
            return { level, node: found };
          }
        }
        return null;
      };

      const located = locateNode();
      if (!located) {
        console.warn("Unable to edit node; node not found", nodeId);
        return;
      }

      const node = located.node;

      if (node.sourceId === "dynamic-flow-node") {
        setDynamicFlowNodeState({
          isOpen: true,
          level: located.level,
          nodeId,
          draft: {
            successType: node.successType ?? FLOW_SUCCESS_TYPES[0],
            childType: node.childType ?? "ALLACTION",
            nodeGraphName: node.nodeGraphName ?? `${node.name.replace(/\s+/g, "")}Graph`,
            temporalRelations: node.temporalRelations ?? [],
          },
        });
        return;
      }

      const category = node.category;
      const items = getItemsForCategory(category);
      const index = items.findIndex((item) => item.id === node.sourceId);

      if (index === -1) {
        window.alert(
          "This element cannot currently be edited via the canvas. Please edit it in the sidebar."
        );
        console.warn(
          "Unable to edit node; no matching source item found",
          node
        );
        return;
      }

      const item = items[index];
      openEditModal(category, index, item);
    },
    [getItemsForCategory, graphs, openEditModal]
  );

  const closeDynamicFlowNodeModal = useCallback(() => {
    setDynamicFlowNodeState({ isOpen: false, level: null, nodeId: null, draft: null });
  }, []);

  /**
   * handles dropping a sidebar item onto the editor canvas.
   */
  const handleDropOnCanvas = useCallback(
    (item: DraggedSidebarItem, position: { x: number; y: number }) => {
      if (item.category === BT_NODES_KEY) {
        const option = BEHAVIOR_NODE_OPTION_MAP.get(item.id);

        if (option) {
          setGraphs((prev) => {
            const graph = prev[activeLevel];
            const created = createBehaviorNode({ option, position });
            const nextRootNodeId =
              option.id === "dynamic-flow-node" && !graph.rootNodeId
                ? created.id
                : graph.rootNodeId;
            return {
              ...prev,
              [activeLevel]: {
                ...graph,
                nodes: [...graph.nodes, created],
                rootNodeId: nextRootNodeId,
              },
            };
          });
          return;
        }
      }

      setGraphs((prev) => {
        const graph = prev[activeLevel];
        const isDynamicFlowNode = item.category === "flowNodes" && item.id === "dynamic-flow-node";
        const nextNodeGraphName = isDynamicFlowNode
          ? `${item.name.replace(/\s+/g, "")}Graph`
          : undefined;
        return {
          ...prev,
          [activeLevel]: {
            ...graph,
            nodes: [
              ...graph.nodes,
              {
                id: createId("canvas-node"),
                sourceId: item.id,
                name: item.name,
                typeLabel: item.type,
                category: item.category,
                kind: item.kind,
                x: position.x,
                y: position.y,
                width: DEFAULT_CANVAS_NODE_WIDTH,
                height: DEFAULT_CANVAS_NODE_HEIGHT,
                isNegated: item.isNegated,
                typeId: item.typeId,
                ...(isDynamicFlowNode
                  ? {
                      childType: "ALLACTION" as const,
                      nodeGraphName: nextNodeGraphName,
                      temporalRelations: [],
                    }
                  : null),
              },
            ],
          },
        };
      });
    },
    [activeLevel]
  );

  /**
   * handles dropping a separator line onto the canvas.
   */
  const handleDropSeparator = useCallback((y: number) => {
    setSeparators((prev) => [
      ...prev,
      {
        id: createId("separator"),
        y,
        label: `Level ${prev.length + 1}`,
      },
    ]);
  }, []);

  /**
   * handles moving a separator line.
   */
  const handleMoveSeparator = useCallback((id: string, y: number) => {
    setSeparators((prev) =>
      prev.map((sep) => (sep.id === id ? { ...sep, y } : sep))
    );
  }, []);

  /**
   * handles removing a separator line.
   */
  const handleRemoveSeparator = useCallback((id: string) => {
    setSeparators((prev) => prev.filter((sep) => sep.id !== id));
  }, []);

  /**
   * handles moving an existing node within the editor canvas.
   */
  const handleMoveNode = useCallback(
    (nodeId: string, position: { x: number; y: number }) => {
      setGraphs((prev) => {
        const graph = prev[activeLevel];
        return {
          ...prev,
          [activeLevel]: {
            ...graph,
            nodes: graph.nodes.map((node) =>
              node.id === nodeId
                ? {
                    ...node,
                    x: position.x,
                    y: position.y,
                  }
                : node
            ),
          },
        };
      });
    },
    [activeLevel]
  );

  /**
   * persists resize interactions emitted from the canvas.
   */
  const handleResizeNode = useCallback(
    (nodeId: string, size: { width: number; height: number }) => {
      setGraphs((prev) => {
        const graph = prev[activeLevel];
        return {
          ...prev,
          [activeLevel]: {
            ...graph,
            nodes: graph.nodes.map((node) =>
              node.id === nodeId
                ? {
                    ...node,
                    width: Math.max(120, size.width),
                    height: Math.max(100, size.height),
                  }
                : node
            ),
          },
        };
      });
    },
    [activeLevel]
  );

  /**
   * handles removing a node from the editor canvas.
   */
  const handleRemoveNode = useCallback((nodeId: string) => {
    setGraphs((prev) => {
      const graph = prev[activeLevel];
      const nextNodes = graph.nodes.filter((node) => node.id !== nodeId);
      const nextConnections = graph.connections.filter(
        (conn) => conn.sourceNodeId !== nodeId && conn.targetNodeId !== nodeId
      );

      const nextRoot = graph.rootNodeId === nodeId ? null : graph.rootNodeId;
      return {
        ...prev,
        [activeLevel]: {
          ...graph,
          nodes: nextNodes,
          connections: nextConnections,
          rootNodeId: nextRoot,
        },
      };
    });
  }, [activeLevel]);

  /**
   * handles adding a connection between two nodes.
   */
  const handleAddConnection = useCallback(
    (
      sourceNodeId: string,
      targetNodeId: string,
      sourcePort: "top" | "right" | "bottom" | "left",
      targetPort: "top" | "right" | "bottom" | "left"
    ) => {
      // Check if connection already exists
      setGraphs((prev) => {
        const graph = prev[activeLevel];
        const exists = graph.connections.some(
          (conn) =>
            conn.sourceNodeId === sourceNodeId &&
            conn.targetNodeId === targetNodeId &&
            conn.sourcePort === sourcePort &&
            conn.targetPort === targetPort
        );

        if (exists) {
          return prev;
        }

        return {
          ...prev,
          [activeLevel]: {
            ...graph,
            connections: [
              ...graph.connections,
              {
                id: createId("connection"),
                sourceNodeId,
                targetNodeId,
                sourcePort,
                targetPort,
              },
            ],
          },
        };
      });
    },
    [activeLevel]
  );

  /**
   * handles removing a connection between nodes.
   */
  const handleRemoveConnection = useCallback((connectionId: string) => {
    setGraphs((prev) => {
      const graph = prev[activeLevel];
      return {
        ...prev,
        [activeLevel]: {
          ...graph,
          connections: graph.connections.filter((conn) => conn.id !== connectionId),
        },
      };
    });
  }, [activeLevel]);

  /**
   * handles adding a precondition to an action node.
   */
  const handleManageActionPredicates = useCallback(
    (nodeId: string, collection: ActionPredicateCollection) => {
      openActionPredicateManager({ level: activeLevel, nodeId, collection });
    },
    [activeLevel, openActionPredicateManager]
  );

  /**
   * opens the predicate modal in edit mode for the requested predicate.
   */
  const handleEditActionPredicate = useCallback(
    (
      nodeId: string,
      predicateId: string,
      collection: ActionPredicateCollection
    ) => {
      const node = graphs[activeLevel].nodes.find((entry) => entry.id === nodeId);
      if (!node) {
        console.warn("Unable to edit predicate; node not found", nodeId);
        return;
      }

      const collectionKey = COLLECTION_KEY_MAP[collection];
      const predicateList = node[collectionKey] ?? [];
      const predicate = predicateList.find((entry) => entry.id === predicateId);

      if (!predicate) {
        console.warn(
          "Unable to edit predicate; predicate not found",
          predicateId
        );
        return;
      }

      openActionPredicateModal({
        mode: "edit",
        level: activeLevel,
        nodeId,
        collection,
        predicate,
      });
    },
    [activeLevel, graphs, openActionPredicateModal]
  );

  /**
   * removes the given predicate from the specified collection on a node.
   */
  const handleRemoveActionPredicate = useCallback(
    (
      nodeId: string,
      predicateId: string,
      collection: ActionPredicateCollection
    ) => {
      setGraphs((prev) => {
        const graph = prev[activeLevel];
        return {
          ...prev,
          [activeLevel]: {
            ...graph,
            nodes: graph.nodes.map((node) => {
              if (node.id !== nodeId) {
                return node;
              }

              const collectionKey = COLLECTION_KEY_MAP[collection];
              const predicateList = node[collectionKey] ?? [];
              if (!predicateList.some((entry) => entry.id === predicateId)) {
                return node;
              }

              return {
                ...node,
                [collectionKey]: predicateList.filter(
                  (entry) => entry.id !== predicateId
                ),
              };
            }),
          },
        };
      });
    },
    [activeLevel]
  );

  /**
   * persists predicate changes emitted from the modal into the owning node.
   */
  const handleSaveActionPredicate = useCallback(
    (value: PredicateInstance) => {
      if (
        !predicateModalState.nodeId ||
        !predicateModalState.collection ||
        !predicateModalState.level
      ) {
        resetPredicateModalState();
        return;
      }

      const collectionKey = COLLECTION_KEY_MAP[predicateModalState.collection];
      const sanitizedValue = clonePredicateInstance(value);

      setGraphs((prev) => {
        const level = predicateModalState.level as CanvasLevel;
        const graph = prev[level];
        return {
          ...prev,
          [level]: {
            ...graph,
            nodes: graph.nodes.map((node) => {
              if (node.id !== predicateModalState.nodeId) {
                return node;
              }

              const predicateList = node[collectionKey] ?? [];
              if (predicateModalState.mode === "edit") {
                if (!predicateList.some((entry) => entry.id === sanitizedValue.id)) {
                  return node;
                }

                return {
                  ...node,
                  [collectionKey]: predicateList.map((entry) =>
                    entry.id === sanitizedValue.id ? sanitizedValue : entry
                  ),
                };
              }

              return {
                ...node,
                [collectionKey]: [...predicateList, sanitizedValue],
              };
            }),
          },
        };
      });

      resetPredicateModalState();
      setPendingManagerReopen((prev) => {
        if (!prev) {
          return null;
        }
        setPredicateManagerState((managerPrev) => ({
          isOpen: true,
          level: prev.level,
          nodeId: prev.nodeId,
          collection: prev.collection,
          revision: managerPrev.revision + 1,
        }));
        return null;
      });
    },
    [predicateModalState, resetPredicateModalState]
  );

  const handleCreateNewPredicateFromManager = useCallback(() => {
    if (
      !predicateManagerState.nodeId ||
      !predicateManagerState.collection ||
      !predicateManagerState.level
    ) {
      return;
    }

    setPendingManagerReopen({
      level: predicateManagerState.level,
      nodeId: predicateManagerState.nodeId,
      collection: predicateManagerState.collection,
    });
    closeActionPredicateManager();
    openActionPredicateModal({
      mode: "add",
      level: predicateManagerState.level,
      nodeId: predicateManagerState.nodeId,
      collection: predicateManagerState.collection,
    });
  }, [
    closeActionPredicateManager,
    openActionPredicateModal,
    predicateManagerState.collection,
    predicateManagerState.level,
    predicateManagerState.nodeId,
  ]);

  const handleAttachExistingPredicate = useCallback(
    (predicateId: string) => {
      if (
        !predicateManagerState.nodeId ||
        !predicateManagerState.collection ||
        !predicateManagerState.level
      ) {
        return;
      }

      const selected = predicateInstances.find((p) => p.id === predicateId);
      if (!selected) {
        console.warn("Unable to attach predicate; instance not found", predicateId);
        return;
      }

      const collectionKey = COLLECTION_KEY_MAP[predicateManagerState.collection];
      const payload = clonePredicateInstance(selected);

      setGraphs((prev) => {
        const level = predicateManagerState.level as CanvasLevel;
        const graph = prev[level];
        return {
          ...prev,
          [level]: {
            ...graph,
            nodes: graph.nodes.map((node) => {
              if (node.id !== predicateManagerState.nodeId) {
                return node;
              }

              const predicateList = node[collectionKey] ?? [];
              if (predicateList.some((entry) => entry.id === payload.id)) {
                return node;
              }

              return {
                ...node,
                [collectionKey]: [...predicateList, payload],
              };
            }),
          },
        };
      });
    },
    [
      predicateInstances,
      predicateManagerState.collection,
      predicateManagerState.level,
      predicateManagerState.nodeId,
    ]
  );

  const handleRemovePredicateFromManager = useCallback(
    (predicateId: string) => {
      if (
        !predicateManagerState.nodeId ||
        !predicateManagerState.collection ||
        !predicateManagerState.level
      ) {
        return;
      }

      const level = predicateManagerState.level;
      const collection = predicateManagerState.collection;
      const nodeId = predicateManagerState.nodeId;
      const collectionKey = COLLECTION_KEY_MAP[collection];

      setGraphs((prev) => {
        const graph = prev[level];
        return {
          ...prev,
          [level]: {
            ...graph,
            nodes: graph.nodes.map((node) => {
              if (node.id !== nodeId) {
                return node;
              }

              const predicateList = node[collectionKey] ?? [];
              if (!predicateList.some((entry) => entry.id === predicateId)) {
                return node;
              }

              return {
                ...node,
                [collectionKey]: predicateList.filter(
                  (entry) => entry.id !== predicateId
                ),
              };
            }),
          },
        };
      });
    },
    [predicateManagerState.collection, predicateManagerState.level, predicateManagerState.nodeId]
  );

  const handleEditPredicateFromManager = useCallback(
    (predicateId: string) => {
      if (
        !predicateManagerState.nodeId ||
        !predicateManagerState.collection ||
        !predicateManagerState.level
      ) {
        return;
      }

      const level = predicateManagerState.level;
      const nodeId = predicateManagerState.nodeId;
      const collection = predicateManagerState.collection;

      const node = graphs[level].nodes.find((entry) => entry.id === nodeId);
      if (!node) {
        console.warn("Unable to edit predicate; node not found", nodeId);
        return;
      }

      const collectionKey = COLLECTION_KEY_MAP[collection];
      const predicateList = node[collectionKey] ?? [];
      const predicate = predicateList.find((entry) => entry.id === predicateId);

      if (!predicate) {
        console.warn(
          "Unable to edit predicate; predicate not found",
          predicateId
        );
        return;
      }

      openActionPredicateModal({
        mode: "edit",
        level,
        nodeId,
        collection,
        predicate,
      });
    },
    [
      graphs,
      openActionPredicateModal,
      predicateManagerState.collection,
      predicateManagerState.level,
      predicateManagerState.nodeId,
    ]
  );

  /**
   * handles cycling the flow success type for a flow node.
   */
  const handleCycleFlowSuccessType = useCallback((nodeId: string) => {
    setGraphs((prev) => {
      const graph = prev[activeLevel];
      return {
        ...prev,
        [activeLevel]: {
          ...graph,
          nodes: graph.nodes.map((node) => {
            if (node.id !== nodeId || !node.successType) {
              return node;
            }

            const currentIndex = Math.max(
              0,
              FLOW_SUCCESS_TYPES.indexOf(node.successType)
            );
            const nextType =
              FLOW_SUCCESS_TYPES[(currentIndex + 1) % FLOW_SUCCESS_TYPES.length];

            return {
              ...node,
              successType: nextType,
            };
          }),
        },
      };
    });
  }, [activeLevel]);

  /**
   * handles creating a new behavior node on the canvas.
   */
  const handleCreateBehaviorNode = useCallback((option: BehaviorNodeOption) => {
    setGraphs((prev) => {
      const graph = prev[activeLevel];
      const nextIndex = graph.nodes.length;
      const offset = 140;
      const position = {
        x: 140 + (nextIndex % 3) * offset,
        y: 140 + Math.floor(nextIndex / 3) * offset,
      };

      return {
        ...prev,
        [activeLevel]: {
          ...graph,
          nodes: [...graph.nodes, createBehaviorNode({ option, position })],
        },
      };
    });
  }, [activeLevel]);

  const activePredicateNode = useMemo(() => {
    if (!predicateModalState.nodeId || !predicateModalState.level) {
      return null;
    }
    return (
      graphs[predicateModalState.level].nodes.find(
        (node) => node.id === predicateModalState.nodeId
      ) ?? null
    );
  }, [graphs, predicateModalState.level, predicateModalState.nodeId]);

  const activePredicateManagerNode = useMemo(() => {
    if (!predicateManagerState.nodeId || !predicateManagerState.level) {
      return null;
    }
    return (
      graphs[predicateManagerState.level].nodes.find(
        (node) => node.id === predicateManagerState.nodeId
      ) ?? null
    );
  }, [graphs, predicateManagerState.level, predicateManagerState.nodeId]);

  const managerAssignedList = useMemo(() => {
    if (!activePredicateManagerNode || !predicateManagerState.collection) {
      return [] as PredicateInstance[];
    }
    const collectionKey = COLLECTION_KEY_MAP[predicateManagerState.collection];
    return (activePredicateManagerNode[collectionKey] ?? []) as PredicateInstance[];
  }, [activePredicateManagerNode, predicateManagerState.collection]);

  const managerAvailableList = useMemo(() => {
    if (!predicateManagerState.collection) {
      return predicateInstances;
    }

    const assignedIds = new Set(managerAssignedList.map((p) => p.id));
    return predicateInstances.filter((p) => !assignedIds.has(p.id));
  }, [managerAssignedList, predicateInstances, predicateManagerState.collection]);

  const predicateModalTitle = useMemo(() => {
    if (!predicateModalState.isOpen) {
      return "Manage Predicate";
    }

    const verb = predicateModalState.mode === "add" ? "Add" : "Edit";
    const scope =
      predicateModalState.collection === "effect"
        ? "Effect"
        : predicateModalState.collection === "precondition"
        ? "Precondition"
        : "Predicate";
    const suffix = activePredicateNode
      ? ` for ${activePredicateNode.name}`
      : "";
    return `${verb} ${scope}${suffix}`;
  }, [predicateModalState, activePredicateNode]);

  return (
    <>
      <div className="app-container">
        <Sidebar
          manager={sidebarManager}
          onCreateBehaviorNode={handleCreateBehaviorNode}
        />
        <div className="main-content">
          <Header
            theme={theme}
            onToggleTheme={toggleTheme}
            onImportParameterInstances={handleImportParameterInstancesFile}
            onImportPredicateInstances={handleImportPredicateInstancesFile}
            onImportActionInstances={handleImportActionInstancesFile}
            onExportCanvasGraph={handleExportCanvasGraph}
            onImportCanvasGraph={handleImportCanvasGraphFile}
          />
          <div className="editor" role="main">
            <div className="editor-canvas-wrap">
              <EditorCanvas
                nodes={mergedNodes}
                connections={mergedConnections}
                separators={separators}
                rootNodeIdsByHierarchyLevel={rootNodeIdsByHierarchyLevel}
                onDropNode={handleDropOnCanvas}
                onDropSeparator={handleDropSeparator}
                onMoveSeparator={handleMoveSeparator}
                onRemoveSeparator={handleRemoveSeparator}
                onMoveNode={handleMoveNode}
                onResizeNode={handleResizeNode}
                onRemoveNode={handleRemoveNode}
                onEditNode={handleEditNodeFromCanvas}
                onSetRootNode={handleSetRootNode}
                onAddConnection={handleAddConnection}
                onRemoveConnection={handleRemoveConnection}
                onShowActionParameterDetail={handleShowActionParameterDetail}
                onManageActionPredicates={handleManageActionPredicates}
                onEditActionPredicate={handleEditActionPredicate}
                onRemoveActionPredicate={handleRemoveActionPredicate}
                onCycleFlowSuccessType={handleCycleFlowSuccessType}
                predicateTypes={PREDICATE_TYPE_CATALOG}
                actionTypes={actionTypes}
                actionInstances={actionInstances}
              />
            </div>
          </div>
        </div>
      </div>

      <PredicateInstanceModal
        key={`${predicateModalState.revision}-${predicateModalState.initialValue.id}`}
        isOpen={predicateModalState.isOpen}
        mode={predicateModalState.mode}
        title={predicateModalTitle}
        initialValue={predicateModalState.initialValue}
        typeDefinitions={PREDICATE_TYPE_CATALOG}
        onClose={closeActionPredicateModal}
        onSave={handleSaveActionPredicate}
      />

      <ActionPredicateManagerModal
        key={`${predicateManagerState.revision}-${predicateManagerState.nodeId}-${predicateManagerState.collection}`}
        isOpen={predicateManagerState.isOpen}
        nodeName={activePredicateManagerNode?.name ?? "Action"}
        nodeTypeLabel={activePredicateManagerNode?.typeLabel ?? ""}
        collection={predicateManagerState.collection ?? "precondition"}
        assigned={managerAssignedList}
        available={managerAvailableList}
        predicateTypeMap={sidebarManager.predicateTypeMap}
        onClose={closeActionPredicateManager}
        onAdd={handleAttachExistingPredicate}
        onRemove={handleRemovePredicateFromManager}
        onEdit={handleEditPredicateFromManager}
        onCreateNew={handleCreateNewPredicateFromManager}
      />

      <ActionParameterDetailsModal
        detail={parameterDetail}
        onClose={handleCloseActionParameterDetail}
      />

      <DynamicFlowNodeModal
        isOpen={dynamicFlowNodeState.isOpen}
        node={
          dynamicFlowNodeState.level && dynamicFlowNodeState.nodeId
            ? graphs[dynamicFlowNodeState.level].nodes.find(
                (n) => n.id === dynamicFlowNodeState.nodeId
              ) ?? null
            : null
        }
        nodes={
          dynamicFlowNodeState.level
            ? graphs[dynamicFlowNodeState.level].nodes
            : []
        }
        connections={
          dynamicFlowNodeState.level
            ? graphs[dynamicFlowNodeState.level].connections.map((c) => ({
                sourceNodeId: c.sourceNodeId,
                targetNodeId: c.targetNodeId,
              }))
            : []
        }
        draft={dynamicFlowNodeState.draft}
        onChangeDraft={(patch) => {
          setDynamicFlowNodeState((prev) =>
            prev.draft
              ? {
                  ...prev,
                  draft: {
                    ...prev.draft,
                    ...patch,
                  },
                }
              : prev
          );
        }}
        onClose={closeDynamicFlowNodeModal}
        onSave={() => {
          if (!dynamicFlowNodeState.level || !dynamicFlowNodeState.nodeId || !dynamicFlowNodeState.draft) {
            closeDynamicFlowNodeModal();
            return;
          }

          const level = dynamicFlowNodeState.level;
          const nodeId = dynamicFlowNodeState.nodeId;
          const draft = dynamicFlowNodeState.draft;

          setGraphs((prev) => {
            const graph = prev[level];
            const index = graph.nodes.findIndex((n) => n.id === nodeId);
            if (index === -1) {
              return prev;
            }

            const nextNodes = [...graph.nodes];
            nextNodes[index] = {
              ...nextNodes[index],
              successType: draft.successType,
              childType: draft.childType,
              nodeGraphName: draft.nodeGraphName,
              temporalRelations: draft.temporalRelations,
            };

            return {
              ...prev,
              [level]: {
                ...graph,
                nodes: nextNodes,
              },
            };
          });

          closeDynamicFlowNodeModal();
        }}
      />
    </>
  );
}

export default App;
