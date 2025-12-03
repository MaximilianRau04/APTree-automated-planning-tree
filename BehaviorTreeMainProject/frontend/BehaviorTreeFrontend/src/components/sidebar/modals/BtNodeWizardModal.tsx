import { useState } from "react";
import type {
  BehaviorNodeOption,
  DecoratorNodeOption,
  FlowNodeOption,
  ServiceNodeOption,
} from "../utils/types";

type WizardStage = "root" | "action" | "flow" | "decorator" | "service";

interface BtNodeWizardModalProps {
  isOpen: boolean;
  flowOptions: FlowNodeOption[];
  decoratorOptions: DecoratorNodeOption[];
  serviceOptions: ServiceNodeOption[];
  onClose: () => void;
  onSelectActionType: () => void;
  onSelectActionInstance: () => void;
  onSelectBehaviorOption: (option: BehaviorNodeOption) => void;
}

export default function BtNodeWizardModal({
  isOpen,
  flowOptions,
  decoratorOptions,
  serviceOptions,
  onClose,
  onSelectActionType,
  onSelectActionInstance,
  onSelectBehaviorOption,
}: BtNodeWizardModalProps) {
  const [stage, setStage] = useState<WizardStage>("root");

  if (!isOpen) {
    return null;
  }

  const handleClose = () => {
    setStage("root");
    onClose();
  };

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true">
      <div className="modal-content wizard-modal" onClick={(event) => event.stopPropagation()}>
        <div className="modal-header">
          <h3>Add Behavior Tree Node</h3>
          <button className="modal-close-btn" onClick={handleClose} type="button" aria-label="Close wizard">
            &times;
          </button>
        </div>

        <div className="wizard-body">
          {stage === "root" ? (
            <div className="wizard-step-grid">
              <button
                type="button"
                className="wizard-card"
                onClick={() => setStage("action")}
              >
                <span className="wizard-card-title">Action Node</span>
                <span className="wizard-card-copy">
                  Define a new action type or create an action instance bound to an existing type.
                </span>
              </button>
              <button
                type="button"
                className="wizard-card"
                onClick={() => setStage("flow")}
              >
                <span className="wizard-card-title">Flow Node</span>
                <span className="wizard-card-copy">
                  Add control-flow structures like Sequence or Selector nodes to the canvas.
                </span>
              </button>
              <button
                type="button"
                className="wizard-card"
                onClick={() => setStage("decorator")}
              >
                <span className="wizard-card-title">Decorator Node</span>
                <span className="wizard-card-copy">
                  Wrap a single child node to extend or modify its runtime behavior.
                </span>
              </button>
              <button
                type="button"
                className="wizard-card"
                onClick={() => setStage("service")}
              >
                <span className="wizard-card-title">Service Node</span>
                <span className="wizard-card-copy">
                  Attach background logic such as sensors or blackboard updates to a branch.
                </span>
              </button>
            </div>
          ) : null}

          {stage === "action" ? (
            <div className="wizard-step-grid">
              <button
                type="button"
                className="wizard-card"
                onClick={() => {
                  onSelectActionType();
                  handleClose();
                }}
              >
                <span className="wizard-card-title">Create Action Type</span>
                <span className="wizard-card-copy">
                  Describe a reusable action template with its parameters.
                </span>
              </button>
              <button
                type="button"
                className="wizard-card"
                onClick={() => {
                  onSelectActionInstance();
                  handleClose();
                }}
              >
                <span className="wizard-card-title">Create Action Instance</span>
                <span className="wizard-card-copy">
                  Instantiate a concrete action based on an existing action type.
                </span>
              </button>
            </div>
          ) : null}

          {stage === "flow" ? (
            <div className="wizard-step-grid">
              {flowOptions.map((option) => (
                <button
                  key={option.id}
                  type="button"
                  className="wizard-card"
                  onClick={() => {
                    onSelectBehaviorOption(option);
                    handleClose();
                  }}
                >
                  <span className="wizard-card-title">{option.label}</span>
                  <span className="wizard-card-copy">{option.description ?? "Add this flow node to the canvas."}</span>
                </button>
              ))}
              {flowOptions.length === 0 ? (
                <p className="wizard-empty">No flow nodes defined.</p>
              ) : null}
            </div>
          ) : null}

          {stage === "decorator" ? (
            <div className="wizard-step-grid">
              {decoratorOptions.map((option) => (
                <button
                  key={option.id}
                  type="button"
                  className="wizard-card"
                  onClick={() => {
                    onSelectBehaviorOption(option);
                    handleClose();
                  }}
                >
                  <span className="wizard-card-title">{option.label}</span>
                  <span className="wizard-card-copy">{option.description ?? "Add this decorator node to the canvas."}</span>
                </button>
              ))}
              {decoratorOptions.length === 0 ? (
                <p className="wizard-empty">No decorator nodes defined.</p>
              ) : null}
            </div>
          ) : null}

          {stage === "service" ? (
            <div className="wizard-step-grid">
              {serviceOptions.map((option) => (
                <button
                  key={option.id}
                  type="button"
                  className="wizard-card"
                  onClick={() => {
                    onSelectBehaviorOption(option);
                    handleClose();
                  }}
                >
                  <span className="wizard-card-title">{option.label}</span>
                  <span className="wizard-card-copy">{option.description ?? "Add this service node to the canvas."}</span>
                </button>
              ))}
              {serviceOptions.length === 0 ? (
                <p className="wizard-empty">No service nodes defined.</p>
              ) : null}
            </div>
          ) : null}
        </div>

        <div className="modal-footer wizard-footer">
          {stage !== "root" ? (
            <button
              type="button"
              className="btn-secondary"
              onClick={() => setStage("root")}
            >
              Back
            </button>
          ) : null}
          <button type="button" className="btn-cancel" onClick={handleClose}>
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}
