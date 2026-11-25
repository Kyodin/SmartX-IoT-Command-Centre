using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeneratorController : ControllerBase
{
    private readonly TelemetryGeneratorService _generatorService;

    public GeneratorController(
        TelemetryGeneratorService generatorService)
    {
        _generatorService = generatorService;
    }

    [HttpGet("single/{sensorId:int}")]
    public ActionResult<TelemetryReading> GenerateSingle(
        int sensorId)
    {
        if (sensorId <= 0)
        {
            return BadRequest("Sensor ID must be greater than zero.");
        }

        var reading = _generatorService.GenerateReading(sensorId);

        return Ok(reading);
    }

    [HttpGet("batch/{sensorId:int}/{quantity:int}")]
    public ActionResult<List<TelemetryReading>> GenerateBatch(
        int sensorId,
        int quantity)
    {
        if (sensorId <= 0)
        {
            return BadRequest("Sensor ID must be greater than zero.");
        }

        if (quantity <= 0 || quantity > 10000)
        {
            return BadRequest(
                "Quantity must be between 1 and 10,000.");
        }

        var readings = _generatorService.GenerateBatch(
            sensorId,
            quantity);

        return Ok(new
        {
            sensorId,
            quantity,
            readings
        });
    }
}