
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly AlertPriorityQueueService _alertService;

    public AlertsController(AlertPriorityQueueService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public IActionResult GetAlerts()
    {
        var alerts = _alertService.GetPendingAlerts();

        return Ok(new
        {
            pendingCount = alerts.Count,
            alerts
        });
    }

    [HttpPost]
    public IActionResult AddAlert([FromBody] DeviceAlert alert)
    {
        if (alert.SensorId <= 0)
        {
            return BadRequest("A valid sensor ID is required.");
        }

        if (string.IsNullOrWhiteSpace(alert.Message))
        {
            return BadRequest("An alert message is required.");
        }

        if (alert.Priority < 1 || alert.Priority > 3)
        {
            return BadRequest(
                "Priority must be 1, 2 or 3.");
        }

        var queuedAlert = _alertService.EnqueueAlert(alert);

        return Ok(new
        {
            message = "Alert added to priority queue.",
            alert = queuedAlert,
            pendingCount = _alertService.GetPendingCount()
        });
    }

    [HttpPost("process-next")]
    public IActionResult ProcessNextAlert()
    {
        var alert = _alertService.ProcessNextAlert();

        if (alert is null)
        {
            return Ok(new
            {
                message = "No alerts waiting to be processed."
            });
        }

        return Ok(new
        {
            message = "Highest-priority alert processed.",
            processedAlert = alert,
            pendingCount = _alertService.GetPendingCount()
        });
    }
}
