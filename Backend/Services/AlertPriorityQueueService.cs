
using Backend.Models;

namespace Backend.Services;

public class AlertPriorityQueueService
{
    private readonly PriorityQueue<DeviceAlert, int> _alertQueue = new();

    private readonly object _queueLock = new();

    private int _nextId = 1;

    public DeviceAlert EnqueueAlert(DeviceAlert alert)
    {
        lock (_queueLock)
        {
            alert.Id = _nextId++;
            alert.CreatedAt = DateTime.UtcNow;

            _alertQueue.Enqueue(alert, alert.Priority);

            return alert;
        }
    }

    public DeviceAlert? ProcessNextAlert()
    {
        lock (_queueLock)
        {
            if (_alertQueue.Count == 0)
            {
                return null;
            }

            return _alertQueue.Dequeue();
        }
    }

    public List<DeviceAlert> GetPendingAlerts()
    {
        lock (_queueLock)
        {
            return _alertQueue.UnorderedItems
                .OrderBy(item => item.Priority)
                .Select(item => item.Element)
                .ToList();
        }
    }

    public int GetPendingCount()
    {
        lock (_queueLock)
        {
            return _alertQueue.Count;
        }
    }
}
