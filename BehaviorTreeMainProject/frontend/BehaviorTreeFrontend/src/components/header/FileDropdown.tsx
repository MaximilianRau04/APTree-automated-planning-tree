import { useEffect, useRef, useState, type ChangeEvent } from "react";
import type { HeaderProps } from "./types";

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

export function FileDropdown({
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
    <button className="dropdown-item" type="button" onClick={onClick}>
      <span className="dropdown-item-title">← Back</span>
    </button>
  );

  /**
   * renders an action item in the dropdown menu.
   * @param label 
   * @param onSelect 
   * @param hint 
   * @param disabled 
   * @returns JSX element for the action item 
   */
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

  /**
   * renders a file input item in the dropdown menu.
   * @param label 
   * @param accept 
   * @param onFileSelect 
   * @param hint 
   * @returns JSX element for the file input item
   */
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

  /**
   * renders the appropriate menu based on the current flow state.
   * @returns JSX elements for the current menu screen 
   */
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
          {renderActionItem("Import and Export", () =>
            setFlow({ screen: "instances" })
          )}
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

          {renderActionItem("Import item…", () =>
            setFlow({ screen: "domain", mode: "import" })
          )}
          {renderActionItem("Export item…", () =>
            setFlow({ screen: "domain", mode: "export" })
          )}
        </>
      );
    }

    if (flow.screen === "domain") {
      return (
        <>
          {renderBack(() => setFlow({ screen: "instances" }))}
          {renderLabel("l", flow.mode === "import" ? "Import" : "Export")}
          {renderActionItem("Action", () =>
            setFlow({ screen: "kind", mode: flow.mode, domain: "action" })
          )}
          {renderActionItem("Predicate", () =>
            setFlow({
              screen: "kind",
              mode: flow.mode,
              domain: "predicate",
            })
          )}
          {renderActionItem("Parameter", () =>
            setFlow({
              screen: "kind",
              mode: flow.mode,
              domain: "parameter",
            })
          )}
        </>
      );
    }

    if (flow.screen === "kind") {
      return (
        <>
          {renderBack(() => setFlow({ screen: "domain", mode: flow.mode }))}
          {renderLabel(
            "l",
            `${flow.mode === "import" ? "Import" : "Export"} ${labelFor(
              flow.domain
            )}`
          )}
          {renderActionItem("Type", () =>
            setFlow({
              screen: "format",
              mode: flow.mode,
              domain: flow.domain,
              kind: "type",
            })
          )}
          {renderActionItem("Instance", () =>
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
      const title = `${flow.mode === "import" ? "Import" : "Export"} ${labelFor(
        flow.domain
      )} ${kindLabel(flow.kind)}`;
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
