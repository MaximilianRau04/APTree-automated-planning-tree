using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a node in the behavior tree graph with order and temporal constraints
/// </summary>
public class GraphNode
{
    public GenericBTAction ActionNode { get; set; }
    public List<GraphNode> OrderSuccessors { get; set; } = new();
    public List<GraphNode> OrderPredecessors { get; set; } = new();
    public Dictionary<GraphNode, TemporalConstraint> TemporalConstraints { get; set; } = new();
    public float StartTime { get; set; } = 0f;
    public float EndTime { get; set; } = 0f;
    public bool IsExecuting { get; set; } = false;
    public bool IsCompleted { get; set; } = false;

    public GraphNode(GenericBTAction actionNode)
    {
        ActionNode = actionNode;
    }
}

/// <summary>
/// Manages a graph of behavior tree action nodes with order relations and temporal constraints
/// </summary>
public class NodeGraph
{
    private List<GraphNode> nodes = new();
    private Dictionary<BTActionNodeBase, GraphNode> nodeMap = new();
    private float elapsedTime = 0f;

    /// <summary>
    /// Add an action node to the graph
    /// </summary>
    public void AddNode(GenericBTAction actionNode)
    {
        if (!nodeMap.ContainsKey(actionNode))
        {
            var graphNode = new GraphNode(actionNode);
            nodes.Add(graphNode);
            nodeMap[actionNode] = graphNode;
        }
    }

    /// <summary>
    /// Add an order relation between two nodes (like Hasse diagram)
    /// </summary>
    public void AddOrderRelation(BTActionNodeBase from, BTActionNodeBase to)
    {
        if (!nodeMap.ContainsKey(from) || !nodeMap.ContainsKey(to))
        {
            Console.WriteLine($"   ❌ NodeGraph: Cannot add order relation - nodes not found in graph");
            return;
        }

        var fromNode = nodeMap[from];
        var toNode = nodeMap[to];

        fromNode.OrderSuccessors.Add(toNode);
        toNode.OrderPredecessors.Add(fromNode);
        
        Console.WriteLine($"   ✅ NodeGraph: Added order relation: {from.InstanceName.ToString()} → {to.InstanceName.ToString()}");
        Console.WriteLine($"   🔍 NodeGraph: {from.InstanceName.ToString()} now has {fromNode.OrderSuccessors.Count} successors");
        Console.WriteLine($"   🔍 NodeGraph: {to.InstanceName.ToString()} now has {toNode.OrderPredecessors.Count} predecessors");
    }

    /// <summary>
    /// Add a temporal constraint between two nodes (based on Allen's theory)
    /// </summary>
    public void AddTemporalConstraint(BTActionNodeBase from, BTActionNodeBase to, TemporalConstraint constraint)
    {
        if (!nodeMap.ContainsKey(from) || !nodeMap.ContainsKey(to))
        {
            Console.WriteLine($"   ❌ NodeGraph: Cannot add temporal constraint - nodes not found in graph");
            return;
        }

        var fromNode = nodeMap[from];
        var toNode = nodeMap[to];
        fromNode.TemporalConstraints[toNode] = constraint;
        
        Console.WriteLine($"   ✅ NodeGraph: Added temporal constraint: {from.InstanceName.ToString()} {constraint} {to.InstanceName.ToString()}");
        Console.WriteLine($"   🔍 NodeGraph: {from.InstanceName.ToString()} now has {fromNode.TemporalConstraints.Count} temporal constraints");
    }

    /// <summary>
    /// Get all nodes in the graph
    /// </summary>
    public List<GenericBTAction> GetAllActionNodes()
    {
        return nodes.Select(n => n.ActionNode as GenericBTAction).ToList();
    }

    /// <summary>
    /// Get nodes that can be executed at the current time based on order and temporal constraints
    /// </summary>
    public List<BTActionNodeBase> GetExecutableNodes(float deltaTime)
    {
        elapsedTime += deltaTime;
        var executableNodes = new List<BTActionNodeBase>();

        Console.WriteLine($"   🔍 NodeGraph: Total nodes in graph: {nodes.Count}");
        Console.WriteLine($"   🔍 NodeGraph: Elapsed time: {elapsedTime}");

        // Check all nodes to see which ones can execute
        foreach (var node in nodes)
        {
            Console.WriteLine($"   🔍 NodeGraph: Checking node {node.ActionNode.InstanceName.ToString()}");
            Console.WriteLine($"   🔍 NodeGraph: Node completed: {node.IsCompleted}, executing: {node.IsExecuting}");
            Console.WriteLine($"   🔍 NodeGraph: Has predecessors: {node.OrderPredecessors.Any()}");
            
            if (node.OrderPredecessors.Any())
            {
                Console.WriteLine($"   🔍 NodeGraph: All predecessors completed: {AllPredecessorsCompleted(node)}");
            }
            
            // A node can execute if:
            // 1. It's not completed and not already executing
            // 2. Either it has no predecessors (first in sequence) OR all its predecessors are completed
            // 3. Any temporal constraints are satisfied
            if (CanExecuteNode(node) && (node.OrderPredecessors.Count == 0 || AllPredecessorsCompleted(node)))
            {
                Console.WriteLine($"   ✅ NodeGraph: Node {node.ActionNode.InstanceName.ToString()} can be executed");
                executableNodes.Add(node.ActionNode);
                node.IsExecuting = true;
                if (node.StartTime == 0f)
                    node.StartTime = elapsedTime;
            }
            else
            {
                Console.WriteLine($"   ❌ NodeGraph: Node {node.ActionNode.InstanceName.ToString()} cannot be executed");
            }
        }

        Console.WriteLine($"   🔍 NodeGraph: Returning {executableNodes.Count} executable nodes");
        return executableNodes;
    }

    /// <summary>
    /// Check if a node can be executed based on temporal constraints
    /// </summary>
    private bool CanExecuteNode(GraphNode node)
    {
        if (node.IsCompleted || node.IsExecuting)
        {
            Console.WriteLine($"   ❌ NodeGraph: Node {node.ActionNode.InstanceName.ToString()} cannot execute - completed: {node.IsCompleted}, executing: {node.IsExecuting}");
            return false;
        }

        // For nodes with no order predecessors (first in sequence), don't check temporal constraints
        // They should be able to start execution immediately
        if (!node.OrderPredecessors.Any())
        {
            Console.WriteLine($"   ✅ NodeGraph: First node {node.ActionNode.InstanceName.ToString()} can execute (no predecessors)");
            return true;
        }

        // For nodes with predecessors, check temporal constraints from other nodes
        // We need to check if any other node has a temporal constraint on this node
        foreach (var otherNode in nodes)
        {
            if (otherNode == node) continue;
            
            if (otherNode.TemporalConstraints.TryGetValue(node, out var temporalConstraint))
            {
                Console.WriteLine($"   🔍 NodeGraph: Checking temporal constraint {temporalConstraint} from {otherNode.ActionNode.InstanceName.ToString()} to {node.ActionNode.InstanceName.ToString()}");

                if (!IsTemporalConstraintSatisfied(otherNode, node, temporalConstraint))
                {
                    Console.WriteLine($"   ❌ NodeGraph: Temporal constraint {temporalConstraint} not satisfied");
                    return false;
                }
            }
        }

        Console.WriteLine($"   ✅ NodeGraph: Node {node.ActionNode.InstanceName.ToString()} can execute");
        return true;
    }

    /// <summary>
    /// Check if all order predecessors have completed
    /// </summary>
    private bool AllPredecessorsCompleted(GraphNode node)
    {
        return node.OrderPredecessors.All(p => p.IsCompleted);
    }

    /// <summary>
    /// Check if a temporal constraint is satisfied based on Allen's theory
    /// </summary>
    private bool IsTemporalConstraintSatisfied(GraphNode from, GraphNode to, TemporalConstraint constraint)
    {
        bool result = false;
        
        switch (constraint)
        {
            case TemporalConstraint.PRECEDES:
                result = from.IsCompleted && !to.IsExecuting;
                Console.WriteLine($"   🔍 NodeGraph: PRECEDES constraint - from completed: {from.IsCompleted}, to executing: {to.IsExecuting}, result: {result}");
                break;
            
            case TemporalConstraint.MEETS:
                // MEETS: the next action starts immediately after the previous one ends
                // We check that the previous action is completed and the next action hasn't started yet
                // (or has just started at the same time)
                result = from.IsCompleted && (to.StartTime == 0f || Math.Abs(from.EndTime - to.StartTime) < 0.001f);
                Console.WriteLine($"   🔍 NodeGraph: MEETS constraint - from completed: {from.IsCompleted}, from end time: {from.EndTime}, to start time: {to.StartTime}, result: {result}");
                break;
            
            case TemporalConstraint.OVERLAPS:
                result = from.IsExecuting && to.IsExecuting && 
                       from.StartTime < to.EndTime && to.StartTime < from.EndTime;
                Console.WriteLine($"   🔍 NodeGraph: OVERLAPS constraint - from executing: {from.IsExecuting}, to executing: {to.IsExecuting}, result: {result}");
                break;
            
            case TemporalConstraint.STARTS:
                result = from.StartTime == to.StartTime;
                Console.WriteLine($"   🔍 NodeGraph: STARTS constraint - from start: {from.StartTime}, to start: {to.StartTime}, result: {result}");
                break;
            
            case TemporalConstraint.FINISHES:
                result = from.EndTime == to.EndTime;
                Console.WriteLine($"   🔍 NodeGraph: FINISHES constraint - from end: {from.EndTime}, to end: {to.EndTime}, result: {result}");
                break;
            
            case TemporalConstraint.CONTAINS:
                result = from.StartTime <= to.StartTime && from.EndTime >= to.EndTime;
                Console.WriteLine($"   🔍 NodeGraph: CONTAINS constraint - result: {result}");
                break;
            
            case TemporalConstraint.EQUALS:
                result = from.StartTime == to.StartTime && from.EndTime == to.EndTime;
                Console.WriteLine($"   🔍 NodeGraph: EQUALS constraint - result: {result}");
                break;
            
            default:
                result = true;
                Console.WriteLine($"   🔍 NodeGraph: Default constraint - result: {result}");
                break;
        }
        
        return result;
    }

    /// <summary>
    /// Mark a node as completed
    /// </summary>
    public void MarkNodeCompleted(BTActionNodeBase actionNode)
    {
        if (nodeMap.TryGetValue(actionNode, out var graphNode))
        {
            graphNode.IsCompleted = true;
            graphNode.IsExecuting = false;
            graphNode.EndTime = elapsedTime;
        }
    }

    /// <summary>
    /// Reset the graph state
    /// </summary>
    public void Reset()
    {
        elapsedTime = 0f;
        foreach (var node in nodes)
        {
            node.IsExecuting = false;
            node.IsCompleted = false;
            node.StartTime = 0f;
            node.EndTime = 0f;
        }
    }

    /// <summary>
    /// Get the execution order as a list (left-to-right like Hasse diagram)
    /// </summary>
    public List<GenericBTAction> GetExecutionOrder()
    {
        var result = new List<GenericBTAction>();
        var visited = new HashSet<GraphNode>();
        var tempVisited = new HashSet<GraphNode>();

        foreach (var node in nodes)
        {
            if (!visited.Contains(node))
            {
                TopologicalSort(node, visited, tempVisited, result);
            }
        }

        return result;
    }

    /// <summary>
    /// Topological sort to determine execution order
    /// </summary>
    private void TopologicalSort(GraphNode node, HashSet<GraphNode> visited, HashSet<GraphNode> tempVisited, List<GenericBTAction> result)
    {
        if (tempVisited.Contains(node))
            return; // Cycle detected

        if (visited.Contains(node))
            return;

        tempVisited.Add(node);

        // Process all successors first (depth-first)
        foreach (var successor in node.OrderSuccessors)
        {
            TopologicalSort(successor, visited, tempVisited, result);
        }

        tempVisited.Remove(node);
        visited.Add(node);
        
        // Add this node to the beginning of the result (reverse topological order)
        // This ensures that nodes with no successors come first
        result.Insert(0, node.ActionNode);
    }
}
