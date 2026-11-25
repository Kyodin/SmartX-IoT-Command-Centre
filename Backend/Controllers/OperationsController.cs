using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/operations")]
public class OperationsController : ControllerBase
{
    private readonly SmartXOperationsService _service;
    public OperationsController(SmartXOperationsService service) => _service = service;

    [HttpGet("devices")]
    public IActionResult Devices() => Ok(_service.GetDevices());

    [HttpGet("devices/{id:int}")]
    public IActionResult Device(int id)
    {
        var device = _service.FindDevice(id);
        return device is null ? NotFound(new { message = "Device not found." }) : Ok(device);
    }

    public record CommandRequest(int DeviceId, string Action);

    [HttpPost("commands")]
    public IActionResult Command([FromBody] CommandRequest request)
    {
        var result = _service.ApplyCommand(request.DeviceId, request.Action ?? "");
        return result.Success ? Ok(new { result.Message, result.Device }) : BadRequest(new { result.Message });
    }

    [HttpPost("commands/undo")]
    public IActionResult Undo()
    {
        var result = _service.Undo();
        return result.Success ? Ok(new { result.Message, result.Device }) : BadRequest(new { result.Message });
    }

    [HttpGet("commands/history")]
    public IActionResult Commands() => Ok(_service.GetCommandHistory());

    public record AlertRequest(int SensorId, string Type, string Message, int Priority);

    [HttpPost("alerts")]
    public IActionResult Alert([FromBody] AlertRequest request)
    {
        if (request.SensorId <= 0 || string.IsNullOrWhiteSpace(request.Type) || string.IsNullOrWhiteSpace(request.Message) || request.Priority is < 1 or > 3)
            return BadRequest(new { message = "Provide sensorId > 0, type, message and priority from 1 to 3." });
        var result = _service.AddAlert(request.SensorId, request.Type, request.Message, request.Priority);
        return Ok(new { added = result.Added, message = result.Added ? "Alert queued." : "Duplicate active alert ignored.", alert = result.Alert });
    }

    [HttpGet("alerts")]
    public IActionResult Alerts() => Ok(_service.GetAlerts());

    [HttpPost("alerts/process-next")]
    public IActionResult ProcessAlert()
    {
        var alert = _service.ProcessAlert();
        return Ok(new { message = alert is null ? "No pending alerts." : "Highest-priority alert processed.", processedAlert = alert });
    }

    [HttpGet("history")]
    public IActionResult History([FromQuery] int? sensorId, [FromQuery] int limit = 50)
        => Ok(_service.GetHistory(sensorId, limit));

    [HttpGet("recommendations")]
    public IActionResult Recommendations() => Ok(_service.GetRecommendations());
}
