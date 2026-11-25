namespace Backend.Models;

public class SensorReading
{
    public double Temperature { get; set; }

    public double Humidity { get; set; }

    public double BatteryLevel { get; set; }

    public static SensorReading operator +(
        SensorReading first,
        SensorReading second)
    {
        return new SensorReading
        {
            Temperature = first.Temperature + second.Temperature,
            Humidity = first.Humidity + second.Humidity,
            BatteryLevel = first.BatteryLevel + second.BatteryLevel
        };
    }

    public override string ToString()
    {
        return $"Temperature: {Temperature}, " +
               $"Humidity: {Humidity}, " +
               $"Battery: {BatteryLevel}%";
    }
}