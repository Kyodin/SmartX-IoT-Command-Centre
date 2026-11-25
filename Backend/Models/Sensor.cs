using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Sensor
{
    public int Id { get; set; }

    [Required]
    public string MacAddress { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    public bool IsOnline { get; set; } = true;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}