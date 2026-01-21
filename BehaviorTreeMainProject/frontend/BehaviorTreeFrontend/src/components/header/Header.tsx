import { useState, useEffect, useRef, type ChangeEvent } from "react";
import "./Header.css";
import type {
  DropdownProps,
  HeaderProps,
  NormalizedDropdownItem,
} from "./types.ts";
import { UserMenu } from "./UserMenu";

type Domain = "action" | "predicate" | "parameter";
type Mode = "import" | "export";
type Kind = "type" | "instance";

type FileFlowState =
  | { screen: "root" }
  | { screen: "graph" }
  | { screen: "instances" }
  | { screen: "domain"; mode: Mode }
  | { screen: "kind"; mode: Mode; domain: Domain }
  | { screen: "format"; mode: Mode; domain: Domain; kind: Kind };

function scopeFor(domain: Domain, kind: Kind) {
  if (domain === "action") {
    return kind === "type" ? "actions" : "actionInstances";
  }
  if (domain === "predicate") {
    return kind === "type" ? "predTypes" : "predInstances";
  }
  return kind === "type" ? "paramTypes" : "paramInstances";
}

function labelFor(domain: Domain) {
  if (domain === "action") {
    return "Action";
  }
  if (domain === "predicate") {
    return "Predicate";
  }
  return "Parameter";
}

function kindLabel(kind: Kind) {
  return kind === "type" ? "Type" : "Instance";
}

function FileDropdown({
  onExportCanvasGraph,
  onImportCanvasGraph,
  onExportTypesAndInstances,
  onImportTypesAndInstances,
  onImportParameterTypes,
  onImportPredicateTypes,
  onImportActionTypes,
  onExportParameterTypesTxt,
  onExportPredicateTypesTxt,
  onExportActionTypesTxt,
  onImportParameterInstances,
  onImportPredicateInstances,
  onImportActionInstances,
}: Pick<
  HeaderProps,
  | "onExportCanvasGraph"
  | "onImportCanvasGraph"
  | "onExportTypesAndInstances"
  | "onImportTypesAndInstances"
  | "onImportParameterTypes"
  | "onImportPredicateTypes"
  | "onImportActionTypes"
  | "onExportParameterTypesTxt"
  | "onExportPredicateTypesTxt"
  | "onExportActionTypesTxt"
  | "onImportParameterInstances"
  | "onImportPredicateInstances"
  | "onImportActionInstances"
>) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const [flow, setFlow] = useState<FileFlowState>({ screen: "root" });

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
        setFlow({ screen: "root" });
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  const close = () => {
    setIsOpen(false);
    setFlow({ screen: "root" });
  };

  const renderDivider = (key: string) => (
    <div key={key} className="dropdown-divider" role="separator" />
  );

  const renderLabel = (key: string, text: string) => (
    <div key={key} className="dropdown-group-label">
      {text}
    </div>
  );

  const renderBack = (onClick: () => void) => (
    <button
      className="dropdown-item"
      type="button"
      onClick={onClick}
    >
      <span className="dropdown-item-title">← Back</span>
    </button>
  );

  const renderActionItem = (
    label: string,
    onSelect: () => void,
    hint?: string,
    disabled?: boolean
  ) => (
    <button
      className="dropdown-item"
      type="button"
      onClick={() => {
        if (disabled) {
          return;
        }
        onSelect();
      }}
      disabled={disabled}
    >
      <span className="dropdown-item-title">{label}</span>
      {hint && <span className="dropdown-item-hint">{hint}</span>}
    </button>
  );

  const renderFileItem = (
    label: string,
    accept: string,
    onFileSelect: (file: File) => void,
    hint?: string
  ) => {
    const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];
      if (file) {
        onFileSelect(file);
      }
      event.target.value = "";
      close();
    };

    return (
      <label className="dropdown-item file-upload">
        <span className="dropdown-item-title">{label}</span>
        {hint && <span className="dropdown-item-hint">{hint}</span>}
        <input
          type="file"
          className="file-upload-input"
          accept={accept}
          onChange={handleFileChange}
        />
      </label>
    );
  };

  const renderMenu = () => {
    if (flow.screen === "root") {
      return (
        <>
          {renderLabel("l1", "Graph")}
          {renderActionItem("Export (JSON)", () => {
            onExportCanvasGraph();
            close();
          })}
          {renderFileItem(
            "Import (JSON)",
            ".json,application/json",
            onImportCanvasGraph
          )}

          {renderDivider("d1")}

          {renderLabel("l2", "Types & Instances")}
          {renderActionItem("Import and Export", () => setFlow({ screen: "instances" }))}
        </>
      );
    }

    if (flow.screen === "instances") {
      return (
        <>
          {renderBack(() => setFlow({ screen: "root" }))}
          {renderLabel("l", "Types & Instances")}

          {renderFileItem(
            "Import all (JSON)",
            ".json,application/json",
            (file) => onImportTypesAndInstances(file, "full")
          )}
          {renderActionItem("Export all (JSON)", () => {
            onExportTypesAndInstances("full");
            close();
          })}

          {renderDivider("d")}

          {renderActionItem("Import item…", () => setFlow({ screen: "domain", mode: "import" }))}
          {renderActionItem("Export item…", () => setFlow({ screen: "domain", mode: "export" }))}
        </>
      );
    }

    if (flow.screen === "domain") {
      return (
        <>
          {renderBack(() => setFlow({ screen: "instances" }))}
          {renderLabel("l", flow.mode === "import" ? "Import" : "Export")}
          {renderActionItem("Action", () => setFlow({ screen: "kind", mode: flow.mode, domain: "action" }))}
          {renderActionItem("Predicate", () => setFlow({ screen: "kind", mode: flow.mode, domain: "predicate" }))}
          {renderActionItem("Parameter", () => setFlow({ screen: "kind", mode: flow.mode, domain: "parameter" }))}
        </>
      );
    }

    if (flow.screen === "kind") {
      return (
        <>
          {renderBack(() => setFlow({ screen: "domain", mode: flow.mode }))}
          {renderLabel(
            "l",
            `${flow.mode === "import" ? "Import" : "Export"} ${labelFor(flow.domain)}`
          )}
          {renderActionItem(
            "Type",
            () =>
              setFlow({
                screen: "format",
                mode: flow.mode,
                domain: flow.domain,
                kind: "type",
              })
          )}
          {renderActionItem(
            "Instance",
            () =>
              setFlow({
                screen: "format",
                mode: flow.mode,
                domain: flow.domain,
                kind: "instance",
              })
          )}
        </>
      );
    }

    if (flow.screen === "format") {
      const scope = scopeFor(flow.domain, flow.kind);
      const title = `${flow.mode === "import" ? "Import" : "Export"} ${labelFor(flow.domain)} ${kindLabel(flow.kind)}`;
      const allowTxt = true;

      const doImportTxt = (file: File) => {
        if (flow.kind === "type") {
          if (flow.domain === "action") {
            onImportActionTypes(file);
            return;
          }
          if (flow.domain === "predicate") {
            onImportPredicateTypes(file);
            return;
          }
          onImportParameterTypes(file);
          return;
        }

        if (flow.domain === "action") {
          onImportActionInstances(file);
          return;
        }
        if (flow.domain === "predicate") {
          onImportPredicateInstances(file);
          return;
        }
        onImportParameterInstances(file);
      };

      return (
        <>
          {renderBack(() =>
            setFlow({ screen: "kind", mode: flow.mode, domain: flow.domain })
          )}
          {renderLabel("l", title)}

          {flow.mode === "import" ? (
            <>
              {renderFileItem(
                "Import (JSON)",
                ".json,application/json",
                (file) => onImportTypesAndInstances(file, scope)
              )}
              {allowTxt
                ? renderFileItem("Import (TXT)", ".txt", doImportTxt)
                : renderActionItem("Import (TXT)", () => {}, undefined, true)}
            </>
          ) : (
            <>
              {renderActionItem("Export (JSON)", () => {
                onExportTypesAndInstances(scope);
                close();
              })}
              {renderActionItem(
                "Export (TXT)",
                () => {
                  if (flow.kind === "instance") {
                    return;
                  }

                  if (flow.domain === "action") {
                    onExportActionTypesTxt();
                    close();
                    return;
                  }
                  if (flow.domain === "predicate") {
                    onExportPredicateTypesTxt();
                    close();
                    return;
                  }
                  onExportParameterTypesTxt();
                  close();
                },
                undefined,
                flow.kind === "instance"
              )}
            </>
          )}
        </>
      );
    }

    return null;
  };

  return (
    <div className="dropdown" ref={dropdownRef}>
      <button
        className="dropdown-trigger"
        onClick={() => {
          if (isOpen) {
            close();
            return;
          }
          setFlow({ screen: "root" });
          setIsOpen(true);
        }}
      >
        File
      </button>
      {isOpen && <div className="dropdown-menu">{renderMenu()}</div>}
    </div>
  );
}

/**
 * Component for a dropdown menu in the header.
 * @param param0 
 * @returns The Dropdown component. 
 */
function Dropdown({ title, items }: DropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const normalizedItems: NormalizedDropdownItem[] = items.map((entry) => {
    if (typeof entry === "string") {
      return { kind: "action", label: entry };
    }

    if (entry.kind === "file" || entry.kind === "divider" || entry.kind === "label") {
      return entry;
    }

    return { ...entry, kind: "action" };
  });

  /**
   * Closes the dropdown when clicking outside of it
   * @param event MouseEvent
   */
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <div 
      className="dropdown"
      ref={dropdownRef}
    >
      <button 
        className="dropdown-trigger"
        onClick={() => setIsOpen(!isOpen)} 
      >
        {title}
      </button>
      
      {isOpen && (
        <div className="dropdown-menu">
          {normalizedItems.map((item, index) => {
            if (item.kind === "divider") {
              return (
                <div
                  key={`divider-${index}`}
                  className="dropdown-divider"
                  role="separator"
                />
              );
            }

            if (item.kind === "label") {
              return (
                <div
                  key={`label-${index}`}
                  className="dropdown-group-label"
                >
                  {item.label}
                </div>
              );
            }

            if (item.kind === "file") {
              const handleFileChange = (
                event: ChangeEvent<HTMLInputElement>
              ) => {
                const file = event.target.files?.[0];
                if (file) {
                  item.onFileSelect(file);
                }
                event.target.value = "";
                setIsOpen(false);
              };

              return (
                <label
                  key={`file-${index}`}
                  className="dropdown-item file-upload"
                >
                  <span className="dropdown-item-title">{item.label}</span>
                  {item.hint && (
                    <span className="dropdown-item-hint">{item.hint}</span>
                  )}
                  <input
                    type="file"
                    className="file-upload-input"
                    accept={item.accept ?? ".txt"}
                    onChange={handleFileChange}
                  />
                </label>
              );
            }

            return (
              <button
                key={`action-${index}`}
                className="dropdown-item"
                type="button"
                onClick={() => {
                  item.onSelect?.();
                  setIsOpen(false);
                }}
                disabled={item.disabled}
              >
                <span className="dropdown-item-title">{item.label}</span>
                {item.hint && (
                  <span className="dropdown-item-hint">{item.hint}</span>
                )}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}

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
        ></button>
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