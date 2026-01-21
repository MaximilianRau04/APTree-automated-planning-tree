import { useEffect, useRef, useState, type ChangeEvent } from "react";
import type { DropdownProps, NormalizedDropdownItem } from "./types";

/**
 * Component for a dropdown menu in the header.
 */
export function Dropdown({ title, items }: DropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const normalizedItems: NormalizedDropdownItem[] = items.map((entry) => {
    if (typeof entry === "string") {
      return { kind: "action", label: entry };
    }

    if (
      entry.kind === "file" ||
      entry.kind === "divider" ||
      entry.kind === "label"
    ) {
      return entry;
    }

    return { ...entry, kind: "action" };
  });

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <div className="dropdown" ref={dropdownRef}>
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
                <div key={`label-${index}`} className="dropdown-group-label">
                  {item.label}
                </div>
              );
            }

            if (item.kind === "file") {
              const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
                const file = event.target.files?.[0];
                if (file) {
                  item.onFileSelect(file);
                }
                event.target.value = "";
                setIsOpen(false);
              };

              return (
                <label key={`file-${index}`} className="dropdown-item file-upload">
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
