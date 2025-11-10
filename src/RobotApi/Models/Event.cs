namespace RobotApi.Models;

/// <summary>
/// Represents an event that occurred in the robot system.
/// </summary>
public sealed class Event : IEntity
{
    /// <summary>
    /// Unique identifier for the event.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID of the robot that generated this event.
    /// </summary>
    public string RobotId { get; set; } = string.Empty;

    /// <summary>
    /// Type of event (e.g., "command_completed", "battery_low", "sensor_threshold", "command_failed", "error").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the event.
    /// </summary>
    public EventSeverity Severity { get; set; }

    /// <summary>
    /// Timestamp when the event occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Human-readable message describing the event.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional event data as key-value pairs.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Event severity levels.
/// </summary>
public enum EventSeverity
{
    /// <summary>
    /// Informational event.
    /// </summary>
    Info,

    /// <summary>
    /// Warning event that may require attention.
    /// </summary>
    Warning,

    /// <summary>
    /// Error event indicating a problem.
    /// </summary>
    Error,

    /// <summary>
    /// Critical event requiring immediate attention.
    /// </summary>
    Critical
}
