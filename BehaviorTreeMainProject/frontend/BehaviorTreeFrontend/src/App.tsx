import { useCallback, useEffect, useMemo, useState } from "react";
import "./App.css";
import Header from "./components/header/Header.tsx";
import Sidebar from "./components/sidebar/Sidebar.tsx";
import EditorCanvas from "./components/editor/EditorCanvas.tsx";
import type { CanvasNode, NodeConnection } from "./components/editor/types";
import type { DraggedSidebarItem } from "./components/editor/dragTypes";
import { createId } from "./utils/id";
import { createBehaviorNode } from "./components/editor/flowNodeFactory";
import {
  BEHAVIOR_NODE_OPTION_MAP,
  BT_NODES_KEY,
} from "./components/sidebar/utils/constants";
import { PredicateInstanceModal } from "./components/sidebar/modals/InstanceModal";
import {
  clonePredicateInstance,
  createEmptyPredicateInstance,
} from "./components/sidebar/utils/helpers";
import { FLOW_SUCCESS_TYPES } from "./components/sidebar/utils/types";
import type {
  BehaviorNodeOption,
  PredicateInstance,
} from "./components/sidebar/utils/types";
import { PREDICATE_TYPE_CATALOG } from "./constants/predicateCatalog";

type ThemeMode = "light" | "dark";

const STORAGE_KEY = "aptree-preferred-theme";

type ActionPredicateCollection = "precondition" | "effect";
type PredicateCollectionKey = "preconditions" | "effects";

const COLLECTION_KEY_MAP: Record<ActionPredicateCollection, PredicateCollectionKey> = {
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

const createInitialPredicateModalState = (): ActionPredicateModalState => ({
  isOpen: false,
  mode: "add",
  nodeId: null,
  collection: null,
  initialValue: createEmptyPredicateInstance(),
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

  const storedTheme = window.localStorage.getItem(STORAGE_KEY) as ThemeMode | null;
  if (storedTheme === "light" || storedTheme === "dark") {
    return storedTheme;
  }

  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

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
  const [predicateModalState, setPredicateModalState] = useState<ActionPredicateModalState>(
    createInitialPredicateModalState
  );

  const resetPredicateModalState = useCallback(() => {
    setPredicateModalState((prev) => ({
      ...createInitialPredicateModalState(),
      revision: prev.revision + 1,
    }));
  }, []);

  const closeActionPredicateModal = useCallback(() => {
    resetPredicateModalState();
  }, [resetPredicateModalState]);

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
          isNegated: item.isNegated,
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
                x: Math.max(0, position.x),
                y: Math.max(0, position.y),
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
  const handleAddConnection = useCallback((
    sourceNodeId: string,
    targetNodeId: string,
    sourcePort: 'top' | 'right' | 'bottom' | 'left',
    targetPort: 'top' | 'right' | 'bottom' | 'left'
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
  }, []);

  /**
   * handles removing a connection between nodes.
   */
  const handleRemoveConnection = useCallback((connectionId: string) => {
    setConnections((prev) => prev.filter((conn) => conn.id !== connectionId));
  }, []);

  /**
   * handles adding a precondition to an action node.
   */
  const handleAddActionPrecondition = useCallback(
    (nodeId: string) => {
      openActionPredicateModal({
        mode: "add",
        nodeId,
        collection: "precondition",
      });
    },
    [openActionPredicateModal]
  );

  /**
   * handles adding an effect to an action node.
   */
  const handleAddActionEffect = useCallback(
    (nodeId: string) => {
      openActionPredicateModal({
        mode: "add",
        nodeId,
        collection: "effect",
      });
    },
    [openActionPredicateModal]
  );

  /**
   * opens the predicate modal in edit mode for the requested predicate.
   */
  const handleEditActionPredicate = useCallback(
    (nodeId: string, predicateId: string, collection: ActionPredicateCollection) => {
      const node = canvasNodes.find((entry) => entry.id === nodeId);
      if (!node) {
        console.warn("Unable to edit predicate; node not found", nodeId);
        return;
      }

      const collectionKey = COLLECTION_KEY_MAP[collection];
      const predicateList = node[collectionKey] ?? [];
      const predicate = predicateList.find((entry) => entry.id === predicateId);

      if (!predicate) {
        console.warn("Unable to edit predicate; predicate not found", predicateId);
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
    (nodeId: string, predicateId: string, collection: ActionPredicateCollection) => {
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
            [collectionKey]: predicateList.filter((entry) => entry.id !== predicateId),
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
        })
      );

      resetPredicateModalState();
    },
    [predicateModalState, resetPredicateModalState]
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
    return canvasNodes.find((node) => node.id === predicateModalState.nodeId) ?? null;
  }, [canvasNodes, predicateModalState.nodeId]);

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
    const suffix = activePredicateNode ? ` for ${activePredicateNode.name}` : "";
    return `${verb} ${scope}${suffix}`;
  }, [predicateModalState, activePredicateNode]);

  return (
    <>
      <div className="app-container">
        <Sidebar onCreateBehaviorNode={handleCreateBehaviorNode} />
        <div className="main-content">
          <Header theme={theme} onToggleTheme={handleToggleTheme} />
          <div className="editor" role="main">
            <EditorCanvas
              nodes={canvasNodes}
              connections={connections}
              onDropNode={handleDropOnCanvas}
              onMoveNode={handleMoveNode}
              onRemoveNode={handleRemoveNode}
              onAddConnection={handleAddConnection}
              onRemoveConnection={handleRemoveConnection}
              onAddActionPrecondition={handleAddActionPrecondition}
              onAddActionEffect={handleAddActionEffect}
              onEditActionPredicate={handleEditActionPredicate}
              onRemoveActionPredicate={handleRemoveActionPredicate}
              onCycleFlowSuccessType={handleCycleFlowSuccessType}
              predicateTypes={PREDICATE_TYPE_CATALOG}
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
    </>
  );
}

export default App;