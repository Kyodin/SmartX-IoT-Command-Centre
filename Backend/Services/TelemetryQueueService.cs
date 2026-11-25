
using Backend.Models;

namespace Backend.Services;

public class TelemetryQueueService
{
    private readonly Queue<TelemetryReading> _telemetryQueue = new();

    private readonly object _queueLock = new();

    // Add a new reading to the end of the queue.
    public void EnqueueReading(TelemetryReading reading)
    {
        lock (_queueLock)
        {
            _telemetryQueue.Enqueue(reading);
        }
    }

    // Add multiple readings while preserving their order.
    public void EnqueueBatch(IEnumerable<TelemetryReading> readings)
    {
        lock (_queueLock)
        {
            foreach (var reading in readings)
            {
                _telemetryQueue.Enqueue(reading);
            }
        }
    }

    // Process the oldest reading first.
    public TelemetryReading? ProcessNextReading()
    {
        lock (_queueLock)
        {
            if (_telemetryQueue.Count == 0)
            {
                return null;
            }

            return _telemetryQueue.Dequeue();
        }
    }

    // View queued readings without removing them.
    public List<TelemetryReading> GetPendingReadings()
    {
        lock (_queueLock)
        {
            return _telemetryQueue.ToList();
        }
    }

    // Check how many readings are waiting.
    public int GetQueueCount()
    {
        lock (_queueLock)
        {
            return _telemetryQueue.Count;
        }
    }
}
