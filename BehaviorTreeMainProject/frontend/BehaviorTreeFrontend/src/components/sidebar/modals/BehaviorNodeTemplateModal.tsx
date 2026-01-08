import { useEffect, useRef, useState } from "react";
import type { StructuredItem } from "../utils/types";

interface BehaviorNodeTemplateModalProps {
  isOpen: boolean;
  mode: "add" | "edit";
  title: string;
  initialValue: StructuredItem;
  defaultTypeLabel: string;
  onClose: () => void;
  onSave: (value: StructuredItem) => void;
}

export default function BehaviorNodeTemplateModal({
  isOpen,
  mode,
  title,
  initialValue,
  defaultTypeLabel,
  onClose,
  onSave,
}: BehaviorNodeTemplateModalProps) {
  const [nameValue, setNameValue] = useState(initialValue.name);
  const [typeLabel, setTypeLabel] = useState(
    initialValue.type || defaultTypeLabel
  );
  const [descriptionValue, setDescriptionValue] = useState(
    initialValue.description ?? ""
  );
  const [itemId, setItemId] = useState(initialValue.id);

  const nameInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!isOpen) {
      return undefined;
    }

    const frame = requestAnimationFrame(() => {
      setNameValue(initialValue.name);
      setTypeLabel(initialValue.type || defaultTypeLabel);
      setDescriptionValue(initialValue.description ?? "");
      setItemId(initialValue.id);
    });

    return () => cancelAnimationFrame(frame);
  }, [
    isOpen,
    initialValue.id,
    initialValue.name,
    initialValue.type,
    initialValue.description,
    defaultTypeLabel,
  ]);

  useEffect(() => {
    if (isOpen && nameInputRef.current) {
      setTimeout(() => nameInputRef.current?.focus(), 50);
    }
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    const trimmedName = nameValue.trim();
    const trimmedType = (typeLabel || defaultTypeLabel).trim();
    const trimmedDescription = descriptionValue.trim();

    if (!trimmedName) {
      return;
    }

    onSave({
      ...initialValue,
      id: itemId,
      name: trimmedName,
      type: trimmedType || defaultTypeLabel,
      description: trimmedDescription,
    });
  };

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true">
      <div className="modal-content" onClick={(event) => event.stopPropagation()}>
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="modal-close-btn" onClick={onClose} type="button">
            &times;
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label className="modal-label" htmlFor="behavior-template-name">
              Display Name
            </label>
            <input
              ref={nameInputRef}
              id="behavior-template-name"
              type="text"
              className="modal-input"
              placeholder="e.g., Patrol Service"
              value={nameValue}
              onChange={(event) => setNameValue(event.target.value)}
            />
          </div>

          <div className="form-group">
            <label className="modal-label" htmlFor="behavior-template-type">
              Badge Label
            </label>
            <input
              id="behavior-template-type"
              type="text"
              className="modal-input"
              placeholder={`Defaults to "${defaultTypeLabel}"`}
              value={typeLabel}
              onChange={(event) => setTypeLabel(event.target.value)}
            />
          </div>

          <div className="form-group">
            <label className="modal-label" htmlFor="behavior-template-description">
              Description
            </label>
            <textarea
              id="behavior-template-description"
              className="modal-textarea"
              placeholder="Optional description shown in the wizard"
              rows={4}
              value={descriptionValue}
              onChange={(event) => setDescriptionValue(event.target.value)}
            />
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
            <button
              type="submit"
              className="btn-save"
              disabled={!nameValue.trim()}
            >
              {mode === "add" ? "Create" : "Save Changes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
