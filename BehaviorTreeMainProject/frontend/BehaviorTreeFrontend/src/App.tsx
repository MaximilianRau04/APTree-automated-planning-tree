import { useCallback, useEffect, useState } from "react";
import "./App.css";
import Header from "./components/header/Header.tsx";
import Sidebar from "./components/sidebar/Sidebar.tsx";
import EditorCanvas from "./components/editor/EditorCanvas.tsx";
import type { CanvasNode } from "./components/editor/types";
import type { DraggedSidebarItem } from "./components/editor/dragTypes";
import { createId } from "./utils/id";

type ThemeMode = "light" | "dark";

const STORAGE_KEY = "aptree-preferred-theme";

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

  useEffect(() => {
    const root = document.documentElement;
    root.dataset.theme = theme;
    root.style.colorScheme = theme;
    window.localStorage.setItem(STORAGE_KEY, theme);
  }, [theme]);

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

  const handleToggleTheme = () => {
    setTheme((current) => (current === "light" ? "dark" : "light"));
    setUserLockedTheme(true);
  };

  const handleDropOnCanvas = useCallback(
    (item: DraggedSidebarItem, position: { x: number; y: number }) => {
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

  return (
    <div className="app-container">
      <Sidebar />
      <div className="main-content">
        <Header theme={theme} onToggleTheme={handleToggleTheme} />
        <div className="editor" role="main">
          <EditorCanvas
            nodes={canvasNodes}
            onDropNode={handleDropOnCanvas}
            onMoveNode={handleMoveNode}
            onRemoveNode={handleRemoveNode}
          />
        </div>
      </div>
    </div>
  );
}

export default App;