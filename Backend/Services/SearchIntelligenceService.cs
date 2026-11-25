
using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Services;

public class SearchIntelligenceService
{
    private readonly object _sync = new();

    // O(1) average lookup of search counts by sensor ID.
    private readonly Dictionary<int, int> _searchCounts = new();

    // FIFO history of the most recent searches.
    private readonly Queue<SearchRecord> _recentSearches = new();

    // Prevent repeated recommendations for one sensor.
    private const int MaximumHistory = 100;
    private const int RepeatedSearchThreshold = 3;

    public record SearchRecord(
        int SensorId,
        DateTime TimestampUtc
    );

    public void RecordSearch(int sensorId)
    {
        if (sensorId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sensorId),
                "Sensor ID must be greater than zero."
            );
        }

        lock (_sync)
        {
            _searchCounts.TryGetValue(
                sensorId,
                out int currentCount
            );

            _searchCounts[sensorId] = currentCount + 1;

            _recentSearches.Enqueue(
                new SearchRecord(
                    sensorId,
                    DateTime.UtcNow
                )
            );

            if (_recentSearches.Count > MaximumHistory)
            {
                _recentSearches.Dequeue();
            }
        }
    }

    public object GetSearchHistory()
    {
        lock (_sync)
        {
            return new
            {
                totalRecordedSearches =
                    _searchCounts.Values.Sum(),

                recentSearches =
                    _recentSearches.Reverse().ToArray(),

                searchCounts = _searchCounts
                    .OrderByDescending(x => x.Value)
                    .Select(x => new
                    {
                        sensorId = x.Key,
                        count = x.Value
                    })
                    .ToArray()
            };
        }
    }

    public object GetRecommendations(
        IEnumerable<int> offlineSensorIds
    )
    {
        lock (_sync)
        {
            var offline = new HashSet<int>(
                offlineSensorIds
            );

            var recommendedSensors = new HashSet<int>();
            var recommendations = new List<object>();

            foreach (var entry in _searchCounts
                .OrderByDescending(x => x.Value))
            {
                int sensorId = entry.Key;
                int searchCount = entry.Value;

                if (searchCount < RepeatedSearchThreshold)
                {
                    continue;
                }

                if (!recommendedSensors.Add(sensorId))
                {
                    continue;
                }

                bool isOffline = offline.Contains(sensorId);

                recommendations.Add(new
                {
                    deviceId = sensorId,
                    severity = isOffline
                        ? "Critical"
                        : "Information",

                    suggestion = isOffline
                        ? $"Sensor {sensorId} has been searched " +
                          $"{searchCount} times and is offline. " +
                          "Investigate its connectivity."
                        : $"Sensor {sensorId} has been searched " +
                          $"{searchCount} times. Review its recent " +
                          "telemetry and command history.",

                    reason = isOffline
                        ? "Repeated searches and offline status"
                        : "Repeated search activity",

                    evidenceCount = searchCount
                });
            }

            return new
            {
                total = recommendations.Count,
                recommendations
            };
        }
    }
}

