using System.Collections.Generic;
using System.Linq;


public class ActionGraph
{
    /// <summary>
    /// A dictionary of nodes and the nodes they can execute after.
    /// </summary>
    private Dictionary<IBTNode, HashSet<(IBTNode Node, IOrderStrategy Strategy, float Delay)>> adjacencyList = new();
    /// <summary>
    /// A dictionary of nodes and the time they started executing.
    /// </summary>
    private Dictionary<IBTNode, float> startTimes = new();
    // Keeps track of which nodes are currently executing
    private HashSet<IBTNode> activeNodes = new();
    private float elapsedTime = 0;
   

    public ActionGraph()
    {
       
    }
/// <summary>
/// Add an order constraint to the graph.
/// </summary>
/// <param name="from">The node that the constraint is coming from.</param>
/// <param name="to">The node that the constraint is going to.</param>
/// <param name="strategy">The strategy for the order constraint.</param>
/// <param name="delay">The delay before the next node can start.</param>
    public void AddOrderConstraint(IBTNode from, IBTNode to, IOrderStrategy strategy, float delay = 0)
    {
        if (!adjacencyList.ContainsKey(from))
            adjacencyList[from] = new();
        
        adjacencyList[from].Add((to, strategy, delay));
        startTimes[from] = 0;
        startTimes[to] = 0;
    }
/// <summary>
/// Get the nodes that can be executed at the current time.
/// </summary>
/// <param name="deltaTime"></param>
/// <returns></returns>
    public List<IBTNode> GetExecutableNodes(float deltaTime)
    {
        elapsedTime += deltaTime;
        var executableNodes = new List<IBTNode>();

        // Check if we have any nodes at all
        if (!adjacencyList.Any())
        {
            return executableNodes;  // Return empty list if no nodes
        }

        // First node is always executable if not started
        if (!activeNodes.Any())
        {
            var firstNode = adjacencyList.Keys.FirstOrDefault();
            if (firstNode != null)
            {
                executableNodes.Add(firstNode);
                activeNodes.Add(firstNode);
                startTimes[firstNode] = elapsedTime;
            }
            return executableNodes;
        }

        foreach (var currentNode in activeNodes.ToList())
        {
            // If the current node is in progress, add it to the executable nodes
            if (currentNode.LastStatus == EBTNodeResult.InProgress)
            {
                executableNodes.Add(currentNode);
            }

            if (adjacencyList.TryGetValue(currentNode, out var neighbors))
            {
                foreach (var (nextNode, strategy, delay) in neighbors)
                {
                    bool canExecute = strategy.CanExecute(
                        currentNode.LastStatus,
                        nextNode.LastStatus,
                        elapsedTime - startTimes[currentNode],
                        delay);

                    if (canExecute && nextNode.LastStatus == EBTNodeResult.readyToTick)
                    {
                        executableNodes.Add(nextNode);
                        activeNodes.Add(nextNode);
                        startTimes[nextNode] = elapsedTime;
                    }
                }
            }
        }

        return executableNodes;
    }

    public void Reset()
    {
        activeNodes.Clear();
        elapsedTime = 0;
        foreach (var node in startTimes.Keys.ToList())
        {
            startTimes[node] = 0;
        }
    }
}