using Backend.Models;

namespace Backend.Services;

public class TelemetryGeneratorService
{
    private readonly Random _random = new();

    public TelemetryReading GenerateReading(int sensorId)
    {
        return new TelemetryReading
        {
            SensorId = sensorId,
            Temperature = Math.Round(
                _random.NextDouble() * 35 - 5, 2),

            Humidity = Math.Round(
                _random.NextDouble() * 70 + 20, 2),

            BatteryLevel = Math.Round(
                _random.NextDouble() * 30 + 70, 2),

            RecordedAt = DateTime.UtcNow
        };
    }

    public List<TelemetryReading> GenerateBatch(
        int sensorId,
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        if (quantity > 10000)
        {
            throw new ArgumentException(
                "Quantity cannot exceed 10,000 readings.");
        }

        var readings = new List<TelemetryReading>(quantity);

        for (var i = 0; i < quantity; i++)
        {
            readings.Add(GenerateReading(sensorId));
        }

        return readings;
    }
}