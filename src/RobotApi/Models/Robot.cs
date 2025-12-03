namespace RobotApi.Models;

/// <summary>
/// Represents a physical IoT robot device registered in the system.
/// </summary>
public class Robot : IEntity
{
    /// <summary>
    /// Unique robot identifier (1-50 characters, alphanumeric plus hyphens).
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Human-readable robot name (1-100 characters).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Robot model/SKU identifier (1-50 characters).
    /// </summary>
    public required string ModelType { get; set; }

    /// <summary>
    /// Current firmware version (semantic versioning format).
    /// </summary>
    public required string FirmwareVersion { get; set; }

    /// <summary>
    /// Current connection state.
    /// </summary>
    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Offline;

    /// <summary>
    /// UTC timestamp of last communication.
    /// </summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// UTC timestamp when robot was registered.
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the operator who registered this robot (for internal auditing only, not exposed in API).
    /// </summary>
    public string? RegisteredByOperatorId { get; set; }

    /// <summary>
    /// Dictionary of supported features (commands and sensors).
    /// </summary>
    public required RobotCapabilities Capabilities { get; set; }
}

/// <summary>
/// Robot connection status.
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// Robot is not connected.
    /// </summary>
    Offline = 0,

    /// <summary>
    /// Robot is connected and operational.
    /// </summary>
    Online = 1
}

/// <summary>
/// Supported capabilities for a robot.
/// </summary>
public class RobotCapabilities
{
    /// <summary>
    /// List of supported command types (e.g., "move", "rotate", "stop").
    /// </summary>
    public required string[] Commands { get; set; }

    /// <summary>
    /// List of available sensors (e.g., "battery", "temperature", "position").
    /// </summary>
    public required string[] Sensors { get; set; }

    /// <summary>
    /// Maximum speed in m/s (optional).
    /// </summary>
    public double? MaxSpeed { get; set; }

    /// <summary>
    /// Maximum distance in meters (optional).
    /// </summary>
    public double? MaxDistance { get; set; }
}
