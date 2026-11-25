using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public IActionResult Search([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest(
                "Please provide a search term."
            );
        }

        var stopwatch = Stopwatch.StartNew();

        var allReadings =
            TelemetryController.Readings.ToList();

        var results = allReadings
            .Where(reading =>
                reading.SensorId
                    .ToString()
                    .Contains(
                        term.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
            )
            .ToList();

        stopwatch.Stop();

        return Ok(new
        {
            algorithm = "Linear Search",
            searchTerm = term,
            totalReadingsAvailable =
                allReadings.Count,
            resultCount = results.Count,
            searchTimeMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds,
            results
        });
    }
}