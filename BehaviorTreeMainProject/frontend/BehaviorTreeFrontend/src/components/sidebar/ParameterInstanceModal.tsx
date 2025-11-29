import { useEffect, useMemo, useState } from "react";
import { createId } from "../../utils/id.ts";
import type { ParameterInstance, ParameterType } from "./types.ts";

interface ParameterInstanceModalProps {
  isOpen: boolean;
  mode: "add" | "edit";
  title: string;
  initialValue: ParameterInstance;
  parameterTypes: ParameterType[];
  onClose: () => void;
  onSave: (value: ParameterInstance) => void;
}

export default function ParameterInstanceModal({
  isOpen,
  mode,
  title,
  initialValue,
  parameterTypes,
  onClose,
  onSave,
}: ParameterInstanceModalProps) {
  const instanceId = useMemo(
    () => initialValue.id || createId("param-instance"),
    [initialValue.id]
  );
  const [nameValue, setNameValue] = useState(initialValue.name);
  const [typeId, setTypeId] = useState(
    () => initialValue.typeId || parameterTypes[0]?.id || ""
  );
  const [propertyValues, setPropertyValues] = useState<Record<string, string>>(
    () => ({ ...initialValue.propertyValues })
  );

  useEffect(() => {
    if (nameValue !== initialValue.name) {
      setNameValue(initialValue.name);
    }

    const fallbackTypeId = initialValue.typeId || parameterTypes[0]?.id || "";
    if (typeId !== fallbackTypeId) {
      setTypeId(fallbackTypeId);
    }

    const nextValues = { ...initialValue.propertyValues };
    const currentKeys = Object.keys(propertyValues);
    const nextKeys = Object.keys(nextValues);
    const valuesChanged =
      currentKeys.length !== nextKeys.length ||
      nextKeys.some((key) => propertyValues[key] !== nextValues[key]);

    if (valuesChanged) {
      setPropertyValues(nextValues);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialValue, parameterTypes]);

  const selectedType = useMemo(
    () => parameterTypes.find((type) => type.id === typeId) ?? null,
    [parameterTypes, typeId]
  );

  const resolvedPropertyValues = useMemo(() => {
    if (!selectedType) {
      return {};
    }

    return selectedType.properties.reduce<Record<string, string>>(
      (acc, property) => {
        acc[property.id] = propertyValues[property.id] ?? "";
        return acc;
      },
      {}
    );
  }, [selectedType, propertyValues]);

  if (!isOpen) {
    return null;
  }

  const isFormDisabled = parameterTypes.length === 0 || !selectedType;
  const trimmedName = nameValue.trim();
  const allPropertiesFilled = selectedType
    ? selectedType.properties.every((property) =>
        (resolvedPropertyValues[property.id] ?? "").trim()
      )
    : false;
  const isFormValid = !isFormDisabled && trimmedName && allPropertiesFilled;

  const handleTypeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const nextTypeId = event.target.value;
    setTypeId(nextTypeId);
    setPropertyValues({});
  };

  const handlePropertyValueChange = (propertyId: string, value: string) => {
    setPropertyValues((prev) => ({ ...prev, [propertyId]: value }));
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!isFormValid || !selectedType) {
      return;
    }

    const sanitizedValues = selectedType.properties.reduce<Record<string, string>>(
      (acc, property) => {
        acc[property.id] = (resolvedPropertyValues[property.id] ?? "").trim();
        return acc;
      },
      {}
    );

    onSave({
      ...initialValue,
      id: instanceId,
      name: trimmedName,
      type: selectedType.name,
      typeId: selectedType.id,
      propertyValues: sanitizedValues,
    });
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content instance-modal" onClick={(event) => event.stopPropagation()}>
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="modal-close-btn" onClick={onClose}>
            &times;
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label className="modal-label" htmlFor="instance-name-input">
              Instance Name
            </label>
            <input
              id="instance-name-input"
              className="modal-input"
              type="text"
              value={nameValue}
              onChange={(event) => setNameValue(event.target.value)}
              placeholder="e.g., selected_tool"
            />
          </div>

          <div className="form-group">
            <label className="modal-label" htmlFor="instance-type-select">
              Parameter Type
            </label>
            <select
              id="instance-type-select"
              className="modal-select"
              value={typeId}
              onChange={handleTypeChange}
              disabled={parameterTypes.length === 0}
            >
              <option value="" disabled>
                Select a parameter type...
              </option>
              {parameterTypes.map((parameterType) => (
                <option key={parameterType.id} value={parameterType.id}>
                  {parameterType.name}
                </option>
              ))}
            </select>
            {selectedType && (
              <p className="instance-type-hint">
                Base type: <strong>{selectedType.type || "n/a"}</strong>
              </p>
            )}
          </div>

          <div className="form-group">
            <span className="modal-label">Property Values</span>
            {selectedType && selectedType.properties.length > 0 ? (
              <div className="instance-properties">
                {selectedType.properties.map((property) => {
                  const fieldId = `instance-${property.id}`;
                  return (
                    <div key={property.id} className="instance-property-row">
                      <label className="instance-property-label" htmlFor={fieldId}>
                        {property.name}
                        <span className="instance-property-type">
                          {property.valueType}
                        </span>
                      </label>
                      <input
                        id={fieldId}
                        className="modal-input instance-property-input"
                        type="text"
                        value={resolvedPropertyValues[property.id] ?? ""}
                        onChange={(event) =>
                          handlePropertyValueChange(property.id, event.target.value)
                        }
                        placeholder={`Enter ${property.valueType}`}
                      />
                    </div>
                  );
                })}
              </div>
            ) : (
              <p className="instance-property-empty">
                {selectedType
                  ? "This parameter type has no properties."
                  : "Define a parameter type to provide property values."}
              </p>
            )}
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-save" disabled={!isFormValid}>
              {mode === "add" ? "Create Instance" : "Save Changes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
