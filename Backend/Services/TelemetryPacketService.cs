using Backend.Models;

namespace Backend.Services;

public class TelemetryPacketService
{
    public TelemetryPacket<SensorReading> CreatePacket(
        int sensorId,
        string macAddress,
        SensorReading reading)
    {
        return new TelemetryPacket<SensorReading>
        {
            SensorId = sensorId,
            MacAddress = macAddress,
            Data = reading,
            ReceivedAt = DateTime.UtcNow
        };
    }

    public SensorReading CombineReadings(
        SensorReading first,
        SensorReading second)
    {
        return first + second;
    }
}