
using Backend.DataStructures;
using Backend.Models;

namespace Backend.Services;

// Shared, thread-safe AVL tree for telemetry history.
public sealed class TelemetryHistoryService
{
    private readonly object _sync = new();
    private readonly TelemetryAvlTree _tree = new();

    // Add a stored reading to the historical index.
    public void Index(TelemetryReading reading)
    {
        lock (_sync)
        {
            _tree.Insert(reading);
        }
    }

    // Search historical readings by date range and sensor.
    public List<TelemetryReading> SearchRange(
        DateTime startUtc,
        DateTime endUtc,
        int? sensorId = null)
    {
        lock (_sync)
        {
            return _tree.SearchRange(
                startUtc,
                endUtc,
                sensorId);
        }
    }

    // Retrieve tree performance and structure information.
    public (
        int readingCount,
        int timestampCount,
        int treeHeight
    ) GetStatistics()
    {
        lock (_sync)
        {
            return (
                _tree.ReadingCount,
                _tree.DistinctTimestampCount,
                _tree.Height
            );
        }
    }
}
