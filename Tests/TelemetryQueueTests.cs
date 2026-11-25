
using System;
using System.Linq;
using Backend.Models;
using Backend.Services;
using Xunit;

namespace SmartX.Tests;

public class TelemetryQueueTests
{
    private readonly TelemetryQueueService _queue = new();

    private static TelemetryReading CreateReading(int sensorId)
    {
        return new TelemetryReading
        {
            SensorId = sensorId,
            Temperature = 25,
            Humidity = 50
        };
    }

    [Fact]
    public void NewQueue_ShouldBeEmpty()
    {
        Assert.Equal(0, _queue.GetQueueCount());
        Assert.Empty(_queue.GetPendingReadings());
    }

    [Fact]
    public void EnqueueReading_ShouldIncreaseQueueCount()
    {
        _queue.EnqueueReading(CreateReading(1));
        _queue.EnqueueReading(CreateReading(2));

        Assert.Equal(2, _queue.GetQueueCount());
    }

    [Fact]
    public void ProcessNextReading_ShouldFollowFIFOOrder()
    {
        _queue.EnqueueReading(CreateReading(1));
        _queue.EnqueueReading(CreateReading(2));
        _queue.EnqueueReading(CreateReading(3));

        var first = _queue.ProcessNextReading();
        var second = _queue.ProcessNextReading();
        var third = _queue.ProcessNextReading();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotNull(third);

        Assert.Equal(1, first.SensorId);
        Assert.Equal(2, second.SensorId);
        Assert.Equal(3, third.SensorId);

        Assert.Equal(0, _queue.GetQueueCount());
    }

    [Fact]
    public void ProcessNextReading_ShouldReduceQueueCount()
    {
        _queue.EnqueueReading(CreateReading(1));
        _queue.EnqueueReading(CreateReading(2));

        _queue.ProcessNextReading();

        Assert.Equal(1, _queue.GetQueueCount());
    }

    [Fact]
    public void ProcessNextReading_WhenEmpty_ShouldReturnNull()
    {
        var result = _queue.ProcessNextReading();

        Assert.Null(result);
        Assert.Equal(0, _queue.GetQueueCount());
    }

    [Fact]
    public void GetPendingReadings_ShouldNotRemoveReadings()
    {
        _queue.EnqueueReading(CreateReading(1));
        _queue.EnqueueReading(CreateReading(2));

        var pending = _queue.GetPendingReadings();

        Assert.Equal(2, pending.Count);
        Assert.Equal(2, _queue.GetQueueCount());

        Assert.Equal(1, pending[0].SensorId);
        Assert.Equal(2, pending[1].SensorId);
    }

    [Fact]
    public void EnqueueBatch_ShouldPreserveInsertionOrder()
    {
        var readings = new[]
        {
            CreateReading(1),
            CreateReading(2),
            CreateReading(3)
        };

        _queue.EnqueueBatch(readings);

        Assert.Equal(3, _queue.GetQueueCount());

        Assert.Equal(1, _queue.ProcessNextReading()!.SensorId);
        Assert.Equal(2, _queue.ProcessNextReading()!.SensorId);
        Assert.Equal(3, _queue.ProcessNextReading()!.SensorId);
    }

    [Fact]
    public void EnqueueBatch_EmptyBatch_ShouldNotChangeQueue()
    {
        _queue.EnqueueBatch(Array.Empty<TelemetryReading>());

        Assert.Equal(0, _queue.GetQueueCount());
    }

    [Fact]
    public void Queue_ShouldAllowMultipleReadingsFromSameSensor()
    {
        _queue.EnqueueReading(CreateReading(1));
        _queue.EnqueueReading(CreateReading(1));
        _queue.EnqueueReading(CreateReading(1));

        Assert.Equal(3, _queue.GetQueueCount());

        Assert.All(
            _queue.GetPendingReadings(),
            reading => Assert.Equal(1, reading.SensorId)
        );
    }

    [Fact]
    public void PendingReadings_ShouldReturnIndependentList()
    {
        _queue.EnqueueReading(CreateReading(1));

        var snapshot = _queue.GetPendingReadings();

        snapshot.Clear();

        Assert.Single(_queue.GetPendingReadings());
        Assert.Equal(1, _queue.GetQueueCount());
    }
}
