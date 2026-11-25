namespace Backend.Models;

public class CombineReadingsRequest
{
    public SensorReading First { get; set; } = new();

    public SensorReading Second { get; set; } = new();
}