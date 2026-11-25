
using Backend.Models;

namespace Backend.DataStructures;

// Self-balancing tree for historical sensor readings.
public sealed class TelemetryAvlTree
{
    private sealed class Node
    {
        public long TimestampTicks { get; }
        public List<TelemetryReading> Readings { get; }

        public Node? Left { get; set; }
        public Node? Right { get; set; }

        public int Height { get; set; } = 1;

        public Node(TelemetryReading reading)
        {
            TimestampTicks = reading.RecordedAt
                .ToUniversalTime().Ticks;

            Readings = new List<TelemetryReading>
            {
                reading
            };
        }
    }

    private Node? _root;

    public int ReadingCount { get; private set; }
    public int DistinctTimestampCount { get; private set; }

    public int Height => GetHeight(_root);

    // Insert telemetry using the timestamp as the key.
    public void Insert(TelemetryReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);

        _root = Insert(_root, reading);
        ReadingCount++;
    }

    // Find readings between two timestamps, including boundaries.
    public List<TelemetryReading> SearchRange(
        DateTime startUtc,
        DateTime endUtc,
        int? sensorId = null)
    {
        long startTicks = startUtc.ToUniversalTime().Ticks;
        long endTicks = endUtc.ToUniversalTime().Ticks;

        if (startTicks > endTicks)
        {
            throw new ArgumentException(
                "Start must be earlier than or equal to end.");
        }

        var results = new List<TelemetryReading>();

        CollectRange(
            _root,
            startTicks,
            endTicks,
            sensorId,
            results);

        return results;
    }

    private Node Insert(Node? node, TelemetryReading reading)
    {
        if (node is null)
        {
            DistinctTimestampCount++;
            return new Node(reading);
        }

        long ticks = reading.RecordedAt
            .ToUniversalTime().Ticks;

        if (ticks < node.TimestampTicks)
        {
            node.Left = Insert(node.Left, reading);
        }
        else if (ticks > node.TimestampTicks)
        {
            node.Right = Insert(node.Right, reading);
        }
        else
        {
            // Readings with identical timestamps share a node.
            node.Readings.Add(reading);
            return node;
        }

        UpdateHeight(node);

        int balance = GetBalance(node);

        // Left-heavy tree.
        if (balance > 1)
        {
            // Left-right rotation.
            if (GetBalance(node.Left!) < 0)
            {
                node.Left = RotateLeft(node.Left!);
            }

            // Left-left rotation.
            return RotateRight(node);
        }

        // Right-heavy tree.
        if (balance < -1)
        {
            // Right-left rotation.
            if (GetBalance(node.Right!) > 0)
            {
                node.Right = RotateRight(node.Right!);
            }

            // Right-right rotation.
            return RotateLeft(node);
        }

        return node;
    }

    // Recursively search only branches that can contain results.
    private static void CollectRange(
        Node? node,
        long startTicks,
        long endTicks,
        int? sensorId,
        List<TelemetryReading> results)
    {
        if (node is null)
        {
            return;
        }

        if (startTicks < node.TimestampTicks)
        {
            CollectRange(
                node.Left,
                startTicks,
                endTicks,
                sensorId,
                results);
        }

        if (startTicks <= node.TimestampTicks &&
            node.TimestampTicks <= endTicks)
        {
            foreach (var reading in node.Readings)
            {
                if (!sensorId.HasValue ||
                    reading.SensorId == sensorId.Value)
                {
                    results.Add(reading);
                }
            }
        }

        if (node.TimestampTicks < endTicks)
        {
            CollectRange(
                node.Right,
                startTicks,
                endTicks,
                sensorId,
                results);
        }
    }

    private static int GetHeight(Node? node)
    {
        return node?.Height ?? 0;
    }

    private static int GetBalance(Node node)
    {
        return GetHeight(node.Left) -
               GetHeight(node.Right);
    }

    private static void UpdateHeight(Node node)
    {
        node.Height = 1 + Math.Max(
            GetHeight(node.Left),
            GetHeight(node.Right));
    }

    // Rotate a left-heavy subtree to the right.
    private static Node RotateRight(Node node)
    {
        Node newRoot = node.Left!;
        Node? middle = newRoot.Right;

        newRoot.Right = node;
        node.Left = middle;

        UpdateHeight(node);
        UpdateHeight(newRoot);

        return newRoot;
    }

    // Rotate a right-heavy subtree to the left.
    private static Node RotateLeft(Node node)
    {
        Node newRoot = node.Right!;
        Node? middle = newRoot.Left;

        newRoot.Left = node;
        node.Right = middle;

        UpdateHeight(node);
        UpdateHeight(newRoot);

        return newRoot;
    }
}
