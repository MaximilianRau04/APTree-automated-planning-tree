import { useMemo, useState } from "react";
import { BASIC_TYPE_OPTIONS } from "../../constants/basicTypes";
import { createId } from "../../utils/id";
import type { ParameterType, TypeProperty } from "./types";

interface TypeDefinitionModalProps {
  isOpen: boolean;
  mode: "add" | "edit";
  title: string;
  initialValue: ParameterType;
  onClose: () => void;
  onSave: (value: ParameterType) => void;
}

function createEmptyProperty(): TypeProperty {
  return {
    id: createId("property"),
    name: "",
    valueType: "",
  };
}

function sanitizeProperties(properties: TypeProperty[]): TypeProperty[] {
  return properties.map((property) => ({
    ...property,
    name: property.name.trim(),
    valueType: property.valueType.trim(),
  }));
}

export default function TypeDefinitionModal({
  isOpen,
  mode,
  title,
  initialValue,
  onClose,
  onSave,
}: TypeDefinitionModalProps) {
  const [nameValue, setNameValue] = useState(initialValue.name);
  const [baseType, setBaseType] = useState(initialValue.type);
  const [properties, setProperties] = useState<TypeProperty[]>(() =>
    initialValue.properties.length > 0
      ? initialValue.properties.map((property) => ({ ...property }))
      : []
  );
  const [typeId] = useState(initialValue.id);

  const hasDuplicatePropertyNames = useMemo(() => {
    const seen = new Set<string>();
    return properties.some((property) => {
      const trimmedName = property.name.trim();
      if (!trimmedName) {
        return false;
      }
      if (seen.has(trimmedName)) {
        return true;
      }
      seen.add(trimmedName);
      return false;
    });
  }, [properties]);

  const isFormValid = useMemo(() => {
    const trimmedName = nameValue.trim();
    if (!trimmedName) {
      return false;
    }
    if (!baseType) {
      return false;
    }
    if (properties.some((property) => !property.name.trim() || !property.valueType)) {
      return false;
    }
    if (hasDuplicatePropertyNames) {
      return false;
    }
    return true;
  }, [nameValue, baseType, properties, hasDuplicatePropertyNames]);

  if (!isOpen) {
    return null;
  }

  const handleAddProperty = () => {
    setProperties((prev) => [...prev, createEmptyProperty()]);
  };

  const handleRemoveProperty = (propertyId: string) => {
    setProperties((prev) => prev.filter((property) => property.id !== propertyId));
  };

  const handlePropertyNameChange = (propertyId: string, value: string) => {
    setProperties((prev) =>
      prev.map((property) =>
        property.id === propertyId ? { ...property, name: value } : property
      )
    );
  };

  const handlePropertyTypeChange = (propertyId: string, value: string) => {
    setProperties((prev) =>
      prev.map((property) =>
        property.id === propertyId ? { ...property, valueType: value } : property
      )
    );
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!isFormValid) {
      return;
    }

    const sanitized = sanitizeProperties(properties);
    const next: ParameterType = {
      ...initialValue,
      id: typeId || createId("param-type"),
      name: nameValue.trim(),
      type: baseType,
      properties: sanitized.map((property) => ({
        ...property,
        id: property.id || createId("property"),
      })),
    };

    onSave(next);
  };

  return (
    <div className="modal-overlay type-modal-overlay">
      <div
        className="modal-content type-modal-content"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="modal-close-btn" onClick={onClose}>
            &times;
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form type-modal-form">
          <div className="form-group">
            <label className="modal-label" htmlFor="type-name-input">
              Type Name
            </label>
            <input
              id="type-name-input"
              className="modal-input"
              value={nameValue}
              onChange={(event) => setNameValue(event.target.value)}
              placeholder="e.g., Sensor"
              type="text"
            />
          </div>

          <div className="form-group">
            <label className="modal-label" htmlFor="type-base-select">
              Base Type
            </label>
            <select
              id="type-base-select"
              className="modal-select"
              value={baseType}
              onChange={(event) => setBaseType(event.target.value)}
            >
              <option value="" disabled>
                Select a base type...
              </option>
              {BASIC_TYPE_OPTIONS.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group type-modal-properties">
            <div className="property-list-header">
              <span className="modal-label">Properties</span>
              <button
                type="button"
                className="property-add-btn"
                onClick={handleAddProperty}
              >
                + Add Property
              </button>
            </div>

            {properties.length === 0 ? (
              <p className="property-empty-hint">No properties defined yet.</p>
            ) : (
              <div className="property-list">
                {properties.map((property) => (
                  <div key={property.id} className="property-row">
                    <input
                      className="modal-input property-input"
                      type="text"
                      value={property.name}
                      onChange={(event) =>
                        handlePropertyNameChange(property.id, event.target.value)
                      }
                      placeholder="property-name"
                    />
                    <select
                      className="modal-select property-select"
                      value={property.valueType}
                      onChange={(event) =>
                        handlePropertyTypeChange(property.id, event.target.value)
                      }
                    >
                      <option value="" disabled>
                        Select type...
                      </option>
                      {BASIC_TYPE_OPTIONS.map((option) => (
                        <option key={option} value={option}>
                          {option}
                        </option>
                      ))}
                    </select>
                    <button
                      type="button"
                      className="property-remove-btn"
                      onClick={() => handleRemoveProperty(property.id)}
                      aria-label="Remove property"
                      title="Remove property"
                    >
                      &times;
                    </button>
                  </div>
                ))}
              </div>
            )}

            {hasDuplicatePropertyNames && (
              <p className="property-error" role="alert">
                Property names must be unique.
              </p>
            )}
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-save" disabled={!isFormValid}>
              {mode === "add" ? "Create Type" : "Save Changes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
