
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Backend.Controllers;

[ApiController]
[Route("api/search-intelligence")]
public class SearchIntelligenceController : ControllerBase
{
    private readonly SearchIntelligenceService _intelligence;
    private readonly SensorService _sensors;

    public SearchIntelligenceController(
        SearchIntelligenceService intelligence,
        SensorService sensors
    )
    {
        _intelligence = intelligence;
        _sensors = sensors;
    }

    public record SearchRequest(int SensorId);

    [HttpPost("record")]
    public IActionResult RecordSearch(
        [FromBody] SearchRequest request
    )
    {
        if (request.SensorId <= 0)
        {
            return BadRequest(new
            {
                message = "Enter a valid sensor ID."
            });
        }

        var sensor = _sensors.GetById(request.SensorId);

        if (sensor is null)
        {
            return NotFound(new
            {
                message = "Sensor not found."
            });
        }

        _intelligence.RecordSearch(request.SensorId);

        return Ok(new
        {
            message = "Search activity recorded.",
            sensorId = request.SensorId
        });
    }

    [HttpGet("history")]
    public IActionResult History()
    {
        return Ok(_intelligence.GetSearchHistory());
    }

    [HttpGet("recommendations")]
    public IActionResult Recommendations()
    {
        var offlineIds = _sensors.GetAll()
            .Where(sensor => !sensor.IsOnline)
            .Select(sensor => sensor.Id);

        return Ok(
            _intelligence.GetRecommendations(offlineIds)
        );
    }
}
