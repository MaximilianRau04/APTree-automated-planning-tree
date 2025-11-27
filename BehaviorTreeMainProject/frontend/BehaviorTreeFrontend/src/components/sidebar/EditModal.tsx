import { useState, useEffect, useRef } from "react";
import type { StructuredItem } from "./types.ts";

interface EditModalProps {
  isOpen: boolean;
  title: string;
  initialValue: StructuredItem;
  onClose: () => void;
  onSave: (value: StructuredItem) => void;
}

// list of basic types for the dropdown
const BASIC_TYPES = [
  "Integer",
  "Double",
  "String",
  "Boolean",
  "Agent",
  "Element",
  "Location",
  "Layer",
  "Module",
  "Tool",
  "Object",
  "List",
  "Set",
  "Map",
];

export default function EditModal({
  isOpen,
  title,
  initialValue,
  onClose,
  onSave,
}: EditModalProps) {
  const [nameValue, setNameValue] = useState(initialValue.name);
  const [typeValue, setTypeValue] = useState(initialValue.type);

  const nameInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    setNameValue(initialValue.name);
    setTypeValue(initialValue.type);
  }, [initialValue]);

  // focus the name input when modal opens
  useEffect(() => {
    if (isOpen && nameInputRef.current) {
      setTimeout(() => nameInputRef.current?.focus(), 50);
    }
  }, [isOpen]);

  if (!isOpen) return null;

  /**
   * handles the form submission
   * @param e React.FormEvent
   */
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (nameValue.trim() && typeValue.trim()) {
      onSave({ name: nameValue.trim(), type: typeValue.trim() });
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="modal-close-btn" onClick={onClose}>
            &times;
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label className="modal-label">Name:</label>
            <input
              ref={nameInputRef}
              type="text"
              className="modal-input"
              value={nameValue}
              onChange={(e) => setNameValue(e.target.value)}
              placeholder="e.g., target_entity"
            />
          </div>

          <div className="form-group">
            <label className="modal-label">Type:</label>
            <select
              className="modal-select"
              value={typeValue}
              onChange={(e) => setTypeValue(e.target.value)}
            >
              <option value="" disabled>
                Select a type...
              </option>
              {BASIC_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
            <button
              type="submit"
              className="btn-save"
              disabled={!nameValue.trim() || !typeValue.trim()}
            >
              Save
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
