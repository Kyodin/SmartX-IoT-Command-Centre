
namespace Backend.Models;

public class DeviceAlert
{
    public int Id { get; set; }

    public int SensorId { get; set; }

    public string Message { get; set; } = string.Empty;

    // 1 = Critical, 2 = Warning, 3 = Low
    public int Priority { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
