import { useCallback, useEffect, useMemo, useState } from "react";
import "./App.css";
import Header from "./components/header/Header.tsx";
import Sidebar from "./components/sidebar/Sidebar.tsx";
import { useSidebarManager } from "./components/sidebar/useSidebarLogic";
import EditorCanvas from "./components/editor/EditorCanvas.tsx";
import type {
  ActionParameterDetail,
  CanvasNode,
  NodeConnection,
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
import {
  clonePredicateInstance,
  createEmptyPredicateInstance,
} from "./components/sidebar/utils/helpers";
import { FLOW_SUCCESS_TYPES } from "./components/sidebar/utils/types";
import type {
  ActionInstance,
  BehaviorNodeOption,
  PredicateInstance,
} from "./components/sidebar/utils/types";
import { PREDICATE_TYPE_CATALOG } from "./constants/predicateCatalog";
import { PREDICATE_INSTANCES_KEY } from "./components/sidebar/utils/constants";

type ThemeMode = "light" | "dark";

const STORAGE_KEY = "aptree-preferred-theme";

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
  nodeId: string | null;
  collection: ActionPredicateCollection | null;
  initialValue: PredicateInstance;
  revision: number;
}

interface ActionPredicateManagerState {
  isOpen: boolean;
  nodeId: string | null;
  collection: ActionPredicateCollection | null;
  revision: number;
}

type PendingManagerReopen = {
  nodeId: string;
  collection: ActionPredicateCollection;
} | null;

const createInitialPredicateModalState = (): ActionPredicateModalState => ({
  isOpen: false,
  mode: "add",
  nodeId: null,
  collection: null,
  initialValue: createEmptyPredicateInstance(),
  revision: 0,
});

const createInitialPredicateManagerState = (): ActionPredicateManagerState => ({
  isOpen: false,
  nodeId: null,
  collection: null,
  revision: 0,
});

/**
 * retrieves the initial theme mode based on user preference or system settings.
 * @returns initial theme mode
 */
function getInitialTheme(): ThemeMode {
  if (typeof window === "undefined") {
    return "dark";
  }

  const storedTheme = window.localStorage.getItem(
    STORAGE_KEY
  ) as ThemeMode | null;
  if (storedTheme === "light" || storedTheme === "dark") {
    return storedTheme;
  }

  return window.matchMedia("(prefers-color-scheme: dark)").matches
    ? "dark"
    : "light";
}

/**
 * application root component.
 * @returns main application element
 */
function App() {
  const [theme, setTheme] = useState<ThemeMode>(getInitialTheme);
  const [userLockedTheme, setUserLockedTheme] = useState<boolean>(() => {
    if (typeof window === "undefined") {
      return false;
    }
    const savedTheme = window.localStorage.getItem(STORAGE_KEY);
    return savedTheme === "light" || savedTheme === "dark";
  });
  const [canvasNodes, setCanvasNodes] = useState<CanvasNode[]>([]);
  const [connections, setConnections] = useState<NodeConnection[]>([]);
  const [predicateModalState, setPredicateModalState] =
    useState<ActionPredicateModalState>(createInitialPredicateModalState);
  const [predicateManagerState, setPredicateManagerState] =
    useState<ActionPredicateManagerState>(createInitialPredicateManagerState);
  const [, setPendingManagerReopen] = useState<PendingManagerReopen>(null);
  const [parameterDetail, setParameterDetail] =
    useState<ActionParameterDetail | null>(null);
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
      nodeId: string;
      collection: ActionPredicateCollection;
      predicate?: PredicateInstance;
    }) => {
      setPredicateModalState((prev) => ({
        isOpen: true,
        mode: config.mode,
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
    (config: { nodeId: string; collection: ActionPredicateCollection }) => {
      setPredicateManagerState((prev) => ({
        isOpen: true,
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

  /**
   * applies the current theme to the document root and persists the preference.
   */
  useEffect(() => {
    const root = document.documentElement;
    root.dataset.theme = theme;
    root.style.colorScheme = theme;
    window.localStorage.setItem(STORAGE_KEY, theme);
  }, [theme]);

  /**
   * listens for system theme changes if the user has not locked their preference.
   */
  useEffect(() => {
    if (typeof window === "undefined" || userLockedTheme) {
      return;
    }

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleSystemChange = (event: MediaQueryListEvent) => {
      setTheme(event.matches ? "dark" : "light");
    };

    if (typeof mediaQuery.addEventListener === "function") {
      mediaQuery.addEventListener("change", handleSystemChange);
      return () => mediaQuery.removeEventListener("change", handleSystemChange);
    }

    mediaQuery.addListener(handleSystemChange);
    return () => mediaQuery.removeListener(handleSystemChange);
  }, [userLockedTheme]);

  /**
   * toggles the application theme between light and dark modes.
   */
  const handleToggleTheme = () => {
    setTheme((current) => (current === "light" ? "dark" : "light"));
    setUserLockedTheme(true);
  };

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

  const handleImportPredicateInstancesFile = useCallback(
    (file: File) =>
      handleImportFromFile(
        file,
        importPredicateInstancesFromText,
        "Predicate Instances"
      ),
    [handleImportFromFile, importPredicateInstancesFromText]
  );

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
   * opens the appropriate sidebar edit modal for the node's source item, if available.
   */
  const handleEditNodeFromCanvas = useCallback(
    (nodeId: string) => {
      const node = canvasNodes.find((entry) => entry.id === nodeId);
      if (!node) {
        console.warn("Unable to edit node; node not found", nodeId);
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
    [canvasNodes, getItemsForCategory, openEditModal]
  );

  /**
   * handles dropping a sidebar item onto the editor canvas.
   */
  const handleDropOnCanvas = useCallback(
    (item: DraggedSidebarItem, position: { x: number; y: number }) => {
      if (item.category === BT_NODES_KEY) {
        const option = BEHAVIOR_NODE_OPTION_MAP.get(item.id);

        if (option) {
          setCanvasNodes((prev) => [
            ...prev,
            createBehaviorNode({ option, position }),
          ]);
          return;
        }
      }

      setCanvasNodes((prev) => [
        ...prev,
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
        },
      ]);
    },
    []
  );

  /**
   * handles moving an existing node within the editor canvas.
   */
  const handleMoveNode = useCallback(
    (nodeId: string, position: { x: number; y: number }) => {
      setCanvasNodes((prev) =>
        prev.map((node) =>
          node.id === nodeId
            ? {
                ...node,
                x: position.x,
                y: position.y,
              }
            : node
        )
      );
    },
    []
  );

  /**
   * persists resize interactions emitted from the canvas.
   */
  const handleResizeNode = useCallback(
    (nodeId: string, size: { width: number; height: number }) => {
      setCanvasNodes((prev) =>
        prev.map((node) =>
          node.id === nodeId
            ? {
                ...node,
                width: Math.max(120, size.width),
                height: Math.max(100, size.height),
              }
            : node
        )
      );
    },
    []
  );

  /**
   * handles removing a node from the editor canvas.
   */
  const handleRemoveNode = useCallback((nodeId: string) => {
    setCanvasNodes((prev) => prev.filter((node) => node.id !== nodeId));
    // Also remove all connections involving this node
    setConnections((prev) =>
      prev.filter(
        (conn) => conn.sourceNodeId !== nodeId && conn.targetNodeId !== nodeId
      )
    );
  }, []);

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
      setConnections((prev) => {
        const exists = prev.some(
          (conn) =>
            conn.sourceNodeId === sourceNodeId &&
            conn.targetNodeId === targetNodeId &&
            conn.sourcePort === sourcePort &&
            conn.targetPort === targetPort
        );

        if (exists) {
          return prev;
        }

        return [
          ...prev,
          {
            id: createId("connection"),
            sourceNodeId,
            targetNodeId,
            sourcePort,
            targetPort,
          },
        ];
      });
    },
    []
  );

  /**
   * handles removing a connection between nodes.
   */
  const handleRemoveConnection = useCallback((connectionId: string) => {
    setConnections((prev) => prev.filter((conn) => conn.id !== connectionId));
  }, []);

  /**
   * handles adding a precondition to an action node.
   */
  const handleManageActionPredicates = useCallback(
    (nodeId: string, collection: ActionPredicateCollection) => {
      openActionPredicateManager({ nodeId, collection });
    },
    [openActionPredicateManager]
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
      const node = canvasNodes.find((entry) => entry.id === nodeId);
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
        nodeId,
        collection,
        predicate,
      });
    },
    [canvasNodes, openActionPredicateModal]
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
      setCanvasNodes((prev) =>
        prev.map((node) => {
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
        })
      );
    },
    []
  );

  /**
   * persists predicate changes emitted from the modal into the owning node.
   */
  const handleSaveActionPredicate = useCallback(
    (value: PredicateInstance) => {
      if (!predicateModalState.nodeId || !predicateModalState.collection) {
        resetPredicateModalState();
        return;
      }

      const collectionKey = COLLECTION_KEY_MAP[predicateModalState.collection];
      const sanitizedValue = clonePredicateInstance(value);

      setCanvasNodes((prev) =>
        prev.map((node) => {
          if (node.id !== predicateModalState.nodeId) {
            return node;
          }

          const predicateList = node[collectionKey] ?? [];
          if (predicateModalState.mode === "edit") {
            if (
              !predicateList.some((entry) => entry.id === sanitizedValue.id)
            ) {
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
        })
      );

      resetPredicateModalState();
      setPendingManagerReopen((prev) => {
        if (!prev) {
          return null;
        }
        setPredicateManagerState((managerPrev) => ({
          isOpen: true,
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
    if (!predicateManagerState.nodeId || !predicateManagerState.collection) {
      return;
    }

    setPendingManagerReopen({
      nodeId: predicateManagerState.nodeId,
      collection: predicateManagerState.collection,
    });
    closeActionPredicateManager();
    openActionPredicateModal({
      mode: "add",
      nodeId: predicateManagerState.nodeId,
      collection: predicateManagerState.collection,
    });
  }, [
    closeActionPredicateManager,
    openActionPredicateModal,
    predicateManagerState.collection,
    predicateManagerState.nodeId,
  ]);

  const handleAttachExistingPredicate = useCallback(
    (predicateId: string) => {
      if (!predicateManagerState.nodeId || !predicateManagerState.collection) {
        return;
      }

      const selected = predicateInstances.find((p) => p.id === predicateId);
      if (!selected) {
        console.warn("Unable to attach predicate; instance not found", predicateId);
        return;
      }

      const collectionKey = COLLECTION_KEY_MAP[predicateManagerState.collection];
      const payload = clonePredicateInstance(selected);

      setCanvasNodes((prev) =>
        prev.map((node) => {
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
        })
      );
    },
    [predicateInstances, predicateManagerState.collection, predicateManagerState.nodeId]
  );

  const handleRemovePredicateFromManager = useCallback(
    (predicateId: string) => {
      if (!predicateManagerState.nodeId || !predicateManagerState.collection) {
        return;
      }
      handleRemoveActionPredicate(
        predicateManagerState.nodeId,
        predicateId,
        predicateManagerState.collection
      );
    },
    [handleRemoveActionPredicate, predicateManagerState.collection, predicateManagerState.nodeId]
  );

  const handleEditPredicateFromManager = useCallback(
    (predicateId: string) => {
      if (!predicateManagerState.nodeId || !predicateManagerState.collection) {
        return;
      }
      handleEditActionPredicate(
        predicateManagerState.nodeId,
        predicateId,
        predicateManagerState.collection
      );
    },
    [handleEditActionPredicate, predicateManagerState.collection, predicateManagerState.nodeId]
  );

  /**
   * handles cycling the flow success type for a flow node.
   */
  const handleCycleFlowSuccessType = useCallback((nodeId: string) => {
    setCanvasNodes((prev) =>
      prev.map((node) => {
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
      })
    );
  }, []);

  /**
   * handles creating a new behavior node on the canvas.
   */
  const handleCreateBehaviorNode = useCallback((option: BehaviorNodeOption) => {
    setCanvasNodes((prev) => {
      const nextIndex = prev.length;
      const offset = 140;
      const position = {
        x: 140 + (nextIndex % 3) * offset,
        y: 140 + Math.floor(nextIndex / 3) * offset,
      };
      return [...prev, createBehaviorNode({ option, position })];
    });
  }, []);

  const activePredicateNode = useMemo(() => {
    if (!predicateModalState.nodeId) {
      return null;
    }
    return (
      canvasNodes.find((node) => node.id === predicateModalState.nodeId) ?? null
    );
  }, [canvasNodes, predicateModalState.nodeId]);

  const activePredicateManagerNode = useMemo(() => {
    if (!predicateManagerState.nodeId) {
      return null;
    }
    return (
      canvasNodes.find((node) => node.id === predicateManagerState.nodeId) ?? null
    );
  }, [canvasNodes, predicateManagerState.nodeId]);

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
            onToggleTheme={handleToggleTheme}
            onImportParameterInstances={handleImportParameterInstancesFile}
            onImportPredicateInstances={handleImportPredicateInstancesFile}
            onImportActionInstances={handleImportActionInstancesFile}
          />
          <div className="editor" role="main">
            <EditorCanvas
              nodes={canvasNodes}
              connections={connections}
              onDropNode={handleDropOnCanvas}
              onMoveNode={handleMoveNode}
              onResizeNode={handleResizeNode}
              onRemoveNode={handleRemoveNode}
              onEditNode={handleEditNodeFromCanvas}
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
    </>
  );
}

export default App;
