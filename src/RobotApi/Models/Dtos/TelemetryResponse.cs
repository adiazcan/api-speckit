using System.Collections.Generic;

namespace RobotApi.Models.Dtos;

/// <summary>
/// Response DTO for telemetry data retrieval.
/// </summary>
public sealed class TelemetryResponse
{
    /// <summary>
    /// Gets or sets the unique telemetry snapshot identifier.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the source robot ID.
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
    /// </summary>
    public required Dictionary<string, double> SensorReadings { get; set; }

    /// <summary>
    /// Gets or sets the data freshness in seconds (computed at query time).
    /// </summary>
    public required double DataFreshness { get; set; }

    /// <summary>
    /// Creates a TelemetryResponse from a Telemetry entity.
    /// </summary>
    public static TelemetryResponse FromTelemetry(Telemetry telemetry)
    {
        return new TelemetryResponse
        {
            Id = telemetry.Id,
            RobotId = telemetry.RobotId,
            Timestamp = telemetry.Timestamp,
            Position = telemetry.Position,
            Orientation = telemetry.Orientation,
            Speed = telemetry.Speed,
            BatteryLevel = telemetry.BatteryLevel,
            Temperature = telemetry.Temperature,
            SensorReadings = telemetry.SensorReadings,
            DataFreshness = telemetry.GetDataFreshness()
        };
    }
}
