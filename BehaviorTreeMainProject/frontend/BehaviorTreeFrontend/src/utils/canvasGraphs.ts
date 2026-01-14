import type { CanvasNode, NodeConnection } from "../components/editor/types";

export type CanvasLevel = "high" | "mid" | "low";

export const CANVAS_LEVELS: Array<{ key: CanvasLevel; label: string }> = [
  { key: "high", label: "High" },
  { key: "mid", label: "Mid" },
  { key: "low", label: "Low" },
];

export const LEVEL_TO_HIERARCHY: Record<CanvasLevel, number> = {
  high: 1,
  mid: 2,
  low: 3,
};

export const HIERARCHY_TO_LEVEL: Record<number, CanvasLevel> = {
  1: "high",
  2: "mid",
  3: "low",
};

export type CanvasGraph = {
  nodes: CanvasNode[];
  connections: NodeConnection[];
  /** Optional root node for this graph (must be a DynamicFlowNode per DynamicBTFlowNode.mc4). */
  rootNodeId?: string | null;
};

export type ExportedCanvasGraphsV1 = {
  version: 1;
  exportedAt: string;
  activeLevel: CanvasLevel;
  graphs: Record<CanvasLevel, CanvasGraph>;
};

export function createEmptyGraphs(): Record<CanvasLevel, CanvasGraph> {
  return {
    high: { nodes: [], connections: [], rootNodeId: null },
    mid: { nodes: [], connections: [], rootNodeId: null },
    low: { nodes: [], connections: [], rootNodeId: null },
  };
}

export function mergeNodesWithHierarchy(graphs: Record<CanvasLevel, CanvasGraph>) {
  const allNodes: CanvasNode[] = [];
  CANVAS_LEVELS.forEach(({ key }) => {
    const levelNodes = graphs[key].nodes.map((node) => ({
      ...node,
      hierarchyLevel: LEVEL_TO_HIERARCHY[key],
    }));
    allNodes.push(...levelNodes);
  });
  return allNodes;
}

export function mergeConnections(graphs: Record<CanvasLevel, CanvasGraph>) {
  const allConnections: NodeConnection[] = [];
  CANVAS_LEVELS.forEach(({ key }) => {
    allConnections.push(...graphs[key].connections);
  });
  return allConnections;
}

type ParseResult = {
  graphs: Record<CanvasLevel, CanvasGraph>;
  activeLevel: CanvasLevel | null;
  migrationNotices: string[];
};

export function parseCanvasGraphImport(text: string): ParseResult {
  const parsed: unknown = JSON.parse(text);

  const isCanvasLevel = (value: unknown): value is CanvasLevel =>
    value === "high" || value === "mid" || value === "low";

  const isCanvasGraph = (value: unknown): value is CanvasGraph => {
    if (!value || typeof value !== "object") {
      return false;
    }

    const graph = value as CanvasGraph;
    const hasBasics = Array.isArray(graph.nodes) && Array.isArray(graph.connections);
    if (!hasBasics) {
      return false;
    }
    if (
      graph.rootNodeId !== undefined &&
      graph.rootNodeId !== null &&
      typeof graph.rootNodeId !== "string"
    ) {
      return false;
    }
    return true;
  };

  const isV1 = (value: unknown): value is ExportedCanvasGraphsV1 => {
    if (!value || typeof value !== "object") {
      return false;
    }

    const obj = value as Partial<ExportedCanvasGraphsV1>;
    if (obj.version !== 1) {
      return false;
    }
    if (!isCanvasLevel(obj.activeLevel)) {
      return false;
    }
    if (!obj.graphs || typeof obj.graphs !== "object") {
      return false;
    }

    const graphsObj = obj.graphs as Record<string, unknown>;
    return (
      isCanvasGraph(graphsObj.high) &&
      isCanvasGraph(graphsObj.mid) &&
      isCanvasGraph(graphsObj.low)
    );
  };

  let nextGraphs: Record<CanvasLevel, CanvasGraph> | null = null;
  let nextLevel: CanvasLevel | null = null;

  if (isV1(parsed)) {
    nextGraphs = parsed.graphs;
    nextLevel = parsed.activeLevel;
  } else if (parsed && typeof parsed === "object") {
    const obj = parsed as Record<string, unknown>;
    const candidateGraphs =
      obj.graphs && typeof obj.graphs === "object"
        ? (obj.graphs as Record<string, unknown>)
        : obj;

    if (
      isCanvasGraph(candidateGraphs.high) &&
      isCanvasGraph(candidateGraphs.mid) &&
      isCanvasGraph(candidateGraphs.low)
    ) {
      nextGraphs = {
        high: candidateGraphs.high as CanvasGraph,
        mid: candidateGraphs.mid as CanvasGraph,
        low: candidateGraphs.low as CanvasGraph,
      };
    }

    if (isCanvasLevel(obj.activeLevel)) {
      nextLevel = obj.activeLevel;
    }
  }

  if (!nextGraphs) {
    throw new Error(
      "JSON did not match the expected canvas graph format."
    );
  }

  const migrationNotices: string[] = [];
  const migratedGraphs = (Object.keys(nextGraphs) as CanvasLevel[]).reduce(
    (acc, level) => {
      const graph = nextGraphs[level];
      const nodes = graph.nodes.map((node) => {
        if (node.kind !== "behaviorNode") {
          return node;
        }

        if (node.sourceId === "selector") {
          migrationNotices.push('Migrated legacy flow node "Selector" -> "Fallback".');
          return {
            ...node,
            sourceId: "fallback",
            name: node.name === "Selector" ? "Fallback" : node.name,
          };
        }

        if (node.sourceId === "parallel") {
          migrationNotices.push('Replaced unsupported flow node "Parallel" with "Sequence".');
          return {
            ...node,
            sourceId: "sequence",
            name: node.name === "Parallel" ? "Sequence" : node.name,
          };
        }

        if (node.sourceId === "dynamic-flow-node") {
          const childType = node.childType === "ALLACTION" || node.childType === "ALLFLOW"
            ? node.childType
            : "ALLACTION";
          const nodeGraphName = typeof node.nodeGraphName === "string" && node.nodeGraphName.trim()
            ? node.nodeGraphName
            : `${(node.name ?? "Dynamic").replace(/\s+/g, "")}Graph`;

          const allowedTemporalTypes = new Set(["MEETS", "BEFORE", "AFTER", "OVERLAPS", "DURING"]);
          const temporalRelations = Array.isArray(node.temporalRelations)
            ? node.temporalRelations.filter((entry) => {
                if (!entry || typeof entry !== "object") {
                  return false;
                }

                const rel = entry as {
                  fromNodeId?: unknown;
                  toNodeId?: unknown;
                  temporalType?: unknown;
                };

                return (
                  typeof rel.fromNodeId === "string" &&
                  typeof rel.toNodeId === "string" &&
                  typeof rel.temporalType === "string" &&
                  allowedTemporalTypes.has(rel.temporalType)
                );
              })
            : [];

          return {
            ...node,
            childType,
            nodeGraphName,
            temporalRelations,
          };
        }

        return node;
      });

      const rootNodeId = graph.rootNodeId ?? null;
      return {
        ...acc,
        [level]: {
          ...graph,
          rootNodeId,
          nodes,
        },
      };
    },
    {} as Record<CanvasLevel, CanvasGraph>
  );

  // Validate root node references (must exist and be a DynamicFlowNode).
  (Object.keys(migratedGraphs) as CanvasLevel[]).forEach((level) => {
    const graph = migratedGraphs[level];
    if (!graph.rootNodeId) {
      return;
    }

    const root = graph.nodes.find((n) => n.id === graph.rootNodeId);
    if (!root) {
      migrationNotices.push(
        `Root node for ${level} was missing; clearing root selection.`
      );
      graph.rootNodeId = null;
      return;
    }

    if (root.category !== "flowNodes" || root.sourceId !== "dynamic-flow-node") {
      migrationNotices.push(
        `Root node for ${level} must be a Dynamic Flow Node (DynamicBTFlowNode.mc4); clearing root selection.`
      );
      graph.rootNodeId = null;
    }
  });

  return { graphs: migratedGraphs, activeLevel: nextLevel, migrationNotices };
}
