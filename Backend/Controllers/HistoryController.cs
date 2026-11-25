
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HistoryController : ControllerBase
{
    private readonly TelemetryHistoryService _history;

    public HistoryController(
        TelemetryHistoryService history)
    {
        _history = history;
    }

    // PART 3: Search historical telemetry using an AVL tree.
    [HttpGet("range")]
    public IActionResult GetRange(
        [FromQuery] DateTimeOffset? start,
        [FromQuery] DateTimeOffset? end,
        [FromQuery] int? sensorId)
    {
        if (!start.HasValue || !end.HasValue)
        {
            return BadRequest(
                "Both start and end timestamps are required.");
        }

        if (start > end)
        {
            return BadRequest(
                "Start must be earlier than or equal to end.");
        }

        if (sensorId.HasValue && sensorId.Value <= 0)
        {
            return BadRequest(
                "Sensor ID must be greater than zero.");
        }

        var stopwatch = Stopwatch.StartNew();

        var readings = _history.SearchRange(
            start.Value.UtcDateTime,
            end.Value.UtcDateTime,
            sensorId);

        stopwatch.Stop();

        return Ok(new
        {
            algorithm = "AVL Tree Timestamp Range Search",

            startUtc = start.Value.UtcDateTime,
            endUtc = end.Value.UtcDateTime,

            sensorId,

            resultCount = readings.Count,

            searchTimeMicroseconds =
                stopwatch.Elapsed.TotalMicroseconds,

            readings
        });
    }

    // PART 3: Display AVL tree statistics.
    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        var stats = _history.GetStatistics();

        return Ok(new
        {
            dataStructure = "AVL Tree",

            totalIndexedReadings =
                stats.readingCount,

            distinctTimestampNodes =
                stats.timestampCount,

            treeHeight =
                stats.treeHeight
        });
    }
}
