
using Backend.Models;

namespace Backend.Services;

public record MeshLink(
    int SourceId,
    int TargetId,
    double Cost,
    bool IsActive
);

public record MeshTopology(
    IReadOnlyList<Sensor> Nodes,
    IReadOnlyList<MeshLink> Links,
    int OnlineSensorCount,
    int ActiveLinkCount
);

public record MeshTraversalResult(
    string Algorithm,
    int StartSensorId,
    IReadOnlyList<int> VisitOrder,
    IReadOnlyList<int> UnreachableOnlineSensorIds
);

public record MeshMstResult(
    IReadOnlyList<MeshLink> Edges,
    double TotalCost,
    int OnlineSensorCount,
    int ConnectedComponents,
    bool SpansAllOnlineSensors
);

// PART 3: Undirected weighted graph for Smart-X sensors.
public sealed class MeshNetworkService
{
    private readonly SensorService _sensors;
    private readonly object _sync = new();

    // Sensor ID -> connected sensor ID -> link cost.
    private readonly Dictionary<int, Dictionary<int, double>>
        _adjacency = new();

    public MeshNetworkService(SensorService sensors)
    {
        _sensors = sensors;
    }

    // Register a bidirectional wireless connection.
    public void AddLink(int sourceId, int targetId, double cost)
    {
        if (sourceId <= 0 || targetId <= 0 ||
            sourceId == targetId)
        {
            throw new ArgumentException(
                "Provide two different positive sensor IDs.");
        }

        if (!double.IsFinite(cost) || cost <= 0)
        {
            throw new ArgumentException(
                "Link cost must be a finite positive number.");
        }

        if (_sensors.GetById(sourceId) is null ||
            _sensors.GetById(targetId) is null)
        {
            throw new KeyNotFoundException(
                "One or both sensors do not exist.");
        }

        lock (_sync)
        {
            if (!_adjacency.ContainsKey(sourceId))
                _adjacency[sourceId] = new();

            if (!_adjacency.ContainsKey(targetId))
                _adjacency[targetId] = new();

            if (_adjacency[sourceId].ContainsKey(targetId))
            {
                throw new InvalidOperationException(
                    "This connection already exists.");
            }

            _adjacency[sourceId][targetId] = cost;
            _adjacency[targetId][sourceId] = cost;
        }
    }

    // Remove a connection in both directions.
    public bool RemoveLink(int sourceId, int targetId)
    {
        lock (_sync)
        {
            if (!_adjacency.TryGetValue(
                sourceId, out var neighbours))
            {
                return false;
            }

            if (!neighbours.Remove(targetId))
                return false;

            if (_adjacency.TryGetValue(
                targetId, out var reverse))
            {
                reverse.Remove(sourceId);
            }

            return true;
        }
    }

    // Return sensor nodes and weighted network links.
    public MeshTopology GetTopology()
    {
        lock (_sync)
        {
            var nodes = _sensors.GetAll().ToArray();

            var online = nodes
                .Where(s => s.IsOnline)
                .Select(s => s.Id)
                .ToHashSet();

            var links = _adjacency
                .SelectMany(pair => pair.Value
                    .Where(edge => pair.Key < edge.Key)
                    .Select(edge => new MeshLink(
                        pair.Key,
                        edge.Key,
                        edge.Value,
                        online.Contains(pair.Key) &&
                        online.Contains(edge.Key))))
                .OrderBy(link => link.SourceId)
                .ThenBy(link => link.TargetId)
                .ToArray();

            return new MeshTopology(
                nodes,
                links,
                online.Count,
                links.Count(link => link.IsActive)
            );
        }
    }

    // Only online sensors can participate in traversal.
    private IEnumerable<int> GetOnlineNeighbours(
        int sensorId,
        HashSet<int> online)
    {
        if (!_adjacency.TryGetValue(
            sensorId, out var neighbours))
        {
            return Enumerable.Empty<int>();
        }

        return neighbours.Keys
            .Where(online.Contains)
            .OrderBy(id => id);
    }

    // BFS uses a Queue; DFS uses a Stack.
    public MeshTraversalResult Traverse(
        int startSensorId,
        string algorithm = "bfs")
    {
        string selected = (algorithm ?? "")
            .Trim()
            .ToLowerInvariant();

        if (selected != "bfs" && selected != "dfs")
        {
            throw new ArgumentException(
                "Algorithm must be bfs or dfs.");
        }

        lock (_sync)
        {
            var online = _sensors.GetAll()
                .Where(sensor => sensor.IsOnline)
                .Select(sensor => sensor.Id)
                .ToHashSet();

            if (!online.Contains(startSensorId))
            {
                throw new ArgumentException(
                    "Starting sensor must exist and be online.");
            }

            var visited = new HashSet<int>();
            var order = new List<int>();

            if (selected == "bfs")
            {
                var queue = new Queue<int>();

                visited.Add(startSensorId);
                queue.Enqueue(startSensorId);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    order.Add(current);

                    foreach (int next in
                        GetOnlineNeighbours(current, online))
                    {
                        if (visited.Add(next))
                            queue.Enqueue(next);
                    }
                }
            }
            else
            {
                var stack = new Stack<int>();
                stack.Push(startSensorId);

                while (stack.Count > 0)
                {
                    int current = stack.Pop();

                    if (!visited.Add(current))
                        continue;

                    order.Add(current);

                    // Reverse push order for consistent DFS.
                    foreach (int next in
                        GetOnlineNeighbours(current, online)
                        .OrderByDescending(id => id))
                    {
                        if (!visited.Contains(next))
                            stack.Push(next);
                    }
                }
            }

            return new MeshTraversalResult(
                selected.ToUpperInvariant(),
                startSensorId,
                order,
                online.Except(visited)
                    .OrderBy(id => id)
                    .ToArray()
            );
        }
    }

    // Prim's algorithm: calculate a minimum spanning forest.
    // Disconnected networks can have multiple components.
    public MeshMstResult CalculateMst()
    {
        lock (_sync)
        {
            var online = _sensors.GetAll()
                .Where(sensor => sensor.IsOnline)
                .Select(sensor => sensor.Id)
                .ToHashSet();

            var visited = new HashSet<int>();
            var selectedEdges = new List<MeshLink>();

            double totalCost = 0;
            int components = 0;

            foreach (int start in online.OrderBy(id => id))
            {
                if (!visited.Add(start))
                    continue;

                components++;

                var queue = new PriorityQueue<
                    (int From, int To, double Cost),
                    (double Cost, int From, int To)>();

                // Add eligible outgoing links to the queue.
                void EnqueueConnections(int from)
                {
                    if (!_adjacency.TryGetValue(
                        from, out var neighbours))
                    {
                        return;
                    }

                    foreach (var edge in neighbours)
                    {
                        if (!online.Contains(edge.Key) ||
                            visited.Contains(edge.Key))
                        {
                            continue;
                        }

                        queue.Enqueue(
                            (from, edge.Key, edge.Value),
                            (edge.Value, from, edge.Key)
                        );
                    }
                }

                EnqueueConnections(start);

                while (queue.TryDequeue(
                    out var candidate, out _))
                {
                    if (!visited.Add(candidate.To))
                        continue;

                    selectedEdges.Add(new MeshLink(
                        candidate.From,
                        candidate.To,
                        candidate.Cost,
                        true
                    ));

                    totalCost += candidate.Cost;

                    EnqueueConnections(candidate.To);
                }
            }

            return new MeshMstResult(
                selectedEdges,
                totalCost,
                online.Count,
                components,
                components == 1
            );
        }
    }
}
