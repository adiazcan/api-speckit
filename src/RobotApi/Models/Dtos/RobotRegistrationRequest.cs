namespace RobotApi.Models.Dtos;

/// <summary>
/// Request to register a new robot.
/// </summary>
public sealed class RobotRegistrationRequest
{
    /// <summary>
    /// Unique name for the robot.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Robot model type (e.g., "AGV-100", "Warehouse-Bot-v2").
    /// </summary>
    public required string ModelType { get; init; }

    /// <summary>
    /// Current firmware version (semantic versioning).
    /// </summary>
    public required string FirmwareVersion { get; init; }

    /// <summary>
    /// Robot capabilities configuration.
    /// </summary>
    public required RobotCapabilities Capabilities { get; init; }
}
