using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class TelemetryReading
{
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SensorId { get; set; }

    [Range(-50, 100)]
    public double Temperature { get; set; }

    [Range(0, 100)]
    public double Humidity { get; set; }

    [Range(0, 100)]
    public double BatteryLevel { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}