
using Backend.Models;

namespace Backend.Services;

public class SensorService
{
    private readonly object _sync = new();

    // Dictionary provides fast lookup using a sensor's unique ID.
    private readonly Dictionary<int, Sensor> _sensors = new();

    // Secondary dictionary provides fast lookup using MAC addresses.
    private readonly Dictionary<string, int> _macIndex =
        new(StringComparer.OrdinalIgnoreCase);

    private int _nextId = 1;

    public SensorService()
    {
        Add(new Sensor
        {
            MacAddress = "AA:BB:CC:DD:EE:01",
            Name = "Temperature Sensor 1",
            Location = "Factory Floor",
            Category = "Temperature",
            IsOnline = true
        });

        Add(new Sensor
        {
            MacAddress = "AA:BB:CC:DD:EE:02",
            Name = "Humidity Sensor 1",
            Location = "Storage Room",
            Category = "Humidity",
            IsOnline = true
        });
    }

    public IReadOnlyList<Sensor> GetAll()
    {
        lock (_sync)
        {
            return _sensors.Values
                .OrderBy(sensor => sensor.Id)
                .ToArray();
        }
    }

    public Sensor? GetById(int id)
    {
        lock (_sync)
        {
            return _sensors.GetValueOrDefault(id);
        }
    }

    public Sensor? GetByMacAddress(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return null;

        lock (_sync)
        {
            var key = macAddress.Trim();

            return _macIndex.TryGetValue(key, out var id)
                ? _sensors.GetValueOrDefault(id)
                : null;
        }
    }

    public void Add(Sensor sensor)
    {
        ArgumentNullException.ThrowIfNull(sensor);

        if (string.IsNullOrWhiteSpace(sensor.MacAddress))
            throw new ArgumentException(
                "A MAC address is required.");

        lock (_sync)
        {
            var mac = sensor.MacAddress.Trim();

            if (_macIndex.ContainsKey(mac))
                throw new InvalidOperationException(
                    "A sensor with this MAC address already exists.");

            sensor.Id = _nextId++;
            sensor.MacAddress = mac;

            _sensors.Add(sensor.Id, sensor);
            _macIndex.Add(mac, sensor.Id);
        }
    }

    public bool SetOnlineStatus(int sensorId, bool isOnline)
    {
        lock (_sync)
        {
            if (!_sensors.TryGetValue(sensorId, out var sensor))
                return false;

            sensor.IsOnline = isOnline;
            return true;
        }
    }
}
