import { useEffect, useMemo, useState } from "react";
import { createId } from "../../../utils/id";
import type {
  ActionInstance,
  ActionInstanceModalProps,
  ParameterInstance,
  ParameterInstanceModalProps,
  PredicateInstance,
  PredicateInstanceModalProps,
  TypedInstanceModalProps,
} from "../utils/types";

type AnyInstance = ParameterInstance | PredicateInstance | ActionInstance;

/** shared modal rendering logic used by all typed instance modals. */
function BaseInstanceModal<TInstance extends AnyInstance>(
  props: TypedInstanceModalProps<TInstance>
) {
  const {
    isOpen,
    mode,
    title,
    initialValue,
    typeDefinitions,
    onClose,
    onSave,
    nameLabel = "Instance Name",
    namePlaceholder = "e.g., selected_item",
    typeLabel = "Type",
    typePlaceholder = "Select a type...",
    propertyValuesLabel = "Property Values",
    propertyEmptyMessage,
    createButtonLabel = "Create Instance",
    saveButtonLabel = "Save Changes",
    baseTypePrefixLabel = "Base type",
    enableNegationToggle = false,
    negationLabel = "Negate",
  } = props;

  const [nameValue, setNameValue] = useState(initialValue.name);
  const [typeId, setTypeId] = useState(
    () => initialValue.typeId || typeDefinitions[0]?.id || ""
  );
  const [propertyValues, setPropertyValues] = useState<Record<string, string>>(
    () => ({ ...initialValue.propertyValues })
  );
  const instanceId = useMemo(
    () => initialValue.id || createId("instance"),
    [initialValue.id]
  );
  const [isNegated, setIsNegated] = useState(() => {
    if (enableNegationToggle && "isNegated" in initialValue) {
      return Boolean(initialValue.isNegated);
    }
    return false;
  });

  useEffect(() => {
    if (nameValue !== initialValue.name) {
      setNameValue(initialValue.name);
    }

    const fallbackTypeId = initialValue.typeId || typeDefinitions[0]?.id || "";
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

    if (enableNegationToggle && "isNegated" in initialValue) {
      setIsNegated(Boolean(initialValue.isNegated));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialValue, typeDefinitions, enableNegationToggle]);

  const selectedType = useMemo(
    () => typeDefinitions.find((type) => type.id === typeId) ?? null,
    [typeDefinitions, typeId]
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

  const isFormDisabled = typeDefinitions.length === 0 || !selectedType;
  const trimmedName = nameValue.trim();
  const allPropertiesFilled = selectedType
    ? selectedType.properties.every((property) =>
        (resolvedPropertyValues[property.id] ?? "").trim()
      )
    : false;
  const isFormValid = !isFormDisabled && trimmedName && allPropertiesFilled;

  const emptyStateMessage = propertyEmptyMessage
    ? propertyEmptyMessage
    : selectedType
    ? "This type has no properties."
    : "Define a type to provide property values.";

  const handleTypeChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const nextTypeId = event.target.value;
    setTypeId(nextTypeId);
    setPropertyValues({});
  };

  const handlePropertyValueChange = (propertyId: string, value: string) => {
    setPropertyValues((prev) => ({ ...prev, [propertyId]: value }));
  };

  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!isFormValid || !selectedType) {
      return;
    }

    const sanitizedValues = selectedType.properties.reduce<
      Record<string, string>
    >((acc, property) => {
      acc[property.id] = (resolvedPropertyValues[property.id] ?? "").trim();
      return acc;
    }, {});

    const payload: AnyInstance = {
      ...initialValue,
      id: instanceId,
      name: trimmedName,
      type: selectedType.name,
      typeId: selectedType.id,
      propertyValues: sanitizedValues,
    };

    if (enableNegationToggle) {
      (payload as PredicateInstance).isNegated = isNegated;
    }

    onSave(payload as TInstance);
  };

  return (
    <div className="modal-overlay">
      <div
        className="modal-content instance-modal"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="modal-header">
          <h3>{title}</h3>
          <button className="modal-close-btn" onClick={onClose}>
            &times;
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label className="modal-label" htmlFor="instance-name-input">
              {nameLabel}
            </label>
            <input
              id="instance-name-input"
              className="modal-input"
              type="text"
              value={nameValue}
              onChange={(event) => setNameValue(event.target.value)}
              placeholder={namePlaceholder}
            />
          </div>

          <div className="form-group">
            <label className="modal-label" htmlFor="instance-type-select">
              {typeLabel}
            </label>
            <select
              id="instance-type-select"
              className="modal-select"
              value={typeId}
              onChange={handleTypeChange}
              disabled={typeDefinitions.length === 0}
            >
              <option value="" disabled>
                {typePlaceholder}
              </option>
              {typeDefinitions.map((definition) => (
                <option key={definition.id} value={definition.id}>
                  {definition.name}
                </option>
              ))}
            </select>
            {selectedType && (
              <p className="instance-type-hint">
                {baseTypePrefixLabel}: <strong>{selectedType.type || "n/a"}</strong>
              </p>
            )}
          </div>

          {enableNegationToggle && (
            <div className="form-group">
              <label className="modal-checkbox">
                <input
                  type="checkbox"
                  checked={isNegated}
                  onChange={(event) => setIsNegated(event.target.checked)}
                />
                {negationLabel}
              </label>
            </div>
          )}

          <div className="form-group">
            <span className="modal-label">{propertyValuesLabel}</span>
            {selectedType && selectedType.properties.length > 0 ? (
              <div className="instance-properties">
                {selectedType.properties.map((property) => {
                  const fieldId = `instance-${property.id}`;
                  return (
                    <div key={property.id} className="instance-property-row">
                      <label
                        className="instance-property-label"
                        htmlFor={fieldId}
                      >
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
                          handlePropertyValueChange(
                            property.id,
                            event.target.value
                          )
                        }
                        placeholder={`Enter ${property.valueType}`}
                      />
                    </div>
                  );
                })}
              </div>
            ) : (
              <p className="instance-property-empty">{emptyStateMessage}</p>
            )}
          </div>

          <div className="modal-footer">
            <button type="button" className="btn-cancel" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="btn-save" disabled={!isFormValid}>
              {mode === "add" ? createButtonLabel : saveButtonLabel}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

/**
 * default export remains for backward compatibility when only parameter instances are required.
 */
export default function ParameterInstanceModal(
  props: ParameterInstanceModalProps
) {
  return <BaseInstanceModal<ParameterInstance> {...props} />;
}

/** predicate instance modal with negation toggle enabled. */
export function PredicateInstanceModal(props: PredicateInstanceModalProps) {
  return (
    <BaseInstanceModal<PredicateInstance>
      {...props}
      enableNegationToggle
      nameLabel={props.nameLabel ?? "Predicate Instance Name"}
      namePlaceholder={
        props.namePlaceholder ?? "e.g., target_location_is_visible"
      }
      typeLabel={props.typeLabel ?? "Predicate Type"}
      negationLabel={props.negationLabel ?? "Negate predicate"}
      typePlaceholder={props.typePlaceholder ?? "Select a predicate type..."}
      propertyEmptyMessage={
        props.propertyEmptyMessage ??
        "This predicate type does not define any properties."
      }
      propertyValuesLabel={props.propertyValuesLabel ?? "Predicate Property Values"}
      baseTypePrefixLabel={props.baseTypePrefixLabel ?? "Base predicate"}
      createButtonLabel={
        props.createButtonLabel ?? "Create Predicate Instance"
      }
      saveButtonLabel={props.saveButtonLabel ?? "Save Predicate Instance"}
    />
  );
}

/** action instance modal configured with action-centric copy. */
export function ActionInstanceModal(props: ActionInstanceModalProps) {
  return (
    <BaseInstanceModal<ActionInstance>
      {...props}
      nameLabel={props.nameLabel ?? "Action Instance Name"}
      namePlaceholder={props.namePlaceholder ?? "e.g., pick_up_wrench"}
      typeLabel={props.typeLabel ?? "Action Type"}
      typePlaceholder={props.typePlaceholder ?? "Select an action type..."}
      propertyEmptyMessage={
        props.propertyEmptyMessage ??
        "This action type does not define any properties."
      }
      propertyValuesLabel={props.propertyValuesLabel ?? "Action Property Values"}
      baseTypePrefixLabel={props.baseTypePrefixLabel ?? "Base action"}
      createButtonLabel={props.createButtonLabel ?? "Create Action Instance"}
      saveButtonLabel={props.saveButtonLabel ?? "Save Action Instance"}
    />
  );
}
