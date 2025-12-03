import { useCallback, useEffect, useState } from "react";
import "./App.css";
import Header from "./components/header/Header.tsx";
import Sidebar from "./components/sidebar/Sidebar.tsx";
import EditorCanvas from "./components/editor/EditorCanvas.tsx";
import type { CanvasNode } from "./components/editor/types";
import type { DraggedSidebarItem } from "./components/editor/dragTypes";
import { createId } from "./utils/id";
import { createBehaviorNode } from "./components/editor/flowNodeFactory";
import {
  BEHAVIOR_NODE_OPTION_MAP,
  BT_NODES_KEY,
} from "./components/sidebar/utils/constants";
import type { BehaviorNodeOption } from "./components/sidebar/utils/types";

type ThemeMode = "light" | "dark";

const STORAGE_KEY = "aptree-preferred-theme";

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

  const handleRemoveNode = useCallback((nodeId: string) => {
    setCanvasNodes((prev) => prev.filter((node) => node.id !== nodeId));
  }, []);

  const handleAddActionPrecondition = useCallback((nodeId: string) => {
    console.info("Add precondition for action node", nodeId);
  }, []);

  const handleAddActionEffect = useCallback((nodeId: string) => {
    console.info("Add effect for action node", nodeId);
  }, []);

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

  return (
    <div className="app-container">
      <Sidebar onCreateBehaviorNode={handleCreateBehaviorNode} />
      <div className="main-content">
        <Header theme={theme} onToggleTheme={handleToggleTheme} />
        <div className="editor" role="main">
          <EditorCanvas
            nodes={canvasNodes}
            onDropNode={handleDropOnCanvas}
            onMoveNode={handleMoveNode}
            onRemoveNode={handleRemoveNode}
            onAddActionPrecondition={handleAddActionPrecondition}
            onAddActionEffect={handleAddActionEffect}
          />
        </div>
      </div>
    </div>
  );
}

export default App;