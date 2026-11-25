using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly TelemetryAnalysisService
        _analysisService;

    public AnalysisController(
        TelemetryAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    [HttpGet("matrix")]
    public ActionResult<double[,]> GetTelemetryMatrix()
    {
        return Ok(
            _analysisService.CreateTelemetryMatrix()
        );
    }

    [HttpPost("count-readings")]
    public IActionResult CountReadings(
        [FromBody]
        List<TelemetryReading> readings)
    {
        var count =
            _analysisService
                .CountReadingsRecursively(readings);

        return Ok(
            new
            {
                method = "Recursive Count",
                totalReadings = count
            }
        );
    }

    [HttpGet("performance/{size:int}")]
    public IActionResult PerformanceTest(
        int size)
    {
        if (size < 1)
        {
            return BadRequest(
                "Size must be greater than zero."
            );
        }

        if (size > 100000)
        {
            return BadRequest(
                "Maximum test size is 100,000 records."
            );
        }

        var result =
            _analysisService
                .RunPerformanceTest(size);

        return Ok(result);
    }
}