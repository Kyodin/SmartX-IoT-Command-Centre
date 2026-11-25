
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    // Shared telemetry storage used by Parts 1 and 2.
    public static readonly List<TelemetryReading> Readings = [];

    private static readonly object ReadingsLock = new();

    private static int GeneratedCount { get; set; }
    private static int TransmittedCount { get; set; }
    private static int ReceivedCount { get; set; }

    private readonly TelemetryQueueService _queueService;

    // PART 3: Historical timestamp index.
    private readonly TelemetryHistoryService _history;

    public TelemetryController(
        TelemetryQueueService queueService,
        TelemetryHistoryService history)
    {
        _queueService = queueService;
        _history = history;
    }

    // PART 1: Retrieve all stored telemetry.
    [HttpGet]
    public IActionResult GetReadings()
    {
        lock (ReadingsLock)
        {
            return Ok(Readings.ToList());
        }
    }

    // PART 1: Retrieve telemetry for one sensor.
    [HttpGet("{sensorId:int}")]
    public IActionResult GetReadingsBySensor(int sensorId)
    {
        var stopwatch = Stopwatch.StartNew();

        List<TelemetryReading> readings;

        lock (ReadingsLock)
        {
            readings = Readings
                .Where(reading => reading.SensorId == sensorId)
                .ToList();
        }

        stopwatch.Stop();

        return Ok(new
        {
            sensorId,

            totalResults = readings.Count,

            searchTimeMicroseconds =
                stopwatch.Elapsed.TotalMicroseconds,

            readings
        });
    }

    // PART 1: Search telemetry with optional filters.
    [HttpGet("search")]
    public IActionResult SearchTelemetry(
        [FromQuery] int? sensorId,
        [FromQuery] double? minimumTemperature,
        [FromQuery] double? maximumTemperature)
    {
        var stopwatch = Stopwatch.StartNew();

        List<TelemetryReading> snapshot;

        lock (ReadingsLock)
        {
            snapshot = Readings.ToList();
        }

        IEnumerable<TelemetryReading> results = snapshot;

        if (sensorId.HasValue)
        {
            results = results.Where(
                reading => reading.SensorId == sensorId.Value);
        }

        if (minimumTemperature.HasValue)
        {
            results = results.Where(
                reading =>
                    reading.Temperature >=
                    minimumTemperature.Value);
        }

        if (maximumTemperature.HasValue)
        {
            results = results.Where(
                reading =>
                    reading.Temperature <=
                    maximumTemperature.Value);
        }

        var matchingReadings = results.ToList();

        stopwatch.Stop();

        return Ok(new
        {
            totalResults = matchingReadings.Count,

            searchTimeMicroseconds =
                stopwatch.Elapsed.TotalMicroseconds,

            readings = matchingReadings
        });
    }

    // PART 1: Store an individual telemetry reading.
    [HttpPost]
    public IActionResult AddReading(
        [FromBody] TelemetryReading reading)
    {
        var stopwatch = Stopwatch.StartNew();

        lock (ReadingsLock)
        {
            reading.Id = Readings.Count == 0
                ? 1
                : Readings.Max(r => r.Id) + 1;

            reading.RecordedAt = DateTime.UtcNow;

            Readings.Add(reading);

            // PART 3: Index the reading in the AVL tree.
            _history.Index(reading);

            ReceivedCount++;
            TransmittedCount++;
        }

        stopwatch.Stop();

        return CreatedAtAction(
            nameof(GetReadingsBySensor),
            new { sensorId = reading.SensorId },
            new
            {
                reading,

                insertionTimeMicroseconds =
                    stopwatch.Elapsed.TotalMicroseconds
            });
    }

    // PART 1: Receive multiple telemetry readings.
    [HttpPost("batch")]
    public IActionResult AddBatch(
        [FromBody] List<TelemetryReading> readings)
    {
        if (readings is null || readings.Count == 0)
        {
            return BadRequest(
                "The telemetry batch cannot be empty.");
        }

        var stopwatch = Stopwatch.StartNew();

        int totalStored;
        int generatedCount;
        int transmittedCount;
        int receivedCount;

        lock (ReadingsLock)
        {
            var nextId = Readings.Count == 0
                ? 1
                : Readings.Max(reading => reading.Id) + 1;

            foreach (var reading in readings)
            {
                reading.Id = nextId++;
                reading.RecordedAt = DateTime.UtcNow;

                // Preserve existing Part 1 telemetry storage.
                Readings.Add(reading);

                // PART 3: Index every batch reading.
                _history.Index(reading);
            }

            GeneratedCount += readings.Count;
            TransmittedCount += readings.Count;
            ReceivedCount += readings.Count;

            totalStored = Readings.Count;
            generatedCount = GeneratedCount;
            transmittedCount = TransmittedCount;
            receivedCount = ReceivedCount;
        }

        stopwatch.Stop();

        return Ok(new
        {
            message = "Telemetry batch received successfully.",

            batchSize = readings.Count,

            totalStored,
            generatedCount,
            transmittedCount,
            receivedCount,

            insertionTimeMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds
        });
    }

    // PART 1: Retrieve telemetry statistics.
    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        lock (ReadingsLock)
        {
            return Ok(new
            {
                totalStored = Readings.Count,
                generatedCount = GeneratedCount,
                transmittedCount = TransmittedCount,
                receivedCount = ReceivedCount
            });
        }
    }

    // PART 2: Add telemetry to the FIFO queue.
    [HttpPost("queue")]
    public IActionResult EnqueueReading(
        [FromBody] TelemetryReading reading)
    {
        if (reading is null)
        {
            return BadRequest(
                "A telemetry reading is required.");
        }

        _queueService.EnqueueReading(reading);

        return Ok(new
        {
            message = "Reading added to the processing queue.",

            sensorId = reading.SensorId,

            pendingCount = _queueService.GetQueueCount()
        });
    }

    // PART 2: View pending queue readings.
    [HttpGet("queue")]
    public IActionResult GetQueue()
    {
        var pendingReadings =
            _queueService.GetPendingReadings();

        return Ok(new
        {
            pendingCount = pendingReadings.Count,

            processingOrder = "FIFO",

            readings = pendingReadings
        });
    }

    // PART 2: Process the oldest queued reading.
    [HttpPost("queue/process-next")]
    public IActionResult ProcessNextReading()
    {
        var reading = _queueService.ProcessNextReading();

        if (reading is null)
        {
            return Ok(new
            {
                message = "The telemetry queue is empty.",
                pendingCount = 0
            });
        }

        lock (ReadingsLock)
        {
            reading.Id = Readings.Count == 0
                ? 1
                : Readings.Max(r => r.Id) + 1;

            reading.RecordedAt = DateTime.UtcNow;

            // Store the processed reading.
            Readings.Add(reading);

            // PART 3: Add processed reading to the AVL index.
            _history.Index(reading);

            ReceivedCount++;
        }

        return Ok(new
        {
            message = "Oldest telemetry reading processed.",

            processedReading = reading,

            pendingCount = _queueService.GetQueueCount()
        });
    }
}
