
using Backend.Controllers;
using Backend.DataStructures;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartX.Tests;

public class TelemetryAvlTreeTests
{
    private static readonly DateTime BaseTime =
        new DateTime(
            2026, 11, 12,
            12, 0, 0,
            DateTimeKind.Utc);

    private static TelemetryReading Create(
        int minutes,
        int sensorId = 1)
    {
        return new TelemetryReading
        {
            SensorId = sensorId,
            Temperature = 25,
            Humidity = 50,
            BatteryLevel = 90,
            RecordedAt = BaseTime.AddMinutes(minutes)
        };
    }

    // Test all four AVL rotation scenarios.
    [Theory]
    [InlineData(30, 20, 10)] // LL
    [InlineData(10, 20, 30)] // RR
    [InlineData(30, 10, 20)] // LR
    [InlineData(10, 30, 20)] // RL
    public void ThreeInserts_ShouldRebalance(
        int first,
        int second,
        int third)
    {
        var tree = new TelemetryAvlTree();

        tree.Insert(Create(first));
        tree.Insert(Create(second));
        tree.Insert(Create(third));

        Assert.Equal(2, tree.Height);
        Assert.Equal(3, tree.ReadingCount);
        Assert.Equal(3, tree.DistinctTimestampCount);

        Assert.Equal(
            3,
            tree.SearchRange(
                BaseTime,
                BaseTime.AddHours(1)).Count);
    }

    // Multiple readings with identical timestamps must be kept.
    [Fact]
    public void DuplicateTimestamp_ShouldPreserveAllReadings()
    {
        var tree = new TelemetryAvlTree();

        tree.Insert(Create(5, 1));
        tree.Insert(Create(5, 2));
        tree.Insert(Create(5, 1));

        Assert.Equal(3, tree.ReadingCount);
        Assert.Equal(1, tree.DistinctTimestampCount);

        Assert.Equal(
            3,
            tree.SearchRange(
                BaseTime.AddMinutes(5),
                BaseTime.AddMinutes(5)).Count);

        Assert.Equal(
            2,
            tree.SearchRange(
                BaseTime.AddMinutes(5),
                BaseTime.AddMinutes(5),
                1).Count);
    }

    // Ensure results are chronological and include boundaries.
    [Fact]
    public void RangeSearch_ShouldReturnCorrectOrder()
    {
        var tree = new TelemetryAvlTree();

        foreach (var minute in new[] { 9, 1, 5, 3, 7 })
        {
            tree.Insert(Create(minute));
        }

        var results = tree.SearchRange(
            BaseTime.AddMinutes(3),
            BaseTime.AddMinutes(7));

        Assert.Equal(
            new[] { 3, 5, 7 },
            results.Select(r =>
                (int)(r.RecordedAt - BaseTime).TotalMinutes));
    }

    // An empty tree should return an empty collection.
    [Fact]
    public void EmptyTree_ShouldReturnNoReadings()
    {
        var tree = new TelemetryAvlTree();

        var results = tree.SearchRange(
            BaseTime,
            BaseTime.AddDays(1));

        Assert.Empty(results);
    }

    // Invalid date ranges must be rejected.
    [Fact]
    public void ReversedRange_ShouldThrowException()
    {
        var tree = new TelemetryAvlTree();

        Assert.Throws<ArgumentException>(() =>
            tree.SearchRange(
                BaseTime.AddMinutes(1),
                BaseTime));
    }

    // Sequential inserts should not make the AVL tree too tall.
    [Fact]
    public void ThousandReadings_ShouldRemainBalanced()
    {
        var tree = new TelemetryAvlTree();

        for (int i = 0; i < 1000; i++)
        {
            tree.Insert(Create(i));
        }

        Assert.Equal(1000, tree.ReadingCount);

        Assert.InRange(tree.Height, 1, 15);

        Assert.Equal(
            101,
            tree.SearchRange(
                BaseTime.AddMinutes(100),
                BaseTime.AddMinutes(200)).Count);
    }

    // Check that batch ingestion updates the AVL index.
    [Fact]
    public void ControllerBatch_ShouldIndexAllReadings()
    {
        var history = new TelemetryHistoryService();

        var controller = new TelemetryController(
            new TelemetryQueueService(),
            history);

        var result = controller.AddBatch(
            new List<TelemetryReading>
            {
                Create(1),
                Create(2),
                Create(3)
            });

        Assert.IsType<OkObjectResult>(result);

        Assert.Equal(
            3,
            history.GetStatistics().readingCount);

        Assert.Equal(
            3,
            history.SearchRange(
                DateTime.UtcNow.AddMinutes(-1),
                DateTime.UtcNow.AddMinutes(1)).Count);
    }

    // Check that processed queue readings enter the AVL index.
    [Fact]
    public void QueueProcessing_ShouldUpdateIndex()
    {
        var history = new TelemetryHistoryService();
        var queue = new TelemetryQueueService();

        var controller = new TelemetryController(
            queue,
            history);

        controller.EnqueueReading(Create(1));
        controller.ProcessNextReading();

        Assert.Equal(
            1,
            history.GetStatistics().readingCount);
    }
}
