
using System;
using System.Linq;
using System.Text.Json;
using Backend.Services;
using Xunit;

namespace SmartX.Tests;

public class OperationsTests
{
    private readonly SensorService _sensors = new();
    private readonly SmartXOperationsService _operations;

    public OperationsTests()
    {
        _operations = new SmartXOperationsService(_sensors);
    }

    private static JsonElement Json(object value) =>
        JsonSerializer.SerializeToElement(value);

    [Fact]
    public void Dictionary_FindsSeededDeviceById()
    {
        var device = _operations.FindDevice(1);

        Assert.NotNull(device);
        Assert.Equal(1, device.Id);
        Assert.Null(_operations.FindDevice(99999));
    }

    [Fact]
    public void InvalidCommand_DoesNotChangeDevice()
    {
        var before = _operations.FindDevice(1)!;
        var result = _operations.ApplyCommand(1, "invalid-action");

        Assert.False(result.Success);
        Assert.Equal(
            before.Status,
            _operations.FindDevice(1)!.Status
        );

        Assert.Equal(
            0,
            Json(_operations.GetCommandHistory())
                .GetProperty("undoCount").GetInt32()
        );
    }

    [Fact]
    public void Disconnect_UpdatesSensorAndDisconnectedHashSet()
    {
        var result = _operations.ApplyCommand(1, "disconnect");

        Assert.True(result.Success);
        Assert.Equal(
            "Disconnected",
            _operations.FindDevice(1)!.Status
        );

        Assert.False(_sensors.GetById(1)!.IsOnline);

        var devices = Json(_operations.GetDevices());

        Assert.Contains(
            devices.GetProperty("disconnectedDeviceIds")
                .EnumerateArray(),
            id => id.GetInt32() == 1
        );
    }

    [Fact]
    public void Stack_UndoRestoresPreviousStateInReverseOrder()
    {
        Assert.True(_operations.ApplyCommand(1, "off").Success);
        Assert.True(_operations.ApplyCommand(1, "connect").Success);

        Assert.True(_operations.Undo().Success);
        Assert.Equal(
            "Offline",
            _operations.FindDevice(1)!.Status
        );
        Assert.False(_sensors.GetById(1)!.IsOnline);

        Assert.True(_operations.Undo().Success);
        Assert.Equal(
            "Online",
            _operations.FindDevice(1)!.Status
        );
        Assert.True(_sensors.GetById(1)!.IsOnline);

        Assert.False(_operations.Undo().Success);
    }

    [Fact]
    public void HashSet_RejectsDuplicateActiveAlert()
    {
        Assert.True(
            _operations.AddAlert(1, "Network", "First", 2).Added
        );

        Assert.False(
            _operations.AddAlert(1, "network", "Second", 1).Added
        );

        var result = Json(_operations.GetAlerts());

        Assert.Equal(
            1,
            result.GetProperty("pendingCount").GetInt32()
        );

        Assert.Equal(
            1,
            result.GetProperty("uniqueActiveCount").GetInt32()
        );
    }

    [Fact]
    public void PriorityQueue_ProcessesCriticalAlertFirst()
    {
        _operations.AddAlert(1, "Info", "Low", 3);
        _operations.AddAlert(1, "Critical", "High", 1);
        _operations.AddAlert(2, "Warning", "Medium", 2);

        Assert.Equal(
            "Critical",
            _operations.ProcessAlert()!.Type
        );

        Assert.Equal(
            "Warning",
            _operations.ProcessAlert()!.Type
        );

        Assert.Equal(
            "Info",
            _operations.ProcessAlert()!.Type
        );

        Assert.Null(_operations.ProcessAlert());
    }

    [Fact]
    public void PriorityQueue_EqualPriorityPreservesInsertionOrder()
    {
        _operations.AddAlert(1, "First", "A", 2);
        _operations.AddAlert(2, "Second", "B", 2);

        Assert.Equal(
            "First",
            _operations.ProcessAlert()!.Type
        );

        Assert.Equal(
            "Second",
            _operations.ProcessAlert()!.Type
        );
    }

    [Fact]
    public void ProcessedAlert_CanBeAddedAgain()
    {
        Assert.True(
            _operations.AddAlert(1, "Network", "First", 2).Added
        );

        Assert.NotNull(_operations.ProcessAlert());

        Assert.True(
            _operations.AddAlert(1, "Network", "New", 2).Added
        );
    }

    [Fact]
    public void SortedDictionary_ReturnsNewestEventsFirst()
    {
        _operations.ApplyCommand(1, "off");
        _operations.ApplyCommand(1, "on");
        _operations.ApplyCommand(2, "restart");

        var events = Json(_operations.GetHistory(null, 50))
            .GetProperty("events")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(3, events.Length);

        Assert.Contains(
            "restart",
            events[0].GetProperty("Message").GetString()!
        );

        var timestamps = events
            .Select(e =>
                e.GetProperty("Timestamp").GetDateTime()
            )
            .ToArray();

        Assert.True(
            timestamps.SequenceEqual(
                timestamps.OrderByDescending(x => x)
            )
        );
    }

    [Fact]
    public void HistoricalRestartRecommendation_IsNotDuplicatedAcrossDevices()
    {
        for (var i = 0; i < 3; i++)
        {
            Assert.True(
                _operations.ApplyCommand(1, "restart").Success
            );
        }

        var result = Json(_operations.GetRecommendations());

        var matching = result
            .GetProperty("recommendations")
            .EnumerateArray()
            .Where(x =>
                x.GetProperty("suggestion")
                    .GetString()!
                    .Contains(
                        "Frequent restarts",
                        StringComparison.OrdinalIgnoreCase
                    )
            )
            .ToArray();

        Assert.Single(matching);

        Assert.Equal(
            3,
            matching[0]
                .GetProperty("evidenceCount")
                .GetInt32()
        );
    }
}
