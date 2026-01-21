import "./Header.css";
import type { HeaderProps } from "./types";
import { Dropdown } from "./Dropdown";
import { FileDropdown } from "./FileDropdown";
import { UserMenu } from "./UserMenu";

export default function Header({
  theme,
  onToggleTheme,
  onImportParameterTypes,
  onImportPredicateTypes,
  onImportActionTypes,
  onExportParameterTypesTxt,
  onExportPredicateTypesTxt,
  onExportActionTypesTxt,
  onImportParameterInstances,
  onImportPredicateInstances,
  onImportActionInstances,
  onExportTypesAndInstances,
  onImportTypesAndInstances,
  onExportCanvasGraph,
  onImportCanvasGraph,
}: HeaderProps) {
  const isDarkMode = theme === "dark";

  return (
    <header className="header">
      <div className="header-left">
        <nav className="header-nav">
          <FileDropdown
            onExportCanvasGraph={onExportCanvasGraph}
            onImportCanvasGraph={onImportCanvasGraph}
            onExportTypesAndInstances={onExportTypesAndInstances}
            onImportTypesAndInstances={onImportTypesAndInstances}
            onImportParameterTypes={onImportParameterTypes}
            onImportPredicateTypes={onImportPredicateTypes}
            onImportActionTypes={onImportActionTypes}
            onExportParameterTypesTxt={onExportParameterTypesTxt}
            onExportPredicateTypesTxt={onExportPredicateTypesTxt}
            onExportActionTypesTxt={onExportActionTypesTxt}
            onImportParameterInstances={onImportParameterInstances}
            onImportPredicateInstances={onImportPredicateInstances}
            onImportActionInstances={onImportActionInstances}
          />
          <Dropdown
            title="Edit"
            items={["Undo", "Redo", "Cut", "Copy", "Paste", "Delete"]}
          />
          <Dropdown
            title="View"
            items={["Zoom In", "Zoom Out", "Reset Zoom", "Toggle Grid"]}
          />
        </nav>

        <div className="header-separator"></div>

        <div className="header-actions">
          <button className="icon-btn" title="Undo" aria-label="Undo">
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M9 14L4 9l5-5" />
              <path d="M4 9h10.5a5.5 5.5 0 0 1 5.5 5.5v0a5.5 5.5 0 0 1-5.5 5.5H11" />
            </svg>
          </button>

          <button className="icon-btn" title="Redo" aria-label="Redo">
            <svg
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M15 14l5-5-5-5" />
              <path d="M20 9H9.5A5.5 5.5 0 0 0 4 14.5v0A5.5 5.5 0 0 0 9.5 20H13" />
            </svg>
          </button>
        </div>
      </div>

      <div className="header-right">
        <UserMenu />

        <button
          className="icon-btn theme-toggle"
          onClick={onToggleTheme}
          aria-label={`Switch to ${isDarkMode ? "light" : "dark"} mode`}
          title={isDarkMode ? "Switch to light mode" : "Switch to dark mode"}
        >
          {isDarkMode ? (
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
            </svg>
          ) : (
            <svg
              width="18"
              height="18"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="12" cy="12" r="5" />
              <line x1="12" y1="1" x2="12" y2="3" />
              <line x1="12" y1="21" x2="12" y2="23" />
              <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
              <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
              <line x1="1" y1="12" x2="3" y2="12" />
              <line x1="21" y1="12" x2="23" y2="12" />
              <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
              <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
            </svg>
          )}
        </button>
      </div>
    </header>
  );
}