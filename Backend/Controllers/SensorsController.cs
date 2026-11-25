using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly SensorService _sensorService;

    public SensorsController(SensorService sensorService)
    {
        _sensorService = sensorService;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<Sensor>> GetSensors()
    {
        return Ok(_sensorService.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Sensor> GetSensor(int id)
    {
        var sensor = _sensorService.GetById(id);

        if (sensor is null)
        {
            return NotFound($"Sensor with ID {id} was not found.");
        }

        return Ok(sensor);
    }

    [HttpGet("mac/{macAddress}")]
    public ActionResult<Sensor> GetSensorByMacAddress(
        string macAddress)
    {
        var sensor = _sensorService.GetByMacAddress(macAddress);

        if (sensor is null)
        {
            return NotFound(
                $"Sensor with MAC address {macAddress} was not found.");
        }

        return Ok(sensor);
    }

    [HttpPost]
    public ActionResult<Sensor> CreateSensor(Sensor sensor)
    {
        _sensorService.Add(sensor);

        return CreatedAtAction(
            nameof(GetSensor),
            new { id = sensor.Id },
            sensor);
    }
}