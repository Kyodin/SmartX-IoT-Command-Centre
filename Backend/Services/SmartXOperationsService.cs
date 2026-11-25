
using Backend.Controllers;
using Backend.Models;

namespace Backend.Services;

public record SmartXDevice(
    int Id,
    string Name,
    string Status,
    bool IsOn,
    DateTime UpdatedAt
);

public record SmartXCommand(
    int DeviceId,
    string Action,
    string PreviousStatus,
    bool PreviousIsOn,
    DateTime Timestamp
);

public record SmartXEvent(
    int SensorId,
    string Type,
    string Message,
    int Priority,
    DateTime Timestamp
);

public class SmartXOperationsService
{
    private readonly object _sync = new();
    private readonly SensorService _sensorService;

    // Dictionary: device state by sensor ID.
    private readonly Dictionary<int, SmartXDevice> _devices = new();

    // Stack: last command is the first one undone.
    private readonly Stack<SmartXCommand> _undo = new();

    // SortedDictionary: chronological operational history.
    private readonly SortedDictionary<DateTime, SmartXEvent> _history = new();

    // HashSets: unique active alerts and disconnected devices.
    private readonly HashSet<string> _activeAlerts =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly HashSet<int> _disconnectedDevices = new();

    // PriorityQueue: lower number means higher urgency.
    private readonly PriorityQueue<
        SmartXEvent,
        (int Priority, long Sequence)
    > _priorityAlerts = new();

    private long _sequence;

    public SmartXOperationsService(SensorService sensorService)
    {
        _sensorService = sensorService;
    }

    private void SynchronizeDevices()
    {
        foreach (var sensor in _sensorService.GetAll())
        {
            if (_devices.ContainsKey(sensor.Id))
                continue;

            _devices.Add(
                sensor.Id,
                new SmartXDevice(
                    sensor.Id,
                    sensor.Name,
                    sensor.IsOnline ? "Online" : "Offline",
                    sensor.IsOnline,
                    DateTime.UtcNow
                )
            );
        }
    }

    public object GetDevices()
    {
        lock (_sync)
        {
            SynchronizeDevices();

            return new
            {
                total = _devices.Count,
                devices = _devices.Values
                    .OrderBy(device => device.Id)
                    .ToArray(),
                disconnectedDeviceIds = _disconnectedDevices
                    .OrderBy(id => id)
                    .ToArray()
            };
        }
    }

    public SmartXDevice? FindDevice(int id)
    {
        lock (_sync)
        {
            SynchronizeDevices();
            return _devices.GetValueOrDefault(id);
        }
    }

    public (bool Success, string Message, SmartXDevice? Device)
        ApplyCommand(int deviceId, string action)
    {
        lock (_sync)
        {
            SynchronizeDevices();

            if (!_devices.TryGetValue(deviceId, out var device))
                return (false, "Device not found.", null);

            var normalized = (action ?? "")
                .Trim()
                .ToLowerInvariant();

            if (normalized is not (
                "on" or
                "off" or
                "restart" or
                "disconnect" or
                "connect"))
            {
                return (
                    false,
                    "Action must be on, off, restart, disconnect, or connect.",
                    null
                );
            }

            var now = DateTime.UtcNow;

            var updated = normalized switch
            {
                "off" => device with
                {
                    IsOn = false,
                    Status = "Offline",
                    UpdatedAt = now
                },

                "disconnect" => device with
                {
                    IsOn = false,
                    Status = "Disconnected",
                    UpdatedAt = now
                },

                _ => device with
                {
                    IsOn = true,
                    Status = "Online",
                    UpdatedAt = now
                }
            };

            _undo.Push(new SmartXCommand(
                deviceId,
                normalized,
                device.Status,
                device.IsOn,
                now
            ));

            _devices[deviceId] = updated;

            UpdateConnectivity(updated);

            AddHistory(new SmartXEvent(
                deviceId,
                "Command",
                $"Action: {normalized}",
                3,
                now
            ));

            return (true, "Command applied.", updated);
        }
    }

    public (bool Success, string Message, SmartXDevice? Device)
        Undo()
    {
        lock (_sync)
        {
            if (_undo.Count == 0)
                return (false, "No commands to undo.", null);

            var previous = _undo.Pop();

            if (!_devices.TryGetValue(previous.DeviceId, out var current))
                return (false, "Device no longer exists.", null);

            var restored = current with
            {
                Status = previous.PreviousStatus,
                IsOn = previous.PreviousIsOn,
                UpdatedAt = DateTime.UtcNow
            };

            _devices[restored.Id] = restored;

            UpdateConnectivity(restored);

            AddHistory(new SmartXEvent(
                restored.Id,
                "Undo",
                $"Undid: {previous.Action}",
                3,
                DateTime.UtcNow
            ));

            return (true, "Last command undone.", restored);
        }
    }

    private void UpdateConnectivity(SmartXDevice device)
    {
        if (device.Status == "Disconnected")
            _disconnectedDevices.Add(device.Id);
        else
            _disconnectedDevices.Remove(device.Id);

        _sensorService.SetOnlineStatus(
            device.Id,
            device.Status == "Online"
        );
    }

    public object GetCommandHistory()
    {
        lock (_sync)
        {
            return new
            {
                undoCount = _undo.Count,
                commands = _undo.ToArray()
            };
        }
    }

    public (bool Added, SmartXEvent Alert)
        AddAlert(int sensorId, string type, string message, int priority)
    {
        lock (_sync)
        {
            SynchronizeDevices();

            var cleanType = (type ?? "").Trim();
            var cleanMessage = (message ?? "").Trim();

            var key = $"{sensorId}:{cleanType.ToLowerInvariant()}";

            var alert = new SmartXEvent(
                sensorId,
                cleanType,
                cleanMessage,
                priority,
                DateTime.UtcNow
            );

            if (!_activeAlerts.Add(key))
                return (false, alert);

            _priorityAlerts.Enqueue(
                alert,
                (priority, ++_sequence)
            );

            AddHistory(alert);

            return (true, alert);
        }
    }

    public object GetAlerts()
    {
        lock (_sync)
        {
            return new
            {
                pendingCount = _priorityAlerts.Count,
                uniqueActiveCount = _activeAlerts.Count,
                alerts = _priorityAlerts.UnorderedItems
                    .OrderBy(item => item.Priority)
                    .Select(item => item.Element)
                    .ToArray()
            };
        }
    }

    public SmartXEvent? ProcessAlert()
    {
        lock (_sync)
        {
            if (!_priorityAlerts.TryDequeue(out var alert, out _))
                return null;

            _activeAlerts.Remove(
                $"{alert.SensorId}:{alert.Type.ToLowerInvariant()}"
            );

            AddHistory(new SmartXEvent(
                alert.SensorId,
                "AlertProcessed",
                $"Processed: {alert.Type}",
                alert.Priority,
                DateTime.UtcNow
            ));

            return alert;
        }
    }

    public object GetHistory(int? sensorId, int limit)
    {
        lock (_sync)
        {
            var events = _history
                .Reverse()
                .Select(item => item.Value);

            if (sensorId.HasValue)
            {
                events = events.Where(
                    item => item.SensorId == sensorId.Value
                );
            }

            return new
            {
                events = events
                    .Take(Math.Clamp(limit, 1, 500))
                    .ToArray(),
                total = _history.Count
            };
        }
    }

    public object GetRecommendations()
    {
        lock (_sync)
        {
            SynchronizeDevices();

            var recommendations = new List<object>();

            // Device-state recommendations.
            foreach (var device in _devices.Values)
            {
                if (device.Status == "Disconnected")
                {
                    recommendations.Add(new
                    {
                        deviceId = device.Id,
                        severity = "Critical",
                        suggestion =
                            "Check device connectivity and reconnect the sensor."
                    });
                }
                else if (!device.IsOn)
                {
                    recommendations.Add(new
                    {
                        deviceId = device.Id,
                        severity = "Warning",
                        suggestion =
                            "Device is switched off. Turn it on if monitoring is required."
                    });
                }
            }

            // Historical analysis runs ONCE, outside the device loop.
            var commandEvents = _history.Values
                .Where(item => item.Type == "Command")
                .ToList();

            foreach (var group in commandEvents
                .GroupBy(item => item.SensorId))
            {
                var disconnectCount = group.Count(item =>
                    item.Message.Equals(
                        "Action: disconnect",
                        StringComparison.OrdinalIgnoreCase));

                if (disconnectCount >= 3)
                {
                    recommendations.Add(new
                    {
                        deviceId = group.Key,
                        severity = "Warning",
                        suggestion =
                            $"Repeated connectivity issue: {disconnectCount} " +
                            "disconnect commands recorded. Inspect network " +
                            "stability and device placement.",
                        reason = "Historical command pattern",
                        evidenceCount = disconnectCount
                    });
                }

                var restartCount = group.Count(item =>
                    item.Message.Equals(
                        "Action: restart",
                        StringComparison.OrdinalIgnoreCase));

                if (restartCount >= 3)
                {
                    recommendations.Add(new
                    {
                        deviceId = group.Key,
                        severity = "Warning",
                        suggestion =
                            $"Frequent restarts detected: {restartCount} " +
                            "restart commands recorded. Investigate device " +
                            "reliability and power stability.",
                        reason = "Historical command pattern",
                        evidenceCount = restartCount
                    });
                }
            }

            // Repeated alert patterns.
            var alertEvents = _history.Values
                .Where(item =>
                    item.Type != "Command" &&
                    item.Type != "Undo" &&
                    item.Type != "AlertProcessed")
                .ToList();

            foreach (var group in alertEvents
                .GroupBy(item => new
                {
                    item.SensorId,
                    item.Type
                }))
            {
                if (group.Count() < 3)
                    continue;

                recommendations.Add(new
                {
                    deviceId = group.Key.SensorId,
                    severity = "Warning",
                    suggestion =
                        $"Recurring {group.Key.Type} alerts detected " +
                        $"({group.Count()} recorded). Investigate the " +
                        "underlying sensor condition.",
                    reason = "Historical alert pattern",
                    evidenceCount = group.Count()
                });
            }

            // Existing telemetry threshold recommendations.
            List<TelemetryReading> readings;

            lock (TelemetryController.Readings)
            {
                readings = TelemetryController.Readings.ToList();
            }

            foreach (var reading in readings
                .GroupBy(item => item.SensorId)
                .Select(group => group.Last()))
            {
                if (reading.Temperature >= 35)
                {
                    recommendations.Add(new
                    {
                        deviceId = reading.SensorId,
                        severity = "Critical",
                        suggestion =
                            "High temperature detected. Inspect cooling and ventilation."
                    });
                }

                if (reading.Humidity >= 80)
                {
                    recommendations.Add(new
                    {
                        deviceId = reading.SensorId,
                        severity = "Warning",
                        suggestion =
                            "High humidity detected. Inspect moisture levels."
                    });
                }
            }

            return new
            {
                total = recommendations.Count,
                recommendations
            };
        }
    }

    private void AddHistory(SmartXEvent item)
    {
        var key = item.Timestamp;

        while (_history.ContainsKey(key))
            key = key.AddTicks(1);

        _history[key] = item with
        {
            Timestamp = key
        };
    }
}
