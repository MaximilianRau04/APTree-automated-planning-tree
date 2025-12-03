import type { CanvasNode } from "./types";
import type { BehaviorNodeOption } from "../sidebar/utils/types";
import { createId } from "../../utils/id";
import {
  DECORATOR_NODES_KEY,
  FLOW_NODES_KEY,
  SERVICE_NODES_KEY,
} from "../sidebar/utils/constants";

const categoryByKind = {
  flow: FLOW_NODES_KEY,
  decorator: DECORATOR_NODES_KEY,
  service: SERVICE_NODES_KEY,
} as const;

interface BehaviorNodeFactoryParams {
  option: BehaviorNodeOption;
  position?: { x: number; y: number };
}

/**
 * creates a default canvas node representation for a chosen behavior node option.
 */
export function createBehaviorNode({
  option,
  position = { x: 120, y: 120 },
}: BehaviorNodeFactoryParams): CanvasNode {
  const category = categoryByKind[option.kind];

  return {
    id: createId("bt-node"),
    sourceId: option.id,
    name: option.label,
    typeLabel: option.typeLabel,
    category,
    kind: "behaviorNode",
    x: position.x,
    y: position.y,
  };
}
