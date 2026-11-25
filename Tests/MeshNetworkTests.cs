
using Backend.Models;
using Backend.Services;

namespace SmartX.Tests;

public class MeshNetworkTests
{
    // Each test receives an independent sensor registry.
    private static (SensorService Sensors,
        MeshNetworkService Network) Create(int extras = 0)
    {
        var sensors = new SensorService();

        for (int i = 0; i < extras; i++)
        {
            sensors.Add(new Sensor
            {
                Name = $"Test Sensor {i + 3}",
                MacAddress = $"AA:BB:CC:00:00:{i + 3:X2}",
                Location = "Testing Lab",
                Category = "Temperature"
            });
        }

        return (sensors, new MeshNetworkService(sensors));
    }

    [Fact]
    public void Topology_ContainsRegisteredSensors()
    {
        var (_, graph) = Create();

        var result = graph.GetTopology();

        Assert.Equal(2, result.Nodes.Count);
        Assert.Equal(2, result.OnlineSensorCount);
        Assert.Empty(result.Links);
    }

    [Fact]
    public void Connection_IsBidirectional()
    {
        var (_, graph) = Create();

        graph.AddLink(1, 2, 4);

        var result = graph.Traverse(2, "bfs");

        Assert.Equal(new[] { 2, 1 }, result.VisitOrder);
        Assert.Single(graph.GetTopology().Links);
    }

    [Fact]
    public void SelfConnection_IsRejected()
    {
        var (_, graph) = Create();

        Assert.Throws<ArgumentException>(() =>
            graph.AddLink(1, 1, 5));
    }

    [Fact]
    public void UnknownSensor_IsRejected()
    {
        var (_, graph) = Create();

        Assert.Throws<KeyNotFoundException>(() =>
            graph.AddLink(1, 999, 5));
    }

    [Fact]
    public void InvalidCost_IsRejected()
    {
        var (_, graph) = Create();

        foreach (double cost in new[]
        {
            0, -1, double.NaN,
            double.PositiveInfinity
        })
        {
            Assert.Throws<ArgumentException>(() =>
                graph.AddLink(1, 2, cost));
        }
    }

    [Fact]
    public void Bfs_VisitsSensorsLevelByLevel()
    {
        var (_, graph) = Create(3);

        graph.AddLink(1, 2, 1);
        graph.AddLink(1, 3, 1);
        graph.AddLink(2, 4, 1);
        graph.AddLink(3, 5, 1);

        var result = graph.Traverse(1, "bfs");

        Assert.Equal(
            new[] { 1, 2, 3, 4, 5 },
            result.VisitOrder);
    }

    [Fact]
    public void Dfs_VisitsAllReachableSensors()
    {
        var (_, graph) = Create(2);

        graph.AddLink(1, 2, 1);
        graph.AddLink(1, 3, 1);
        graph.AddLink(2, 4, 1);

        var result = graph.Traverse(1, "dfs");

        Assert.Equal("DFS", result.Algorithm);
        Assert.Equal(4, result.VisitOrder.Distinct().Count());
        Assert.Empty(result.UnreachableOnlineSensorIds);
    }

    [Fact]
    public void Prim_FindsMinimumCostSpanningTree()
    {
        var (_, graph) = Create(2);

        graph.AddLink(1, 2, 10);
        graph.AddLink(1, 3, 1);
        graph.AddLink(3, 2, 2);
        graph.AddLink(2, 4, 3);
        graph.AddLink(3, 4, 9);

        var result = graph.CalculateMst();

        Assert.Equal(3, result.Edges.Count);
        Assert.Equal(6.0, result.TotalCost);
        Assert.True(result.SpansAllOnlineSensors);
        Assert.Equal(1, result.ConnectedComponents);
    }

    [Fact]
    public void DisconnectedGraph_ReturnsSpanningForest()
    {
        var (_, graph) = Create(1);

        graph.AddLink(1, 2, 3);

        var result = graph.CalculateMst();

        Assert.Equal(2, result.ConnectedComponents);
        Assert.False(result.SpansAllOnlineSensors);
        Assert.Single(result.Edges);
    }

    [Fact]
    public void OfflineSensor_IsExcludedFromRouting()
    {
        var (sensors, graph) = Create();

        graph.AddLink(1, 2, 2);
        sensors.SetOnlineStatus(2, false);

        var traversal = graph.Traverse(1, "bfs");
        var mst = graph.CalculateMst();

        Assert.Equal(new[] { 1 }, traversal.VisitOrder);
        Assert.Empty(mst.Edges);
        Assert.Equal(1, mst.OnlineSensorCount);
        Assert.False(graph.GetTopology().Links[0].IsActive);
    }
}
