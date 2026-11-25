using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OopController : ControllerBase
{
    private readonly TelemetryPacketService _packetService;

    public OopController(TelemetryPacketService packetService)
    {
        _packetService = packetService;
    }

    [HttpPost("create-packet")]
    public ActionResult<TelemetryPacket<SensorReading>> CreatePacket(
        int sensorId,
        string macAddress,
        SensorReading reading)
    {
        var packet = _packetService.CreatePacket(
            sensorId,
            macAddress,
            reading);

        return Ok(packet);
    }

   [HttpPost("combine-readings")]
public ActionResult<SensorReading> CombineReadings(
    [FromBody] CombineReadingsRequest request)
{
    var combined = _packetService.CombineReadings(
        request.First,
        request.Second);

    return Ok(combined);
}
}