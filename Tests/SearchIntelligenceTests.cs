
using System;
using System.Linq;
using System.Text.Json;
using Backend.Services;
using Xunit;

namespace SmartX.Tests;

public class SearchIntelligenceTests
{
    private readonly SearchIntelligenceService _service = new();

    private static JsonElement AsJson(object value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    [Fact]
    public void RecordSearch_RejectsInvalidSensorId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _service.RecordSearch(0)
        );
    }

    [Fact]
    public void RecordSearch_CountsRepeatedSearches()
    {
        _service.RecordSearch(1);
        _service.RecordSearch(1);
        _service.RecordSearch(1);

        var history = AsJson(_service.GetSearchHistory());

        Assert.Equal(
            3,
            history.GetProperty("totalRecordedSearches").GetInt32()
        );

        var counts = history.GetProperty("searchCounts");

        Assert.Single(counts.EnumerateArray());

        Assert.Equal(
            3,
            counts[0].GetProperty("count").GetInt32()
        );
    }

    [Fact]
    public void RecentSearches_UsesFifoAndKeepsLast100()
    {
        for (int i = 1; i <= 105; i++)
        {
            _service.RecordSearch(i);
        }

        var history = AsJson(_service.GetSearchHistory());
        var recent = history.GetProperty("recentSearches");

        Assert.Equal(100, recent.GetArrayLength());

        // The API returns newest searches first.
        Assert.Equal(
            105,
            recent[0].GetProperty("SensorId").GetInt32()
        );

        Assert.Equal(
            6,
            recent[99].GetProperty("SensorId").GetInt32()
        );
    }

    [Fact]
    public void TwoSearches_DoNotTriggerRecommendation()
    {
        _service.RecordSearch(1);
        _service.RecordSearch(1);

        var result = AsJson(
            _service.GetRecommendations(Array.Empty<int>())
        );

        Assert.Equal(
            0,
            result.GetProperty("total").GetInt32()
        );
    }

    [Fact]
    public void ThreeSearches_TriggerInformationRecommendation()
    {
        for (int i = 0; i < 3; i++)
        {
            _service.RecordSearch(1);
        }

        var result = AsJson(
            _service.GetRecommendations(Array.Empty<int>())
        );

        Assert.Equal(
            1,
            result.GetProperty("total").GetInt32()
        );

        var recommendation =
            result.GetProperty("recommendations")[0];

        Assert.Equal(
            "Information",
            recommendation.GetProperty("severity").GetString()
        );

        Assert.Equal(
            3,
            recommendation.GetProperty("evidenceCount").GetInt32()
        );
    }

    [Fact]
    public void OfflineSensor_EscalatesRecommendationToCritical()
    {
        for (int i = 0; i < 3; i++)
        {
            _service.RecordSearch(1);
        }

        var result = AsJson(
            _service.GetRecommendations(new[] { 1 })
        );

        var recommendation =
            result.GetProperty("recommendations")[0];

        Assert.Equal(
            "Critical",
            recommendation.GetProperty("severity").GetString()
        );

        Assert.Contains(
            "offline",
            recommendation.GetProperty("suggestion")
                .GetString()!,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public void Recommendations_DoNotDuplicateSameSensor()
    {
        for (int i = 0; i < 10; i++)
        {
            _service.RecordSearch(1);
        }

        var result = AsJson(
            _service.GetRecommendations(new[] { 1 })
        );

        Assert.Equal(
            1,
            result.GetProperty("total").GetInt32()
        );
    }

    [Fact]
    public void DifferentSensors_HaveIndependentSearchCounts()
    {
        _service.RecordSearch(1);
        _service.RecordSearch(1);
        _service.RecordSearch(2);
        _service.RecordSearch(2);
        _service.RecordSearch(2);

        var result = AsJson(
            _service.GetRecommendations(Array.Empty<int>())
        );

        Assert.Equal(
            1,
            result.GetProperty("total").GetInt32()
        );

        Assert.Equal(
            2,
            result.GetProperty("recommendations")[0]
                .GetProperty("deviceId").GetInt32()
        );
    }
}
