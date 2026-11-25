namespace Backend.Models;

public class TelemetryPacket<T>
{
    public int SensorId { get; set; }

    public string MacAddress { get; set; } = string.Empty;

    public T Data { get; set; } = default!;

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return $"Sensor {SensorId} ({MacAddress}) received data: {Data}";
    }
}