using System.Collections.Generic;

namespace RobotApi.Models;

/// <summary>
/// Represents a snapshot of robot sensor data at a point in time.
/// </summary>
public sealed class Telemetry : IEntity
{
    /// <summary>
    /// Gets or sets the unique telemetry snapshot identifier (GUID).
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the source robot ID (foreign key).
    /// </summary>
    public required string RobotId { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when data was captured.
    /// </summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the robot position coordinates.
    /// </summary>
    public required Position Position { get; set; }

    /// <summary>
    /// Gets or sets the robot orientation (pitch, roll, yaw).
    /// </summary>
    public required Orientation Orientation { get; set; }

    /// <summary>
    /// Gets or sets the current speed in m/s.
    /// </summary>
    public required double Speed { get; set; }

    /// <summary>
    /// Gets or sets the battery percentage (0-100).
    /// </summary>
    public required int BatteryLevel { get; set; }

    /// <summary>
    /// Gets or sets the internal temperature in Celsius.
    /// </summary>
    public required double Temperature { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of sensor-specific values.
    /// Keys must match robot.capabilities.sensors.
    /// </summary>
    public required Dictionary<string, double> SensorReadings { get; set; }

    /// <summary>
    /// Computes data freshness in seconds (time since timestamp).
    /// </summary>
    public double GetDataFreshness()
    {
        return (DateTime.UtcNow - Timestamp).TotalSeconds;
    }
}
