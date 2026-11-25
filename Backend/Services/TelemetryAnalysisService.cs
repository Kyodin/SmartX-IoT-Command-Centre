using Backend.Models;
using System.Diagnostics;

namespace Backend.Services;

public class TelemetryAnalysisService
{
    public double[,] CreateTelemetryMatrix()
    {
        return new double[,]
        {
            { 22.5, 50.0, 95.0 },
            { 24.1, 55.5, 90.0 },
            { 26.8, 60.2, 85.0 }
        };
    }

    public int CountReadingsRecursively(
        List<TelemetryReading> readings,
        int index = 0)
    {
        if (index >= readings.Count)
        {
            return 0;
        }

        return 1 + CountReadingsRecursively(
            readings,
            index + 1
        );
    }

    public object RunPerformanceTest(int size)
    {
        var readings =
            new List<TelemetryReading>(size);

        // Measure insertion performance
        var insertionTimer =
            Stopwatch.StartNew();

        for (int i = 0; i < size; i++)
        {
            readings.Add(
                new TelemetryReading
                {
                    Id = i + 1,
                    SensorId = (i % 2) + 1,
                    Temperature =
                        20 + (i % 15),
                    Humidity =
                        40 + (i % 30),
                    BatteryLevel =
                        70 + (i % 30),
                    RecordedAt =
                        DateTime.UtcNow
                }
            );
        }

        insertionTimer.Stop();

        // Measure linear search performance
        var searchTimer =
            Stopwatch.StartNew();

        var searchResults = readings
            .Where(r => r.SensorId == 2)
            .ToList();

        searchTimer.Stop();

        return new
        {
            recordsTested = size,

            dataStructure =
                "List<TelemetryReading>",

            searchAlgorithm =
                "Linear Search",

            insertionTimeMilliseconds =
                insertionTimer.Elapsed
                    .TotalMilliseconds,

            searchTimeMilliseconds =
                searchTimer.Elapsed
                    .TotalMilliseconds,

            matchingRecords =
                searchResults.Count
        };
    }
}